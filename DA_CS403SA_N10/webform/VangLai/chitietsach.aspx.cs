using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Diagnostics;

namespace Webebook.WebForm.VangLai
{
    public partial class chitietsach : System.Web.UI.Page
    {
        string connectionString = ConfigurationManager.ConnectionStrings["datawebebookConnectionString"].ConnectionString;
        int sachIdFromQueryString = 0;
        public int CurrentSachId { get { return sachIdFromQueryString; } }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!int.TryParse(Request.QueryString["IDSach"], out sachIdFromQueryString) || sachIdFromQueryString <= 0)
            {
                ShowMessage("ID sách không hợp lệ.", true); DisableActionButtonsPublic(); HideSectionsOnErrorPublic(); return;
            }
            if (!IsPostBack)
            {
                if (LoadBookDetailsPublic())
                {
                    LoadOverallRating();
                    LoadCommentsAndReviews();
                    // *** ĐÃ XÓA LoadRecommendations() ***
                }
                else { HideSectionsOnErrorPublic(); }
            }
        }

        private void HideSectionsOnErrorPublic()
        {
            var reviewSection = FindControl("MainContent")?.FindControl("reviewSection"); if (reviewSection is Control) ((Control)reviewSection).Visible = false;
            var detailContainer = FindControl("MainContent")?.FindControl("bookDetailContainer"); if (detailContainer is Control) ((Control)detailContainer).Visible = false;
        }

        private void DisableActionButtonsPublic()
        {
            if (btnThemGioHang != null) { btnThemGioHang.Enabled = false; string css = btnThemGioHang.CssClass ?? ""; if (!css.Contains("disabled:opacity-50")) btnThemGioHang.CssClass = css + " disabled:opacity-50 disabled:cursor-not-allowed"; }
            if (btnMuaNgay != null) { btnMuaNgay.Enabled = false; string css = btnMuaNgay.CssClass ?? ""; if (!css.Contains("disabled:opacity-50")) btnMuaNgay.CssClass = css + " disabled:opacity-50 disabled:cursor-not-allowed"; }
        }

        private bool LoadBookDetailsPublic()
        {
            bool success = false; using (SqlConnection con = new SqlConnection(connectionString)) { string query = @"SELECT TenSach, TacGia, GiaSach, MoTa, DuongDanBiaSach, LoaiSach, TheLoaiChuoi, NhomDich, TrangThaiNoiDung FROM Sach WHERE IDSach = @IDSach"; using (SqlCommand cmd = new SqlCommand(query, con)) { cmd.Parameters.AddWithValue("@IDSach", this.sachIdFromQueryString); try { con.Open(); using (SqlDataReader reader = cmd.ExecuteReader()) { if (reader.Read()) { litTenSach.Text = HttpUtility.HtmlEncode(reader["TenSach"].ToString()); Page.Title = reader["TenSach"].ToString(); lblTacGia.Text = HttpUtility.HtmlEncode(reader["TacGia"].ToString()); lblGiaSach.Text = Convert.ToDecimal(reader["GiaSach"]).ToString("N0") + " VNĐ"; litMoTa.Text = FormatDescription(reader["MoTa"]?.ToString()); imgBiaSach.ImageUrl = GetImageUrl(reader["DuongDanBiaSach"]); imgBiaSach.AlternateText = "Bìa sách " + HttpUtility.HtmlEncode(reader["TenSach"].ToString()); string loaiSach = reader["LoaiSach"]?.ToString(); if (!string.IsNullOrEmpty(loaiSach)) { lblLoaiSach.Text = HttpUtility.HtmlEncode(loaiSach); lblLoaiSach.Visible = true; } else { lblLoaiSach.Visible = false; } string genresString = reader["TheLoaiChuoi"]?.ToString() ?? ""; List<string> genres = genresString.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(g => HttpUtility.HtmlEncode(g.Trim())).Where(g => !string.IsNullOrEmpty(g)).ToList(); rptGenres.DataSource = genres; rptGenres.DataBind(); lblNhomDich.Text = HttpUtility.HtmlEncode(reader["NhomDich"]?.ToString() ?? "[Chưa cập nhật]"); lblTrangThai.Text = HttpUtility.HtmlEncode(reader["TrangThaiNoiDung"]?.ToString() ?? "[Chưa xác định]"); success = true; } else { ShowMessage("Không tìm thấy thông tin sách.", true); DisableActionButtonsPublic(); } } } catch (Exception ex) { ShowMessage("Lỗi tải chi tiết sách.", true); LogError($"ERROR LoadBookDetails (Public): {ex}"); DisableActionButtonsPublic(); } } }
            return success;
        }

        protected string GetImageUrl(object pathData) { string placeholder = ResolveUrl("~/Images/placeholder_cover.png"); if (pathData != DBNull.Value && !string.IsNullOrEmpty(pathData?.ToString())) { string path = pathData.ToString(); if (path.StartsWith("~") || path.StartsWith("/")) { try { return ResolveUrl(path); } catch { return placeholder; } } else if (Uri.IsWellFormedUriString(path, UriKind.Absolute)) { return path; } } return placeholder; }
        private string FormatDescription(string description) { if (string.IsNullOrWhiteSpace(description)) return "<p>Chưa có mô tả cho sách này.</p>"; return HttpUtility.HtmlEncode(description).Replace("\r\n", "<br />").Replace("\n", "<br />"); }
        private void LoadOverallRating() { double avgRating = 0; int totalReviews = 0; using (SqlConnection con = new SqlConnection(connectionString)) { string query = @"SELECT COUNT(IDDanhGia) AS TotalReviews, AVG(CAST(Diem AS FLOAT)) AS AverageRating FROM DanhGiaSach WHERE IDSach = @IDSach AND Diem IS NOT NULL AND Diem > 0"; using (SqlCommand cmd = new SqlCommand(query, con)) { cmd.Parameters.AddWithValue("@IDSach", this.sachIdFromQueryString); try { con.Open(); using (SqlDataReader reader = cmd.ExecuteReader()) { if (reader.Read()) { totalReviews = Convert.ToInt32(reader["TotalReviews"]); if (reader["AverageRating"] != DBNull.Value) avgRating = Convert.ToDouble(reader["AverageRating"]); } } } catch (Exception ex) { LogError($"ERROR LoadOverallRating (Public): {ex}"); } } } litTotalReviews.Text = totalReviews.ToString(); litAverageRating.Text = totalReviews > 0 ? avgRating.ToString("0.0") : "N/A"; }
        private void LoadCommentsAndReviews() { string query = @"SELECT TOP 30 'Review' AS EntryType, dg.IDDanhGia AS EntryID, dg.IDNguoiDung, dg.NgayDanhGia AS EntryDate, dg.NhanXet AS ContentText, dg.Diem AS Rating, nd.Ten AS TenNguoiDung, nd.Username, nd.AnhNen FROM DanhGiaSach dg JOIN NguoiDung nd ON dg.IDNguoiDung = nd.IDNguoiDung WHERE dg.IDSach = @IDSach AND dg.NhanXet IS NOT NULL AND LTRIM(RTRIM(dg.NhanXet)) <> '' ORDER BY dg.NgayDanhGia DESC;"; DataTable dt = GetData(query, new SqlParameter("@IDSach", this.sachIdFromQueryString)); bool hasData = false; if (dt != null) { if (!dt.Columns.Contains("AnhNenUrl")) dt.Columns.Add("AnhNenUrl", typeof(string)); if (!dt.Columns.Contains("TenHienThi")) dt.Columns.Add("TenHienThi", typeof(string)); foreach (DataRow row in dt.Rows) { row["TenHienThi"] = row["TenNguoiDung"] != DBNull.Value && !string.IsNullOrEmpty(row["TenNguoiDung"].ToString()) ? HttpUtility.HtmlEncode(row["TenNguoiDung"].ToString()) : HttpUtility.HtmlEncode(row["Username"].ToString()); row["AnhNenUrl"] = GetAvatarUrl(row["AnhNen"]); row["ContentText"] = FormatCommentText(row["ContentText"]); } litCommentReviewCount.Text = dt.Rows.Count.ToString(); hasData = dt.Rows.Count > 0; rptComments.DataSource = dt; rptComments.DataBind(); } else { litCommentReviewCount.Text = "0"; rptComments.DataSource = null; rptComments.DataBind(); ShowMessage("Lỗi khi tải đánh giá.", true); } pnlNoComments.Visible = !hasData; if (!hasData && lblNoCommentsText != null) { lblNoCommentsText.Text = (dt != null) ? "Chưa có bình luận hoặc đánh giá nào." : "Lỗi khi tải đánh giá."; } }
        protected void rptComments_ItemDataBound(object sender, RepeaterItemEventArgs e) { if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem) { DataRowView drv = e.Item.DataItem as DataRowView; if (drv == null) return; Literal litStars = e.Item.FindControl("litStars") as Literal; if (litStars != null) litStars.Text = RenderStars(drv["Rating"]); Image imgAvatar = e.Item.FindControl("imgCommentAvatar") as Image; if (imgAvatar != null) imgAvatar.ImageUrl = drv["AnhNenUrl"]?.ToString(); Literal litComment = e.Item.FindControl("litCommentText") as Literal; if (litComment != null) litComment.Text = drv["ContentText"]?.ToString(); } }
        protected string RenderStars(object ratingObj) { if (ratingObj == DBNull.Value || ratingObj == null) return ""; int rating = 0; try { rating = Convert.ToInt32(ratingObj); } catch { return ""; } if (rating <= 0 || rating > 5) return ""; StringBuilder starsHtml = new StringBuilder(); for (int i = 1; i <= 5; i++) { starsHtml.Append(i <= rating ? "<i class='fas fa-star text-yellow-500'></i>" : "<i class='far fa-star text-gray-400'></i>"); } return starsHtml.ToString(); }
        protected string GetAvatarUrl(object anhNenData) { string defaultAvatar = ResolveUrl("~/Images/default_avatar.png"); if (anhNenData != DBNull.Value && anhNenData is byte[] avatarBytes && avatarBytes.Length > 0) { try { return "data:image;base64," + Convert.ToBase64String(avatarBytes); } catch { return defaultAvatar; } } return defaultAvatar; }
        protected string FormatCommentText(object textData) { if (textData != DBNull.Value && textData != null) { string encodedText = HttpUtility.HtmlEncode(textData.ToString()); return encodedText.Replace("\r\n", "<br />").Replace("\n", "<br />"); } return string.Empty; }
        private DataTable GetData(string query, params SqlParameter[] parameters) { DataTable dt = new DataTable(); try { using (SqlConnection con = new SqlConnection(connectionString)) { using (SqlCommand cmd = new SqlCommand(query, con)) { if (parameters != null) { cmd.Parameters.AddRange(parameters); } con.Open(); SqlDataAdapter da = new SqlDataAdapter(cmd); da.Fill(dt); } } } catch (SqlException sqlEx) { LogError($"SQL ERROR in GetData: {sqlEx.Message} (Query: {query})"); ShowMessage("Lỗi truy vấn cơ sở dữ liệu.", true); return null; } catch (Exception ex) { LogError($"General ERROR in GetData: {ex}"); ShowMessage("Đã xảy ra lỗi không mong muốn.", true); return null; } return dt; }
        private void ShowMessage(string message, bool isError) { if (lblMessage == null) return; lblMessage.Text = HttpUtility.HtmlEncode(message); string cssClass = "block w-full p-4 mb-6 text-sm rounded-lg border "; if (isError) { cssClass += "bg-red-50 border-red-300 text-red-800"; } else { cssClass += "bg-green-50 border-green-300 text-green-800"; } lblMessage.CssClass = cssClass; lblMessage.Visible = true; }
        private void LogError(string errorMessage) { Debug.WriteLine(errorMessage); }
    }
}