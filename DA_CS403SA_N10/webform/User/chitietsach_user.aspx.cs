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

namespace Webebook.WebForm.User
{
    public enum CartAddResultd { Success, AlreadyExists, Error } // Giữ Enum gốc

    public partial class chitietsach_user : System.Web.UI.Page
    {
        string connectionString = ConfigurationManager.ConnectionStrings["datawebebookConnectionString"].ConnectionString;
        int currentSachId = 0;
        int userId = 0;
        public int CurrentSachId { get { return currentSachId; } }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!int.TryParse(Request.QueryString["IDSach"], out currentSachId) || currentSachId <= 0) { ShowMessage("ID sách không hợp lệ.", true); DisableActionButtons(); HideSectionsOnError(); return; }
            if (Session["UserID"] == null || !int.TryParse(Session["UserID"].ToString(), out userId) || userId <= 0) { string loginUrl = ResolveUrl("~/WebForm/VangLai/dangnhap.aspx"); Response.Redirect($"{loginUrl}?returnUrl={HttpUtility.UrlEncode(Request.RawUrl)}&message=loginrequired", true); return; }
            if (!IsPostBack) { if (LoadBookDetails()) { LoadOverallRating(); LoadCommentsAndReviews(); LoadRecommendations(); } else { HideSectionsOnError(); } }
        }

        private void HideSectionsOnError()
        {
            var reviewSection = FindControl("MainContent")?.FindControl("reviewSection"); if (reviewSection is Control) ((Control)reviewSection).Visible = false;
            var detailContainer = FindControl("MainContent")?.FindControl("bookDetailContainer"); if (detailContainer is Control) ((Control)detailContainer).Visible = false;
            // Section recommendation không còn runat=server nên không cần ẩn ở đây
        }

        private void DisableActionButtons() { if (btnThemGioHangUser != null) { btnThemGioHangUser.Enabled = false; string css = btnThemGioHangUser.CssClass ?? ""; if (!css.Contains("disabled:opacity-50")) btnThemGioHangUser.CssClass = css + " disabled:opacity-50 disabled:cursor-not-allowed"; } if (hlMuaNgay != null) { hlMuaNgay.Visible = false; } }

        private bool LoadBookDetails()
        {
            bool success = false; using (SqlConnection con = new SqlConnection(connectionString)) { string query = @"SELECT TenSach, TacGia, GiaSach, MoTa, DuongDanBiaSach, LoaiSach, TheLoaiChuoi, NhomDich, TrangThaiNoiDung FROM Sach WHERE IDSach = @IDSach"; using (SqlCommand cmd = new SqlCommand(query, con)) { cmd.Parameters.AddWithValue("@IDSach", this.currentSachId); try { con.Open(); using (SqlDataReader reader = cmd.ExecuteReader()) { if (reader.Read()) { litTenSach.Text = HttpUtility.HtmlEncode(reader["TenSach"].ToString()); Page.Title = "Chi tiết: " + reader["TenSach"].ToString(); lblTacGia.Text = HttpUtility.HtmlEncode(reader["TacGia"].ToString()); lblGiaSach.Text = Convert.ToDecimal(reader["GiaSach"]).ToString("N0") + " VNĐ"; litMoTa.Text = FormatDescription(reader["MoTa"]?.ToString()); imgBiaSach.ImageUrl = GetImageUrl(reader["DuongDanBiaSach"]); imgBiaSach.AlternateText = "Bìa sách " + HttpUtility.HtmlEncode(reader["TenSach"].ToString()); string loaiSach = reader["LoaiSach"]?.ToString() ?? string.Empty; if (!string.IsNullOrEmpty(loaiSach)) { lblLoaiSach.Text = HttpUtility.HtmlEncode(loaiSach); lblLoaiSach.Visible = true; } else { lblLoaiSach.Visible = false; } string genresString = reader["TheLoaiChuoi"]?.ToString() ?? string.Empty; List<string> genres = genresString.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(g => HttpUtility.HtmlEncode(g.Trim())).Where(g => !string.IsNullOrEmpty(g)).ToList(); rptGenres.DataSource = genres; rptGenres.DataBind(); lblNhomDich.Text = HttpUtility.HtmlEncode(reader["NhomDich"]?.ToString() ?? "[Chưa cập nhật]"); lblTrangThai.Text = HttpUtility.HtmlEncode(reader["TrangThaiNoiDung"]?.ToString() ?? "[Chưa xác định]"); btnThemGioHangUser.Enabled = true; hlMuaNgay.Visible = true; hlMuaNgay.NavigateUrl = ResolveUrl($"~/WebForm/User/thanhtoan.aspx?buyNowId={currentSachId}"); success = true; } else { this.currentSachId = 0; ShowMessage("Không tìm thấy thông tin sách.", true); DisableActionButtons(); } } } catch (Exception ex) { this.currentSachId = 0; ShowMessage("Lỗi tải chi tiết sách.", true); LogError($"ERROR LoadBookDetails (User): {ex}"); DisableActionButtons(); } } }
            return success;
        }

        protected string GetImageUrl(object pathData) { string placeholder = ResolveUrl("~/Images/placeholder_cover.png"); if (pathData != DBNull.Value && !string.IsNullOrEmpty(pathData?.ToString())) { string path = pathData.ToString(); if (path.StartsWith("~") || path.StartsWith("/")) { try { return ResolveUrl(path); } catch { return placeholder; } } else if (Uri.IsWellFormedUriString(path, UriKind.Absolute)) { return path; } } return placeholder; }
        private string FormatDescription(string description) { if (string.IsNullOrWhiteSpace(description)) return "<p class='text-gray-500 italic'>Chưa có mô tả cho sách này.</p>"; return HttpUtility.HtmlEncode(description).Replace("\r\n", "<br />").Replace("\n", "<br />"); }
        private void LoadOverallRating() { double avgRating = 0; int totalReviews = 0; using (SqlConnection con = new SqlConnection(connectionString)) { string query = @"SELECT COUNT(IDDanhGia) AS TotalReviews, AVG(CAST(Diem AS FLOAT)) AS AverageRating FROM DanhGiaSach WHERE IDSach = @IDSach AND Diem IS NOT NULL AND Diem > 0"; using (SqlCommand cmd = new SqlCommand(query, con)) { cmd.Parameters.AddWithValue("@IDSach", this.currentSachId); try { con.Open(); using (SqlDataReader reader = cmd.ExecuteReader()) { if (reader.Read()) { totalReviews = Convert.ToInt32(reader["TotalReviews"]); if (reader["AverageRating"] != DBNull.Value) avgRating = Convert.ToDouble(reader["AverageRating"]); } } } catch (Exception ex) { LogError($"ERROR LoadOverallRating (User): {ex}"); } } } litTotalReviews.Text = totalReviews.ToString(); litAverageRating.Text = totalReviews > 0 ? avgRating.ToString("0.0") : "N/A"; }
        private void LoadCommentsAndReviews() { string query = @"SELECT TOP 30 'Review' AS EntryType, dg.IDDanhGia AS EntryID, dg.IDNguoiDung, dg.NgayDanhGia AS EntryDate, dg.NhanXet AS ContentText, dg.Diem AS Rating, nd.Ten AS TenNguoiDung, nd.Username, nd.AnhNen FROM DanhGiaSach dg JOIN NguoiDung nd ON dg.IDNguoiDung = nd.IDNguoiDung WHERE dg.IDSach = @IDSach AND dg.NhanXet IS NOT NULL AND LTRIM(RTRIM(dg.NhanXet)) <> '' ORDER BY dg.NgayDanhGia DESC;"; DataTable dt = GetData(query, new SqlParameter("@IDSach", this.currentSachId)); bool hasData = false; if (dt != null) { if (!dt.Columns.Contains("AnhNenUrl")) dt.Columns.Add("AnhNenUrl", typeof(string)); if (!dt.Columns.Contains("TenHienThi")) dt.Columns.Add("TenHienThi", typeof(string)); foreach (DataRow row in dt.Rows) { row["TenHienThi"] = row["TenNguoiDung"] != DBNull.Value && !string.IsNullOrEmpty(row["TenNguoiDung"].ToString()) ? HttpUtility.HtmlEncode(row["TenNguoiDung"].ToString()) : HttpUtility.HtmlEncode(row["Username"].ToString()); row["AnhNenUrl"] = GetAvatarUrl(row["AnhNen"]); row["ContentText"] = FormatCommentText(row["ContentText"]); } litCommentReviewCount.Text = dt.Rows.Count.ToString(); hasData = dt.Rows.Count > 0; rptComments.DataSource = dt; rptComments.DataBind(); } else { litCommentReviewCount.Text = "0"; rptComments.DataSource = null; rptComments.DataBind(); ShowMessage("Lỗi khi tải đánh giá.", true); } pnlNoComments.Visible = !hasData; if (!hasData && lblNoCommentsText != null) { lblNoCommentsText.Text = (dt != null) ? "Chưa có bình luận hoặc đánh giá nào." : "Lỗi khi tải đánh giá."; } }
        protected void rptComments_ItemDataBound(object sender, RepeaterItemEventArgs e) { if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem) { DataRowView drv = e.Item.DataItem as DataRowView; if (drv == null) return; Literal litStars = e.Item.FindControl("litStars") as Literal; if (litStars != null) litStars.Text = RenderStars(drv["Rating"]); Image imgAvatar = e.Item.FindControl("imgCommentAvatar") as Image; if (imgAvatar != null) imgAvatar.ImageUrl = drv["AnhNenUrl"]?.ToString(); Literal litComment = e.Item.FindControl("litCommentText") as Literal; if (litComment != null) litComment.Text = drv["ContentText"]?.ToString(); } }
        protected string RenderStars(object ratingObj) { if (ratingObj == DBNull.Value || ratingObj == null) return ""; int rating = 0; try { rating = Convert.ToInt32(ratingObj); } catch { return ""; } if (rating <= 0 || rating > 5) rating = 0; StringBuilder starsHtml = new StringBuilder(); for (int i = 1; i <= 5; i++) { starsHtml.Append(i <= rating ? "<i class='fas fa-star text-yellow-500'></i>" : "<i class='far fa-star text-gray-400'></i>"); } return starsHtml.ToString(); }
        protected string GetAvatarUrl(object anhNenData) { string defaultAvatar = ResolveUrl("~/Images/default_avatar.png"); if (anhNenData != DBNull.Value && anhNenData is byte[] avatarBytes && avatarBytes.Length > 0) { try { return "data:image;base64," + Convert.ToBase64String(avatarBytes); } catch { return defaultAvatar; } } return defaultAvatar; }
        protected string FormatCommentText(object textData) { if (textData != DBNull.Value && textData != null) { string encodedText = HttpUtility.HtmlEncode(textData.ToString()); return encodedText.Replace("\r\n", "<br />").Replace("\n", "<br />"); } return string.Empty; }

        // === LoadRecommendations LOGIC GỐC ===
        private void LoadRecommendations()
        {
            string query = @"SELECT TOP 6 IDSach, TenSach, TacGia, DuongDanBiaSach FROM Sach WHERE IDSach <> @CurrentIDSach ORDER BY NEWID()";
            LoadDataForRepeaterOriginal(rptRecommendations, pnlRecommendations, pnlNoRecommendations, query, new SqlParameter("@CurrentIDSach", currentSachId));
        }

        // === LoadDataForRepeater GỐC (Có xử lý panel visibility) ===
        private void LoadDataForRepeaterOriginal(Repeater rpt, Panel pnlData, Panel pnlNoData, string query, params SqlParameter[] parameters)
        {
            DataTable dt = GetData(query, parameters);
            bool hasData = (dt != null && dt.Rows.Count > 0);
            if (rpt != null) { rpt.DataSource = hasData ? dt : null; rpt.DataBind(); }
            if (pnlData != null) pnlData.Visible = hasData;
            if (pnlNoData != null) { pnlNoData.Visible = !hasData; if (!hasData && pnlNoData == pnlNoRecommendations) { var lbl = pnlNoData.FindControl("lblNoRecText") as Label; if (lbl != null) lbl.Text = (dt != null) ? "Chưa có gợi ý nào phù hợp." : "Lỗi tải gợi ý."; } }
        }

        private DataTable GetData(string query, params SqlParameter[] parameters) { DataTable dt = new DataTable(); try { using (SqlConnection con = new SqlConnection(connectionString)) { using (SqlCommand cmd = new SqlCommand(query, con)) { if (parameters != null) { cmd.Parameters.AddRange(parameters); } con.Open(); SqlDataAdapter da = new SqlDataAdapter(cmd); da.Fill(dt); } } } catch (SqlException sqlEx) { LogError($"SQL ERROR in GetData: {sqlEx.Message} (Query: {query})"); ShowMessage("Lỗi truy vấn cơ sở dữ liệu.", true); return null; } catch (Exception ex) { LogError($"General ERROR in GetData: {ex}"); ShowMessage("Đã xảy ra lỗi không mong muốn.", true); return null; } return dt; }

        // btnThemGioHangUser_Click (Đã cập nhật popup)
        protected void btnThemGioHangUser_Click(object sender, EventArgs e) { if (userId <= 0) { /* Redirect */ return; } try { string bookName = GetBookName(currentSachId); CartAddResultd result = AddToCart(userId, currentSachId); switch (result) { case CartAddResultd.Success: ShowMessage($"Đã thêm '{HttpUtility.HtmlEncode(bookName)}' vào giỏ hàng!", false); if (Master is UserMaster master) { master.UpdateCartCount(); } break; case CartAddResultd.AlreadyExists: string encodedBookNameJs = HttpUtility.JavaScriptStringEncode(bookName); string script = $"showAlreadyInCartPopup('{encodedBookNameJs}');"; ScriptManager.RegisterStartupScript(this, this.GetType(), $"ShowAlreadyInCartPopup_{currentSachId}", script, true); break; case CartAddResultd.Error: break; } } catch (Exception ex) { ShowMessage("Đã xảy ra lỗi không mong muốn: " + ex.Message, true); LogError("Lỗi btnThemGioHangUser_Click: " + ex.ToString()); } }

        // AddToCart giữ nguyên logic gốc (dùng enum CartAddResultd)
        private CartAddResultd AddToCart(int currentUserId, int idSach) { using (SqlConnection con = new SqlConnection(connectionString)) { string checkQuery = "SELECT COUNT(*) FROM GioHang WHERE IDNguoiDung = @UserId AND IDSach = @IDSach"; try { con.Open(); using (SqlCommand checkCmd = new SqlCommand(checkQuery, con)) { checkCmd.Parameters.AddWithValue("@UserId", currentUserId); checkCmd.Parameters.AddWithValue("@IDSach", idSach); int existingCount = (int)checkCmd.ExecuteScalar(); if (existingCount > 0) { return CartAddResultd.AlreadyExists; } } string insertQuery = "INSERT INTO GioHang (IDNguoiDung, IDSach, SoLuong) VALUES (@UserId, @IDSach, 1)"; using (SqlCommand insertCmd = new SqlCommand(insertQuery, con)) { insertCmd.Parameters.AddWithValue("@UserId", currentUserId); insertCmd.Parameters.AddWithValue("@IDSach", idSach); int rowsAffected = insertCmd.ExecuteNonQuery(); if (rowsAffected > 0) { return CartAddResultd.Success; } else { ShowMessage("Không thể thêm sách vào giỏ hàng.", true); return CartAddResultd.Error; } } } catch (SqlException sqlEx) { ShowMessage("Lỗi cơ sở dữ liệu khi thao tác với giỏ hàng.", true); LogError($"SQL Lỗi AddToCart User {currentUserId}, Sach {idSach}: {sqlEx}"); return CartAddResultd.Error; } catch (Exception ex) { ShowMessage("Lỗi khi thêm vào giỏ hàng: " + ex.Message, true); LogError($"Lỗi AddToCart User {currentUserId}, Sach {idSach}: {ex}"); return CartAddResultd.Error; } } }
        private string GetBookName(int idSach) { string bookName = "Sách"; using (SqlConnection con = new SqlConnection(connectionString)) { string query = "SELECT TenSach FROM Sach WHERE IDSach = @IDSach"; using (SqlCommand cmd = new SqlCommand(query, con)) { cmd.Parameters.AddWithValue("@IDSach", idSach); try { con.Open(); object result = cmd.ExecuteScalar(); if (result != null && result != DBNull.Value) { bookName = result.ToString(); } } catch (Exception ex) { LogError($"Lỗi GetBookName ({idSach}): {ex.Message}"); } } } return bookName; }
        private void ShowMessage(string message, bool isError) { if (lblMessage == null) return; lblMessage.Text = HttpUtility.HtmlEncode(message); string cssClass = "block w-full p-4 mb-6 text-sm rounded-lg border "; if (isError) { cssClass += "bg-red-50 border-red-300 text-red-800"; } else { cssClass += "bg-green-50 border-green-300 text-green-800"; } lblMessage.CssClass = cssClass; lblMessage.Visible = true; }
        private void LogError(string errorMessage) { Debug.WriteLine($"[UserDetail][{DateTime.Now:HH:mm:ss.fff}] {errorMessage}"); }
    }
}