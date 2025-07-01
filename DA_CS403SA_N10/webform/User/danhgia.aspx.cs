using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Diagnostics;

namespace Webebook.WebForm.User
{
    public partial class danhgia : System.Web.UI.Page
    {
        // --- Khai báo biến / Hằng số ---
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["datawebebookConnectionString"]?.ConnectionString;
        private int _userId = 0;
        private int _bookId = 0;
        private int _orderId = 0;
        private const string RatingGroupName = "bookRating"; // Tên cố định

        // --- Helper tìm control (Giữ nguyên) ---
        private T FindControlRecursive<T>(Control rootControl, string controlID) where T : Control { if (rootControl == null) return null; if (rootControl.ID == controlID && rootControl is T) return (T)rootControl; foreach (Control controlToSearch in rootControl.Controls) { T controlToReturn = FindControlRecursive<T>(controlToSearch, controlID); if (controlToReturn != null) return controlToReturn; } return null; }
        private Button GetSubmitButton() => FindControlRecursive<Button>(this.Page, "btnSubmitReview");
        private Panel GetReviewPanel() => FindControlRecursive<Panel>(this.Page, "pnlReviewForm");
        private Label GetMessageLabel() => FindControlRecursive<Label>(this.Page, "lblMessage");
        private TextBox GetCommentTextBox() => FindControlRecursive<TextBox>(this.Page, "txtComment");
        private Image GetBookCoverImage() => FindControlRecursive<Image>(this.Page, "imgBookCover");
        private Label GetBookTitleLabel() => FindControlRecursive<Label>(this.Page, "lblBookTitle");
        private HyperLink GetBackLink() => FindControlRecursive<HyperLink>(this.Page, "hlBack");

        // --- Page Load (Giữ nguyên) ---
        protected void Page_Load(object sender, EventArgs e)
        { /*...*/
            if (string.IsNullOrEmpty(_connectionString)) { ShowMessage("Lỗi cấu hình hệ thống.", true); LogError("Connection string missing."); DisableForm(); return; }
            if (!AuthenticateAndGetIds()) return;
            if (!IsPostBack)
            {
                LoadBookInfo();
                Panel reviewPanel = GetReviewPanel();
                if (reviewPanel != null && reviewPanel.Visible) { CheckIfCanReview(); }
            }
        }
        // --- Xác thực và lấy IDs (Giữ nguyên) ---
        private bool AuthenticateAndGetIds()
        { /*...*/
            if (Session["UserID"] == null || !int.TryParse(Session["UserID"].ToString(), out _userId) || _userId <= 0) { Response.Redirect(ResolveUrl("~/WebForm/VangLai/dangnhap.aspx") + "?returnUrl=" + Server.UrlEncode(Request.Url.PathAndQuery), false); Context.ApplicationInstance.CompleteRequest(); return false; }
            if (!int.TryParse(Request.QueryString["IDSach"], out _bookId) || _bookId <= 0) { ShowMessage("ID sách không hợp lệ.", true); DisableForm(); return false; }
            int.TryParse(Request.QueryString["orderId"], out _orderId); return true;
        }
        // --- Tải thông tin sách (Giữ nguyên) ---
        private void LoadBookInfo()
        { /*...*/
            Panel reviewPanel = GetReviewPanel(); if (_bookId <= 0 || reviewPanel == null) return;
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                const string query = "SELECT TenSach, DuongDanBiaSach FROM Sach WHERE IDSach = @IDSach";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@IDSach", this._bookId);
                    try
                    {
                        con.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                Label titleLabel = GetBookTitleLabel(); Image coverImage = GetBookCoverImage(); string tenSach = reader["TenSach"]?.ToString() ?? "[Không có tên]";
                                if (titleLabel != null) titleLabel.Text = Server.HtmlEncode(tenSach);
                                if (coverImage != null) { coverImage.ImageUrl = GetBookImageUrl(reader["DuongDanBiaSach"]); coverImage.AlternateText = "Bìa sách: " + Server.HtmlEncode(tenSach); }
                                reviewPanel.Visible = true;
                            }
                            else { ShowMessage("Không tìm thấy thông tin sách (ID: " + _bookId + ").", true); reviewPanel.Visible = false; }
                        }
                    }
                    catch (Exception ex) { ShowMessage("Lỗi khi tải thông tin sách.", true); LogError($"Load Book Info Error (BookID: {_bookId}): {ex.ToString()}"); reviewPanel.Visible = false; }
                }
            }
        }
        // --- Kiểm tra quyền đánh giá (Giữ nguyên) ---
        private bool CheckIfCanReview()
        { /*...*/
            Button submitButton = GetSubmitButton(); Panel reviewPanel = GetReviewPanel(); HyperLink backLink = GetBackLink();
            if (reviewPanel == null || submitButton == null) { LogError("CheckIfCanReview Error: Required controls not found."); if (reviewPanel != null) reviewPanel.Visible = false; return false; }
            if (_userId <= 0 || _bookId <= 0) { reviewPanel.Visible = false; submitButton.Enabled = false; return false; }
            bool canPurchase = false; bool alreadyReviewed = false;
            try
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    con.Open();
                    const string checkPurchaseQuery = @"SELECT CASE WHEN EXISTS (SELECT 1 FROM DonHang dh JOIN ChiTietDonHang ctdh ON dh.IDDonHang = ctdh.IDDonHang WHERE dh.IDNguoiDung = @UserId AND ctdh.IDSach = @IDSach AND dh.TrangThaiThanhToan IN ('Completed', 'Paid', N'Đã thanh toán', N'Hoàn thành')) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END";
                    using (SqlCommand cmdCheckPurchase = new SqlCommand(checkPurchaseQuery, con)) { cmdCheckPurchase.Parameters.AddWithValue("@UserId", _userId); cmdCheckPurchase.Parameters.AddWithValue("@IDSach", _bookId); canPurchase = (bool)cmdCheckPurchase.ExecuteScalar(); }
                    if (canPurchase)
                    {
                        const string checkExistingReviewQuery = @"SELECT CASE WHEN EXISTS (SELECT 1 FROM DanhGiaSach WHERE IDNguoiDung = @UserId AND IDSach = @IDSach) THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END";
                        using (SqlCommand cmdCheckExist = new SqlCommand(checkExistingReviewQuery, con)) { cmdCheckExist.Parameters.AddWithValue("@UserId", _userId); cmdCheckExist.Parameters.AddWithValue("@IDSach", _bookId); alreadyReviewed = (bool)cmdCheckExist.ExecuteScalar(); }
                    }
                }
            }
            catch (Exception ex) { ShowMessage("Xảy ra lỗi khi kiểm tra quyền đánh giá.", true); LogError($"Check Can Review Error (BookID: {_bookId}, UserID: {_userId}): {ex.ToString()}"); reviewPanel.Visible = false; submitButton.Enabled = false; return false; }
            if (alreadyReviewed) { ShowMessage("Bạn đã gửi đánh giá cho cuốn sách này rồi. Cảm ơn!", false); reviewPanel.Visible = false; submitButton.Enabled = false; if (backLink != null) backLink.Text = "<i class=\"fas fa-arrow-left mr-1\"></i> Quay lại"; return false; }
            else if (!canPurchase) { ShowMessage("Bạn cần mua (và đơn hàng đã hoàn thành) sách này trước khi đánh giá.", true); reviewPanel.Visible = false; submitButton.Enabled = false; return false; }
            else { reviewPanel.Visible = true; submitButton.Enabled = true; Label msgLabel = GetMessageLabel(); if (msgLabel != null) msgLabel.Visible = false; return true; }
        }

        // --- Server-Side Validation Handler (Giữ nguyên) ---
        protected void cvRating_ServerValidate(object source, ServerValidateEventArgs args)
        {
            string rawValue = Request.Form[RatingGroupName]; // Dùng tên cố định
            Debug.WriteLine($"[Server Validation] Raw value from Request.Form[\"{RatingGroupName}\"]: '{rawValue}'");
            int rating = 0;
            bool parsedSuccessfully = int.TryParse(rawValue, out rating);
            args.IsValid = !string.IsNullOrEmpty(rawValue) && parsedSuccessfully && rating >= 1 && rating <= 5;
            string ratingStringForLog = parsedSuccessfully ? rating.ToString() : "[TryParse Failed]";
            Debug.WriteLine($"[Server Validation] Parsed Int: {ratingStringForLog}, IsValid set to: {args.IsValid}");
        }

        // --- Submit Button Click Handler (Giữ nguyên) ---
        protected void btnSubmitReview_Click(object sender, EventArgs e)
        { /*...*/
            Panel reviewPanel = GetReviewPanel(); Button submitButton = GetSubmitButton();
            if (!CheckIfCanReview()) { return; }
            bool isFormVisible = (reviewPanel != null && reviewPanel.Visible); bool isButtonEnabled = (submitButton != null && submitButton.Enabled);
            if (!isFormVisible || !isButtonEnabled) { LogError("Submit prevented: invalid control state."); return; }

            Page.Validate("ReviewGroup");
            if (!Page.IsValid) { Debug.WriteLine("Submit stopped: Page.IsValid is false."); return; }

            try
            {
                int rating = int.Parse(Request.Form[RatingGroupName]); // Dùng tên cố định
                TextBox commentBox = GetCommentTextBox(); string comment = (commentBox != null) ? commentBox.Text.Trim() : string.Empty;

                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    const string query = @"INSERT INTO DanhGiaSach (IDNguoiDung, IDSach, Diem, NhanXet, NgayDanhGia) VALUES (@IDNguoiDung, @IDSach, @Diem, @NhanXet, @NgayDanhGia)";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@IDNguoiDung", _userId); cmd.Parameters.AddWithValue("@IDSach", _bookId); cmd.Parameters.AddWithValue("@Diem", rating); cmd.Parameters.AddWithValue("@NhanXet", string.IsNullOrEmpty(comment) ? (object)DBNull.Value : comment); cmd.Parameters.AddWithValue("@NgayDanhGia", DateTime.Now);
                        con.Open(); int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0) { ShowMessage("Đánh giá của bạn đã được gửi thành công. Xin cảm ơn!", false); if (reviewPanel != null) reviewPanel.Visible = false; }
                        else { ShowMessage("Không thể lưu đánh giá. Vui lòng thử lại.", true); LogError($"Submit Review Warning: 0 rows affected (BookID: {_bookId}, UserID: {_userId})"); }
                    }
                }
            }
            catch (SqlException sqlEx) when (sqlEx.Number == 2627 || sqlEx.Number == 2601) { ShowMessage("Lỗi: Bạn đã gửi đánh giá cho sách này rồi.", false); if (reviewPanel != null) reviewPanel.Visible = false; LogError($"Submit Review - Unique Constraint (BookID: {_bookId}, UserID: {_userId}): {sqlEx.Message}"); }
            catch (Exception ex) { ShowMessage("Đã có lỗi xảy ra khi gửi đánh giá.", true); LogError($"Submit Review Error (BookID: {_bookId}, UserID: {_userId}): {ex.ToString()}"); }
        }

        // --- Helper Methods ---
        private void ShowMessage(string message, bool isError)
        {
            Label msgLabel = GetMessageLabel();
            if (msgLabel == null) { LogError("ShowMessage Error: lblMessage control not found."); return; }
            msgLabel.CssClass = isError ? "message-error" : "message-success";
            msgLabel.Text = Server.HtmlEncode(message);
            msgLabel.Visible = true;
        }
        private void LogError(string message) { System.Diagnostics.Debug.WriteLine("ERROR: " + DateTime.Now.ToString("u") + " - " + message); }
        protected string GetBookImageUrl(object imageUrl)
        { /* ... giữ nguyên ... */
            string url = imageUrl?.ToString();
            string placeholderUrl = "~/Content/Images/BookCovers/placeholder_cover.png"; // Cập nhật nếu cần
            if (string.IsNullOrWhiteSpace(url)) { return ResolveUrl(placeholderUrl); }
            if (url.StartsWith("http", StringComparison.OrdinalIgnoreCase) || url.StartsWith("/")) { return url; }
            if (url.StartsWith("~/")) { return ResolveUrl(url); }
            return url;
        }
        private void DisableForm()
        { // Vô hiệu hóa form
            Panel reviewPanel = GetReviewPanel(); if (reviewPanel != null) reviewPanel.Visible = false;
            Button submitButton = GetSubmitButton(); if (submitButton != null) submitButton.Enabled = false;
            HyperLink backLink = GetBackLink(); if (backLink != null) backLink.Visible = true;
        }
    }
}