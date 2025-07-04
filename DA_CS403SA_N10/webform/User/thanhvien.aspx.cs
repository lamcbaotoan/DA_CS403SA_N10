// Generated on: 06/04/2025
using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls; // Cần cho Label, Image, Panel
using System.Globalization; // Cần nếu dùng FormatCurrency hoặc các hàm Culture khác

namespace Webebook.WebForm.User // Hoặc namespace phù hợp nếu đặt ở chỗ khác
{
    public partial class thanhvien : System.Web.UI.Page
    {
        string connectionString = ConfigurationManager.ConnectionStrings["datawebebookConnectionString"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Lấy ID thành viên từ QueryString
                if (int.TryParse(Request.QueryString["id"], out int memberId) && memberId > 0)
                {
                    LoadMemberInfo(memberId);
                }
                else
                {
                    ShowMessage("ID thành viên không hợp lệ.", true);
                    pnlMemberProfile.Visible = false; // Ẩn panel profile nếu ID sai
                }

                // Cập nhật giỏ hàng nếu người xem ĐÃ đăng nhập
                if (Session["UserID"] != null && Master is UserMaster master)
                {
                    master.UpdateCartCount();
                }
            }
            // Ẩn message nếu là postback trừ khi có lỗi mới
            if (IsPostBack) lblMemberMessage.Visible = false;
        }

        /// <summary>
        /// Tải thông tin công khai của thành viên và hiển thị.
        /// </summary>
        /// <param name="memberId">ID của thành viên cần xem.</param>
        private void LoadMemberInfo(int memberId)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                // Chỉ lấy các thông tin công khai và kiểm tra trạng thái Active
                string query = @"SELECT Username, Ten, AnhNen
                                 FROM NguoiDung
                                 WHERE IDNguoiDung = @MemberId AND TrangThai = 'Active'";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@MemberId", memberId);
                    try
                    {
                        con.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string dbTen = reader["Ten"] != DBNull.Value ? reader["Ten"].ToString() : null;
                                string dbUsername = reader["Username"].ToString();
                                object dbAnhNen = reader["AnhNen"];
                                string currentAvatarUrl = GetUserAvatarUrl(dbAnhNen); // Dùng hàm helper

                                // Xác định tên hiển thị (Ưu tiên Tên, nếu không có thì dùng Username)
                                string displayName = string.IsNullOrEmpty(dbTen) ? dbUsername : dbTen;

                                // Hiển thị thông tin
                                lblMemberUsername.Text = "@" + dbUsername;
                                lblMemberDisplayName.Text = displayName;
                                imgMemberAvatar.ImageUrl = currentAvatarUrl;
                                imgMemberAvatar.AlternateText = "Ảnh đại diện của " + displayName; // Thêm Alt text

                                pnlMemberProfile.Visible = true; // Hiển thị panel profile
                            }
                            else
                            {
                                // Không tìm thấy user hoặc user không Active
                                ShowMessage("Không tìm thấy thành viên này hoặc tài khoản không hoạt động.", true);
                                pnlMemberProfile.Visible = false;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ShowMessage("Lỗi khi tải thông tin thành viên: " + ex.Message, true);
                        pnlMemberProfile.Visible = false;
                        LogError($"Load Member Info Error (MemberID: {memberId}): {ex.ToString()}");
                    }
                }
            }
        }


        // --- CÁC HÀM HỖ TRỢ (Copy từ các trang khác hoặc tạo lớp dùng chung) ---

        /// <summary>
        /// Hiển thị thông báo.
        /// </summary>
        private void ShowMessage(string message, bool isError)
        {
            lblMemberMessage.Text = Server.HtmlEncode(message);
            lblMemberMessage.CssClass = "block mb-4 p-3 rounded-md border text-sm " + (isError ? "bg-red-50 border-red-300 text-red-700" : "bg-blue-50 border-blue-300 text-blue-700"); // Dùng màu xanh cho info/warning
            lblMemberMessage.Visible = true;
        }

        /// <summary>
        /// Ghi log lỗi.
        /// </summary>
        private void LogError(string message)
        {
            System.Diagnostics.Trace.TraceError("MemberProfileError: " + message);
            // Nên dùng thư viện log chuyên nghiệp
        }

        /// <summary>
        /// Chuyển đổi dữ liệu byte[] ảnh thành Data URL hoặc trả về ảnh mặc định.
        /// </summary>
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