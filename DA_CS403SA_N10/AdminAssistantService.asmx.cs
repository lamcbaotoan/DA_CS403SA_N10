using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Web.Script.Services;
using System.Web.Services;

// Namespace phải khớp với project của bạn
namespace Webebook
{
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    [ScriptService]
    public class AdminAssistantService : System.Web.Services.WebService
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["datawebebookConnectionString"].ConnectionString;
        private static readonly HttpClient client = new HttpClient();

        // Static constructor để thiết lập TLS 1.2 một lần duy nhất khi ứng dụng khởi chạy
        static AdminAssistantService()
        {
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public ChatResponse GetAdminResponse(string userMessage)
        {
            return Task.Run(() => GetGeminiResponseAsync(userMessage)).Result;
        }

        private async Task<ChatResponse> GetGeminiResponseAsync(string userMessage)
        {
            string apiKey = ConfigurationManager.AppSettings["GeminiApiKey"];
            if (string.IsNullOrEmpty(apiKey) || apiKey.Contains("API_KEY_CUA_BAN"))
                return new ChatResponse { Text = "Lỗi cấu hình: API Key của Gemini chưa được thiết lập." };

            var apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={apiKey}";

            // System Prompt đã được nâng cấp
            string systemPrompt = @"Bạn là Trợ lý Quản trị cho trang web bán sách Webebook. Bạn thông minh, chuyên nghiệp và chỉ cung cấp thông tin quản trị.
                Bạn có các khả năng sau và chỉ được trả về MỘT LỆNH DUY NHẤT:

                **I. THỐNG KÊ TỔNG QUAN:**
                1.  Tổng số sách đang có: [COMMAND:GET_TOTAL_BOOK_COUNT]
                2.  Tổng số người dùng đã đăng ký: [COMMAND:GET_TOTAL_USER_COUNT]
                3.  Điểm đánh giá trung bình của tất cả sách: [COMMAND:GET_AVERAGE_RATING]
                4.  Phân bố các điểm đánh giá (1 đến 5 sao): [COMMAND:GET_RATING_DISTRIBUTION]

                **II. PHÂN TÍCH DOANH THU & BÁN HÀNG:**
                5.  Doanh thu theo khoảng thời gian: [COMMAND:GET_REVENUE:period] (period: TODAY, THIS_MONTH, THIS_QUARTER, THIS_YEAR)
                6.  Liệt kê sách bán chạy nhất: [COMMAND:GET_TOP_SELLING_BOOKS:limit] (Mặc định limit=5 nếu không chỉ định)
                7.  Liệt kê sách bán chậm nhất: [COMMAND:GET_LEAST_SELLING_BOOKS:limit] (Mặc định limit=5)
                8.  Tính giá trị đơn hàng trung bình (AOV): [COMMAND:GET_AOV]
                9.  Tìm đơn hàng có giá trị lớn nhất: [COMMAND:GET_LARGEST_ORDER]

                **III. QUẢN LÝ ĐƠN HÀNG:**
                10. Liệt kê các đơn hàng theo trạng thái: [COMMAND:GET_ORDERS_BY_STATUS:status] (status: Pending, Completed, Cancelled...)
                11. Kiểm tra trạng thái của một đơn hàng cụ thể: [COMMAND:GET_ORDER_STATUS:order_id]
                12. Cập nhật trạng thái một đơn hàng: [COMMAND:UPDATE_ORDER_STATUS:order_id,new_status] (VÍ DỤ: [COMMAND:UPDATE_ORDER_STATUS:123,Completed])
                13. Thống kê đơn hàng theo phương thức thanh toán: [COMMAND:COUNT_ORDERS_BY_PAYMENT_METHOD]

                **IV. QUẢN LÝ NGƯỜI DÙNG:**
                14. Tìm thông tin người dùng qua email: [COMMAND:FIND_USER_BY_EMAIL:email]
                15. Tìm thông tin người dùng qua SĐT: [COMMAND:FIND_USER_BY_PHONE:phone_number]
                16. Khóa một người dùng: [COMMAND:UPDATE_USER_STATUS:email,Banned]
                17. Mở khóa một người dùng: [COMMAND:UPDATE_USER_STATUS:email,Active]
                18. Thống kê số lượng người dùng theo trạng thái: [COMMAND:COUNT_USERS_BY_STATUS]
                
                **V. QUẢN LÝ NỘI DUNG & TƯƠNG TÁC:**
                19. Liệt kê sách chưa có nội dung: [COMMAND:GET_BOOKS_NO_CONTENT]
                20. Đếm số chương của một sách: [COMMAND:GET_CHAPTER_COUNT:tên sách]
                21. Tìm sách có bình luận gần nhất: [COMMAND:GET_LATEST_COMMENTED_BOOK]
                22. Xem các bình luận gần đây của một sách: [COMMAND:GET_RECENT_COMMENTS:tên sách]
                23. Tìm sách theo nhà xuất bản: [COMMAND:FIND_BOOKS_BY_PUBLISHER:tên nhà xuất bản]
                24. Tìm sách theo nhóm dịch: [COMMAND:FIND_BOOKS_BY_TEAM:tên nhóm dịch]
                25. Thống kê sách theo trạng thái nội dung (Hoàn thành, Đang cập nhật...): [COMMAND:COUNT_BOOKS_BY_STATUS]

                **VI. PHÂN TÍCH NGƯỜI DÙNG NÂNG CAO:**
                26. Liệt kê người dùng chi tiêu nhiều nhất: [COMMAND:GET_TOP_SPENDING_USERS:limit] (Mặc định limit=5)
                27. Liệt kê người dùng đánh giá nhiều nhất: [COMMAND:GET_TOP_REVIEWERS:limit] (Mặc định limit=5)
                28. Tìm những người dùng chưa mua hàng: [COMMAND:GET_USERS_NO_PURCHASE]

                **VII. BẢO TRÌ & HÀNH ĐỘNG (THẬN TRỌNG):**
                29. Xóa TẤT CẢ đánh giá của một cuốn sách: [COMMAND:DELETE_REVIEWS_FOR_BOOK:tên sách] (HÀNH ĐỘNG NGUY HIỂM)
                30. Xóa TẤT CẢ đánh giá của một người dùng: [COMMAND:DELETE_REVIEWS_BY_USER:email] (HÀNH ĐỘNG NGUY HIỂM)
                31. Dọn dẹp giỏ hàng của một người dùng: [COMMAND:CLEAR_USER_CART:email]

                Với các câu hỏi khác, hãy trả lời trong vai trò một trợ lý quản trị chuyên nghiệp.";

            var fullPrompt = $"{systemPrompt}\n\nCâu hỏi của quản trị viên: {userMessage}";
            var requestPayload = new GeminiRequest { contents = new List<Content> { new Content { parts = new List<Part> { new Part { text = fullPrompt } } } } };
            var jsonPayload = new JavaScriptSerializer().Serialize(requestPayload);
            var httpContent = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            try
            {
                var httpResponse = await client.PostAsync(apiUrl, httpContent);
                var jsonResponse = await httpResponse.Content.ReadAsStringAsync();
                var geminiResponse = new JavaScriptSerializer().Deserialize<GeminiResponse>(jsonResponse);

                if (httpResponse.IsSuccessStatusCode)
                {
                    string aiResponse = geminiResponse?.candidates?[0]?.content?.parts?[0]?.text?.Trim() ?? "Xin lỗi, tôi không nhận được phản hồi hợp lệ.";
                    if (aiResponse.ToUpper().Contains("COMMAND:"))
                    {
                        return ProcessAdminCommand(aiResponse);
                    }
                    return new ChatResponse { Text = aiResponse };
                }
                return new ChatResponse { Text = $"Lỗi khi kết nối đến dịch vụ AI: {geminiResponse?.error?.message ?? jsonResponse}" };
            }
            catch (Exception ex)
            {
                return new ChatResponse { Text = $"Rất xin lỗi, có sự cố kỹ thuật: {ex.Message}" };
            }
        }

        private ChatResponse ProcessAdminCommand(string command)
        {
            command = command.Trim('[', ']');
            var parts = command.Split(new[] { ':' }, 3);
            string commandType = parts.Length > 1 ? parts[1].Trim().ToUpper() : string.Empty;
            string argument = parts.Length > 2 ? parts[2].Trim() : string.Empty;
            string[] args = argument.Split(',');
            int.TryParse(argument, out int limit);
            limit = (limit <= 0) ? 5 : limit; // Default limit

            switch (commandType)
            {
                // I. THỐNG KÊ TỔNG QUAN
                case "GET_TOTAL_BOOK_COUNT": return new ChatResponse { Text = GetTotalBookCount() };
                case "GET_TOTAL_USER_COUNT": return new ChatResponse { Text = GetTotalUserCount() };
                case "GET_AVERAGE_RATING": return new ChatResponse { Text = GetAverageRating() };
                case "GET_RATING_DISTRIBUTION": return new ChatResponse { Text = GetRatingDistribution() };

                // II. PHÂN TÍCH DOANH THU & BÁN HÀNG
                case "GET_REVENUE": return new ChatResponse { Text = GetRevenueByPeriod(argument) };
                case "GET_TOP_SELLING_BOOKS": return new ChatResponse { Text = GetTopSellingBooks(true, limit) };
                case "GET_LEAST_SELLING_BOOKS": return new ChatResponse { Text = GetTopSellingBooks(false, limit) };
                case "GET_AOV": return new ChatResponse { Text = GetAverageOrderValue() }; // Mới
                case "GET_LARGEST_ORDER": return new ChatResponse { Text = GetLargestOrder() }; // Mới

                // III. QUẢN LÝ ĐƠN HÀNG
                case "GET_ORDERS_BY_STATUS": return new ChatResponse { Text = GetOrdersByStatus(argument) };
                case "GET_ORDER_STATUS": if (int.TryParse(argument, out int orderId)) return new ChatResponse { Text = GetOrderStatus(orderId) }; return new ChatResponse { Text = "Mã đơn hàng không hợp lệ." };
                case "UPDATE_ORDER_STATUS": if (args.Length == 2 && int.TryParse(args[0].Trim(), out int orderIdToUpdate) && !string.IsNullOrWhiteSpace(args[1])) return new ChatResponse { Text = UpdateOrderStatus(orderIdToUpdate, args[1].Trim()) }; return new ChatResponse { Text = "Cú pháp lệnh không đúng. Cần: [COMMAND:UPDATE_ORDER_STATUS:mã_đơn_hàng,trạng_thái_mới]" };
                case "COUNT_ORDERS_BY_PAYMENT_METHOD": return new ChatResponse { Text = CountOrdersByPaymentMethod() }; // Mới

                // IV. QUẢN LÝ NGƯỜI DÙNG
                case "FIND_USER_BY_EMAIL": return new ChatResponse { Text = FindUserByEmail(argument) };
                case "FIND_USER_BY_PHONE": return new ChatResponse { Text = FindUserByPhone(argument) }; // Mới
                case "UPDATE_USER_STATUS": if (args.Length == 2 && !string.IsNullOrWhiteSpace(args[0]) && !string.IsNullOrWhiteSpace(args[1])) return new ChatResponse { Text = UpdateUserStatus(args[0].Trim(), args[1].Trim()) }; return new ChatResponse { Text = "Cú pháp lệnh không đúng. Cần: [COMMAND:UPDATE_USER_STATUS:email,trạng_thái_mới]" };
                case "COUNT_USERS_BY_STATUS": return new ChatResponse { Text = CountUsersByStatus() }; // Mới

                // V. QUẢN LÝ NỘI DUNG & TƯƠNG TÁC
                case "GET_BOOKS_NO_CONTENT": return new ChatResponse { Text = GetBooksNoContent() }; // Mới
                case "GET_CHAPTER_COUNT": return new ChatResponse { Text = GetChapterCount(argument) }; // Mới
                case "GET_LATEST_COMMENTED_BOOK": return new ChatResponse { Text = GetLatestCommentedBook() }; // Mới
                case "GET_RECENT_COMMENTS": return new ChatResponse { Text = GetRecentComments(argument) }; // Mới
                case "FIND_BOOKS_BY_PUBLISHER": return new ChatResponse { Text = FindBooksByPublisher(argument) }; // Mới
                case "FIND_BOOKS_BY_TEAM": return new ChatResponse { Text = FindBooksByTeam(argument) }; // Mới
                case "COUNT_BOOKS_BY_STATUS": return new ChatResponse { Text = CountBooksByContentStatus() }; // Mới

                // VI. PHÂN TÍCH NGƯỜI DÙNG NÂNG CAO
                case "GET_TOP_SPENDING_USERS": return new ChatResponse { Text = GetTopSpendingUsers(limit) }; // Mới
                case "GET_TOP_REVIEWERS": return new ChatResponse { Text = GetTopReviewers(limit) }; // Mới
                case "GET_USERS_NO_PURCHASE": return new ChatResponse { Text = GetUsersNoPurchase() }; // Mới

                // VII. BẢO TRÌ & HÀNH ĐỘNG
                case "DELETE_REVIEWS_FOR_BOOK": return new ChatResponse { Text = DeleteReviewsForBook(argument) }; // Mới
                case "DELETE_REVIEWS_BY_USER": return new ChatResponse { Text = DeleteReviewsByUser(argument) }; // Mới
                case "CLEAR_USER_CART": return new ChatResponse { Text = ClearUserCart(argument) }; // Mới

                default:
                    return new ChatResponse { Text = "Lệnh từ AI không hợp lệ." };
            }
        }

        #region Các Hàm Truy Vấn CSDL Cho Admin (Hiện có & Nâng cấp)

        // I. THỐNG KÊ TỔNG QUAN (KHÔNG ĐỔI)
        private string GetTotalBookCount()
        {
            string query = "SELECT COUNT(*) FROM Sach";
            object result = ExecuteScalar(query);
            return $"Hiện đang có tổng cộng <b>{result}</b> đầu sách trên hệ thống.";
        }

        private string GetTotalUserCount()
        {
            string query = "SELECT COUNT(*) FROM NguoiDung";
            object result = ExecuteScalar(query);
            return $"Hiện đang có tổng cộng <b>{result}</b> người dùng đã đăng ký.";
        }

        private string GetAverageRating()
        {
            string query = "SELECT AVG(CAST(Diem AS float)) FROM DanhGiaSach";
            object result = ExecuteScalar(query);
            if (result == null || result == DBNull.Value) return "Chưa có đánh giá nào.";
            return $"Điểm đánh giá trung bình của tất cả các sách là: <b>{Convert.ToDouble(result):F2} / 5.0</b> sao.";
        }

        private string GetRatingDistribution()
        {
            var result = new StringBuilder("Phân bố điểm đánh giá trên toàn hệ thống:<br/>");
            string query = "SELECT Diem, COUNT(*) AS SoLuong FROM DanhGiaSach GROUP BY Diem ORDER BY Diem DESC";
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(query, con))
            {
                try
                {
                    con.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.HasRows) return "Chưa có dữ liệu phân bố đánh giá.";
                        while (reader.Read())
                        {
                            result.AppendLine($"- <b>{reader["Diem"]} sao:</b> {reader["SoLuong"]} lượt đánh giá<br/>");
                        }
                    }
                }
                catch { return "Không thể truy vấn phân bố đánh giá."; }
            }
            return result.ToString();
        }

        // II. PHÂN TÍCH DOANH THU & BÁN HÀNG (BỔ SUNG)
        private string GetRevenueByPeriod(string period)
        {
            string whereClause;
            string timeDescription;
            switch (period.Trim().ToUpper())
            {
                case "TODAY":
                    whereClause = "WHERE TrangThaiThanhToan = 'Completed' AND CONVERT(date, NgayDat) = CONVERT(date, GETDATE())";
                    timeDescription = "hôm nay";
                    break;
                case "THIS_MONTH":
                    whereClause = "WHERE TrangThaiThanhToan = 'Completed' AND MONTH(NgayDat) = MONTH(GETDATE()) AND YEAR(NgayDat) = YEAR(GETDATE())";
                    timeDescription = "tháng này";
                    break;
                case "THIS_QUARTER":
                    whereClause = "WHERE TrangThaiThanhToan = 'Completed' AND DATEPART(quarter, NgayDat) = DATEPART(quarter, GETDATE()) AND YEAR(NgayDat) = YEAR(GETDATE())";
                    timeDescription = "quý này";
                    break;
                case "THIS_YEAR":
                    whereClause = "WHERE TrangThaiThanhToan = 'Completed' AND YEAR(NgayDat) = YEAR(GETDATE())";
                    timeDescription = "năm nay";
                    break;
                default:
                    return "Khoảng thời gian không hợp lệ. Chỉ hỗ trợ: TODAY, THIS_MONTH, THIS_QUARTER, THIS_YEAR.";
            }

            string query = $"SELECT SUM(SoTien) FROM DonHang {whereClause}";
            object result = ExecuteScalar(query);
            if (result == null || result == DBNull.Value) return $"Chưa có doanh thu cho {timeDescription}.";
            return $"Tổng doanh thu {timeDescription} là: <b>{Convert.ToDecimal(result):N0} VNĐ</b>.";
        }

        private string GetTopSellingBooks(bool isTop, int limit)
        {
            var result = new StringBuilder(isTop ? $"Top {limit} sách bán chạy nhất:<br/>" : $"Top {limit} sách bán chậm nhất (có ít nhất 1 lượt mua):<br/>");
            string orderDirection = isTop ? "DESC" : "ASC";
            string query = $@"SELECT TOP {limit} s.TenSach, SUM(ct.SoLuong) AS TongSoLuongBan 
                             FROM ChiTietDonHang ct JOIN Sach s ON ct.IDSach = s.IDSach 
                             JOIN DonHang dh ON ct.IDDonHang = dh.IDDonHang
                             WHERE dh.TrangThaiThanhToan = 'Completed'
                             GROUP BY s.TenSach 
                             HAVING SUM(ct.SoLuong) > 0
                             ORDER BY TongSoLuongBan {orderDirection}";
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(query, con))
            {
                try
                {
                    con.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.HasRows) return "Chưa có dữ liệu sách bán chạy để thống kê.";
                        while (reader.Read())
                        {
                            result.AppendLine($"- <b>{reader["TenSach"]}</b> (đã bán: {reader["TongSoLuongBan"]} quyển)<br/>");
                        }
                    }
                }
                catch { return "Không thể truy vấn sách bán chạy."; }
            }
            return result.ToString();
        }

        // III. QUẢN LÝ ĐƠN HÀNG (BỔ SUNG)
        private string GetOrdersByStatus(string status)
        {
            if (string.IsNullOrWhiteSpace(status)) return "Vui lòng cung cấp trạng thái đơn hàng (Pending, Completed, Cancelled...).";
            var result = new StringBuilder($"10 đơn hàng gần nhất có trạng thái '{status}':<br/>");
            string query = @"SELECT TOP 10 IDDonHang, NgayDat, SoTien 
                             FROM DonHang WHERE TrangThaiThanhToan LIKE @Status 
                             ORDER BY NgayDat DESC";
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@Status", "%" + status + "%");
                try
                {
                    con.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.HasRows) return $"Không có đơn hàng nào có trạng thái '{status}'.";
                        while (reader.Read())
                        {
                            result.AppendLine($"- Đơn #{reader["IDDonHang"]} ngày {((DateTime)reader["NgayDat"]):dd/MM/yyyy}, tổng tiền {((decimal)reader["SoTien"]):N0}đ<br/>");
                        }
                    }
                }
                catch { return "Không thể truy vấn danh sách đơn hàng."; }
            }
            return result.ToString();
        }

        private string UpdateOrderStatus(int orderId, string newStatus)
        {
            if (orderId <= 0) return "Mã đơn hàng không hợp lệ.";
            if (string.IsNullOrWhiteSpace(newStatus)) return "Trạng thái mới không được để trống.";

            if (!newStatus.Equals("Completed", StringComparison.OrdinalIgnoreCase))
            {
                string updateQuery = "UPDATE DonHang SET TrangThaiThanhToan = @NewStatus WHERE IDDonHang = @OrderId";
                using (var con = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand(updateQuery, con))
                {
                    cmd.Parameters.AddWithValue("@NewStatus", newStatus);
                    cmd.Parameters.AddWithValue("@OrderId", orderId);
                    try
                    {
                        con.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();
                        return rowsAffected > 0
                            ? $"Đã cập nhật trạng thái cho đơn hàng #{orderId} thành <b>{newStatus}</b>."
                            : $"Không tìm thấy đơn hàng #{orderId} để cập nhật.";
                    }
                    catch { return "Cập nhật trạng thái đơn hàng thất bại do lỗi hệ thống."; }
                }
            }

            using (var con = new SqlConnection(connectionString))
            {
                con.Open();
                using (var transaction = con.BeginTransaction())
                {
                    try
                    {
                        int userId = 0;
                        var sachIds = new List<int>();

                        string getOrderInfoQuery = "SELECT IDNguoiDung FROM DonHang WHERE IDDonHang = @OrderId";
                        using (var cmd = new SqlCommand(getOrderInfoQuery, con, transaction))
                        {
                            cmd.Parameters.AddWithValue("@OrderId", orderId);
                            var result = cmd.ExecuteScalar();
                            if (result == null || result == DBNull.Value) throw new Exception($"Không tìm thấy đơn hàng #{orderId}.");
                            userId = Convert.ToInt32(result);
                        }

                        string getBookIdsQuery = "SELECT IDSach FROM ChiTietDonHang WHERE IDDonHang = @OrderId";
                        using (var cmd = new SqlCommand(getBookIdsQuery, con, transaction))
                        {
                            cmd.Parameters.AddWithValue("@OrderId", orderId);
                            using (var reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    sachIds.Add(Convert.ToInt32(reader["IDSach"]));
                                }
                            }
                        }

                        if (sachIds.Count == 0) throw new Exception("Đơn hàng không có sản phẩm nào.");

                        string updateStatusQuery = "UPDATE DonHang SET TrangThaiThanhToan = @NewStatus WHERE IDDonHang = @OrderId";
                        using (var cmd = new SqlCommand(updateStatusQuery, con, transaction))
                        {
                            cmd.Parameters.AddWithValue("@NewStatus", newStatus);
                            cmd.Parameters.AddWithValue("@OrderId", orderId);
                            cmd.ExecuteNonQuery();
                        }

                        int booksAddedCount = 0;
                        foreach (int sachId in sachIds)
                        {
                            string checkExistQuery = "SELECT COUNT(*) FROM TuSach WHERE IDNguoiDung = @UserId AND IDSach = @SachId";
                            using (var cmdCheck = new SqlCommand(checkExistQuery, con, transaction))
                            {
                                cmdCheck.Parameters.AddWithValue("@UserId", userId);
                                cmdCheck.Parameters.AddWithValue("@SachId", sachId);
                                int existingCount = (int)cmdCheck.ExecuteScalar();
                                if (existingCount == 0)
                                {
                                    string insertQuery = "INSERT INTO TuSach (IDNguoiDung, IDSach, NgayThem) VALUES (@UserId, @SachId, GETDATE())";
                                    using (var cmdInsert = new SqlCommand(insertQuery, con, transaction))
                                    {
                                        cmdInsert.Parameters.AddWithValue("@UserId", userId);
                                        cmdInsert.Parameters.AddWithValue("@SachId", sachId);
                                        cmdInsert.ExecuteNonQuery();
                                        booksAddedCount++;
                                    }
                                }
                            }
                        }

                        transaction.Commit();
                        return $"Đã cập nhật đơn hàng #{orderId} thành <b>{newStatus}</b> và đã thêm <b>{booksAddedCount} / {sachIds.Count}</b> sách mới vào tủ sách của người dùng.";
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        System.Diagnostics.Debug.WriteLine($"Lỗi khi hoàn thành đơn hàng #{orderId}: {ex.Message}");
                        return $"Cập nhật đơn hàng thất bại: {ex.Message}";
                    }
                }
            }
        }

        // IV. QUẢN LÝ NGƯỜI DÙNG (BỔ SUNG)
        private string UpdateUserStatus(string email, string newStatus)
        {
            if (string.IsNullOrWhiteSpace(email)) return "Vui lòng cung cấp email người dùng.";
            if (!newStatus.Equals("Active", StringComparison.OrdinalIgnoreCase) && !newStatus.Equals("Banned", StringComparison.OrdinalIgnoreCase))
                return "Trạng thái mới không hợp lệ. Chỉ chấp nhận 'Active' hoặc 'Banned'.";
            if (email.Equals("admin@admin.com", StringComparison.OrdinalIgnoreCase)) return "Không thể thay đổi trạng thái của tài khoản admin.";

            string query = "UPDATE NguoiDung SET TrangThai = @NewStatus WHERE Email = @Email";
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@NewStatus", newStatus);
                cmd.Parameters.AddWithValue("@Email", email);
                try
                {
                    con.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        string action = newStatus.Equals("Active", StringComparison.OrdinalIgnoreCase) ? "Mở khóa" : "Khóa";
                        return $"Đã <b>{action}</b> tài khoản người dùng có email: {email}.";
                    }
                    return $"Không tìm thấy người dùng có email: {email}.";
                }
                catch { return "Cập nhật trạng thái người dùng thất bại do lỗi hệ thống."; }
            }
        }

        private string FindUserByEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return "Vui lòng cung cấp địa chỉ email để tìm kiếm.";
            var result = new StringBuilder();
            string query = "SELECT Username, Ten, DienThoai, Email, TrangThai FROM NguoiDung WHERE Email = @Email";
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@Email", email);
                try
                {
                    con.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read()) return $"Không tìm thấy người dùng nào có email: {email}";
                        result.AppendLine($"Thông tin người dùng <b>{email}</b>:<br/>");
                        result.AppendLine($"- Tên tài khoản: {reader["Username"]}<br/>");
                        result.AppendLine($"- Tên hiển thị: {reader["Ten"]}<br/>");
                        result.AppendLine($"- SĐT: {reader["DienThoai"]}<br/>");
                        result.AppendLine($"- Trạng thái: {reader["TrangThai"]}");
                    }
                }
                catch { return "Không thể truy vấn thông tin người dùng."; }
            }
            return result.ToString();
        }

        private string GetOrderStatus(int orderId)
        {
            if (orderId <= 0) return "Mã đơn hàng không hợp lệ.";
            string query = "SELECT TrangThaiThanhToan FROM DonHang WHERE IDDonHang = @OrderId";
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@OrderId", orderId);
                try
                {
                    con.Open();
                    object status = cmd.ExecuteScalar();
                    if (status == null || status == DBNull.Value) return $"Không tìm thấy đơn hàng với mã #{orderId}.";
                    return $"Trạng thái đơn hàng #{orderId} là: <b>{status}</b>.";
                }
                catch { return "Không thể truy vấn trạng thái đơn hàng."; }
            }
        }

        #endregion

        #region Các Hàm Truy Vấn MỚI

        // II. PHÂN TÍCH DOANH THU & BÁN HÀNG (MỚI)
        private string GetAverageOrderValue()
        {
            string query = "SELECT AVG(SoTien) FROM DonHang WHERE TrangThaiThanhToan = 'Completed'";
            object result = ExecuteScalar(query);
            if (result == null || result == DBNull.Value) return "Chưa có đơn hàng nào được hoàn thành để tính AOV.";
            return $"Giá trị đơn hàng trung bình (AOV) là: <b>{Convert.ToDecimal(result):N0} VNĐ</b>.";
        }

        private string GetLargestOrder()
        {
            string query = "SELECT TOP 1 IDDonHang, SoTien, NgayDat FROM DonHang ORDER BY SoTien DESC";
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(query, con))
            {
                try
                {
                    con.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return $"Đơn hàng có giá trị lớn nhất là <b>#{reader["IDDonHang"]}</b> với tổng tiền <b>{Convert.ToDecimal(reader["SoTien"]):N0} VNĐ</b>, đặt ngày {((DateTime)reader["NgayDat"]):dd/MM/yyyy}.";
                        }
                        return "Chưa có đơn hàng nào trong hệ thống.";
                    }
                }
                catch { return "Lỗi khi truy vấn đơn hàng lớn nhất."; }
            }
        }

        // III. QUẢN LÝ ĐƠN HÀNG (MỚI)
        private string CountOrdersByPaymentMethod()
        {
            var result = new StringBuilder("Thống kê đơn hàng theo phương thức thanh toán:<br/>");
            string query = "SELECT PhuongThucThanhToan, COUNT(*) AS SoLuong FROM DonHang GROUP BY PhuongThucThanhToan";
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(query, con))
            {
                try
                {
                    con.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.HasRows) return "Chưa có dữ liệu về phương thức thanh toán.";
                        while (reader.Read())
                        {
                            result.AppendLine($"- <b>{reader["PhuongThucThanhToan"]}:</b> {reader["SoLuong"]} đơn hàng<br/>");
                        }
                    }
                }
                catch { return "Không thể truy vấn thống kê phương thức thanh toán."; }
            }
            return result.ToString();
        }

        // IV. QUẢN LÝ NGƯỜI DÙNG (MỚI)
        private string FindUserByPhone(string phone)
        {
            if (string.IsNullOrWhiteSpace(phone)) return "Vui lòng cung cấp số điện thoại để tìm kiếm.";
            var result = new StringBuilder();
            string query = "SELECT Username, Ten, DienThoai, Email, TrangThai FROM NguoiDung WHERE DienThoai = @Phone";
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@Phone", phone);
                try
                {
                    con.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read()) return $"Không tìm thấy người dùng nào có SĐT: {phone}";
                        result.AppendLine($"Thông tin người dùng có SĐT <b>{phone}</b>:<br/>");
                        result.AppendLine($"- Tên tài khoản: {reader["Username"]}<br/>");
                        result.AppendLine($"- Tên hiển thị: {reader["Ten"]}<br/>");
                        result.AppendLine($"- Email: {reader["Email"]}<br/>");
                        result.AppendLine($"- Trạng thái: {reader["TrangThai"]}");
                    }
                }
                catch { return "Không thể truy vấn thông tin người dùng bằng SĐT."; }
            }
            return result.ToString();
        }

        private string CountUsersByStatus()
        {
            var result = new StringBuilder("Thống kê người dùng theo trạng thái:<br/>");
            string query = "SELECT TrangThai, COUNT(*) as SoLuong FROM NguoiDung GROUP BY TrangThai";
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(query, con))
            {
                try
                {
                    con.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.HasRows) return "Chưa có dữ liệu về trạng thái người dùng.";
                        while (reader.Read())
                        {
                            result.AppendLine($"- <b>{reader["TrangThai"]}:</b> {reader["SoLuong"]} người dùng<br/>");
                        }
                    }
                }
                catch { return "Không thể truy vấn thống kê trạng thái người dùng."; }
            }
            return result.ToString();
        }

        // V. QUẢN LÝ NỘI DUNG & TƯƠNG TÁC (MỚI)
        private string GetBooksNoContent()
        {
            var result = new StringBuilder("Các sách được ghi nhận chưa có nội dung:<br/>");
            string query = "SELECT s.TenSach FROM Sach s LEFT JOIN NoiDungSach ns ON s.IDSach = ns.IDSach WHERE ns.IDNoiDung IS NULL";
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(query, con))
            {
                try
                {
                    con.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.HasRows) return "Tất cả sách đều đã có nội dung.";
                        while (reader.Read())
                        {
                            result.AppendLine($"- <b>{reader["TenSach"]}</b><br/>");
                        }
                    }
                }
                catch { return "Lỗi khi truy vấn sách chưa có nội dung."; }
            }
            return result.ToString();
        }

        private string GetChapterCount(string bookName)
        {
            string query = "SELECT COUNT(ns.IDNoiDung) FROM NoiDungSach ns JOIN Sach s ON ns.IDSach = s.IDSach WHERE s.TenSach LIKE @TenSach";
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@TenSach", $"%{bookName}%");
                try
                {
                    con.Open();
                    object result = cmd.ExecuteScalar();
                    if (result == null || result == DBNull.Value) return $"Không tìm thấy sách nào khớp với '{bookName}'.";
                    return $"Sách '{bookName}' có <b>{result}</b> chương.";
                }
                catch { return "Lỗi khi đếm số chương của sách."; }
            }
        }

        private string GetLatestCommentedBook()
        {
            string query = "SELECT TOP 1 s.TenSach, t.NgayBinhLuan FROM TuongTac t JOIN Sach s ON t.IDSach = s.IDSach ORDER BY t.NgayBinhLuan DESC";
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(query, con))
            {
                try
                {
                    con.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return $"Sách có bình luận gần đây nhất là <b>{reader["TenSach"]}</b> vào lúc {((DateTime)reader["NgayBinhLuan"]):dd/MM/yyyy HH:mm}.";
                        }
                        return "Chưa có bình luận nào trên hệ thống.";
                    }
                }
                catch { return "Lỗi khi truy vấn bình luận gần nhất."; }
            }
        }

        private string GetRecentComments(string bookName)
        {
            var result = new StringBuilder($"5 bình luận gần nhất của sách '{bookName}':<br/>");
            string query = @"SELECT TOP 5 t.BinhLuan, u.Username, t.NgayBinhLuan 
                             FROM TuongTac t 
                             JOIN Sach s ON t.IDSach = s.IDSach
                             JOIN NguoiDung u ON t.IDNguoiDung = u.IDNguoiDung
                             WHERE s.TenSach LIKE @TenSach
                             ORDER BY t.NgayBinhLuan DESC";
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@TenSach", $"%{bookName}%");
                try
                {
                    con.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.HasRows) return $"Không tìm thấy bình luận nào cho sách '{bookName}'.";
                        while (reader.Read())
                        {
                            result.AppendLine($"- <b>{reader["Username"]}</b> ({((DateTime)reader["NgayBinhLuan"]):dd/MM/yy}): <i>{reader["BinhLuan"]}</i><br/>");
                        }
                    }
                }
                catch { return "Lỗi khi lấy bình luận của sách."; }
            }
            return result.ToString();
        }

        private string FindBooksByPublisher(string publisherName)
        {
            var result = new StringBuilder($"Các sách của NXB '{publisherName}':<br/>");
            string query = "SELECT TOP 10 TenSach FROM Sach WHERE NhaXuatBan LIKE @Publisher";
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@Publisher", $"%{publisherName}%");
                try
                {
                    con.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.HasRows) return $"Không tìm thấy sách nào của NXB '{publisherName}'.";
                        while (reader.Read())
                        {
                            result.AppendLine($"- {reader["TenSach"]}<br/>");
                        }
                    }
                }
                catch { return "Lỗi khi tìm sách theo NXB."; }
            }
            return result.ToString();
        }

        private string FindBooksByTeam(string teamName)
        {
            var result = new StringBuilder($"Các sách của nhóm dịch '{teamName}':<br/>");
            string query = "SELECT TOP 10 TenSach FROM Sach WHERE NhomDich LIKE @Team";
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@Team", $"%{teamName}%");
                try
                {
                    con.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.HasRows) return $"Không tìm thấy sách nào của nhóm dịch '{teamName}'.";
                        while (reader.Read())
                        {
                            result.AppendLine($"- {reader["TenSach"]}<br/>");
                        }
                    }
                }
                catch { return "Lỗi khi tìm sách theo nhóm dịch."; }
            }
            return result.ToString();
        }

        private string CountBooksByContentStatus()
        {
            var result = new StringBuilder("Thống kê sách theo trạng thái nội dung:<br/>");
            string query = "SELECT TrangThaiNoiDung, COUNT(*) as SoLuong FROM Sach WHERE TrangThaiNoiDung IS NOT NULL GROUP BY TrangThaiNoiDung";
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(query, con))
            {
                try
                {
                    con.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.HasRows) return "Chưa có dữ liệu về trạng thái nội dung sách.";
                        while (reader.Read())
                        {
                            result.AppendLine($"- <b>{reader["TrangThaiNoiDung"]}:</b> {reader["SoLuong"]} sách<br/>");
                        }
                    }
                }
                catch { return "Không thể truy vấn thống kê trạng thái sách."; }
            }
            return result.ToString();
        }

        // VI. PHÂN TÍCH NGƯỜI DÙNG NÂNG CAO (MỚI)
        private string GetTopSpendingUsers(int limit)
        {
            var result = new StringBuilder($"Top {limit} người dùng chi tiêu nhiều nhất:<br/>");
            string query = $@"SELECT TOP {limit} ISNULL(u.Ten, u.Username) as TenHienThi, SUM(d.SoTien) as TongChiTieu
                             FROM DonHang d
                             JOIN NguoiDung u ON d.IDNguoiDung = u.IDNguoiDung
                             WHERE d.TrangThaiThanhToan = 'Completed'
                             GROUP BY ISNULL(u.Ten, u.Username)
                             ORDER BY TongChiTieu DESC";
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(query, con))
            {
                try
                {
                    con.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.HasRows) return "Chưa có dữ liệu chi tiêu của người dùng.";
                        while (reader.Read())
                        {
                            result.AppendLine($"- <b>{reader["TenHienThi"]}</b>: {Convert.ToDecimal(reader["TongChiTieu"]):N0} VNĐ<br/>");
                        }
                    }
                }
                catch { return "Lỗi khi truy vấn người dùng chi tiêu nhiều nhất."; }
            }
            return result.ToString();
        }

        private string GetTopReviewers(int limit)
        {
            var result = new StringBuilder($"Top {limit} người dùng đánh giá nhiều nhất:<br/>");
            string query = $@"SELECT TOP {limit} ISNULL(u.Ten, u.Username) as TenHienThi, COUNT(dg.IDDanhGia) as SoLuongDanhGia
                              FROM DanhGiaSach dg
                              JOIN NguoiDung u ON dg.IDNguoiDung = u.IDNguoiDung
                              GROUP BY ISNULL(u.Ten, u.Username)
                              ORDER BY SoLuongDanhGia DESC";
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(query, con))
            {
                try
                {
                    con.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.HasRows) return "Chưa có ai gửi đánh giá.";
                        while (reader.Read())
                        {
                            result.AppendLine($"- <b>{reader["TenHienThi"]}</b>: {reader["SoLuongDanhGia"]} lượt đánh giá<br/>");
                        }
                    }
                }
                catch { return "Lỗi khi truy vấn người dùng đánh giá nhiều nhất."; }
            }
            return result.ToString();
        }

        private string GetUsersNoPurchase()
        {
            var result = new StringBuilder("Danh sách 5 người dùng chưa mua hàng:<br/>");
            string query = @"SELECT TOP 5 u.Username, u.Email
                             FROM NguoiDung u
                             LEFT JOIN DonHang d ON u.IDNguoiDung = d.IDNguoiDung
                             WHERE d.IDDonHang IS NULL";
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(query, con))
            {
                try
                {
                    con.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.HasRows) return "Tất cả người dùng đều đã có ít nhất một đơn hàng.";
                        while (reader.Read())
                        {
                            result.AppendLine($"- <b>{reader["Username"]}</b> (Email: {reader["Email"]})<br/>");
                        }
                    }
                }
                catch { return "Lỗi khi truy vấn người dùng chưa mua hàng."; }
            }
            return result.ToString();
        }

        // VII. BẢO TRÌ & HÀNH ĐỘNG (MỚI)
        private string DeleteReviewsForBook(string bookName)
        {
            if (string.IsNullOrWhiteSpace(bookName)) return "Vui lòng cung cấp tên sách.";
            string findBookQuery = "SELECT IDSach FROM Sach WHERE TenSach LIKE @TenSach";
            int bookId = 0;
            using (var con = new SqlConnection(connectionString))
            using (var cmdFind = new SqlCommand(findBookQuery, con))
            {
                cmdFind.Parameters.AddWithValue("@TenSach", $"%{bookName}%");
                try
                {
                    con.Open();
                    var result = cmdFind.ExecuteScalar();
                    if (result == null || result == DBNull.Value) return $"Không tìm thấy sách nào tên '{bookName}'.";
                    bookId = Convert.ToInt32(result);
                }
                catch { return "Lỗi khi tìm ID sách."; }
            }

            string deleteQuery = "DELETE FROM DanhGiaSach WHERE IDSach = @IDSach";
            using (var con = new SqlConnection(connectionString))
            using (var cmdDelete = new SqlCommand(deleteQuery, con))
            {
                cmdDelete.Parameters.AddWithValue("@IDSach", bookId);
                try
                {
                    con.Open();
                    int rowsAffected = cmdDelete.ExecuteNonQuery();
                    return $"Đã xóa <b>{rowsAffected}</b> đánh giá của sách '{bookName}'.";
                }
                catch { return $"Lỗi khi xóa đánh giá của sách '{bookName}'."; }
            }
        }

        private string DeleteReviewsByUser(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return "Vui lòng cung cấp email.";
            string findUserQuery = "SELECT IDNguoiDung FROM NguoiDung WHERE Email = @Email";
            int userId = 0;
            using (var con = new SqlConnection(connectionString))
            using (var cmdFind = new SqlCommand(findUserQuery, con))
            {
                cmdFind.Parameters.AddWithValue("@Email", email);
                try
                {
                    con.Open();
                    var result = cmdFind.ExecuteScalar();
                    if (result == null || result == DBNull.Value) return $"Không tìm thấy người dùng có email '{email}'.";
                    userId = Convert.ToInt32(result);
                }
                catch { return "Lỗi khi tìm ID người dùng."; }
            }

            string deleteQuery = "DELETE FROM DanhGiaSach WHERE IDNguoiDung = @IDNguoiDung";
            using (var con = new SqlConnection(connectionString))
            using (var cmdDelete = new SqlCommand(deleteQuery, con))
            {
                cmdDelete.Parameters.AddWithValue("@IDNguoiDung", userId);
                try
                {
                    con.Open();
                    int rowsAffected = cmdDelete.ExecuteNonQuery();
                    return $"Đã xóa <b>{rowsAffected}</b> đánh giá của người dùng '{email}'.";
                }
                catch { return $"Lỗi khi xóa đánh giá của người dùng '{email}'."; }
            }
        }

        private string ClearUserCart(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return "Vui lòng cung cấp email.";
            string findUserQuery = "SELECT IDNguoiDung FROM NguoiDung WHERE Email = @Email";
            int userId = 0;
            using (var con = new SqlConnection(connectionString))
            using (var cmdFind = new SqlCommand(findUserQuery, con))
            {
                cmdFind.Parameters.AddWithValue("@Email", email);
                try
                {
                    con.Open();
                    var result = cmdFind.ExecuteScalar();
                    if (result == null || result == DBNull.Value) return $"Không tìm thấy người dùng có email '{email}'.";
                    userId = Convert.ToInt32(result);
                }
                catch { return "Lỗi khi tìm ID người dùng."; }
            }

            string deleteQuery = "DELETE FROM GioHang WHERE IDNguoiDung = @IDNguoiDung";
            using (var con = new SqlConnection(connectionString))
            using (var cmdDelete = new SqlCommand(deleteQuery, con))
            {
                cmdDelete.Parameters.AddWithValue("@IDNguoiDung", userId);
                try
                {
                    con.Open();
                    int rowsAffected = cmdDelete.ExecuteNonQuery();
                    return $"Đã dọn dẹp giỏ hàng ({rowsAffected} mục) của người dùng '{email}'.";
                }
                catch { return $"Lỗi khi dọn dẹp giỏ hàng của người dùng '{email}'."; }
            }
        }

        #endregion

        // Hàm helper để chạy các câu lệnh trả về một giá trị duy nhất
        private object ExecuteScalar(string query)
        {
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(query, con))
            {
                try
                {
                    con.Open();
                    return cmd.ExecuteScalar();
                }
                catch { return null; }
            }
        }
    }
}
