using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Webebook.WebForm.Admin
{
    public partial class QuanLyDanhGia : System.Web.UI.Page
    {
        string connectionString = ConfigurationManager.ConnectionStrings["datawebebookConnectionString"].ConnectionString;

        #region ViewState Properties
        private string CurrentUserBookFilter
        {
            get { return ViewState["UserBookFilter"] as string ?? string.Empty; }
            set { ViewState["UserBookFilter"] = value; }
        }
        private int CurrentRatingFilter
        {
            get { return (ViewState["RatingFilter"] != null) ? Convert.ToInt32(ViewState["RatingFilter"]) : 0; }
            set { ViewState["RatingFilter"] = value; }
        }
        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadStatistics();
                PopulateRatingFilterDropdown();
                BindGrid();
                ShowListPanels();
            }
            if (IsPostBack)
            {
                pnlAdminMessage.Visible = false;
            }
        }

        #region Load Data and Statistics
        private void LoadStatistics()
        {
            LoadTotalReviewCount();
            LoadOverallAverageRating();
            LoadAverageRatingPerBook();
            LoadRatingDistributionChartData();
        }

        private void LoadTotalReviewCount()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = "SELECT COUNT(*) FROM DanhGiaSach";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        con.Open();
                        lblTotalReviews.Text = ((int)cmd.ExecuteScalar()).ToString("N0", CultureInfo.InvariantCulture);
                    }
                }
            }
            catch (Exception ex) { lblTotalReviews.Text = "Lỗi"; LogError("LoadTotalReviewCount Error: " + ex.Message); }
        }

        private void LoadOverallAverageRating()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = "SELECT AVG(CAST(Diem AS DECIMAL(3, 2))) FROM DanhGiaSach";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        con.Open();
                        object result = cmd.ExecuteScalar();
                        if (result != DBNull.Value && result != null)
                        {
                            lblOverallAverage.Text = Convert.ToDecimal(result).ToString("N1", CultureInfo.InvariantCulture);
                        }
                        else { lblOverallAverage.Text = "N/A"; }
                    }
                }
            }
            catch (Exception ex) { lblOverallAverage.Text = "Lỗi"; LogError("LoadOverallAverageRating Error: " + ex.Message); }
        }

        private void LoadAverageRatingPerBook()
        {
            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = @"SELECT TOP 10 s.TenSach, AVG(CAST(dg.Diem AS DECIMAL(3, 2))) AS AvgRating, COUNT(dg.IDDanhGia) AS ReviewCount
                                     FROM DanhGiaSach dg JOIN Sach s ON dg.IDSach = s.IDSach
                                     GROUP BY s.TenSach ORDER BY AvgRating DESC, ReviewCount DESC";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        con.Open();
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);
                        rptAveragePerBook.DataSource = dt;
                        rptAveragePerBook.DataBind();
                        pnlAveragePerBook.Visible = dt.Rows.Count > 0;
                    }
                }
            }
            catch (Exception ex) { pnlAveragePerBook.Visible = false; LogError("LoadAverageRatingPerBook Error: " + ex.Message); ShowAdminMessage("Lỗi tải điểm trung bình sách.", true); }
        }

        private void LoadRatingDistributionChartData()
        {
            try
            {
                // Phần lấy dữ liệu từ CSDL không đổi
                var ratingCounts = new Dictionary<int, int> { { 1, 0 }, { 2, 0 }, { 3, 0 }, { 4, 0 }, { 5, 0 } };
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    string query = "SELECT Diem, COUNT(*) AS Count FROM DanhGiaSach WHERE Diem BETWEEN 1 AND 5 GROUP BY Diem";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        con.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                if (reader["Diem"] != DBNull.Value && reader["Count"] != DBNull.Value)
                                {
                                    int diem = Convert.ToInt32(reader["Diem"]);
                                    if (ratingCounts.ContainsKey(diem)) { ratingCounts[diem] = Convert.ToInt32(reader["Count"]); }
                                }
                            }
                        }
                    }
                }

                // Kiểm tra xem có dữ liệu không
                bool hasData = ratingCounts.Values.Sum() > 0;
                pnlChart.Visible = hasData; // Ẩn/hiện panel chứa biểu đồ

                if (hasData)
                {
                    var labels = ratingCounts.Keys.OrderBy(k => k).Select(k => $"{k} sao").ToList();
                    var data = ratingCounts.OrderBy(kv => kv.Key).Select(kv => kv.Value).ToList();

                    JavaScriptSerializer serializer = new JavaScriptSerializer();
                    string chartLabelsJson = serializer.Serialize(labels);
                    string chartDataJson = serializer.Serialize(data);

                    // === THAY ĐỔI QUAN TRỌNG: Chỉ đăng ký dữ liệu và gọi hàm render tĩnh ===
                    // Gán dữ liệu vào các biến JavaScript toàn cục (window)
                    // và sau đó gọi một hàm render đã được định nghĩa sẵn trên trang aspx.
                    string script = $@"
                window.chartLabels = {chartLabelsJson};
                window.chartData = {chartDataJson};
                if (document.readyState === 'complete') {{
                    renderRatingsChart();
                }} else {{
                    window.addEventListener('load', renderRatingsChart);
                }}";

                    ScriptManager.RegisterStartupScript(this, this.GetType(), "RatingChartDataScript", script, true);
                }
            }
            catch (Exception ex)
            {
                pnlChart.Visible = false;
                LogError("LoadRatingDistributionChartData Error: " + ex.Message);
                ShowAdminMessage("Lỗi tải dữ liệu biểu đồ.", true);
            }
        }
        #endregion

        #region GridView Binding and Actions
        private void BindGrid()
        {
            string searchTerm = CurrentUserBookFilter;
            int ratingFilter = CurrentRatingFilter;

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                StringBuilder queryBuilder = new StringBuilder(@"
                    SELECT dg.IDDanhGia, nd.Ten, nd.Username, s.TenSach, dg.Diem, dg.NhanXet, dg.NgayDanhGia
                    FROM DanhGiaSach dg
                    JOIN NguoiDung nd ON dg.IDNguoiDung = nd.IDNguoiDung
                    JOIN Sach s ON dg.IDSach = s.IDSach
                ");
                List<string> conditions = new List<string>();
                SqlCommand cmd = new SqlCommand();

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    conditions.Add("(nd.Ten LIKE @SearchTerm OR s.TenSach LIKE @SearchTerm OR nd.Username LIKE @SearchTerm)");
                    cmd.Parameters.AddWithValue("@SearchTerm", "%" + searchTerm.Trim() + "%");
                }
                if (ratingFilter > 0)
                {
                    conditions.Add("dg.Diem = @Rating");
                    cmd.Parameters.AddWithValue("@Rating", ratingFilter);
                }

                if (conditions.Any()) { queryBuilder.Append(" WHERE " + string.Join(" AND ", conditions)); }
                queryBuilder.Append(" ORDER BY dg.NgayDanhGia DESC");

                cmd.CommandText = queryBuilder.ToString();
                cmd.Connection = con;

                try
                {
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    gvReviews.DataSource = dt;
                    gvReviews.DataBind();
                }
                catch (Exception ex)
                {
                    ShowAdminMessage("Lỗi tải danh sách đánh giá: " + ex.Message, true);
                    LogError("BindGrid Error: " + ex.ToString());
                    gvReviews.DataSource = null;
                    gvReviews.DataBind();
                }
            }
        }

        protected void btnSearch_Click(object sender, EventArgs e)
        {
            CurrentUserBookFilter = txtSearchUserBook.Text.Trim();
            CurrentRatingFilter = Convert.ToInt32(ddlRatingFilter.SelectedValue);
            gvReviews.PageIndex = 0;
            BindGrid();
            ShowListPanels();
        }

        protected void btnReset_Click(object sender, EventArgs e)
        {
            txtSearchUserBook.Text = string.Empty;
            ddlRatingFilter.SelectedIndex = 0;
            CurrentUserBookFilter = string.Empty;
            CurrentRatingFilter = 0;
            gvReviews.PageIndex = 0;
            BindGrid();
            ShowListPanels();
        }

        protected void gvReviews_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            gvReviews.PageIndex = e.NewPageIndex;
            BindGrid();
            ShowListPanels();
        }

        protected void gvReviews_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                var lnkDelete = e.Row.FindControl("lnkDelete") as LinkButton;
                if (lnkDelete != null)
                {
                    string reviewId = DataBinder.Eval(e.Row.DataItem, "IDDanhGia").ToString();
                    string username = DataBinder.Eval(e.Row.DataItem, "Ten")?.ToString() ?? "N/A";
                    string bookTitle = DataBinder.Eval(e.Row.DataItem, "TenSach")?.ToString() ?? "N/A";
                    string comment = DataBinder.Eval(e.Row.DataItem, "NhanXet")?.ToString() ?? "";

                    string encodedUsername = HttpUtility.JavaScriptStringEncode(username);
                    string encodedBookTitle = HttpUtility.JavaScriptStringEncode(bookTitle);
                    string encodedComment = HttpUtility.JavaScriptStringEncode(comment);

                    lnkDelete.OnClientClick = $"showReviewDeleteConfirmation('{reviewId}', '{encodedUsername}', '{encodedBookTitle}', '{encodedComment}', '{lnkDelete.UniqueID}'); return false;";
                }
            }
        }

        protected void gvReviews_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "EditReview")
            {
                if (!int.TryParse(e.CommandArgument?.ToString(), out int reviewId) || reviewId <= 0)
                {
                    ShowAdminMessage("ID đánh giá không hợp lệ để sửa.", true);
                    return;
                }
                if (PopulateEditForm(reviewId))
                {
                    pnlReviewList.Visible = false;
                    pnlFilters.Visible = false;
                    pnlStatistics.Visible = false;
                    pnlEditReview.Visible = true;
                    ScriptManager.RegisterStartupScript(this, GetType(), "ShowEditPanelScript", "showEditPanelAnimated();", true);
                }
                else
                {
                    ShowListPanels();
                }
            }
            else if (e.CommandName == "CustomDelete")
            {
                if (!int.TryParse(e.CommandArgument?.ToString(), out int reviewId) || reviewId <= 0)
                {
                    ShowAdminMessage("ID đánh giá không hợp lệ để xóa.", true);
                    return;
                }
                try
                {
                    DeleteReview(reviewId);
                    LoadStatistics();
                    BindGrid();
                    ShowListPanels();
                }
                catch (Exception ex)
                {
                    ShowAdminMessage("Lỗi khi xóa đánh giá: " + ex.Message, true);
                    LogError($"Delete Review Error (ID: {reviewId}): {ex.ToString()}");
                    ShowListPanels();
                }
            }
        }

        private void DeleteReview(int reviewId)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "DELETE FROM DanhGiaSach WHERE IDDanhGia = @IDDanhGia";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@IDDanhGia", reviewId);
                    con.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected > 0) { ShowAdminMessage("Xóa đánh giá thành công.", false); }
                    else { ShowAdminMessage("Không tìm thấy đánh giá để xóa (ID: " + reviewId + ").", true); }
                }
            }
        }
        #endregion

        #region Edit Review Panel
        private bool PopulateEditForm(int reviewId)
        {
            bool success = false;
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"SELECT dg.IDDanhGia, nd.Ten, s.TenSach, dg.Diem, dg.NhanXet
                                 FROM DanhGiaSach dg
                                 JOIN NguoiDung nd ON dg.IDNguoiDung = nd.IDNguoiDung
                                 JOIN Sach s ON dg.IDSach = s.IDSach
                                 WHERE dg.IDDanhGia = @IDDanhGia";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@IDDanhGia", reviewId);
                    try
                    {
                        con.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                hfEditReviewId.Value = reviewId.ToString();
                                lblEditUser.Text = Server.HtmlEncode(reader["Ten"] != DBNull.Value ? reader["Ten"].ToString() : "N/A");
                                lblEditBook.Text = Server.HtmlEncode(reader["TenSach"] != DBNull.Value ? reader["TenSach"].ToString() : "N/A");
                                txtEditComment.Text = reader["NhanXet"] != DBNull.Value ? reader["NhanXet"].ToString() : string.Empty;

                                string diemStr = reader["Diem"] != DBNull.Value ? reader["Diem"].ToString() : null;
                                rblEditRating.ClearSelection();
                                ListItem itemToSelect = rblEditRating.Items.FindByValue(diemStr);
                                if (itemToSelect != null)
                                {
                                    itemToSelect.Selected = true;
                                }
                                success = true;
                            }
                            else
                            {
                                ShowAdminMessage("Không tìm thấy đánh giá để sửa (ID: " + reviewId + ").", true);
                                success = false;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ShowAdminMessage("Lỗi tải thông tin đánh giá để sửa: " + ex.Message, true);
                        LogError($"PopulateEditForm Error (ID: {reviewId}): {ex.ToString()}");
                        success = false;
                    }
                }
            }
            return success;
        }

        protected void btnSaveChanges_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
            {
                pnlEditReview.Visible = true; pnlReviewList.Visible = false; pnlFilters.Visible = false; pnlStatistics.Visible = false;
                ScriptManager.RegisterStartupScript(this, GetType(), "ShowEditPanelScript", "showEditPanelAnimated();", true);
                return;
            }

            if (!int.TryParse(hfEditReviewId.Value, out int reviewId) || reviewId <= 0)
            {
                ShowAdminMessage("ID đánh giá không hợp lệ để lưu.", true);
                pnlEditReview.Visible = true; pnlReviewList.Visible = false; pnlFilters.Visible = false; pnlStatistics.Visible = false;
                ScriptManager.RegisterStartupScript(this, GetType(), "ShowEditPanelScript", "showEditPanelAnimated();", true);
                return;
            }
            if (!int.TryParse(rblEditRating.SelectedValue, out int rating) || rating < 1 || rating > 5)
            {
                ShowAdminMessage("Điểm đánh giá không hợp lệ.", true);
                pnlEditReview.Visible = true; pnlReviewList.Visible = false; pnlFilters.Visible = false; pnlStatistics.Visible = false;
                ScriptManager.RegisterStartupScript(this, GetType(), "ShowEditPanelScript", "showEditPanelAnimated();", true);
                return;
            }
            string comment = txtEditComment.Text.Trim();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "UPDATE DanhGiaSach SET Diem = @Diem, NhanXet = @NhanXet WHERE IDDanhGia = @IDDanhGia";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Diem", rating);
                    cmd.Parameters.AddWithValue("@NhanXet", string.IsNullOrEmpty(comment) ? (object)DBNull.Value : comment);
                    cmd.Parameters.AddWithValue("@IDDanhGia", reviewId);
                    try
                    {
                        con.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            ShowAdminMessage("Cập nhật đánh giá thành công.", false);
                            LoadStatistics();
                            BindGrid();
                            ShowListPanels();
                        }
                        else
                        {
                            ShowAdminMessage("Không có thay đổi nào được lưu hoặc không tìm thấy đánh giá.", true);
                            pnlEditReview.Visible = true; pnlReviewList.Visible = false; pnlFilters.Visible = false; pnlStatistics.Visible = false;
                            ScriptManager.RegisterStartupScript(this, GetType(), "ShowEditPanelScript", "showEditPanelAnimated();", true);
                        }
                    }
                    catch (Exception ex)
                    {
                        ShowAdminMessage("Lỗi khi cập nhật đánh giá: " + ex.Message, true);
                        LogError($"Save Review Changes Error (ID: {reviewId}): {ex.ToString()}");
                        pnlEditReview.Visible = true; pnlReviewList.Visible = false; pnlFilters.Visible = false; pnlStatistics.Visible = false;
                        ScriptManager.RegisterStartupScript(this, GetType(), "ShowEditPanelScript", "showEditPanelAnimated();", true);
                    }
                }
            }
        }

        protected void btnCancelEdit_Click(object sender, EventArgs e)
        {
            ShowListPanels();
        }
        #endregion

        #region Helpers
        private void PopulateRatingFilterDropdown()
        {
            if (ddlRatingFilter.Items.Count == 0)
            {
                ddlRatingFilter.Items.Add(new ListItem("Tất cả điểm", "0"));
                for (int i = 5; i >= 1; i--) { ddlRatingFilter.Items.Add(new ListItem($"{i} sao", i.ToString())); }
            }
            if (ViewState["RatingFilter"] != null)
            {
                ddlRatingFilter.SelectedValue = ViewState["RatingFilter"].ToString();
            }
            if (ViewState["UserBookFilter"] != null)
            {
                txtSearchUserBook.Text = ViewState["UserBookFilter"].ToString();
            }
        }

        public string TruncateString(object inputObject, int maxLength)
        {
            if (inputObject == null || inputObject == DBNull.Value) return string.Empty;
            string input = inputObject.ToString();
            if (string.IsNullOrEmpty(input) || input.Length <= maxLength) return input;
            return input.Substring(0, maxLength).TrimEnd('.', ' ', ',', ';') + "...";
        }

        private void ShowAdminMessage(string message, bool isError)
        {
            lblAdminMessage.Text = Server.HtmlEncode(message);
            string cssClass = "block p-4 rounded-md border text-sm font-medium ";
            cssClass += isError
                ? "bg-red-100 border-red-300 text-red-800"
                : "bg-green-100 border-green-300 text-green-800";
            lblAdminMessage.CssClass = cssClass;
            pnlAdminMessage.Visible = true;
        }

        private void LogError(string message)
        {
            System.Diagnostics.Trace.TraceError("ADMIN_ERROR [QuanLyDanhGia]: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " - " + message);
        }

        public string GetStarRatingHtml(object ratingObj)
        {
            if (ratingObj == null || ratingObj == DBNull.Value || !int.TryParse(ratingObj.ToString(), out int rating)) return "<span class='text-gray-400 text-xs'>Chưa có</span>";
            rating = Math.Max(0, Math.Min(5, rating));
            StringBuilder stars = new StringBuilder("<span class='inline-block' title='" + rating + " sao'>");
            for (int i = 1; i <= 5; i++)
            {
                string starClass = (i <= rating) ? "fas fa-star text-yellow-400" : "far fa-star text-gray-300";
                stars.Append($"<i class='{starClass} mx-px'></i>");
            }
            stars.Append("</span>");
            return stars.ToString();
        }

        private void ShowListPanels()
        {
            pnlEditReview.Visible = false;
            pnlStatistics.Visible = true;
            pnlFilters.Visible = true;
            pnlReviewList.Visible = true;
        }
        #endregion
    }
}