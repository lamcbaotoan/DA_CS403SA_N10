using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Script.Services;
using System.Web.Services;

namespace Webebook
{
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    [ScriptService]
    public class ChatbotService : System.Web.Services.WebService
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["datawebebookConnectionString"].ConnectionString;
        private static readonly HttpClient client = new HttpClient();

        static ChatbotService()
        {
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public ChatResponse GetChatbotResponse(string userMessage, int userId)
        {
            if (userId <= 0)
            {
                return new ChatResponse { Text = "Phiên làm việc đã hết hạn. Vui lòng tải lại trang." };
            }
            return Task.Run(() => GetGeminiResponseAsync(userMessage, userId)).Result;
        }

        private async Task<ChatResponse> GetGeminiResponseAsync(string userMessage, int userId)
        {
            string apiKey = ConfigurationManager.AppSettings["GeminiApiKey"];
            if (string.IsNullOrEmpty(apiKey) || apiKey.Contains("API_KEY_CUA_BAN"))
            {
                return new ChatResponse { Text = "Lỗi cấu hình: API Key của Gemini chưa được thiết lập." };
            }

            var apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={apiKey}";

            string systemPrompt = @"Bạn là trợ lý ảo của Webebook, một trang web bán sách. Nhiệm vụ của bạn là hỗ trợ người dùng đã đăng nhập.
                Bạn có các khả năng đặc biệt sau, và chỉ được trả về MỘT LỆNH DUY NHẤT:

                **A. VỀ SÁCH (CHUNG):**
                1.  Tìm kiếm sách theo tên: [COMMAND:SEARCH_BOOK:tên sách]
                2.  Báo giá sách: [COMMAND:GET_PRICE:tên sách]
                3.  Tìm sách theo thể loại: [COMMAND:FILTER_BY_GENRE:tên thể loại]
                4.  Tìm sách theo tác giả: [COMMAND:FIND_BOOKS_BY_AUTHOR:tên tác giả]
                5.  Tìm sách rẻ nhất: [COMMAND:GET_CHEAPEST_BOOK]
                6.  Tìm sách đắt nhất: [COMMAND:GET_MOST_EXPENSIVE_BOOK]
                7.  Gợi ý sách ngẫu nhiên: [COMMAND:GET_RANDOM_BOOK]
                8.  Liệt kê sách mới nhất: [COMMAND:GET_NEWEST_BOOKS:số lượng] (Mặc định là 5)
                9.  Liệt kê sách được đánh giá cao nhất: [COMMAND:GET_TOP_RATED_BOOKS:số lượng] (Mặc định là 5)

                **B. ĐƠN HÀNG & GIỎ HÀNG:**
                10. Xem giỏ hàng: [COMMAND:VIEW_CART]
                11. Thêm sách vào giỏ hàng: [COMMAND:ADD_TO_CART:tên sách]
                12. Xóa sách khỏi giỏ hàng: [COMMAND:REMOVE_FROM_CART:tên sách]
                13. Dọn dẹp toàn bộ giỏ hàng: [COMMAND:CLEAR_CART]
                14. Xem lịch sử mua hàng: [COMMAND:VIEW_PURCHASE_HISTORY]
                15. Kiểm tra trạng thái đơn hàng gần nhất: [COMMAND:CHECK_LAST_ORDER_STATUS]
                16. Tìm một đơn hàng cụ thể: [COMMAND:FIND_ORDER_BY_ID:mã đơn hàng]

                **C. ĐỌC SÁCH & TỦ SÁCH:**
                17. Xem tủ sách (sách đã mua): [COMMAND:VIEW_BOOKSHELF]
                18. Kiểm tra tiến độ đọc sách: [COMMAND:CHECK_READING_PROGRESS:tên sách]
                19. Cập nhật tiến độ đọc: [COMMAND:UPDATE_READING_PROGRESS:tên sách,số chương]
                20. Lấy chương đầu tiên của sách: [COMMAND:GET_FIRST_CHAPTER:tên sách]
                21. Lấy chương mới nhất của sách: [COMMAND:GET_LATEST_CHAPTER:tên sách]

                **D. ĐÁNH GIÁ & TƯƠNG TÁC:**
                22. Hướng dẫn viết đánh giá cho một sách: [COMMAND:GUIDE_REVIEW:tên sách]
                23. Xem lại các đánh giá của tôi: [COMMAND:VIEW_MY_REVIEWS]

                QUAN TRỌNG:
                - Luôn trích xuất từ khóa chính xác từ câu hỏi của người dùng.
                - Với các câu hỏi trò chuyện khác, hãy trả lời tự nhiên, thân thiện và hữu ích.";

            var fullPrompt = $"{systemPrompt}\n\nCâu hỏi của người dùng: {userMessage}";
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
                    if (aiResponse.StartsWith("[COMMAND:"))
                    {
                        return ProcessCommand(aiResponse, userId);
                    }
                    return new ChatResponse { Text = aiResponse };
                }
                return new ChatResponse { Text = $"Lỗi khi kết nối đến dịch vụ AI: {geminiResponse?.error?.message ?? jsonResponse}" };
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi hệ thống khi gọi API Gemini: {ex.Message}");
                return new ChatResponse { Text = "Rất xin lỗi, tôi đang gặp một chút sự cố kỹ thuật." };
            }
        }

        private ChatResponse ProcessCommand(string command, int userId)
        {
            command = command.Trim('[', ']');
            var parts = command.Split(new[] { ':' }, 3);
            string commandType = parts.Length > 1 ? parts[1].Trim().ToUpper() : string.Empty;
            string argument = parts.Length > 2 ? parts[2].Trim() : string.Empty;

            switch (commandType)
            {
                // A. SÁCH (CHUNG)
                case "SEARCH_BOOK": return new ChatResponse { Text = SearchBookInDatabase(argument) };
                case "GET_PRICE": return new ChatResponse { Text = GetBookPrice(argument) };
                case "GET_CHEAPEST_BOOK": return new ChatResponse { Text = GetCheapestBook() };
                case "GET_MOST_EXPENSIVE_BOOK": return new ChatResponse { Text = GetMostExpensiveBook() };
                case "FILTER_BY_GENRE": return new ChatResponse { Text = FilterByGenre(argument) };
                case "GET_RANDOM_BOOK": return new ChatResponse { Text = GetRandomBook() };
                case "FIND_BOOKS_BY_AUTHOR": return new ChatResponse { Text = FindBooksByAuthor(argument) }; // Mới
                case "GET_NEWEST_BOOKS": return new ChatResponse { Text = GetNewestBooks(argument) }; // Mới
                case "GET_TOP_RATED_BOOKS": return new ChatResponse { Text = GetTopRatedBooks(argument) }; // Mới

                // B. ĐƠN HÀNG & GIỎ HÀNG
                case "VIEW_CART": return ViewCart(userId);
                case "ADD_TO_CART": return new ChatResponse { Text = AddToCart(userId, argument) }; // Mới
                case "REMOVE_FROM_CART": return new ChatResponse { Text = RemoveFromCart(userId, argument) }; // Mới
                case "CLEAR_CART": return new ChatResponse { Text = ClearCart(userId) }; // Mới
                case "VIEW_PURCHASE_HISTORY": return ViewPurchaseHistory(userId);
                case "CHECK_LAST_ORDER_STATUS": return new ChatResponse { Text = CheckLastOrderStatus(userId) };
                case "FIND_ORDER_BY_ID": return new ChatResponse { Text = FindOrderById(userId, argument) }; // Mới

                // C. ĐỌC SÁCH & TỦ SÁCH
                case "VIEW_BOOKSHELF": return ViewBookshelf(userId);
                case "CHECK_READING_PROGRESS": return new ChatResponse { Text = CheckReadingProgress(userId, argument) };
                case "UPDATE_READING_PROGRESS": return new ChatResponse { Text = UpdateReadingProgress(userId, argument) }; // Mới
                case "GET_FIRST_CHAPTER": return GetChapterLink(userId, argument, "first"); // Mới
                case "GET_LATEST_CHAPTER": return GetChapterLink(userId, argument, "latest"); // Mới

                // D. ĐÁNH GIÁ & TƯƠNG TÁC
                case "GUIDE_REVIEW": return GuideReview(userId, argument); // Mới
                case "VIEW_MY_REVIEWS": return ViewMyReviews(userId); // Mới

                default:
                    System.Diagnostics.Debug.WriteLine($"Lệnh không nhận dạng được: '{command}'");
                    return new ChatResponse { Text = "Lệnh từ AI không hợp lệ." };
            }
        }

        #region (A) Các Hàm Truy Vấn Về Sách (Chung)

        private string SearchBookInDatabase(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm)) return "Tôi cần tên sách để tìm kiếm ạ.";
            var booksFound = new StringBuilder();
            string query = "SELECT TOP 3 TenSach, TacGia FROM Sach WHERE TenSach LIKE @SearchTerm OR TacGia LIKE @SearchTerm";
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@SearchTerm", "%" + searchTerm + "%");
                try
                {
                    con.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.HasRows) return $"Rất tiếc, tôi không tìm thấy sách nào khớp với '{searchTerm}'.";
                        booksFound.AppendLine($"Tôi tìm thấy một vài kết quả cho '{searchTerm}':<br/>");
                        while (reader.Read())
                        {
                            booksFound.AppendLine($"- <b>{reader["TenSach"]}</b> của tác giả {reader["TacGia"]}<br/>");
                        }
                    }
                }
                catch { return "Hệ thống CSDL đang gặp sự cố, không thể tìm sách lúc này."; }
            }
            return booksFound.ToString();
        }

        private string GetBookPrice(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm)) return "Bạn muốn hỏi giá của sách nào ạ?";
            string query = "SELECT TOP 1 TenSach, GiaSach FROM Sach WHERE TenSach LIKE @SearchTerm";
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@SearchTerm", "%" + searchTerm + "%");
                try
                {
                    con.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string tenSach = reader["TenSach"].ToString();
                            decimal giaSach = Convert.ToDecimal(reader["GiaSach"]);
                            return $"Sách '<b>{tenSach}</b>' có giá là {giaSach:N0} VNĐ.";
                        }
                        return $"Tôi không tìm thấy sách nào tên là '{searchTerm}' để báo giá.";
                    }
                }
                catch { return "Hệ thống CSDL đang gặp sự cố, không thể lấy giá sách lúc này."; }
            }
        }

        private string GetCheapestBook()
        {
            string query = "SELECT TOP 1 TenSach, GiaSach FROM Sach WHERE GiaSach > 0 ORDER BY GiaSach ASC";
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
                            string tenSach = reader["TenSach"].ToString();
                            decimal giaSach = Convert.ToDecimal(reader["GiaSach"]);
                            return $"Sách có giá rẻ nhất hiện tại là '<b>{tenSach}</b>' với giá {giaSach:N0} VNĐ.";
                        }
                        return "Xin lỗi, hiện không có thông tin về giá sách để so sánh.";
                    }
                }
                catch { return "Hệ thống CSDL đang gặp sự cố."; }
            }
        }

        private string GetMostExpensiveBook()
        {
            string query = "SELECT TOP 1 TenSach, GiaSach FROM Sach ORDER BY GiaSach DESC";
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
                            string tenSach = reader["TenSach"].ToString();
                            decimal giaSach = Convert.ToDecimal(reader["GiaSach"]);
                            return $"Sách có giá cao nhất hiện tại là '<b>{tenSach}</b>' với giá {giaSach:N0} VNĐ.";
                        }
                        return "Xin lỗi, hiện không có thông tin về giá sách để so sánh.";
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Lỗi truy vấn sách đắt nhất: {ex.Message}");
                    return "Hệ thống CSDL đang gặp sự cố.";
                }
            }
        }

        private string FilterByGenre(string genre)
        {
            if (string.IsNullOrWhiteSpace(genre)) return "Bạn muốn tìm sách theo thể loại nào?";
            var result = new StringBuilder($"Các sách thuộc thể loại '{genre}':<br/>");
            string query = "SELECT TOP 5 TenSach, TacGia FROM Sach WHERE TheLoaiChuoi LIKE @Genre";
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@Genre", "%" + genre + "%");
                try
                {
                    con.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.HasRows) return $"Không tìm thấy sách nào thuộc thể loại '{genre}'.";
                        while (reader.Read())
                        {
                            result.AppendLine($"- <b>{reader["TenSach"]}</b> của tác giả {reader["TacGia"]}<br/>");
                        }
                    }
                }
                catch { return "Hệ thống CSDL đang gặp sự cố."; }
            }
            return result.ToString();
        }

        private string GetRandomBook()
        {
            string query = "SELECT TOP 1 TenSach, TacGia, MoTa FROM Sach ORDER BY NEWID()";
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
                            string tenSach = reader["TenSach"].ToString();
                            string tacGia = reader["TacGia"].ToString();
                            string moTa = reader["MoTa"]?.ToString();

                            var result = new StringBuilder();
                            result.AppendLine("Tất nhiên rồi! Hôm nay bạn thử đọc cuốn này xem sao nhé:<br/><br/>");
                            result.AppendLine($"<b>Sách:</b> {tenSach}<br/>");
                            result.AppendLine($"<b>Tác giả:</b> {tacGia}<br/><br/>");
                            if (!string.IsNullOrWhiteSpace(moTa))
                            {
                                result.AppendLine($"<i>{moTa.Substring(0, Math.Min(moTa.Length, 150))}...</i>");
                            }
                            return result.ToString();
                        }
                        return "Xin lỗi, kho sách hiện đang trống nên tôi không thể gợi ý cho bạn.";
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Lỗi truy vấn sách ngẫu nhiên: {ex.Message}");
                    return "Hệ thống CSDL đang gặp sự cố.";
                }
            }
        }

        private string FindBooksByAuthor(string authorName)
        {
            if (string.IsNullOrWhiteSpace(authorName)) return "Bạn muốn tìm sách của tác giả nào?";
            var result = new StringBuilder($"Các sách của tác giả '{authorName}':<br/>");
            string query = "SELECT TOP 5 TenSach FROM Sach WHERE TacGia LIKE @AuthorName";
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@AuthorName", "%" + authorName + "%");
                try
                {
                    con.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.HasRows) return $"Không tìm thấy sách nào của tác giả '{authorName}'.";
                        while (reader.Read())
                        {
                            result.AppendLine($"- <b>{reader["TenSach"]}</b><br/>");
                        }
                    }
                }
                catch { return "Hệ thống CSDL đang gặp sự cố khi tìm sách theo tác giả."; }
            }
            return result.ToString();
        }

        private string GetNewestBooks(string limitStr)
        {
            if (!int.TryParse(limitStr, out int limit) || limit <= 0)
            {
                limit = 5;
            }
            var result = new StringBuilder($"{limit} sách mới nhất trên Webebook:<br/>");
            string query = $"SELECT TOP {limit} TenSach, TacGia FROM Sach ORDER BY IDSach DESC";
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(query, con))
            {
                try
                {
                    con.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.HasRows) return "Chưa có sách nào trên hệ thống.";
                        while (reader.Read())
                        {
                            result.AppendLine($"- <b>{reader["TenSach"]}</b> của {reader["TacGia"]}<br/>");
                        }
                    }
                }
                catch { return "Hệ thống CSDL đang gặp sự cố."; }
            }
            return result.ToString();
        }

        private string GetTopRatedBooks(string limitStr)
        {
            if (!int.TryParse(limitStr, out int limit) || limit <= 0)
            {
                limit = 5;
            }
            var result = new StringBuilder($"{limit} sách được đánh giá cao nhất:<br/>");
            string query = $@"SELECT TOP {limit} s.TenSach, AVG(CAST(dg.Diem AS FLOAT)) as AvgRating
                              FROM DanhGiaSach dg
                              JOIN Sach s ON dg.IDSach = s.IDSach
                              GROUP BY s.TenSach
                              ORDER BY AvgRating DESC";
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(query, con))
            {
                try
                {
                    con.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.HasRows) return "Chưa có sách nào được đánh giá.";
                        while (reader.Read())
                        {
                            result.AppendLine($"- <b>{reader["TenSach"]}</b> (Điểm: {Convert.ToDouble(reader["AvgRating"]):F1}/5)<br/>");
                        }
                    }
                }
                catch { return "Hệ thống CSDL đang gặp sự cố."; }
            }
            return result.ToString();
        }

        #endregion

        #region (B) Các Hàm Về Đơn Hàng & Giỏ Hàng

        private ChatResponse ViewCart(int userId)
        {
            var result = new StringBuilder("Trong giỏ hàng của bạn hiện có:<br/>");
            string query = @"SELECT s.TenSach, gh.SoLuong FROM GioHang gh JOIN Sach s ON gh.IDSach = s.IDSach WHERE gh.IDNguoiDung = @UserId";
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@UserId", userId);
                try
                {
                    con.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.HasRows)
                        {
                            return new ChatResponse { Text = "Giỏ hàng của bạn đang trống." };
                        }
                        while (reader.Read())
                        {
                            result.AppendLine($"- <b>{reader["TenSach"]}</b> (Số lượng: {reader["SoLuong"]})<br/>");
                        }
                    }
                }
                catch { result.Clear().Append("Hệ thống CSDL đang gặp sự cố."); }
            }
            return new ChatResponse { Text = result.ToString(), ButtonText = "Đi đến Giỏ Hàng", ButtonUrl = "/WebForm/User/giohang_user.aspx" };
        }

        private string AddToCart(int userId, string bookName)
        {
            if (string.IsNullOrWhiteSpace(bookName)) return "Bạn muốn thêm sách nào vào giỏ?";

            int bookId = 0;
            using (var con = new SqlConnection(connectionString))
            {
                // Tìm ID của sách
                string findQuery = "SELECT TOP 1 IDSach FROM Sach WHERE TenSach LIKE @BookName";
                using (var findCmd = new SqlCommand(findQuery, con))
                {
                    findCmd.Parameters.AddWithValue("@BookName", $"%{bookName}%");
                    try
                    {
                        con.Open();
                        var result = findCmd.ExecuteScalar();
                        if (result == null || result == DBNull.Value)
                        {
                            return $"Không tìm thấy sách nào có tên giống '{bookName}'.";
                        }
                        bookId = Convert.ToInt32(result);
                    }
                    catch { return "Lỗi khi tìm sách."; }
                }

                // Kiểm tra sách đã có trong giỏ chưa
                string checkQuery = "SELECT SoLuong FROM GioHang WHERE IDNguoiDung = @UserId AND IDSach = @BookId";
                using (var checkCmd = new SqlCommand(checkQuery, con))
                {
                    checkCmd.Parameters.AddWithValue("@UserId", userId);
                    checkCmd.Parameters.AddWithValue("@BookId", bookId);
                    var currentQuantity = checkCmd.ExecuteScalar();
                    if (currentQuantity != null && currentQuantity != DBNull.Value)
                    {
                        return $"Sách '{bookName}' đã có trong giỏ hàng rồi.";
                    }
                }

                // Thêm sách vào giỏ
                string insertQuery = "INSERT INTO GioHang (IDNguoiDung, IDSach, SoLuong) VALUES (@UserId, @BookId, 1)";
                using (var insertCmd = new SqlCommand(insertQuery, con))
                {
                    insertCmd.Parameters.AddWithValue("@UserId", userId);
                    insertCmd.Parameters.AddWithValue("@BookId", bookId);
                    int rowsAffected = insertCmd.ExecuteNonQuery();
                    return rowsAffected > 0 ? $"Đã thêm sách '{bookName}' vào giỏ hàng thành công!" : "Không thể thêm sách vào giỏ hàng lúc này.";
                }
            }
        }

        private string RemoveFromCart(int userId, string bookName)
        {
            if (string.IsNullOrWhiteSpace(bookName)) return "Bạn muốn xóa sách nào khỏi giỏ hàng?";
            // Logic tương tự AddToCart nhưng là lệnh DELETE
            // Do phức tạp trong việc xác định chính xác sách, chức năng này có thể cần cải tiến thêm
            return "Chức năng xóa sách khỏi giỏ hàng qua chat đang được phát triển.";
        }

        private string ClearCart(int userId)
        {
            string query = "DELETE FROM GioHang WHERE IDNguoiDung = @UserId";
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@UserId", userId);
                try
                {
                    con.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected > 0 ? "Đã dọn dẹp giỏ hàng của bạn." : "Giỏ hàng của bạn vốn đã trống rồi.";
                }
                catch { return "Lỗi khi dọn dẹp giỏ hàng."; }
            }
        }

        private ChatResponse ViewPurchaseHistory(int userId)
        {
            var result = new StringBuilder("5 đơn hàng gần nhất của bạn:<br/>");
            string query = @"SELECT TOP 5 IDDonHang, NgayDat, SoTien, TrangThaiThanhToan FROM DonHang WHERE IDNguoiDung = @UserId ORDER BY NgayDat DESC";
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@UserId", userId);
                try
                {
                    con.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.HasRows)
                        {
                            return new ChatResponse { Text = "Bạn chưa có đơn hàng nào." };
                        }
                        while (reader.Read())
                        {
                            result.AppendLine($"- Đơn #{reader["IDDonHang"]} ngày {((DateTime)reader["NgayDat"]):dd/MM/yyyy}, tổng tiền {((decimal)reader["SoTien"]):N0}đ, TT: {reader["TrangThaiThanhToan"]}<br/>");
                        }
                    }
                }
                catch { result.Clear().Append("Hệ thống CSDL đang gặp sự cố."); }
            }
            return new ChatResponse { Text = result.ToString(), ButtonText = "Xem Toàn Bộ Lịch Sử", ButtonUrl = "/WebForm/User/lichsumuahang.aspx" };
        }

        private string CheckLastOrderStatus(int userId)
        {
            string query = @"SELECT TOP 1 TrangThaiThanhToan FROM DonHang WHERE IDNguoiDung = @UserId ORDER BY NgayDat DESC";
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@UserId", userId);
                try
                {
                    con.Open();
                    var status = cmd.ExecuteScalar();
                    if (status == null || status == DBNull.Value) return "Bạn chưa có đơn hàng nào để kiểm tra.";
                    return $"Trạng thái đơn hàng gần nhất của bạn là: <b>{status}</b>.";
                }
                catch { return "Hệ thống CSDL đang gặp sự cố."; }
            }
        }

        private string FindOrderById(int userId, string orderIdStr)
        {
            if (!int.TryParse(orderIdStr, out int orderId))
            {
                return "Mã đơn hàng không hợp lệ. Vui lòng nhập một số.";
            }

            string query = "SELECT NgayDat, SoTien, TrangThaiThanhToan FROM DonHang WHERE IDDonHang = @OrderId AND IDNguoiDung = @UserId";
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@OrderId", orderId);
                cmd.Parameters.AddWithValue("@UserId", userId);
                try
                {
                    con.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return $"Thông tin đơn hàng #{orderId}:<br/>- Ngày đặt: {((DateTime)reader["NgayDat"]):dd/MM/yyyy}<br/>- Tổng tiền: {((decimal)reader["SoTien"]):N0} VNĐ<br/>- Trạng thái: <b>{reader["TrangThaiThanhToan"]}</b>";
                        }
                        return $"Không tìm thấy đơn hàng nào của bạn có mã là #{orderId}.";
                    }
                }
                catch { return "Lỗi khi tìm kiếm đơn hàng."; }
            }
        }

        #endregion

        #region (C) Các Hàm Về Đọc Sách & Tủ Sách

        private ChatResponse ViewBookshelf(int userId)
        {
            var result = new StringBuilder("Những cuốn sách bạn đã sở hữu:<br/>");
            string query = @"SELECT DISTINCT s.TenSach FROM TuSach ts JOIN Sach s ON ts.IDSach = s.IDSach WHERE ts.IDNguoiDung = @UserId";
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@UserId", userId);
                try
                {
                    con.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.HasRows)
                        {
                            return new ChatResponse { Text = "Tủ sách của bạn đang trống. Hãy mua sách để lấp đầy nó nhé!" };
                        }
                        while (reader.Read())
                        {
                            result.AppendLine($"- <b>{reader["TenSach"]}</b><br/>");
                        }
                    }
                }
                catch { result.Clear().Append("Hệ thống CSDL đang gặp sự cố."); }
            }
            return new ChatResponse { Text = result.ToString(), ButtonText = "Đi đến Tủ Sách", ButtonUrl = "/WebForm/User/tusach.aspx" };
        }

        private string CheckReadingProgress(int userId, string bookTitle)
        {
            if (string.IsNullOrWhiteSpace(bookTitle)) return "Bạn muốn kiểm tra tiến độ của sách nào vậy?";
            string query = @"SELECT ts.ViTriDoc FROM TuSach ts JOIN Sach s ON ts.IDSach = s.IDSach WHERE ts.IDNguoiDung = @UserId AND s.TenSach LIKE @BookTitle";
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@BookTitle", "%" + bookTitle + "%");
                try
                {
                    con.Open();
                    var readingPosition = cmd.ExecuteScalar();
                    if (readingPosition != null && readingPosition != DBNull.Value && !string.IsNullOrWhiteSpace(readingPosition.ToString()))
                    {
                        return $"Bạn đã đọc sách '<b>{bookTitle}</b>' đến: <b>{readingPosition}</b>.";
                    }
                    return $"Tôi không tìm thấy sách '<b>{bookTitle}</b>' trong tủ sách của bạn, hoặc bạn chưa bắt đầu đọc sách này.";
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Lỗi kiểm tra tiến độ đọc: {ex.Message}");
                    return "Hệ thống CSDL đang gặp sự cố khi kiểm tra tiến độ đọc sách.";
                }
            }
        }

        private string UpdateReadingProgress(int userId, string argument)
        {
            var parts = argument.Split(new[] { ',' }, 2);
            if (parts.Length < 2) return "Cú pháp không đúng. Cần: [COMMAND:UPDATE_READING_PROGRESS:tên sách,số chương]";

            string bookName = parts[0].Trim();
            string chapter = parts[1].Trim();

            string query = @"UPDATE TuSach SET ViTriDoc = @ViTriDoc
                             WHERE IDNguoiDung = @UserId AND IDSach = (SELECT IDSach FROM Sach WHERE TenSach LIKE @TenSach)";

            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@ViTriDoc", chapter);
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@TenSach", $"%{bookName}%");
                try
                {
                    con.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected > 0 ? $"Đã cập nhật tiến độ đọc cho sách '{bookName}' đến chương {chapter}." : $"Không thể cập nhật tiến độ. Sách '{bookName}' có thể không có trong tủ sách của bạn.";
                }
                catch { return "Lỗi khi cập nhật tiến độ đọc."; }
            }
        }

        private ChatResponse GetChapterLink(int userId, string bookName, string chapterType)
        {
            string order = chapterType == "first" ? "ASC" : "DESC";
            string query = $@"SELECT TOP 1 s.IDSach, nd.SoChuong
                              FROM NoiDungSach nd
                              JOIN Sach s ON nd.IDSach = s.IDSach
                              JOIN TuSach ts ON s.IDSach = ts.IDSach
                              WHERE ts.IDNguoiDung = @UserId AND s.TenSach LIKE @BookName
                              ORDER BY nd.SoChuong {order}";

            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@BookName", $"%{bookName}%");
                try
                {
                    con.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string sachId = reader["IDSach"].ToString();
                            string soChuong = reader["SoChuong"].ToString();
                            return new ChatResponse
                            {
                                Text = $"Đây là chương {(chapterType == "first" ? "đầu tiên" : "mới nhất")} của sách '{bookName}'.",
                                ButtonText = "Đọc Ngay",
                                ButtonUrl = $"/WebForm/User/docsach.aspx?IDSach={sachId}&SoChuong={soChuong}"
                            };
                        }
                    }
                }
                catch { }
            }
            return new ChatResponse { Text = $"Không thể tìm thấy chương cho sách '{bookName}' trong tủ sách của bạn." };
        }

        #endregion

        #region (D) Các Hàm Về Đánh Giá & Tương Tác

        private ChatResponse GuideReview(int userId, string bookName)
        {
            if (string.IsNullOrWhiteSpace(bookName)) return new ChatResponse { Text = "Bạn muốn viết đánh giá cho sách nào?" };

            string query = @"SELECT s.IDSach FROM Sach s JOIN TuSach ts ON s.IDSach = ts.IDSach
                             WHERE ts.IDNguoiDung = @UserId AND s.TenSach LIKE @BookName";
            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@UserId", userId);
                cmd.Parameters.AddWithValue("@BookName", $"%{bookName}%");
                try
                {
                    con.Open();
                    object result = cmd.ExecuteScalar();
                    if (result != null && result != DBNull.Value)
                    {
                        string sachId = result.ToString();
                        return new ChatResponse
                        {
                            Text = $"Tuyệt vời! Bạn có thể viết đánh giá cho sách '{bookName}' tại đây.",
                            ButtonText = "Viết Đánh Giá",
                            ButtonUrl = $"/WebForm/User/danhgia.aspx?IDSach={sachId}"
                        };
                    }
                }
                catch { }
            }
            return new ChatResponse { Text = $"Bạn cần sở hữu sách '{bookName}' để có thể viết đánh giá." };
        }

        private ChatResponse ViewMyReviews(int userId)
        {
            var result = new StringBuilder("Các đánh giá của bạn:<br/>");
            string query = @"SELECT TOP 5 s.TenSach, dg.Diem, dg.NhanXet, dg.NgayDanhGia
                             FROM DanhGiaSach dg
                             JOIN Sach s ON dg.IDSach = s.IDSach
                             WHERE dg.IDNguoiDung = @UserId
                             ORDER BY dg.NgayDanhGia DESC";

            using (var con = new SqlConnection(connectionString))
            using (var cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@UserId", userId);
                try
                {
                    con.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (!reader.HasRows) return new ChatResponse { Text = "Bạn chưa viết đánh giá nào." };
                        while (reader.Read())
                        {
                            result.AppendLine($"- <b>{reader["TenSach"]}</b> ({reader["Diem"]}/5 sao): <i>{reader["NhanXet"]}</i><br/>");
                        }
                    }
                }
                catch { return new ChatResponse { Text = "Lỗi khi truy vấn đánh giá của bạn." }; }
            }
            // Không có nút bấm cho chức năng này vì trang Quản lý đánh giá của Admin không dành cho User
            return new ChatResponse { Text = result.ToString() };
        }

        #endregion
    }
}
