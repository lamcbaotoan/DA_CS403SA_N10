// Webebook/WebForm/VangLai/trangchu.aspx.cs
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Diagnostics;

namespace Webebook.WebForm.VangLai
{
    public partial class trangchu : Page
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
            }

            if (!IsPostBack)
            {
                LoadFeaturedBooks();
                LoadNewestBooks();
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
                Response.Redirect(defaultRedirect, true);
            }
            catch (FormatException) { LogoutCurrentUser(); }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi chuyển hướng từ trang chủ khách: {ex.Message}");
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

        private void LoadFeaturedBooks()
        {
            string query = @"
                SELECT TOP 10
                    s.IDSach, s.TenSach, s.TacGia, s.GiaSach, s.DuongDanBiaSach
                FROM
                    Sach s
                INNER JOIN
                    ChiTietDonHang ctdh ON s.IDSach = ctdh.IDSach
                WHERE
                    s.DuongDanBiaSach IS NOT NULL AND s.DuongDanBiaSach <> ''
                GROUP BY
                    s.IDSach, s.TenSach, s.TacGia, s.GiaSach, s.DuongDanBiaSach
                ORDER BY
                    SUM(ctdh.SoLuong) DESC";

            DataTable dt = GetData(query);
            BindDataToRepeater(rptSachNoiBat, dt, pnlSachNoiBat, pnlNoSachNoiBat);
        }

        private void LoadNewestBooks()
        {
            string query = "SELECT TOP 10 IDSach, TenSach, TacGia, GiaSach, DuongDanBiaSach FROM Sach WHERE DuongDanBiaSach IS NOT NULL AND DuongDanBiaSach <> '' ORDER BY IDSach DESC";
            DataTable dt = GetData(query);
            BindDataToRepeater(rptSachMoi, dt, pnlSachMoi, pnlNoSachMoi);
        }

        private void BindDataToRepeater(Repeater rpt, DataTable dt, Panel pnlDataContainer, Panel pnlNoDataMessage)
        {
            if (dt != null && dt.Rows.Count > 0)
            {
                rpt.DataSource = dt;
                rpt.DataBind();
                if (pnlDataContainer != null) pnlDataContainer.Visible = true;
                if (pnlNoDataMessage != null) pnlNoDataMessage.Visible = false;
            }
            else
            {
                rpt.DataSource = null;
                rpt.DataBind();
                if (pnlDataContainer != null) pnlDataContainer.Visible = false;
                if (pnlNoDataMessage != null) pnlNoDataMessage.Visible = true;
            }
        }

        private DataTable GetData(string query, params SqlParameter[] parameters)
        {
            DataTable dt = new DataTable();
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    if (parameters != null && parameters.Length > 0)
                    {
                        cmd.Parameters.AddRange(parameters);
                    }
                    try
                    {
                        con.Open();
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        da.Fill(dt);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Lỗi khi thực thi query '{query}': {ex.ToString()}");

                        return new DataTable(); 
                    }
                }
            }
            return dt;
        }

        protected string GetImageUrl(object pathData)
        {
            string defaultImage = ResolveUrl("~/Images/placeholder_cover.png");
            if (pathData != DBNull.Value && pathData != null && !string.IsNullOrEmpty(pathData.ToString()))
            {
                string path = pathData.ToString();
                if (path.StartsWith("~") || path.StartsWith("/"))
                {
                    try { return ResolveUrl(path); }
                    catch { return defaultImage; }
                }
                else if (path.StartsWith("http://") || path.StartsWith("https://"))
                {
                    return path;
                }
                else
                {
                    // Xử lý trường hợp chỉ có tên file hoặc đường dẫn tương đối khác nếu cần
                    // Ví dụ: return ResolveUrl("~/Uploads/Images/" + path);
                    return defaultImage; // Mặc định an toàn
                }
            }
            return defaultImage;
        }
    }
}