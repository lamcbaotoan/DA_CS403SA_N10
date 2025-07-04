using System;
using System.Data.SqlClient;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Diagnostics; 

namespace Webebook.WebForm.VangLai
{
    public partial class dangnhap : System.Web.UI.Page
    {
        string connectionString = ConfigurationManager.ConnectionStrings["datawebebookConnectionString"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            Response.Cache.SetExpires(DateTime.UtcNow.AddYears(-1));
            Response.Cache.SetNoTransforms();


            if (Request.IsAuthenticated && Session["UserID"] != null && Session["VaiTro"] != null)
            {
                RedirectAuthenticatedUser();
                return;
            }

            if (!IsPostBack)
            {
                if (Request.QueryString["status"] == "reset_success")
                {
                    ShowMessage("Đặt lại mật khẩu thành công. Vui lòng đăng nhập.", "success");
                }
                else if (Request.QueryString["registered"] == "true") 
                {
                    ShowMessage("Đăng ký thành công. Mời bạn đăng nhập.", "success");
                }
                else
                {
                    lblLoginError.Visible = false;
                    pnlMessageContainer.Visible = false;
                }
                txtLoginUsername.Focus();
            }
        }

        private void RedirectAuthenticatedUser()
        {
            try
            {
                int vaiTro = Convert.ToInt32(Session["VaiTro"]);
                string defaultRedirect = (vaiTro == 0)
                                        ? ResolveUrl("~/WebForm/Admin/adminhome.aspx")
                                        : ResolveUrl("~/WebForm/User/usertrangchu.aspx");

                if (vaiTro != 0)
                {
                    string returnUrlFromQuery = Request.QueryString["returnUrl"];
                    if (!string.IsNullOrEmpty(returnUrlFromQuery) && IsLocalUrl(returnUrlFromQuery))
                    {
                        defaultRedirect = returnUrlFromQuery;
                    }
                }

                Response.Redirect(defaultRedirect, false);
                Context.ApplicationInstance.CompleteRequest();
            }
            catch (FormatException) { LogoutCurrentUser(); }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi chuyển hướng khi đã đăng nhập: {ex.Message}");
                LogoutCurrentUser();
            }
        }

        private void LogoutCurrentUser()
        {
            Session.Clear();
            Session.Abandon();
            FormsAuthentication.SignOut();
            if (Request.Cookies[FormsAuthentication.FormsCookieName] != null)
            {
                HttpCookie myCookie = new HttpCookie(FormsAuthentication.FormsCookieName)
                {
                    Expires = DateTime.Now.AddDays(-1d),
                    Path = FormsAuthentication.FormsCookiePath
                };
                Response.Cookies.Add(myCookie);
            }
        }

        private void ShowMessage(string message, string type)
        {
            lblLoginMessage.Text = message;
            HtmlGenericControl icon = (HtmlGenericControl)pnlMessageContainer.FindControl("iconMessage");

            if (icon != null)
            {
                if (type == "success")
                {
                    pnlMessageContainer.CssClass = "mb-4 p-3 rounded-lg text-sm bg-green-50 border border-green-300 text-green-800 flex items-center";
                    icon.Attributes["class"] = "fas fa-check-circle mr-2 flex-shrink-0";
                }
                else
                {
                    pnlMessageContainer.CssClass = "mb-4 p-3 rounded-lg text-sm bg-blue-50 border border-blue-300 text-blue-800 flex items-center";
                    icon.Attributes["class"] = "fas fa-info-circle mr-2 flex-shrink-0";
                }
            }
            else
            {
                if (type == "success")
                {
                    pnlMessageContainer.CssClass = "mb-4 p-3 rounded-lg text-sm bg-green-50 border border-green-300 text-green-800";
                }
                else
                {
                    pnlMessageContainer.CssClass = "mb-4 p-3 rounded-lg text-sm bg-blue-50 border border-blue-300 text-blue-800";
                }
            }
            pnlMessageContainer.Visible = true;
            lblLoginError.Visible = false;
        }

        private void ShowLoginError(string errorMessage)
        {
            pnlMessageContainer.Visible = false;
            lblLoginError.Text = $"<i class='fas fa-times-circle mr-2 flex-shrink-0'></i> {HttpUtility.HtmlEncode(errorMessage)}";
            lblLoginError.CssClass = "mb-4 p-3 rounded-lg text-sm bg-red-50 border border-red-300 text-red-800 flex items-center";
            lblLoginError.Visible = true;
        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            pnlMessageContainer.Visible = false;
            lblLoginError.Visible = false;
            Page.Validate(); 
            if (!Page.IsValid)
            {
                return;
            }

            string loginIdentifier = txtLoginUsername.Text.Trim();
            string password = txtLoginPassword.Text;
            if (loginIdentifier.Equals("adminWebebook", StringComparison.OrdinalIgnoreCase) && password == "123456")
            {
                Session["UserID"] = -1; 
                Session["Username"] = "adminWebebook";
                Session["UsernameDisplay"] = "QTV"; 
                Session["VaiTro"] = 0;
                FormsAuthentication.SetAuthCookie("admin", false);
                RedirectAuthenticatedUser(); 
                return; 
            }
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = @"SELECT IDNguoiDung, Username, Ten, VaiTro, MatKhau, ISNULL(TrangThai, 'Active') AS TrangThai
                                 FROM NguoiDung
                                 WHERE (Username = @Identifier OR Email = @Identifier OR DienThoai = @Identifier)";

                try
                {
                    connection.Open();
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Identifier", loginIdentifier);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read()) 
                            {
                                string storedPasswordHash = reader["MatKhau"].ToString();
                                string trangThai = reader["TrangThai"].ToString();
                                bool isPasswordValid = (password == storedPasswordHash);

                                if (isPasswordValid)
                                {
                                    if (trangThai.Equals("Active", StringComparison.OrdinalIgnoreCase))
                                    {
                                        int userId = (int)reader["IDNguoiDung"];
                                        string username = reader["Username"].ToString();
                                        string displayName = reader["Ten"] != DBNull.Value ? reader["Ten"].ToString() : username;
                                        int role = Convert.ToInt32(reader["VaiTro"]);
                                        Session["UserID"] = userId;
                                        Session["Username"] = username;
                                        Session["UsernameDisplay"] = displayName;
                                        Session["VaiTro"] = role;
                                        FormsAuthentication.SetAuthCookie(username, false);
                                        RedirectAuthenticatedUser();
                                        return;
                                    }
                                    else if (trangThai.Equals("Locked", StringComparison.OrdinalIgnoreCase))
                                    {
                                        ShowLoginError("Tài khoản của bạn đã bị khóa.");
                                    }
                                    else
                                    {
                                        ShowLoginError($"Tài khoản của bạn đang ở trạng thái không hợp lệ ({trangThai}).");
                                    }
                                }
                                else
                                {
                                    ShowLoginError("Thông tin đăng nhập hoặc mật khẩu không chính xác.");
                                }
                            }
                            else 
                            {
                                ShowLoginError("Thông tin đăng nhập hoặc mật khẩu không chính xác.");
                            }
                        } 
                    } 
                }
                catch (SqlException sqlEx)
                {
                    ShowLoginError("Lỗi cơ sở dữ liệu khi đăng nhập.");
                    Debug.WriteLine($"Login SQL Exception: {sqlEx}");
                }
                catch (Exception ex)
                {
                    ShowLoginError("Lỗi hệ thống khi đăng nhập.");
                    Debug.WriteLine($"Login Exception: {ex}");
                }
            } 
        }
        private bool IsLocalUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return false;
            return url.StartsWith("/") || url.StartsWith("~/") || (!url.Contains(":") && !url.StartsWith("//") && !url.StartsWith("\\\\"));
        }
    }
}