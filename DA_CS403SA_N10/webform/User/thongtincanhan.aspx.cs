// Webebook/WebForm/User/thongtincanhan.aspx.cs
// Final Version: 16/04/2025 (Refined based on Master Page)
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Web; // Needed for HttpUtility
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Webebook.WebForm.User
{
    public partial class thongtincanhan : System.Web.UI.Page
    {
        readonly string connectionString = ConfigurationManager.ConnectionStrings["datawebebookConnectionString"]?.ConnectionString;
        int currentUserId = 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            // *** Bỏ ScriptManager ở đây nếu Master Page đã có ***
            // ScriptManager sm = ScriptManager.GetCurrent(this.Page);
            // if (sm == null) {
            //     sm = new ScriptManager();
            //     sm.ID = "ScriptManager1";
            //     this.Form.Controls.AddAt(0, sm); // Thêm vào đầu form
            // }

            if (string.IsNullOrEmpty(connectionString))
            {
                ShowMessage("Lỗi cấu hình: Không tìm thấy chuỗi kết nối cơ sở dữ liệu (datawebebookConnectionString).", true);
                var editBtn = FindControl("btnShowEditPopup") as LinkButton; if (editBtn != null) editBtn.Enabled = false;
                var contactBtn = FindControl("btnShowContact") as LinkButton; if (contactBtn != null) contactBtn.Enabled = false;
                if (pnlDashboardStats != null) pnlDashboardStats.Visible = false;
                return;
            }

            if (Session["UserID"] == null || !int.TryParse(Session["UserID"].ToString(), out currentUserId) || currentUserId <= 0)
            {
                string returnUrl = Server.UrlEncode(Request.Url.PathAndQuery);
                Response.Redirect($"~/WebForm/VangLai/dangnhap.aspx?returnUrl={returnUrl}", true);
                return;
            }

            if (!IsPostBack)
            {
                LoadUserInfo(); // Tải thông tin cho trang này
                LoadDashboardStats();
                // Không cần gọi Master.UpdateCartCount() ở đây vì Master tự gọi
            }

            lblMessage.Visible = false;
            lblEditMessage.Visible = false;
        }

        /// <summary>
        /// Tải thông tin người dùng cho trang cá nhân, cập nhật Session và các control liên quan.
        /// </summary>
        private void LoadUserInfo()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                // Lấy cả Username, Ten, Email, DienThoai, AnhNen
                string query = "SELECT Username, Email, Ten, DienThoai, AnhNen FROM NguoiDung WHERE IDNguoiDung = @UserId";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@UserId", currentUserId);
                    try
                    {
                        con.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string dbUsername = reader["Username"].ToString();
                                string dbTen = reader["Ten"] != DBNull.Value ? reader["Ten"].ToString() : "";
                                string dbEmail = reader["Email"] != DBNull.Value ? reader["Email"].ToString() : "";
                                string dbDienThoai = reader["DienThoai"] != DBNull.Value ? reader["DienThoai"].ToString() : "";
                                object dbAnhNen = reader["AnhNen"];

                                string currentAvatarUrl = GetUserAvatarUrl(dbAnhNen); // Lấy URL avatar (Base64 hoặc default)
                                string displayName = string.IsNullOrWhiteSpace(dbTen) ? dbUsername : dbTen;

                                // --- Cập nhật Session cho Master Page ---
                                Session["UsernameDisplay"] = displayName;
                                Session["AvatarUrl"] = currentAvatarUrl;

                                // --- Populate Profile Summary display (Trang này) ---
                                lblProfileUserID.Text = "@" + dbUsername;
                                lblProfileDisplayName.Text = Server.HtmlEncode(displayName);
                                imgProfileAvatar.ImageUrl = currentAvatarUrl;
                                imgProfileAvatar.AlternateText = $"Ảnh đại diện của {Server.HtmlEncode(displayName)}";

                                // --- Populate Hidden Fields for JS Modals ---
                                hfUsername.Value = dbUsername; // Lưu Username gốc
                                hfCurrentTen.Value = dbTen;
                                hfCurrentDienThoai.Value = dbDienThoai;
                                hfCurrentEmail.Value = dbEmail;
                                hfCurrentAvatarUrl.Value = currentAvatarUrl;

                                // --- Populate Edit Modal initial values (Trang này) ---
                                litEditUserID.Text = "@" + dbUsername; // Hiển thị trong modal
                                // JS sẽ tự điền các trường còn lại khi mở modal
                            }
                            else
                            {
                                ShowMessage("Lỗi: Không tìm thấy thông tin người dùng.", true);
                                var editBtn = FindControl("btnShowEditPopup") as LinkButton; if (editBtn != null) editBtn.Enabled = false;
                                var contactBtn = FindControl("btnShowContact") as LinkButton; if (contactBtn != null) contactBtn.Enabled = false;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ShowMessage("Lỗi khi tải thông tin cá nhân.", true);
                        LogError($"LoadUserInfo Error (UserID: {currentUserId}): {ex.ToString()}");
                        var editBtn = FindControl("btnShowEditPopup") as LinkButton; if (editBtn != null) editBtn.Enabled = false;
                        var contactBtn = FindControl("btnShowContact") as LinkButton; if (contactBtn != null) contactBtn.Enabled = false;
                        if (pnlDashboardStats != null) pnlDashboardStats.Visible = false;
                    }
                }
            }
        }

        // --- LoadDashboardStats() giữ nguyên ---
        private void LoadDashboardStats()
        {
            // ... (code như cũ) ...
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();
                    string orderCountQuery = "SELECT COUNT(*) FROM DonHang WHERE IDNguoiDung = @UserId";
                    using (SqlCommand cmdOrders = new SqlCommand(orderCountQuery, con))
                    {
                        cmdOrders.Parameters.AddWithValue("@UserId", currentUserId);
                        object result = cmdOrders.ExecuteScalar();
                        litTotalOrders.Text = (result != null && result != DBNull.Value) ? Convert.ToInt32(result).ToString("N0") : "0";
                    }
                    string reviewCountQuery = "SELECT COUNT(*) FROM DanhGiaSach WHERE IDNguoiDung = @UserId";
                    using (SqlCommand cmdReviews = new SqlCommand(reviewCountQuery, con))
                    {
                        cmdReviews.Parameters.AddWithValue("@UserId", currentUserId);
                        object result = cmdReviews.ExecuteScalar();
                        litTotalReviews.Text = (result != null && result != DBNull.Value) ? Convert.ToInt32(result).ToString("N0") : "0";
                    }
                    pnlDashboardStats.Visible = true;
                }
            }
            catch (Exception ex)
            {
                LogError($"LoadDashboardStats Error (UserID: {currentUserId}): {ex.ToString()}");
                pnlDashboardStats.Visible = false;
            }
        }


        /// <summary>
        /// Lưu các thay đổi hồ sơ, cập nhật DB, Session và Master Page Header.
        /// </summary>
        protected void btnSaveChanges_Click(object sender, EventArgs e)
        {
            string ten = txtEditTen.Text.Trim();
            string dienThoai = txtEditDienThoai.Text.Trim();
            string email = txtEditEmail.Text.Trim();
            string username = hfUsername.Value; // Lấy username từ hidden field

            if (string.IsNullOrEmpty(email)) { ShowEditMessage("Email không được để trống.", true); return; }
            if (!IsValidEmail(email)) { ShowEditMessage("Địa chỉ email không hợp lệ.", true); return; }

            byte[] newAvatarBytes = null;
            bool avatarChanged = false;
            string newAvatarUrl = hfCurrentAvatarUrl.Value; // Giữ URL cũ làm mặc định

            // --- Process File Upload ---
            if (fuEditAvatar.HasFile)
            {
                // ... (code xử lý upload như cũ, nếu thành công thì gán newAvatarBytes và avatarChanged = true) ...
                try
                {
                    int maxFileSize = 5 * 1024 * 1024;
                    string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
                    string fileExtension = Path.GetExtension(fuEditAvatar.FileName).ToLowerInvariant();

                    if (fuEditAvatar.PostedFile.ContentLength > 0 && fuEditAvatar.PostedFile.ContentLength <= maxFileSize && allowedExtensions.Contains(fileExtension))
                    {
                        newAvatarBytes = fuEditAvatar.FileBytes;
                        avatarChanged = true;
                        // Nếu upload thành công, tính URL mới ngay lập tức
                        newAvatarUrl = GetUserAvatarUrl(newAvatarBytes); // Cập nhật newAvatarUrl
                    }
                    else if (fuEditAvatar.PostedFile.ContentLength > maxFileSize) { ShowEditMessage($"Lỗi: Kích thước ảnh phải <= 5MB.", true); return; }
                    else if (!allowedExtensions.Contains(fileExtension) && fuEditAvatar.PostedFile.ContentLength > 0) { ShowEditMessage("Lỗi: Chỉ chấp nhận ảnh JPG, PNG, GIF.", true); return; }
                }
                catch (Exception ex) { ShowEditMessage("Lỗi xử lý ảnh tải lên.", true); LogError($"Avatar Upload Error (UserID: {currentUserId}): {ex.ToString()}"); return; }
            }

            // --- Update Database ---
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                // ... (code update DB như cũ, sử dụng newAvatarBytes nếu avatarChanged) ...
                StringBuilder queryBuilder = new StringBuilder("UPDATE NguoiDung SET Email = @Email");
                SqlCommand cmd = new SqlCommand();
                cmd.Parameters.AddWithValue("@Email", email);
                queryBuilder.Append(", Ten = @Ten");
                cmd.Parameters.AddWithValue("@Ten", string.IsNullOrWhiteSpace(ten) ? (object)DBNull.Value : ten);
                queryBuilder.Append(", DienThoai = @DienThoai");
                cmd.Parameters.AddWithValue("@DienThoai", string.IsNullOrWhiteSpace(dienThoai) ? (object)DBNull.Value : dienThoai);
                if (avatarChanged && newAvatarBytes != null)
                {
                    queryBuilder.Append(", AnhNen = @AnhNen");
                    cmd.Parameters.Add("@AnhNen", SqlDbType.VarBinary, -1).Value = newAvatarBytes;
                }
                queryBuilder.Append(" WHERE IDNguoiDung = @UserId");
                cmd.Parameters.AddWithValue("@UserId", currentUserId);
                cmd.CommandText = queryBuilder.ToString();
                cmd.Connection = con;

                try
                {
                    con.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        ShowMessage("Cập nhật thông tin hồ sơ thành công!", false);

                        // --- Cập nhật Session ---
                        string newDisplayName = string.IsNullOrWhiteSpace(ten) ? username : ten;
                        Session["UsernameDisplay"] = newDisplayName;
                        Session["AvatarUrl"] = newAvatarUrl; // Sử dụng URL đã tính toán

                        // --- Cập nhật Header Master Page ---
                        if (this.Master is UserMaster master)
                        {
                            master.UpdateHeaderUserInfo();
                        }

                        // --- Tải lại thông tin cho các control trên trang này ---
                        LoadUserInfo(); // Để cập nhật các hidden field và display controls

                        // --- Đóng Modal ---
                        ScriptManager.RegisterStartupScript(this, this.GetType(), "HideEditModalScript", "hideEditModal();", true);
                    }
                    else
                    {
                        ShowEditMessage("Không có thay đổi nào được lưu.", false);
                    }
                }
                catch (SqlException sqlEx)
                {
                    if (sqlEx.Number == 2627 || sqlEx.Number == 2601) { ShowEditMessage("Lỗi: Địa chỉ email này đã được sử dụng.", true); }
                    else { ShowEditMessage("Lỗi cơ sở dữ liệu khi cập nhật hồ sơ.", true); LogError($"Update User Info SQL Error (UserID: {currentUserId}): {sqlEx.ToString()}"); }
                }
                catch (Exception ex)
                {
                    ShowEditMessage("Lỗi không xác định khi cập nhật thông tin.", true);
                    LogError($"Update User Info Error (UserID: {currentUserId}): {ex.ToString()}");
                }
            }
        }

        // --- btnLuuMatKhau_Click giữ nguyên ---
        protected void btnLuuMatKhau_Click(object sender, EventArgs e)
        {
            // ... (code đổi mật khẩu như cũ - VẪN CÒN VẤN ĐỀ BẢO MẬT) ...
            string oldPass = txtMatKhauCu.Text;
            string newPass = txtMatKhauMoi.Text;
            string confirmPass = txtXacNhanMatKhau.Text;

            if (string.IsNullOrWhiteSpace(oldPass)) { ShowEditMessage("Vui lòng nhập mật khẩu hiện tại.", true); return; }
            if (string.IsNullOrWhiteSpace(newPass)) { ShowEditMessage("Mật khẩu mới không được để trống.", true); return; }
            if (newPass.Length < 6) { ShowEditMessage("Mật khẩu mới phải có ít nhất 6 ký tự.", true); return; }
            if (newPass != confirmPass) { ShowEditMessage("Mật khẩu mới và xác nhận không khớp.", true); return; }

            string storedPassword = "";
            try
            {
                using (SqlConnection conCheck = new SqlConnection(connectionString))
                {
                    conCheck.Open();
                    string queryCheck = "SELECT MatKhau FROM NguoiDung WHERE IDNguoiDung = @UserId";
                    using (SqlCommand cmdCheck = new SqlCommand(queryCheck, conCheck))
                    {
                        cmdCheck.Parameters.AddWithValue("@UserId", currentUserId);
                        object result = cmdCheck.ExecuteScalar();
                        if (result != null && result != DBNull.Value) { storedPassword = result.ToString(); }
                        else { ShowEditMessage("Lỗi: Không tìm thấy tài khoản để xác thực.", true); return; }
                    }
                }
            }
            catch (Exception ex) { ShowEditMessage("Lỗi khi kiểm tra mật khẩu cũ.", true); LogError($"Check Old Password Error (UserID: {currentUserId}): {ex.ToString()}"); return; }

            bool isOldPasswordValid = VerifyPassword_INSECURE(oldPass, storedPassword);
            if (!isOldPasswordValid) { ShowEditMessage("Mật khẩu hiện tại không đúng.", true); return; }

            string plainTextNewPasswordToStore = HashPassword_INSECURE(newPass);
            if (string.IsNullOrEmpty(plainTextNewPasswordToStore)) { ShowEditMessage("Lỗi xử lý mật khẩu mới.", true); return; }

            try
            {
                using (SqlConnection conUpdate = new SqlConnection(connectionString))
                {
                    string queryUpdate = "UPDATE NguoiDung SET MatKhau = @MatKhau WHERE IDNguoiDung = @UserId";
                    using (SqlCommand cmdUpdate = new SqlCommand(queryUpdate, conUpdate))
                    {
                        cmdUpdate.Parameters.AddWithValue("@MatKhau", plainTextNewPasswordToStore);
                        cmdUpdate.Parameters.AddWithValue("@UserId", currentUserId);
                        conUpdate.Open();
                        int rowsAffected = cmdUpdate.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            ShowMessage("Đổi mật khẩu thành công!", false);
                            txtMatKhauCu.Text = ""; txtMatKhauMoi.Text = ""; txtXacNhanMatKhau.Text = "";
                            ScriptManager.RegisterStartupScript(this, this.GetType(), "HideEditModalAfterPassChangeScript", "hideEditModal();", true);
                        }
                        else { ShowEditMessage("Không thể đổi mật khẩu vào lúc này.", true); }
                    }
                }
            }
            catch (Exception ex) { ShowEditMessage("Lỗi khi cập nhật mật khẩu mới.", true); LogError($"Change Password Update DB Error (UserID: {currentUserId}): {ex.ToString()}"); }
        }

        // --- Các hàm Password Hashing Placeholders (INSECURE) giữ nguyên ---
        #region Password Hashing Placeholders - INSECURE - DO NOT USE IN PRODUCTION
        private string HashPassword_INSECURE(string password) { /* ... */ return password; }
        private bool VerifyPassword_INSECURE(string enteredPassword, string storedPassword) { /* ... */ return string.Equals(enteredPassword, storedPassword, StringComparison.Ordinal); }
        #endregion

        // --- Các hàm Helper (ShowMessage, ShowEditMessage, LogError, GetUserAvatarUrl, IsValidEmail) giữ nguyên ---
        #region Helper Functions
        private void ShowMessage(string message, bool isError) { /* ... code như cũ ... */ lblMessage.Text = Server.HtmlEncode(message); string cssClass = "block mb-4 p-4 rounded-lg border text-sm font-medium "; cssClass += isError ? "bg-red-50 border-red-300 text-red-700" : "bg-green-50 border-green-300 text-green-700"; lblMessage.CssClass = cssClass; lblMessage.Visible = true; lblEditMessage.Visible = false; }
        private void ShowEditMessage(string message, bool isError) { /* ... code như cũ ... */ lblEditMessage.Text = Server.HtmlEncode(message); string cssClass = "block my-3 p-3 rounded-md border text-xs font-medium "; cssClass += isError ? "bg-red-100 border-red-400 text-red-700" : "bg-green-100 border-green-400 text-green-700"; lblEditMessage.CssClass = cssClass; lblEditMessage.Visible = true; lblMessage.Visible = false; if (isError) { ScriptManager.RegisterStartupScript(this, this.GetType(), "KeepEditModalOpenScript", "showEditModalOnError();", true); } }
        private void LogError(string message) { System.Diagnostics.Trace.TraceError("UserProfilePage Error: " + message); }
        private string GetUserAvatarUrl(object avatarData) { /* ... code như cũ ... */ string defaultAvatar = ResolveUrl("~/Images/default-avatar.png"); if (avatarData != null && avatarData != DBNull.Value) { try { byte[] avatarBytes = (byte[])avatarData; if (avatarBytes.Length > 0) { string mimeType = "image/jpeg"; if (avatarBytes.Length > 4 && avatarBytes[0] == 0x89 && avatarBytes[1] == 0x50 && avatarBytes[2] == 0x4E && avatarBytes[3] == 0x47) mimeType = "image/png"; else if (avatarBytes.Length > 3 && avatarBytes[0] == 0x47 && avatarBytes[1] == 0x49 && avatarBytes[2] == 0x46) mimeType = "image/gif"; string base64String = Convert.ToBase64String(avatarBytes); return $"data:{mimeType};base64,{base64String}"; } } catch (Exception ex) { LogError($"Error converting avatar data to Base64 (UserID: {currentUserId}): {ex.Message}"); } } return defaultAvatar; }
        private bool IsValidEmail(string email) { /* ... code như cũ ... */ if (string.IsNullOrWhiteSpace(email)) return false; try { var addr = new System.Net.Mail.MailAddress(email); return addr.Address == email; } catch { return false; } }
        #endregion

    } // End Class
} // End namespace