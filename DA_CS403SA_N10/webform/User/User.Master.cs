using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Webebook.WebForm.User
{
    public partial class UserMaster : System.Web.UI.MasterPage
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["datawebebookConnectionString"].ConnectionString;
        private int userId = 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Set chính sách cache để ngăn trình duyệt lưu lại trang, tránh lỗi khi nhấn nút Back sau khi logout
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            Response.Cache.SetExpires(DateTime.UtcNow.AddYears(-1));
            Response.Cache.SetNoTransforms();


            // *** BẮT ĐẦU CODE MỚI ***
            // Kiểm tra xem trang hiện tại có phải là trang đọc sách không
            string currentPage = Path.GetFileName(Request.Url.AbsolutePath);
            if (currentPage.Equals("docsach.aspx", StringComparison.OrdinalIgnoreCase))
            {
                // Nếu là trang đọc sách, ẩn nút chatbot
                if (chatbotToggleButton != null)
                {
                    chatbotToggleButton.Visible = false;
                }
            }
            // *** KẾT THÚC CODE MỚI ***

            // Kiểm tra session người dùng
            if (Session["UserID"] == null || !int.TryParse(Session["UserID"].ToString(), out userId) || userId <= 0)
            {
                // Nếu không có session hợp lệ, thực hiện đăng xuất và chuyển hướng về trang đăng nhập
                LogoutCurrentUser(false); // Dọn dẹp cookie (nếu có)
                string returnUrl = HttpUtility.UrlEncode(Request.Url.PathAndQuery);
                Response.Redirect($"~/WebForm/VangLai/dangnhap.aspx?returnUrl={returnUrl}", false);
                Context.ApplicationInstance.CompleteRequest();
            }
            else
            {
                // Nếu có session hợp lệ
                hdnUserId.Value = userId.ToString(); // Gán userId cho chatbot sử dụng
                if (!IsPostBack)
                {
                    LoadUserInfo();
                    UpdateCartCount();
                }
                SetAvatarImageFromSession();
            }
        }

        private void LoadUserInfo()
        {
            if (userId <= 0) return;

            string tenHienThi = "Tài khoản";
            string avatarUrl = ResolveUrl("~/Images/default_avatar.png");

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "SELECT ISNULL(Ten, Username) AS TenHienThi, AnhNen FROM NguoiDung WHERE IDNguoiDung = @UserId";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    try
                    {
                        con.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                tenHienThi = reader["TenHienThi"]?.ToString() ?? "Tài khoản";
                                if (reader["AnhNen"] != DBNull.Value)
                                {
                                    byte[] avatarBytes = (byte[])reader["AnhNen"];
                                    if (avatarBytes.Length > 0)
                                    {
                                        avatarUrl = "data:image/jpeg;base64," + Convert.ToBase64String(avatarBytes);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Lỗi tải thông tin người dùng (ID: {userId}): {ex.Message}");
                        tenHienThi = "Lỗi Tải";
                    }
                }
            }

            Session["UsernameDisplay"] = tenHienThi;
            Session["AvatarUrl"] = avatarUrl;

            UpdateHeaderUI();
        }

        private void SetAvatarImageFromSession()
        {
            if (imgUserAvatar != null)
            {
                imgUserAvatar.ImageUrl = Session["AvatarUrl"] as string ?? ResolveUrl("~/Images/default_avatar.png");
            }
        }

        private void UpdateHeaderUI()
        {
            string tenHienThi = Session["UsernameDisplay"] as string ?? "Tài khoản";
            string avatarUrl = Session["AvatarUrl"] as string ?? ResolveUrl("~/Images/default_avatar.png");

            if (lblUserNameDisplay != null) lblUserNameDisplay.Text = HttpUtility.HtmlEncode(tenHienThi);
            if (imgUserAvatar != null)
            {
                imgUserAvatar.ImageUrl = avatarUrl;
                imgUserAvatar.AlternateText = $"Avatar của {HttpUtility.HtmlEncode(tenHienThi)}";
            }
            if (hlProfile != null) hlProfile.ToolTip = $"Xem hồ sơ của {HttpUtility.HtmlEncode(tenHienThi)}";
        }

        public void UpdateCartCount()
        {
            if (userId <= 0) return;
            int totalItems = 0;
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "SELECT ISNULL(SUM(SoLuong), 0) FROM GioHang WHERE IDNguoiDung = @UserId";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    try
                    {
                        con.Open();
                        totalItems = (int)(cmd.ExecuteScalar() ?? 0);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Lỗi lấy số lượng giỏ hàng: {ex.Message}");
                    }
                }
            }

            bool hasItems = totalItems > 0;
            string countText = hasItems ? totalItems.ToString() : "";
            if (lblCartCountBadge != null)
            {
                lblCartCountBadge.Text = countText;
                lblCartCountBadge.Visible = hasItems;
            }
            if (lblCartCountDropdown != null)
            {
                lblCartCountDropdown.Text = countText;
                lblCartCountDropdown.Visible = hasItems;
            }
        }

        protected void btnSearchUser_Click(object sender, EventArgs e)
        {
            string searchTerm = txtSearchUser.Text.Trim();
            if (string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = txtSearchUserMobile.Text.Trim();
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                Response.Redirect($"~/WebForm/User/timkiem_user.aspx?q={HttpUtility.UrlEncode(searchTerm)}", false);
                Context.ApplicationInstance.CompleteRequest();
            }
        }

        protected void lnkLogout_Click(object sender, EventArgs e)
        {
            LogoutCurrentUser(true);
        }

        private void LogoutCurrentUser(bool redirectAfterLogout)
        {
            Session.Clear();
            Session.Abandon();

            FormsAuthentication.SignOut();
            if (Request.Cookies[FormsAuthentication.FormsCookieName] != null)
            {
                HttpCookie myCookie = new HttpCookie(FormsAuthentication.FormsCookieName)
                {
                    Expires = DateTime.Now.AddDays(-1d)
                };
                Response.Cookies.Add(myCookie);
            }

            if (redirectAfterLogout)
            {
                // **FIXED**: Sử dụng phương pháp chuyển hướng an toàn hơn
                Response.Redirect("~/WebForm/VangLai/dangnhap.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
            }
        }

        public void UpdateHeaderUserInfo()
        {
            LoadUserInfo();
        }
    }
}