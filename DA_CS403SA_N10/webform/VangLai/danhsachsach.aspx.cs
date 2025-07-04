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
using System.Diagnostics;

namespace Webebook.WebForm.VangLai
{
    public partial class danhsachsach : System.Web.UI.Page
    {
        string connectionString = ConfigurationManager.ConnectionStrings["datawebebookConnectionString"].ConnectionString;
        private const int PageSize = 10; // Số sách mỗi trang

        // ViewState properties
        private string CurrentSearchTerm { get { return ViewState["CurrentSearchTerm"] as string ?? string.Empty; } set { ViewState["CurrentSearchTerm"] = value; } }
        private string CurrentGenre { get { return ViewState["CurrentGenre"] as string ?? string.Empty; } set { ViewState["CurrentGenre"] = value; } }
        private int CurrentPageIndex { get { return (int)(ViewState["CurrentPageIndex"] ?? 1); } set { ViewState["CurrentPageIndex"] = value; } }
        private int TotalRows { get { return (int)(ViewState["TotalRows"] ?? 0); } set { ViewState["TotalRows"] = value; } }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadGenres();
                txtSearchFilter.Text = CurrentSearchTerm;
                try
                {
                    if (!string.IsNullOrEmpty(CurrentGenre) && ddlGenreFilter.Items.FindByValue(CurrentGenre) != null)
                    {
                        ddlGenreFilter.SelectedValue = CurrentGenre;
                    }
                    else
                    {
                        CurrentGenre = string.Empty;
                        ddlGenreFilter.SelectedIndex = 0;
                    }
                }
                catch
                {
                    CurrentGenre = string.Empty;
                    if (ddlGenreFilter.Items.Count > 0) ddlGenreFilter.SelectedIndex = 0;
                }
                LoadBookList();
            }
            if (!IsPostBack && string.IsNullOrEmpty(lblMessage.Text))
            {
                lblMessage.Visible = false;
            }
        }

        private void LoadGenres()
        {
            DataTable dtGenres = GetDistinctGenres();
            ddlGenreFilter.Items.Clear();
            ddlGenreFilter.Items.Insert(0, new ListItem("-- Tất cả thể loại --", ""));
            if (dtGenres != null && dtGenres.Rows.Count > 0)
            {
                ddlGenreFilter.DataSource = dtGenres;
                ddlGenreFilter.DataTextField = "Value";
                ddlGenreFilter.DataValueField = "Value";
                ddlGenreFilter.DataBind();
            }
            if (string.IsNullOrEmpty(CurrentGenre))
            {
                ddlGenreFilter.SelectedIndex = 0;
            }
        }

        private DataTable GetDistinctGenres()
        {
            DataTable dt = new DataTable();
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"
                    SELECT DISTINCT LTRIM(RTRIM(Value)) AS Value FROM (
                        SELECT DISTINCT LoaiSach as Value FROM Sach WHERE LoaiSach IS NOT NULL AND LoaiSach <> ''
                        UNION
                        SELECT value
                        FROM Sach
                        CROSS APPLY STRING_SPLIT(ISNULL(TheLoaiChuoi,''), ',')
                        WHERE NULLIF(LTRIM(RTRIM(value)), '') IS NOT NULL
                    ) AS Genres
                    WHERE Value <> ''
                    ORDER BY Value;";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    try { con.Open(); SqlDataAdapter da = new SqlDataAdapter(cmd); da.Fill(dt); }
                    catch (Exception ex) { LogError("Error Loading Genres (Public): " + ex.ToString()); ShowMessage("Lỗi tải danh sách thể loại.", true); return null; }
                }
            }
            return dt;
        }

        private void LoadBookList()
        {
            DataTable dt = new DataTable();
            StringBuilder queryBuilder = new StringBuilder(@"WITH FilteredSach AS (SELECT IDSach, TenSach, TacGia, GiaSach, DuongDanBiaSach, LoaiSach, TheLoaiChuoi, ROW_NUMBER() OVER (ORDER BY TenSach) AS RowNum FROM Sach WHERE 1 = 1 "); // ORDER BY bạn có thể thay đổi (VD: IDSach DESC)
            List<SqlParameter> parameters = new List<SqlParameter>();
            if (!string.IsNullOrEmpty(CurrentSearchTerm)) { queryBuilder.Append("AND (TenSach LIKE @SearchTerm OR TacGia LIKE @SearchTerm) "); parameters.Add(new SqlParameter("@SearchTerm", $"%{CurrentSearchTerm}%")); }
            if (!string.IsNullOrEmpty(CurrentGenre)) { queryBuilder.Append(@"AND (LoaiSach = @Genre OR CHARINDEX(',' + @TrimmedGenre + ',', ',' + LTRIM(RTRIM(ISNULL(TheLoaiChuoi, ''))) + ',') > 0) "); parameters.Add(new SqlParameter("@Genre", CurrentGenre)); parameters.Add(new SqlParameter("@TrimmedGenre", CurrentGenre.Trim())); }
            queryBuilder.Append(@") SELECT IDSach, TenSach, TacGia, GiaSach, DuongDanBiaSach, LoaiSach, TheLoaiChuoi FROM FilteredSach WHERE RowNum > @StartRowIndex AND RowNum <= @EndRowIndex; ");
            StringBuilder countQueryBuilder = new StringBuilder("SELECT COUNT(*) FROM Sach WHERE 1 = 1 ");
            if (!string.IsNullOrEmpty(CurrentSearchTerm)) { countQueryBuilder.Append("AND (TenSach LIKE @SearchTerm OR TacGia LIKE @SearchTerm) "); }
            if (!string.IsNullOrEmpty(CurrentGenre)) { countQueryBuilder.Append(@"AND (LoaiSach = @Genre OR CHARINDEX(',' + @TrimmedGenre + ',', ',' + LTRIM(RTRIM(ISNULL(TheLoaiChuoi, ''))) + ',') > 0) "); }
            int startRowIndex = (CurrentPageIndex - 1) * PageSize;
            int endRowIndex = CurrentPageIndex * PageSize;
            parameters.Add(new SqlParameter("@StartRowIndex", startRowIndex));
            parameters.Add(new SqlParameter("@EndRowIndex", endRowIndex));

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
                            con.Open();
                            object countResult = countCmd.ExecuteScalar();
                            TotalRows = (countResult != DBNull.Value) ? Convert.ToInt32(countResult) : 0;
                            SqlDataAdapter da = new SqlDataAdapter(cmd); da.Fill(dt);
                            rptSach.DataSource = dt; rptSach.DataBind();
                            bool hasData = dt.Rows.Count > 0;
                            pnlEmptyData.Visible = !hasData;
                            if (bookGridContainer != null) bookGridContainer.Visible = hasData; else rptSach.Visible = hasData;

                            // Gọi lại JS animation sau khi bind nếu là postback và có dữ liệu
                            if (hasData && IsPostBack)
                            {
                                ScriptManager.RegisterStartupScript(this, GetType(), "ReInitFadeIn", "setTimeout(initializeCardFadeIn, 100);", true);
                            }
                        }
                        catch (Exception ex) { LogError("LoadBookList Error (Public): " + ex.ToString()); ShowMessage("Lỗi tải danh sách sách.", true); TotalRows = 0; rptSach.DataSource = null; rptSach.DataBind(); pnlEmptyData.Visible = true; if (bookGridContainer != null) bookGridContainer.Visible = false; else rptSach.Visible = false; }
                        finally { UpdatePagerControls(); }
                    }
                }
            }
        }

        private void UpdatePagerControls()
        {
            int totalPages = (int)Math.Ceiling((double)TotalRows / PageSize);
            lblPagerInfo.Text = totalPages > 0 ? $"Trang {CurrentPageIndex} / {totalPages}" : "Không có sách";
            btnPrevPage.Enabled = (CurrentPageIndex > 1);
            btnNextPage.Enabled = (CurrentPageIndex < totalPages);
            bool pagerVisible = (totalPages > 1);
            btnPrevPage.Visible = pagerVisible;
            lblPagerInfo.Visible = (totalPages > 0);
            btnNextPage.Visible = pagerVisible;
        }

        protected void btnApplyFilter_Click(object sender, EventArgs e)
        {
            CurrentSearchTerm = txtSearchFilter.Text.Trim();
            CurrentGenre = ddlGenreFilter.SelectedValue;
            CurrentPageIndex = 1;
            LoadBookList();
        }

        protected void btnClearFilter_Click(object sender, EventArgs e)
        {
            CurrentSearchTerm = string.Empty;
            CurrentGenre = string.Empty;
            txtSearchFilter.Text = string.Empty;
            if (ddlGenreFilter.Items.Count > 0) ddlGenreFilter.SelectedIndex = 0;
            CurrentPageIndex = 1;
            LoadBookList();
            ShowMessage("Đã xóa bộ lọc.", false);
        }

        protected void Pager_Click(object sender, EventArgs e)
        {
            Button btn = (Button)sender;
            string command = btn.CommandArgument;
            int totalPages = (int)Math.Ceiling((double)TotalRows / PageSize);
            if (command == "Prev" && CurrentPageIndex > 1) { CurrentPageIndex--; }
            else if (command == "Next" && CurrentPageIndex < totalPages) { CurrentPageIndex++; }
            LoadBookList();
        }

        private void ShowMessage(string message, bool isError)
        {
            lblMessage.Text = HttpUtility.HtmlEncode(message);
            string cssClass = "block w-full p-4 mb-6 text-sm rounded-lg border ";
            if (isError) { cssClass += "bg-red-50 border-red-300 text-red-800"; }
            else { cssClass += "bg-green-50 border-green-300 text-green-800"; }
            lblMessage.CssClass = cssClass;
            lblMessage.Visible = true;
        }

        private void LogError(string errorMessage) { Debug.WriteLine(errorMessage); }

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
                else { return defaultImage; }
            }
            return defaultImage;
        }
    }
}