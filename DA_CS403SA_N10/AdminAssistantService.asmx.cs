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

            string systemPrompt = @"Bạn là Trợ lý Quản trị cho trang web bán sách Webebook. Bạn thông minh, chuyên nghiệp và chỉ cung cấp thông tin quản trị.
                Bạn có các khả năng sau và chỉ được trả về MỘT LỆNH DUY NHẤT:

                **I. THỐNG KÊ TỔNG QUAN:**
                1.  Tổng số sách đang có: [COMMAND:GET_TOTAL_BOOK_COUNT]
                2.  Tổng số người dùng đã đăng ký: [COMMAND:GET_TOTAL_USER_COUNT]
                3.  Điểm đánh giá trung bình của tất cả sách: [COMMAND:GET_AVERAGE_RATING]
                4.  Phân bố các điểm đánh giá (1 đến 5 sao): [COMMAND:GET_RATING_DISTRIBUTION]
                
                **II. PHÂN TÍCH DOANH THU & BÁN HÀNG:**
                5.  Doanh thu theo khoảng thời gian: [COMMAND:GET_REVENUE:period] (period có thể là: TODAY, THIS_MONTH, THIS_QUARTER, THIS_YEAR)
                6.  Liệt kê 5 sách bán chạy nhất: [COMMAND:GET_TOP_SELLING_BOOKS]
                7.  Liệt kê 5 sách bán chậm nhất: [COMMAND:GET_LEAST_SELLING_BOOKS]

                **III. QUẢN LÝ ĐƠN HÀNG:**
                8.  Liệt kê các đơn hàng theo trạng thái: [COMMAND:GET_ORDERS_BY_STATUS:status] (ví dụ status: Pending, Completed, Cancelled)
                9.  Kiểm tra trạng thái của một đơn hàng cụ thể: [COMMAND:GET_ORDER_STATUS:order_id]
                10. Cập nhật trạng thái một đơn hàng: [COMMAND:UPDATE_ORDER_STATUS:order_id,new_status] (VÍ DỤ: [COMMAND:UPDATE_ORDER_STATUS:123,Completed])

                **IV. QUẢN LÝ NGƯỜI DÙNG:**
                11. Tìm thông tin người dùng qua email: [COMMAND:FIND_USER_BY_EMAIL:email]
                12. Khóa một người dùng: [COMMAND:UPDATE_USER_STATUS:email,Banned]
                13. Mở khóa một người dùng: [COMMAND:UPDATE_USER_STATUS:email,Active]
                (Lưu ý: Không hỗ trợ thống kê người dùng mới do hạn chế CSDL)

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

            switch (commandType)
            {
                case "GET_TOTAL_BOOK_COUNT": return new ChatResponse { Text = GetTotalBookCount() };
                case "GET_TOTAL_USER_COUNT": return new ChatResponse { Text = GetTotalUserCount() };
                case "GET_AVERAGE_RATING": return new ChatResponse { Text = GetAverageRating() };
                case "GET_RATING_DISTRIBUTION": return new ChatResponse { Text = GetRatingDistribution() };
                case "GET_REVENUE": return new ChatResponse { Text = GetRevenueByPeriod(argument) };
                case "GET_TOP_SELLING_BOOKS": return new ChatResponse { Text = GetTopSellingBooks(true) };
                case "GET_LEAST_SELLING_BOOKS": return new ChatResponse { Text = GetTopSellingBooks(false) };
                case "GET_ORDERS_BY_STATUS": return new ChatResponse { Text = GetOrdersByStatus(argument) };
                case "FIND_USER_BY_EMAIL": return new ChatResponse { Text = FindUserByEmail(argument) };
                case "GET_ORDER_STATUS":
                    if (int.TryParse(argument, out int orderId))
                        return new ChatResponse { Text = GetOrderStatus(orderId) };
                    return new ChatResponse { Text = "Mã đơn hàng không hợp lệ." };

                case "UPDATE_ORDER_STATUS":
                    if (args.Length == 2 && int.TryParse(args[0].Trim(), out int orderIdToUpdate) && !string.IsNullOrWhiteSpace(args[1]))
                    {
                        return new ChatResponse { Text = UpdateOrderStatus(orderIdToUpdate, args[1].Trim()) };
                    }
                    return new ChatResponse { Text = "Cú pháp lệnh không đúng. Cần: [COMMAND:UPDATE_ORDER_STATUS:mã_đơn_hàng,trạng_thái_mới]" };

                case "UPDATE_USER_STATUS":
                    if (args.Length == 2 && !string.IsNullOrWhiteSpace(args[0]) && !string.IsNullOrWhiteSpace(args[1]))
                    {
                        return new ChatResponse { Text = UpdateUserStatus(args[0].Trim(), args[1].Trim()) };
                    }
                    return new ChatResponse { Text = "Cú pháp lệnh không đúng. Cần: [COMMAND:UPDATE_USER_STATUS:email,trạng_thái_mới]" };

                default:
                    return new ChatResponse { Text = "Lệnh từ AI không hợp lệ." };
            }
        }

        #region Các Hàm Truy Vấn CSDL Cho Admin
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

        private string GetRevenueByPeriod(string period)
        {
            string whereClause;
            string timeDescription;
            switch (period.Trim().ToUpper())
            {
                case "TODAY":
                    whereClause = "WHERE CONVERT(date, NgayDat) = CONVERT(date, GETDATE())";
                    timeDescription = "hôm nay";
                    break;
                case "THIS_MONTH":
                    whereClause = "WHERE MONTH(NgayDat) = MONTH(GETDATE()) AND YEAR(NgayDat) = YEAR(GETDATE())";
                    timeDescription = "tháng này";
                    break;
                case "THIS_QUARTER":
                    whereClause = "WHERE DATEPART(quarter, NgayDat) = DATEPART(quarter, GETDATE()) AND YEAR(NgayDat) = YEAR(GETDATE())";
                    timeDescription = "quý này";
                    break;
                case "THIS_YEAR":
                    whereClause = "WHERE YEAR(NgayDat) = YEAR(GETDATE())";
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

        private string GetTopSellingBooks(bool isTop)
        {
            var result = new StringBuilder(isTop ? "Top 5 sách bán chạy nhất:<br/>" : "Top 5 sách bán chậm nhất (có ít nhất 1 lượt mua):<br/>");
            string orderDirection = isTop ? "DESC" : "ASC";
            string query = $@"SELECT TOP 5 s.TenSach, SUM(ct.SoLuong) AS TongSoLuongBan 
                             FROM ChiTietDonHang ct JOIN Sach s ON ct.IDSach = s.IDSach 
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

            // Chỉ xử lý đặc biệt khi trạng thái là "Completed"
            if (!newStatus.Equals("Completed", StringComparison.OrdinalIgnoreCase))
            {
                // Nếu không phải "Completed", chỉ cập nhật trạng thái như cũ
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

            // --- Logic mới khi trạng thái là "Completed" ---
            using (var con = new SqlConnection(connectionString))
            {
                con.Open();
                // Bắt đầu một Transaction để đảm bảo an toàn dữ liệu
                using (var transaction = con.BeginTransaction())
                {
                    try
                    {
                        // Bước 1: Lấy IDNguoiDung và danh sách IDSach từ đơn hàng
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

                        // Bước 2: Cập nhật trạng thái đơn hàng thành "Completed"
                        string updateStatusQuery = "UPDATE DonHang SET TrangThaiThanhToan = @NewStatus WHERE IDDonHang = @OrderId";
                        using (var cmd = new SqlCommand(updateStatusQuery, con, transaction))
                        {
                            cmd.Parameters.AddWithValue("@NewStatus", newStatus);
                            cmd.Parameters.AddWithValue("@OrderId", orderId);
                            cmd.ExecuteNonQuery();
                        }

                        // Bước 3: Thêm từng sách vào Tủ Sách (TuSach), chỉ thêm nếu chưa có
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

                        // Nếu mọi thứ thành công, xác nhận transaction
                        transaction.Commit();
                        return $"Đã cập nhật đơn hàng #{orderId} thành <b>{newStatus}</b> và đã thêm <b>{booksAddedCount} / {sachIds.Count}</b> sách mới vào tủ sách của người dùng.";
                    }
                    catch (Exception ex)
                    {
                        // Nếu có bất kỳ lỗi nào, hoàn tác tất cả các thay đổi
                        transaction.Rollback();
                        System.Diagnostics.Debug.WriteLine($"Lỗi khi hoàn thành đơn hàng #{orderId}: {ex.Message}");
                        return $"Cập nhật đơn hàng thất bại: {ex.Message}";
                    }
                }
            }
        }

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

        #endregion
    }
}