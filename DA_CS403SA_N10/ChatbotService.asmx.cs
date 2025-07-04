using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
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

        // Static constructor để thiết lập TLS 1.2 một lần duy nhất khi ứng dụng khởi chạy
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
                1. Tìm kiếm sách: [COMMAND:SEARCH_BOOK:tên sách]
                2. Báo giá sách: [COMMAND:GET_PRICE:tên sách]
                3. Tìm sách rẻ nhất: [COMMAND:GET_CHEAPEST_BOOK]
                4. Tìm sách đắt nhất: [COMMAND:GET_MOST_EXPENSIVE_BOOK]
                5. Tìm sách theo thể loại: [COMMAND:FILTER_BY_GENRE:tên thể loại]
                6. Gợi ý sách ngẫu nhiên: [COMMAND:GET_RANDOM_BOOK]

                **B. DÀNH RIÊNG CHO NGƯỜI DÙNG:**
                7. Xem giỏ hàng: [COMMAND:VIEW_CART]
                8. Xem lịch sử mua hàng: [COMMAND:VIEW_PURCHASE_HISTORY]
                9. Kiểm tra trạng thái đơn hàng gần nhất: [COMMAND:CHECK_LAST_ORDER_STATUS]
                10. Xem tủ sách (sách đã mua): [COMMAND:VIEW_BOOKSHELF]
                11. Kiểm tra tiến độ đọc sách: [COMMAND:CHECK_READING_PROGRESS:tên sách]

                QUAN TRỌNG:
                - Luôn trích xuất từ khóa chính xác. Ví dụ, nếu người dùng hỏi 'tôi đọc tới đâu trong sách Thiều Quang Mạn rồi', bạn trả về '[COMMAND:CHECK_READING_PROGRESS:Thiều Quang Mạn]'.
                - Với các câu hỏi trò chuyện khác, hãy trả lời tự nhiên.";

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
                // Lệnh có nút bấm
                case "VIEW_CART": return ViewCart(userId);
                case "VIEW_PURCHASE_HISTORY": return ViewPurchaseHistory(userId);
                case "VIEW_BOOKSHELF": return ViewBookshelf(userId);

                // Lệnh không có nút bấm
                case "SEARCH_BOOK": return new ChatResponse { Text = SearchBookInDatabase(argument) };
                case "GET_PRICE": return new ChatResponse { Text = GetBookPrice(argument) };
                case "GET_CHEAPEST_BOOK": return new ChatResponse { Text = GetCheapestBook() };
                case "GET_MOST_EXPENSIVE_BOOK": return new ChatResponse { Text = GetMostExpensiveBook() };
                case "FILTER_BY_GENRE": return new ChatResponse { Text = FilterByGenre(argument) };
                case "GET_RANDOM_BOOK": return new ChatResponse { Text = GetRandomBook() };
                case "CHECK_LAST_ORDER_STATUS": return new ChatResponse { Text = CheckLastOrderStatus(userId) };
                case "CHECK_READING_PROGRESS": return new ChatResponse { Text = CheckReadingProgress(userId, argument) };

                default:
                    System.Diagnostics.Debug.WriteLine($"Lệnh không nhận dạng được: '{command}'");
                    return new ChatResponse { Text = "Lệnh từ AI không hợp lệ." };
            }
        }

        #region Các Hàm Truy Vấn CSDL

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

        #endregion
    }
}