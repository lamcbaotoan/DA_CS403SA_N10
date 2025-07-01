using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Diagnostics; // Cho Debug

namespace Webebook.WebForm.User
{
    public partial class usertrangchu : System.Web.UI.Page
    {
        string connectionString = ConfigurationManager.ConnectionStrings["datawebebookConnectionString"].ConnectionString;
        int userId = 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null || !int.TryParse(Session["UserID"].ToString(), out userId) || userId <= 0)
            {
                Response.Redirect("~/WebForm/VangLai/dangnhap.aspx?message=login_required", true);
            }

            if (!IsPostBack)
            {
                LoadUsername();
                LoadTiepTucDoc();
                LoadDeXuat();
                LoadSachMoi();
            }
        }

        private void LoadUsername()
        {
            string username = Session["UsernameDisplay"]?.ToString();
            if (string.IsNullOrEmpty(username))
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = "SELECT ISNULL(Ten, Username) FROM NguoiDung WHERE IDNguoiDung = @UserId";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        try
                        {
                            con.Open(); object result = cmd.ExecuteScalar();
                            if (result != null && result != DBNull.Value)
                            {
                                username = result.ToString(); Session["UsernameDisplay"] = username;
                            }
                            else username = "Bạn";
                        }
                        catch (Exception ex) { LogError($"Lỗi load username từ DB: {ex.Message}"); username = "Bạn"; }
                    }
                }
            }
            if (lblUsername != null) { lblUsername.Text = HttpUtility.HtmlEncode(username); }
        }

        private void LoadTiepTucDoc()
        {
            // Cần cột lưu ngày cập nhật vị trí đọc (VD: NgayCapNhatViTri) để ORDER BY chính xác hơn
            string query = @"SELECT TOP 6 ts.IDSach, s.TenSach, s.TacGia, s.DuongDanBiaSach, ts.ViTriDoc
                             FROM TuSach ts JOIN Sach s ON ts.IDSach = s.IDSach
                             WHERE ts.IDNguoiDung = @UserId
                               AND ts.ViTriDoc IS NOT NULL AND ts.ViTriDoc <> '' AND ts.ViTriDoc <> '0'
                             ORDER BY ts.NgayThem DESC"; // Nên ORDER BY NgayCapNhatViTri DESC nếu có
            LoadDataForRepeater(rptTiepTucDoc, pnlTiepTucDoc, pnlNoTiepTucDoc, query, new SqlParameter("@UserId", userId));
            // Luôn hiển thị section này, panel bên trong sẽ ẩn/hiện
            if (pnlTiepTucDocSection != null) pnlTiepTucDocSection.Visible = true;
        }

        private void LoadDeXuat()
        {
            // Query này có thể chậm với lượng sách/user lớn. Cần cơ chế đề xuất tốt hơn.
            string query = @"SELECT TOP 6 IDSach, TenSach, TacGia, DuongDanBiaSach, GiaSach
                             FROM Sach
                             WHERE IDSach NOT IN (SELECT IDSach FROM TuSach WHERE IDNguoiDung = @UserId)
                             ORDER BY NEWID()";
            LoadDataForRepeater(rptDeXuat, pnlDeXuat, pnlNoDeXuat, query, new SqlParameter("@UserId", userId));
        }

        private void LoadSachMoi()
        {
            string query = @"SELECT TOP 12 IDSach, TenSach, TacGia, DuongDanBiaSach, GiaSach FROM Sach ORDER BY IDSach DESC"; // Lấy nhiều sách mới hơn
            LoadDataForRepeater(rptSachMoiUser, pnlSachMoiUser, pnlNoSachMoiUser, query);
        }

        // --- Hàm Helper ---
        private void LoadDataForRepeater(Repeater rpt, Panel pnlData, Panel pnlNoData, string query, params SqlParameter[] parameters)
        {
            DataTable dt = GetData(query, parameters);
            if (dt != null && dt.Rows.Count > 0)
            {
                rpt.DataSource = dt; rpt.DataBind();
                if (pnlData != null) pnlData.Visible = true; if (pnlNoData != null) pnlNoData.Visible = false;
            }
            else
            {
                rpt.DataSource = null; rpt.DataBind();
                if (pnlData != null) pnlData.Visible = false; if (pnlNoData != null) pnlNoData.Visible = true;
            }
            // Gọi JS nếu là PostBack và có dữ liệu (có thể thêm check cho từng repeater)
            if (dt != null && dt.Rows.Count > 0 && IsPostBack)
            {
                // ScriptManager.RegisterStartupScript(this, GetType(), $"ReInitFadeIn_{rpt.ID}", "setTimeout(initializeCardFadeInUser, 100);", true);
            }
        }

        private DataTable GetData(string query, params SqlParameter[] parameters)
        {
            DataTable dt = new DataTable(); try { using (SqlConnection con = new SqlConnection(connectionString)) { using (SqlCommand cmd = new SqlCommand(query, con)) { if (parameters != null && parameters.Length > 0) { cmd.Parameters.AddRange(parameters); } con.Open(); SqlDataAdapter da = new SqlDataAdapter(cmd); da.Fill(dt); } } } catch (Exception ex) { LogError($"Lỗi GetData ({this.ID}): {ex.ToString()}"); return null; }
            return dt;
        }

        protected string GetImageUrl(object pathData)
        {
            string defaultImage = ResolveUrl("~/Images/placeholder_cover.png"); if (pathData != DBNull.Value && !string.IsNullOrWhiteSpace(pathData?.ToString())) { string path = pathData.ToString(); if (path.StartsWith("~/")) { return ResolveUrl(path); } return path; }
            return defaultImage;
        }

        protected string FormatViTriDoc(object viTriDocData)
        {
            if (viTriDocData != DBNull.Value && viTriDocData != null) { string viTri = viTriDocData.ToString(); if (!string.IsNullOrWhiteSpace(viTri) && viTri != "0") { if (int.TryParse(viTri, out _)) { return "Chương " + viTri; } return viTri; } }
            return "Chưa đọc";
        }

        private void LogError(string message) { Debug.WriteLine(message); }
    }
}