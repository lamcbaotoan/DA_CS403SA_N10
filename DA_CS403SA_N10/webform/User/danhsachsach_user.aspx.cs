using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls;
using System.Diagnostics; // Cho Debug

namespace Webebook.WebForm.User
{
    // Enum giữ nguyên
    public enum CartAddResult { Success, AlreadyExists, Error }

    public partial class danhsachsach_user : System.Web.UI.Page
    {
        string connectionString = ConfigurationManager.ConnectionStrings["datawebebookConnectionString"].ConnectionString;
        int userId = 0;
        private const int PageSize = 10; // Hoặc giá trị bạn muốn

        // ViewState properties giữ nguyên
        private string CurrentSearchTerm { get { return ViewState["CurrentSearchTerm"] as string ?? string.Empty; } set { ViewState["CurrentSearchTerm"] = value; } }
        private string CurrentGenre { get { return ViewState["CurrentGenre"] as string ?? string.Empty; } set { ViewState["CurrentGenre"] = value; } }
        private int CurrentPageIndex { get { return (int)(ViewState["CurrentPageIndex"] ?? 1); } set { ViewState["CurrentPageIndex"] = value; } }
        private int TotalRows { get { return (int)(ViewState["TotalRows"] ?? 0); } set { ViewState["TotalRows"] = value; } }

        protected void Page_Load(object sender, EventArgs e)
        {
            // Kiểm tra đăng nhập giữ nguyên
            if (Session["UserID"] == null || !int.TryParse(Session["UserID"].ToString(), out userId) || userId <= 0)
            {
                Response.Redirect("~/WebForm/VangLai/dangnhap.aspx?message=notloggedin&returnUrl=" + HttpUtility.UrlEncode(Request.Url.PathAndQuery), true);
                return; // Thoát khỏi hàm sau khi chuyển hướng
            }

            if (!IsPostBack)
            {
                LoadGenres();
                // Khôi phục filter giữ nguyên
                txtSearchFilter.Text = CurrentSearchTerm;
                try
                {
                    if (!string.IsNullOrEmpty(CurrentGenre) && ddlGenreFilter.Items.FindByValue(CurrentGenre) != null) { ddlGenreFilter.SelectedValue = CurrentGenre; }
                    else { CurrentGenre = string.Empty; ddlGenreFilter.SelectedIndex = 0; }
                }
                catch
                {
                    CurrentGenre = string.Empty; if (ddlGenreFilter.Items.Count > 0) ddlGenreFilter.SelectedIndex = 0;
                }
                LoadBookList();
            }
            // Cập nhật logic ẩn message: Chỉ ẩn khi không phải postback và không có nội dung
            if (!IsPostBack && string.IsNullOrEmpty(lblMessage.Text))
            {
                lblMessage.Visible = false;
            }
        }

        // LoadGenres giữ nguyên
        private void LoadGenres()
        {
            DataTable dtGenres = GetDistinctGenres();
            ddlGenreFilter.Items.Clear();
            ddlGenreFilter.Items.Insert(0, new ListItem("-- Tất cả thể loại --", ""));
            if (dtGenres != null && dtGenres.Rows.Count > 0)
            {
                ddlGenreFilter.DataSource = dtGenres; ddlGenreFilter.DataTextField = "Value"; ddlGenreFilter.DataValueField = "Value"; ddlGenreFilter.DataBind();
            }
            if (string.IsNullOrEmpty(CurrentGenre)) { ddlGenreFilter.SelectedIndex = 0; }
        }

        // GetDistinctGenres giữ nguyên
        private DataTable GetDistinctGenres()
        {
            DataTable dt = new DataTable(); using (SqlConnection con = new SqlConnection(connectionString)) { string query = @" SELECT DISTINCT LTRIM(RTRIM(Value)) AS Value FROM ( SELECT DISTINCT LoaiSach as Value FROM Sach WHERE LoaiSach IS NOT NULL AND LoaiSach <> '' UNION SELECT value FROM Sach CROSS APPLY STRING_SPLIT(ISNULL(TheLoaiChuoi,''), ',') WHERE NULLIF(LTRIM(RTRIM(value)), '') IS NOT NULL ) AS Genres WHERE Value <> '' ORDER BY Value;"; using (SqlCommand cmd = new SqlCommand(query, con)) { try { con.Open(); SqlDataAdapter da = new SqlDataAdapter(cmd); da.Fill(dt); } catch (Exception ex) { LogError("Error Loading Genres: " + ex.ToString()); ShowMessage("Lỗi tải danh sách thể loại.", true); return null; } } }
            return dt;
        }

        // LoadBookList giữ nguyên logic, có thể thêm gọi JS
        private void LoadBookList()
        {
            DataTable dt = new DataTable();
            StringBuilder queryBuilder = new StringBuilder(@"WITH FilteredSach AS (SELECT IDSach, TenSach, TacGia, GiaSach, DuongDanBiaSach, LoaiSach, TheLoaiChuoi, ROW_NUMBER() OVER (ORDER BY TenSach) AS RowNum FROM Sach WHERE 1 = 1 "); // Sửa ORDER BY nếu muốn (VD: IDSach DESC)
            List<SqlParameter> parameters = new List<SqlParameter>();
            if (!string.IsNullOrEmpty(CurrentSearchTerm)) { queryBuilder.Append("AND (TenSach LIKE @SearchTerm OR TacGia LIKE @SearchTerm) "); parameters.Add(new SqlParameter("@SearchTerm", $"%{CurrentSearchTerm}%")); }
            if (!string.IsNullOrEmpty(CurrentGenre)) { queryBuilder.Append(@"AND (LoaiSach = @Genre OR CHARINDEX(',' + @TrimmedGenre + ',', ',' + LTRIM(RTRIM(ISNULL(TheLoaiChuoi, ''))) + ',') > 0) "); parameters.Add(new SqlParameter("@Genre", CurrentGenre)); parameters.Add(new SqlParameter("@TrimmedGenre", CurrentGenre.Trim())); }
            queryBuilder.Append(@") SELECT IDSach, TenSach, TacGia, GiaSach, DuongDanBiaSach, LoaiSach, TheLoaiChuoi FROM FilteredSach WHERE RowNum > @StartRowIndex AND RowNum <= @EndRowIndex; ");
            StringBuilder countQueryBuilder = new StringBuilder("SELECT COUNT(*) FROM Sach WHERE 1 = 1 ");
            if (!string.IsNullOrEmpty(CurrentSearchTerm)) { countQueryBuilder.Append("AND (TenSach LIKE @SearchTerm OR TacGia LIKE @SearchTerm) "); }
            if (!string.IsNullOrEmpty(CurrentGenre)) { countQueryBuilder.Append(@"AND (LoaiSach = @Genre OR CHARINDEX(',' + @TrimmedGenre + ',', ',' + LTRIM(RTRIM(ISNULL(TheLoaiChuoi, ''))) + ',') > 0) "); }
            int startRowIndex = (CurrentPageIndex - 1) * PageSize; int endRowIndex = CurrentPageIndex * PageSize;
            parameters.Add(new SqlParameter("@StartRowIndex", startRowIndex)); parameters.Add(new SqlParameter("@EndRowIndex", endRowIndex));

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                using (SqlCommand cmd = new SqlCommand(queryBuilder.ToString(), con))
                {
                    cmd.Parameters.AddRange(parameters.ToArray());
                    using (SqlCommand countCmd = new SqlCommand(countQueryBuilder.ToString(), con))
                    {
                        if (!string.IsNullOrEmpty(CurrentSearchTerm)) { countCmd.Parameters.Add(new SqlParameter("@SearchTerm", $"%{CurrentSearchTerm}%")); }
                        if (!string.IsNullOrEmpty(CurrentGenre)) { countCmd.Parameters.Add(new SqlParameter("@Genre", CurrentGenre)); countCmd.Parameters.Add(new SqlParameter("@TrimmedGenre", CurrentGenre.Trim())); }
                        try
                        {
                            con.Open(); object countResult = countCmd.ExecuteScalar(); TotalRows = (countResult != DBNull.Value) ? Convert.ToInt32(countResult) : 0;
                            SqlDataAdapter da = new SqlDataAdapter(cmd); da.Fill(dt);
                            rptSachUser.DataSource = dt; rptSachUser.DataBind();
                            bool hasData = dt.Rows.Count > 0; pnlEmptyData.Visible = !hasData;
                            if (bookGridContainer != null) bookGridContainer.Visible = hasData; else rptSachUser.Visible = hasData; // Ưu tiên panel
                                                                                                                                    // Gọi lại JS animation nếu cần
                            if (hasData && IsPostBack)
                            {
                                ScriptManager.RegisterStartupScript(this, GetType(), "ReInitFadeInUserList", "setTimeout(initializeCardFadeInUserList, 100);", true);
                            }
                        }
                        catch (Exception ex) { LogError("LoadBookList Error: " + ex.ToString()); ShowMessage("Lỗi tải danh sách sách. Vui lòng thử lại.", true); TotalRows = 0; rptSachUser.DataSource = null; rptSachUser.DataBind(); pnlEmptyData.Visible = true; if (bookGridContainer != null) bookGridContainer.Visible = false; else rptSachUser.Visible = false; }
                        finally { UpdatePagerControls(); }
                    }
                }
            }
        }

        // UpdatePagerControls giữ nguyên
        private void UpdatePagerControls()
        {
            int totalPages = (int)Math.Ceiling((double)TotalRows / PageSize); lblPagerInfo.Text = totalPages > 0 ? $"Trang {CurrentPageIndex} / {totalPages}" : "Không có sách"; btnPrevPage.Enabled = (CurrentPageIndex > 1); btnNextPage.Enabled = (CurrentPageIndex < totalPages); bool pagerVisible = (totalPages > 1); btnPrevPage.Visible = pagerVisible; lblPagerInfo.Visible = (totalPages > 0); btnNextPage.Visible = pagerVisible;
        }

        // Filter Event Handlers giữ nguyên
        protected void Filter_Changed(object sender, EventArgs e) { /* Xóa nếu không dùng AutoPostBack */ }
        protected void btnApplyFilter_Click(object sender, EventArgs e) { CurrentSearchTerm = txtSearchFilter.Text.Trim(); CurrentGenre = ddlGenreFilter.SelectedValue; CurrentPageIndex = 1; LoadBookList(); }
        protected void btnClearFilter_Click(object sender, EventArgs e) { CurrentSearchTerm = string.Empty; CurrentGenre = string.Empty; txtSearchFilter.Text = string.Empty; if (ddlGenreFilter.Items.Count > 0) ddlGenreFilter.SelectedIndex = 0; CurrentPageIndex = 1; LoadBookList(); ShowMessage("Đã xóa bộ lọc.", false); }

        // rptSachUser_ItemCommand giữ nguyên logic xử lý AddToCart và popup
        protected void rptSachUser_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "AddToCart")
            {
                if (userId <= 0) { Response.Redirect("~/WebForm/VangLai/dangnhap.aspx?message=sessionexpired&returnUrl=" + HttpUtility.UrlEncode(Request.Url.PathAndQuery), true); return; }
                try
                {
                    int idSach = Convert.ToInt32(e.CommandArgument); string bookName = GetBookName(idSach);
                    CartAddResult result = AddToCart(userId, idSach);
                    switch (result)
                    {
                        case CartAddResult.Success:
                            ShowMessage($"Đã thêm '{HttpUtility.HtmlEncode(bookName)}' vào giỏ hàng.", false);
                            if (Master is UserMaster master) { master.UpdateCartCount(); }
                            break;
                        case CartAddResult.AlreadyExists:
                            string cartUrl = ResolveUrl("~/WebForm/User/giohang_user.aspx"); string encodedBookName = HttpUtility.JavaScriptStringEncode(bookName);
                            string script = $@"if(confirm('Sách ""{encodedBookName}"" đã có trong giỏ hàng của bạn. Bạn có muốn đến trang giỏ hàng không?')) {{ window.location.href = '{cartUrl}'; }}";
                            // Dùng control trong Item làm gốc để đăng ký script
                            Control btnSender = e.Item.FindControl("btnAddToCart"); // Tìm control đã bấm
                            if (btnSender != null) { ScriptManager.RegisterStartupScript(btnSender, btnSender.GetType(), $"ConfirmCartRedirect_{idSach}", script, true); }
                            else { ScriptManager.RegisterStartupScript(this, this.GetType(), $"ConfirmCartRedirect_{idSach}", script, true); } // Fallback về Page
                            break;
                        case CartAddResult.Error: /* ShowMessage đã gọi */ break;
                    }
                }
                catch (FormatException) { ShowMessage("Lỗi: ID sách không hợp lệ.", true); LogError("Lỗi FormatException trong rptSachUser_ItemCommand, ID: " + e.CommandArgument?.ToString()); }
                catch (Exception ex) { LogError("Lỗi rptSachUser_ItemCommand khi thêm vào giỏ hàng: " + ex.ToString()); ShowMessage("Đã xảy ra lỗi không mong muốn khi thêm vào giỏ hàng.", true); }
            }
        }

        // Pager_Click giữ nguyên
        protected void Pager_Click(object sender, EventArgs e) { Button btn = (Button)sender; string command = btn.CommandArgument; int totalPages = (int)Math.Ceiling((double)TotalRows / PageSize); if (command == "Prev" && CurrentPageIndex > 1) { CurrentPageIndex--; } else if (command == "Next" && CurrentPageIndex < totalPages) { CurrentPageIndex++; } LoadBookList(); }

        // AddToCart giữ nguyên
        private CartAddResult AddToCart(int currentUserId, int idSach) { using (SqlConnection con = new SqlConnection(connectionString)) { string checkQuery = "SELECT COUNT(*) FROM GioHang WHERE IDNguoiDung = @UserId AND IDSach = @IDSach"; try { con.Open(); using (SqlCommand checkCmd = new SqlCommand(checkQuery, con)) { checkCmd.Parameters.AddWithValue("@UserId", currentUserId); checkCmd.Parameters.AddWithValue("@IDSach", idSach); int existingCount = (int)checkCmd.ExecuteScalar(); if (existingCount > 0) { return CartAddResult.AlreadyExists; } } string insertQuery = "INSERT INTO GioHang (IDNguoiDung, IDSach, SoLuong) VALUES (@UserId, @IDSach, 1)"; using (SqlCommand insertCmd = new SqlCommand(insertQuery, con)) { insertCmd.Parameters.AddWithValue("@UserId", currentUserId); insertCmd.Parameters.AddWithValue("@IDSach", idSach); int rowsAffected = insertCmd.ExecuteNonQuery(); if (rowsAffected > 0) { return CartAddResult.Success; } else { ShowMessage("Không thể thêm sách vào giỏ hàng.", true); return CartAddResult.Error; } } } catch (SqlException sqlEx) { ShowMessage("Lỗi cơ sở dữ liệu khi thao tác với giỏ hàng.", true); LogError($"SQL Lỗi AddToCart User {currentUserId}, Sach {idSach}: {sqlEx}"); return CartAddResult.Error; } catch (Exception ex) { ShowMessage("Lỗi khi thêm vào giỏ hàng: " + ex.Message, true); LogError($"Lỗi AddToCart User {currentUserId}, Sach {idSach}: {ex}"); return CartAddResult.Error; } } }

        // GetBookName giữ nguyên
        private string GetBookName(int idSach) { string bookName = "Sách"; using (SqlConnection con = new SqlConnection(connectionString)) { string query = "SELECT TenSach FROM Sach WHERE IDSach = @IDSach"; using (SqlCommand cmd = new SqlCommand(query, con)) { cmd.Parameters.AddWithValue("@IDSach", idSach); try { con.Open(); object result = cmd.ExecuteScalar(); if (result != null && result != DBNull.Value) { bookName = result.ToString(); } } catch (Exception ex) { LogError("GetBookName Error: " + ex.ToString()); } } } return bookName; }

        // ShowMessage giữ nguyên (sử dụng CSS mới)
        private void ShowMessage(string message, bool isError) { if (lblMessage == null) return; lblMessage.Text = HttpUtility.HtmlEncode(message); string cssClass = "block w-full p-4 mb-6 text-sm rounded-lg border "; if (isError) { cssClass += "bg-red-50 border-red-300 text-red-800"; } else { cssClass += "bg-green-50 border-green-300 text-green-800"; } lblMessage.CssClass = cssClass; lblMessage.Visible = true; }

        // LogError giữ nguyên
        private void LogError(string errorMessage) { Debug.WriteLine(errorMessage); }

        // GetImageUrl giữ nguyên
        protected string GetImageUrl(object pathData) { string defaultImage = ResolveUrl("~/Images/placeholder_cover.png"); if (pathData != DBNull.Value && !string.IsNullOrWhiteSpace(pathData?.ToString())) { string path = pathData.ToString(); if (path.StartsWith("~/")) { return ResolveUrl(path); } return path; } return defaultImage; }
    }
}