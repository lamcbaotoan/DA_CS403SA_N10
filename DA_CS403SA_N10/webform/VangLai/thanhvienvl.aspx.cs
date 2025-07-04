// File: /WebForm/VangLai/thanhvienvl.aspx.cs
// Generated on: 2025-04-17 (Adapted for VangLai / Site.Master)
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Globalization; // Không dùng trực tiếp trong code này nhưng có thể cần cho các xử lý khác

// Đảm bảo namespace khớp với vị trí file (ví dụ: trong thư mục VangLai)
namespace Webebook.WebForm.VangLai
{
    public partial class thanhvienvl : System.Web.UI.Page // Đổi tên class thành thanhvienvl
    {
        // Chuỗi kết nối đến cơ sở dữ liệu lấy từ Web.config
        string connectionString = ConfigurationManager.ConnectionStrings["datawebebookConnectionString"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Chỉ thực hiện khi trang được tải lần đầu (không phải postback)
            if (!IsPostBack)
            {
                // Cố gắng lấy ID thành viên từ QueryString (tham số 'id' trên URL)
                // Ví dụ: thanhvienvl.aspx?id=123
                if (int.TryParse(Request.QueryString["id"], out int memberId) && memberId > 0)
                {
                    // Nếu có ID hợp lệ, tải thông tin thành viên
                    LoadMemberInfo(memberId);
                }
                else
                {
                    // Nếu ID không hợp lệ hoặc không có, hiển thị thông báo lỗi
                    ShowMessage("ID thành viên không hợp lệ hoặc không được cung cấp.", true);
                    pnlMemberProfile.Visible = false; // Ẩn khu vực hiển thị hồ sơ
                }

                // Không có logic cập nhật giỏ hàng ở đây vì đây là trang công khai (VangLai)
                // và sử dụng Site.Master không quản lý trạng thái giỏ hàng như User.Master.
            }

            // Ẩn thông báo trên các lần postback (trừ khi có lỗi mới được hiển thị)
            if (IsPostBack)
            {
                lblMemberMessage.Visible = false;
            }
        }

        /// <summary>
        /// Tải và hiển thị thông tin công khai của một thành viên dựa vào ID.
        /// Chỉ lấy các trường công khai và kiểm tra tài khoản có đang hoạt động ('Active').
        /// </summary>
        /// <param name="memberId">ID của thành viên cần xem hồ sơ.</param>
        private void LoadMemberInfo(int memberId)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                // Câu lệnh SQL chỉ chọn các cột cần thiết cho hồ sơ công khai
                string query = @"SELECT Username, Ten, AnhNen
                                 FROM NguoiDung
                                 WHERE IDNguoiDung = @MemberId AND TrangThai = 'Active'"; // Chỉ lấy user đang hoạt động

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    // Thêm tham số ID để tránh SQL Injection
                    cmd.Parameters.AddWithValue("@MemberId", memberId);

                    try
                    {
                        con.Open(); // Mở kết nối
                        using (SqlDataReader reader = cmd.ExecuteReader()) // Thực thi lệnh và đọc dữ liệu
                        {
                            if (reader.Read()) // Nếu tìm thấy bản ghi khớp
                            {
                                // Đọc dữ liệu từ reader, kiểm tra DBNull cho các trường có thể null
                                string dbTen = reader["Ten"] != DBNull.Value ? reader["Ten"].ToString() : null;
                                string dbUsername = reader["Username"].ToString(); // Username không được null
                                object dbAnhNen = reader["AnhNen"]; // Đọc dữ liệu ảnh dưới dạng object

                                // Lấy URL ảnh đại diện (có thể là data URL hoặc link ảnh mặc định)
                                string currentAvatarUrl = GetUserAvatarUrl(dbAnhNen);

                                // Xác định tên hiển thị: Ưu tiên 'Ten', nếu không có thì dùng 'Username'
                                string displayName = string.IsNullOrEmpty(dbTen) ? dbUsername : dbTen;

                                // Cập nhật các control trên trang ASPX
                                lblMemberUsername.Text = "@" + dbUsername;
                                lblMemberDisplayName.Text = Server.HtmlEncode(displayName); // Encode để chống XSS
                                imgMemberAvatar.ImageUrl = currentAvatarUrl;
                                imgMemberAvatar.AlternateText = "Ảnh đại diện của " + Server.HtmlEncode(displayName); // Alt text cho ảnh

                                // Hiển thị panel chứa thông tin hồ sơ
                                pnlMemberProfile.Visible = true;
                            }
                            else
                            {
                                // Nếu không tìm thấy user hoặc user không 'Active'
                                ShowMessage("Không tìm thấy thành viên này hoặc tài khoản không hoạt động.", true);
                                pnlMemberProfile.Visible = false; // Ẩn panel hồ sơ
                            }
                        }
                    }
                    catch (SqlException ex) // Bắt lỗi liên quan đến SQL
                    {
                        ShowMessage("Lỗi truy cập cơ sở dữ liệu khi tải thông tin.", true);
                        pnlMemberProfile.Visible = false;
                        LogError($"SQL Error Load Member Info (MemberID: {memberId}): {ex.Message}");
                    }
                    catch (Exception ex) // Bắt các lỗi chung khác
                    {
                        ShowMessage("Đã xảy ra lỗi không mong muốn khi tải thông tin thành viên.", true);
                        pnlMemberProfile.Visible = false;
                        LogError($"General Error Load Member Info (MemberID: {memberId}): {ex.ToString()}");
                    }
                } // SqlCommand được dispose tự động
            } // SqlConnection được dispose tự động
        }

        // --- CÁC HÀM HỖ TRỢ ---

        /// <summary>
        /// Hiển thị thông báo trên trang (lỗi hoặc thông tin).
        /// </summary>
        /// <param name="message">Nội dung thông báo.</param>
        /// <param name="isError">True nếu là thông báo lỗi (màu đỏ), False nếu là thông tin (màu xanh).</param>
        private void ShowMessage(string message, bool isError)
        {
            lblMemberMessage.Text = Server.HtmlEncode(message); // Encode nội dung để tránh XSS
            // Đặt class CSS tương ứng (sử dụng Tailwind CSS)
            lblMemberMessage.CssClass = "block mb-4 p-3 rounded-md border text-sm " +
                                        (isError ? "bg-red-50 border-red-300 text-red-700"
                                                 : "bg-blue-50 border-blue-300 text-blue-700");
            lblMemberMessage.Visible = true; // Hiển thị label thông báo
        }

        /// <summary>
        /// Ghi log lỗi. Trong môi trường thực tế, nên sử dụng thư viện logging chuyên nghiệp
        /// như NLog, Serilog, log4net hoặc Azure Application Insights.
        /// </summary>
        /// <param name="message">Thông điệp lỗi cần ghi.</param>
        private void LogError(string message)
        {
            // Ví dụ đơn giản: Ghi vào cửa sổ Output khi chạy ở chế độ Debug
            System.Diagnostics.Debug.WriteLine("ERROR_THANHVIENVL: " + message);

            // Ví dụ ghi vào file (cần cấp quyền ghi cho ứng dụng web):
            /*
            try
            {
                string logPath = Server.MapPath("~/App_Data/ErrorLog_VangLai.txt");
                System.IO.File.AppendAllText(logPath, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}{Environment.NewLine}");
            }
            catch { } // Bỏ qua lỗi ghi log để tránh lỗi vòng lặp
            */
        }

        /// <summary>
        /// Chuyển đổi dữ liệu ảnh (byte array) từ database thành Data URL (Base64)
        /// để hiển thị trực tiếp trong thẻ <img>. Nếu không có ảnh hoặc có lỗi,
        /// trả về đường dẫn đến ảnh đại diện mặc định.
        /// </summary>
        /// <param name="avatarData">Dữ liệu ảnh dạng object (thường là byte[] hoặc DBNull).</param>
        /// <returns>Chuỗi Data URL hoặc đường dẫn ảnh mặc định.</returns>
        private string GetUserAvatarUrl(object avatarData)
        {
            // ***** THAY ĐỔI ĐƯỜNG DẪN Ở ĐÂY *****
            // Đường dẫn đến ảnh mặc định mới
            string defaultAvatar = ResolveUrl("~/Images/default_avatar.png"); // <--- Đã cập nhật

            // Kiểm tra xem dữ liệu có tồn tại và không phải là DBNull
            if (avatarData != null && avatarData != DBNull.Value)
            {
                try
                {
                    // Ép kiểu dữ liệu sang byte array
                    byte[] avatarBytes = (byte[])avatarData;

                    // Chỉ xử lý nếu có dữ liệu byte
                    if (avatarBytes.Length > 0)
                    {
                        // Chuyển byte array thành chuỗi Base64
                        string base64String = Convert.ToBase64String(avatarBytes);

                        // Cố gắng xác định kiểu MIME dựa trên "magic numbers" (byte đầu tiên)
                        string mimeType = "image/jpeg"; // Mặc định là JPEG
                        if (avatarBytes.Length > 4 && avatarBytes[0] == 0x89 && avatarBytes[1] == 0x50 && avatarBytes[2] == 0x4E && avatarBytes[3] == 0x47)
                        {
                            mimeType = "image/png"; // PNG
                        }
                        else if (avatarBytes.Length > 3 && avatarBytes[0] == 0x47 && avatarBytes[1] == 0x49 && avatarBytes[2] == 0x46)
                        {
                            mimeType = "image/gif"; // GIF
                        }
                        // Có thể thêm kiểm tra cho các định dạng khác như WebP nếu cần

                        // Trả về chuỗi Data URL hoàn chỉnh
                        return $"data:{mimeType};base64," + base64String;
                    }
                }
                catch (InvalidCastException ex) // Lỗi nếu dữ liệu trong DB không phải byte[]
                {
                    LogError("Error casting avatar data to byte[]: " + ex.Message); // Gọi hàm LogError của class
                }
                catch (Exception ex) // Các lỗi khác khi xử lý ảnh
                {
                    LogError("Error converting avatar data to Base64: " + ex.Message); // Gọi hàm LogError của class
                }
            }

            // Nếu không có dữ liệu, dữ liệu rỗng, hoặc có lỗi, trả về ảnh mặc định đã cập nhật
            return defaultAvatar;
        }
    }
}