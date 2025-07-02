using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Globalization; // Potentially needed for date/number formatting

namespace Webebook.WebForm.User
{
    public partial class chitietsach_chap : System.Web.UI.Page
    {
        // Ensure this matches the name in your Web.config
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["datawebebookConnectionString"]?.ConnectionString;
        private int userId = 0;
        private int sachId = 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Check connection string early
            if (string.IsNullOrEmpty(connectionString))
            {
                ShowMessage("Lỗi cấu hình hệ thống. Không thể kết nối cơ sở dữ liệu.", true);
                DisableContentPanels();
                return;
            }

            // Validate User Session
            if (Session["UserID"] == null || !int.TryParse(Session["UserID"].ToString(), out userId) || userId <= 0)
            {
                // Redirect to login, preserving the current URL
                string returnUrl = Server.UrlEncode(Request.Url.PathAndQuery);
                Response.Redirect(ResolveUrl("~/WebForm/VangLai/dangnhap.aspx") + "?returnUrl=" + returnUrl, false);
                Context.ApplicationInstance.CompleteRequest(); // Prevent further processing
                return;
            }

            // Validate Book ID QueryString
            if (!int.TryParse(Request.QueryString["IDSach"], out sachId) || sachId <= 0)
            {
                ShowMessage("ID Sách không hợp lệ hoặc không được cung cấp.", true);
                DisableContentPanels();
                hlBackToBookshelf.Visible = false; // Hide back button too if no valid book ID
                return;
            }

            if (!IsPostBack)
            {
                try
                {
                    LoadBookDetails();
                    // Only load dependent parts if book details loaded successfully
                    if (pnlBookDetails != null && pnlBookDetails.Visible)
                    {
                        LoadChapterList();
                        LoadContinueButton();
                        LoadBookComments();
                    }
                    // Always try to update cart count if logged in
                    UpdateMasterCartCount();
                }
                catch (Exception ex)
                {
                    // Catch unexpected errors during initial load sequence
                    ShowMessage("Đã xảy ra lỗi không mong muốn khi tải trang.", true);
                    DisableContentPanels();
                    // Log the detailed error for debugging
                    System.Diagnostics.Trace.TraceError($"Fatal Page_Load error (User, IDSach={sachId}, UserID={userId}): {ex}");
                }
            }

            // Hide message label on postback to prevent it showing stale messages
            if (IsPostBack && lblMessage != null && lblMessage.Visible)
            {
                lblMessage.Visible = false;
            }
        }

        private void LoadBookDetails()
        {
            // Defensive check for essential controls
            if (pnlBookDetails == null || lblTenSach == null || lblTacGia == null || lblMoTa == null ||
                imgBiaSach == null || rptGenres == null || lblNoGenres == null || lblLoaiSach == null ||
                lblNhaXuatBan == null || lblNhomDich == null || lblTrangThai == null)
            {
                LogErrorAndShowMessage("Lỗi giao diện: Thiếu control chi tiết sách.", "Giao diện trang bị lỗi. Vui lòng liên hệ quản trị viên.");
                DisableContentPanels();
                return;
            }

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                // Query optimized slightly, added ISNULL checks
                string query = @"SELECT
                                        s.TenSach, s.TacGia, s.MoTa, s.DuongDanBiaSach,
                                        ISNULL(s.LoaiSach, 'N/A') AS LoaiSach,
                                        ISNULL(s.NhaXuatBan, 'N/A') AS NhaXuatBan,
                                        ISNULL(s.NhomDich, 'N/A') AS NhomDich,
                                        ISNULL(s.TrangThaiNoiDung, 'Đang cập nhật') AS TrangThaiNoiDung,
                                        ISNULL(s.TheLoaiChuoi, '') AS TheLoaiChuoi
                                    FROM Sach s
                                    WHERE s.IDSach = @IDSach";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@IDSach", sachId);
                    try
                    {
                        con.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Populate controls, ensuring HTML encoding for safety
                                string tenSach = reader["TenSach"].ToString();
                                lblTenSach.Text = HttpUtility.HtmlEncode(tenSach);
                                Page.Title = "Chi tiết: " + tenSach; // Title doesn't need encoding here
                                lblTacGia.Text = HttpUtility.HtmlEncode(reader["TacGia"]?.ToString() ?? "Chưa cập nhật");

                                // Use helper for description formatting (includes safety)
                                lblMoTa.Text = FormatDescription(reader["MoTa"]?.ToString()); // Data goes here for JS/CSS to handle display

                                // Use helper for image URL (handles placeholders)
                                imgBiaSach.ImageUrl = GetImageUrl(reader["DuongDanBiaSach"]);
                                imgBiaSach.AlternateText = "Bìa sách " + HttpUtility.HtmlEncode(tenSach); // Use encoded title

                                lblLoaiSach.Text = HttpUtility.HtmlEncode(reader["LoaiSach"].ToString());
                                lblNhaXuatBan.Text = HttpUtility.HtmlEncode(reader["NhaXuatBan"].ToString());
                                lblNhomDich.Text = HttpUtility.HtmlEncode(reader["NhomDich"].ToString());
                                lblTrangThai.Text = HttpUtility.HtmlEncode(reader["TrangThaiNoiDung"].ToString());
                                // Set color based on status if needed (example)
                                // if (reader["TrangThaiNoiDung"].ToString().Equals("Hoàn thành", StringComparison.OrdinalIgnoreCase))
                                //     lblTrangThai.CssClass = "font-semibold text-blue-600 min-w-0 flex-1"; // Change class

                                // Process Genres
                                string genresString = reader["TheLoaiChuoi"].ToString();
                                List<string> genres = genresString.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                                                  .Select(g => HttpUtility.HtmlEncode(g.Trim())) // Encode each genre
                                                                  .Where(g => !string.IsNullOrEmpty(g))
                                                                  .ToList();

                                if (genres.Any())
                                {
                                    rptGenres.DataSource = genres;
                                    rptGenres.DataBind();
                                    rptGenres.Visible = true;
                                    lblNoGenres.Visible = false;
                                }
                                else
                                {
                                    rptGenres.Visible = false;
                                    lblNoGenres.Visible = true;
                                }

                                pnlBookDetails.Visible = true; // Show the panel now
                            }
                            else
                            {
                                ShowMessage("Không tìm thấy thông tin chi tiết cho sách này (ID: " + sachId + ").", true);
                                DisableContentPanels();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogErrorAndShowMessage($"Lỗi LoadBookDetails (User, IDSach={sachId}): {ex}", "Đã xảy ra lỗi khi tải chi tiết sách.");
                        DisableContentPanels();
                    }
                }
            }
        }

        private void LoadChapterList()
        {
            if (rptChapters == null || pnlChapterList == null || lblNoChapters == null)
            {
                LogErrorAndShowMessage("Lỗi giao diện: Thiếu control danh sách chương.", "Giao diện trang bị lỗi (phần chương).");
                return;
            }

            DataTable dt = new DataTable();
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                // Select necessary columns, order by chapter number
                string query = @"SELECT IDSach, SoChuong, TenChuong
                            FROM NoiDungSach
                            WHERE IDSach = @IDSach
                            ORDER BY SoChuong ASC";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@IDSach", sachId);
                    try
                    {
                        con.Open();
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        da.Fill(dt);

                        if (dt.Rows.Count > 0)
                        {
                            rptChapters.DataSource = dt;
                            rptChapters.DataBind();
                            pnlChapterList.Visible = true;
                            lblNoChapters.Visible = false;
                        }
                        else
                        {
                            // Ensure repeater is cleared if no data
                            rptChapters.DataSource = null;
                            rptChapters.DataBind();
                            pnlChapterList.Visible = false;
                            lblNoChapters.Visible = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        LogErrorAndShowMessage($"Lỗi LoadChapterList (User, IDSach={sachId}): {ex}", "Lỗi khi tải danh sách chương.");
                        pnlChapterList.Visible = false; // Ensure panel is hidden on error
                        lblNoChapters.Visible = true; // Show no chapters message on error
                    }
                }
            }
        }

        private void LoadContinueButton()
        {
            if (hlReadContinue == null)
            {
                LogErrorAndShowMessage("Lỗi giao diện: Thiếu control nút đọc tiếp.", "Giao diện trang bị lỗi (nút đọc).");
                return;
            }

            string viTriDoc = null; // Last read chapter number as string
            int totalChapters = 0;
            int firstChapter = 0; // First available chapter number

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                try
                {
                    con.Open();
                    // Get last read position for this user/book
                    string queryViTriDoc = "SELECT ViTriDoc FROM TuSach WHERE IDNguoiDung = @UserId AND IDSach = @IDSach";
                    using (SqlCommand cmdViTri = new SqlCommand(queryViTriDoc, con))
                    {
                        cmdViTri.Parameters.AddWithValue("@UserId", userId);
                        cmdViTri.Parameters.AddWithValue("@IDSach", sachId);
                        object resultViTri = cmdViTri.ExecuteScalar();
                        // Check for valid, non-zero position
                        if (resultViTri != null && resultViTri != DBNull.Value && !string.IsNullOrWhiteSpace(resultViTri.ToString()) && resultViTri.ToString() != "0")
                        {
                            viTriDoc = resultViTri.ToString();
                        }
                    }

                    // Get total chapter count and the first chapter number in one query if possible
                    string queryCounts = @"SELECT COUNT(DISTINCT SoChuong), MIN(SoChuong)
                                        FROM NoiDungSach WHERE IDSach = @IDSach";
                    using (SqlCommand cmdCounts = new SqlCommand(queryCounts, con))
                    {
                        cmdCounts.Parameters.AddWithValue("@IDSach", sachId);
                        using (SqlDataReader reader = cmdCounts.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                if (reader[0] != DBNull.Value) totalChapters = Convert.ToInt32(reader[0]);
                                if (reader[1] != DBNull.Value) firstChapter = Convert.ToInt32(reader[1]);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogErrorAndShowMessage($"Lỗi LoadContinueButton (User, IDSach={sachId}, UserID={userId}): {ex}", "Lỗi khi kiểm tra tiến độ đọc.");
                    hlReadContinue.Visible = false; // Hide button on error
                    return;
                }
            }

            string buttonText = "";
            string navigateUrl = "#"; // Default URL if disabled
            bool enabled = false;
            string cssClass = hlReadContinue.CssClass.Replace(" disabled", "").Trim(); // Base CSS

            if (totalChapters == 0 || firstChapter == 0) // No content or couldn't find first chapter
            {
                buttonText = "<i class='fas fa-book mr-2'></i> Chưa có nội dung";
                enabled = false;
                cssClass += " disabled";
            }
            else if (string.IsNullOrEmpty(viTriDoc)) // Never read or reset progress
            {
                buttonText = $"<i class='fas fa-book-open mr-2'></i> Bắt đầu đọc (Chương {firstChapter})";
                navigateUrl = ResolveUrl($"~/WebForm/User/docsach.aspx?IDSach={sachId}&SoChuong={firstChapter}");
                enabled = true;
            }
            else // Has reading progress
            {
                buttonText = $"<i class='fas fa-play mr-2'></i> Tiếp tục đọc (Chương {HttpUtility.HtmlEncode(viTriDoc)})";
                // Ensure URL uses the stored ViTriDoc
                navigateUrl = ResolveUrl($"~/WebForm/User/docsach.aspx?IDSach={sachId}&SoChuong={HttpUtility.UrlEncode(viTriDoc)}");
                enabled = true;
            }

            hlReadContinue.Text = buttonText;
            hlReadContinue.NavigateUrl = navigateUrl;
            // Note: Setting Enabled=false on HyperLink doesn't prevent navigation visually like CSS disabled class does.
            // The CSS class 'disabled' handles the visual and interaction blocking.
            // hlReadContinue.Enabled = enabled; // Can be set, but CSS is more effective for UI
            hlReadContinue.CssClass = cssClass; // Apply base + 'disabled' if needed
            hlReadContinue.Visible = true; // Make the button visible
        }

        private void LoadBookComments()
        {
            if (rptBookComments == null || lblNoBookComments == null)
            {
                LogErrorAndShowMessage("Lỗi giao diện: Thiếu control bình luận.", "Giao diện trang bị lỗi (phần bình luận).");
                return;
            }

            DataTable dtComments = new DataTable();
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                // Get recent comments with user display name and avatar
                string query = @"SELECT TOP 20 -- Limit number of comments
                                        t.IDNguoiDung, t.SoChap, t.BinhLuan, t.NgayBinhLuan,
                                        -- Prioritize display name (Ten), fallback to Username
                                        ISNULL(nd.Ten, nd.Username) AS TenHienThi,
                                        nd.AnhNen -- Avatar data
                                    FROM TuongTac t
                                    LEFT JOIN NguoiDung nd ON t.IDNguoiDung = nd.IDNguoiDung
                                    WHERE t.IDSach = @IDSach
                                    AND t.BinhLuan IS NOT NULL AND LTRIM(RTRIM(t.BinhLuan)) <> '' -- Ensure comment is not empty
                                    ORDER BY t.NgayBinhLuan DESC"; // Newest first

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@IDSach", sachId);
                    try
                    {
                        con.Open();
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        da.Fill(dtComments);

                        rptBookComments.DataSource = dtComments;
                        rptBookComments.DataBind();
                        // Show/hide the 'no comments' label based on results
                        lblNoBookComments.Visible = (dtComments.Rows.Count == 0);
                    }
                    catch (Exception ex)
                    {
                        LogErrorAndShowMessage($"Lỗi LoadBookComments (User, IDSach={sachId}): {ex}", "Lỗi khi tải bình luận.");
                        lblNoBookComments.Visible = true; // Assume no comments if error occurs
                    }
                }
            }
        }

        // --- Helper Functions ---

        protected string GetImageUrl(object pathData)
        {
            // Provides a placeholder if image path is missing or invalid
            string placeholder = ResolveUrl("~/Images/placeholder_cover.png"); // Define placeholder path
            if (pathData != DBNull.Value && pathData != null)
            {
                string path = pathData.ToString();
                if (!string.IsNullOrWhiteSpace(path))
                {
                    // Check if it's a relative path needing ResolveUrl
                    if (path.StartsWith("~") || path.StartsWith("/"))
                    {
                        try { return ResolveUrl(path); }
                        catch (HttpException hex) // Catch errors resolving URL
                        {
                            System.Diagnostics.Trace.TraceWarning($"Invalid image path format '{path}': {hex.Message}");
                            return placeholder;
                        }
                        catch (ArgumentException aex) // Catch other potential path errors
                        {
                            System.Diagnostics.Trace.TraceWarning($"Argument error resolving image path '{path}': {aex.Message}");
                            return placeholder;
                        }
                    }
                    // Assume it's a full URL if it doesn't start with ~ or /
                    // Basic check for http/https, could be more robust
                    else if (path.StartsWith("http://") || path.StartsWith("https://"))
                    {
                        return path;
                    }
                    // If it's just a filename maybe prepend a base path? (Adjust if needed)
                    // else { return ResolveUrl("~/Uploads/Images/" + path); }
                }
            }
            // Return placeholder if path is null, empty, whitespace, or invalid
            return placeholder;
        }

        protected string FormatDescription(string description)
        {
            // Formats description text for display, ensuring safety
            if (string.IsNullOrWhiteSpace(description))
            {
                // Return empty or a placeholder. The 'prose' class handles empty elements gracefully.
                // Adding explicit placeholder might be better UX.
                return "<p class='italic text-gray-500'>Chưa có mô tả cho sách này.</p>";
            }
            // Encode the raw description to prevent XSS
            string encodedDesc = HttpUtility.HtmlEncode(description);
            // Replace encoded newlines with <br /> tags for display
            // This is kept from the original code. If 'prose' causes issues with <br>,
            // you might remove this replacement and let 'prose' handle paragraphs/whitespace.
            string formattedDesc = encodedDesc.Replace("\r\n", "<br />").Replace("\n", "<br />");
            return formattedDesc;
        }

        protected string GetAvatarUrl(object anhNenData)
        {
            // Generates a Data URI for byte array avatars or returns a default image URL
            string defaultAvatar = ResolveUrl("~/Images/default_avatar.png"); // Define default avatar path
            if (anhNenData != DBNull.Value && anhNenData is byte[] bytes && bytes.Length > 0)
            {
                try
                {
                    // Determine MIME type (simple check, could be improved with magic byte detection)
                    string mimeType = "image/png"; // Default assumption
                    if (bytes.Length > 3 && bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF) mimeType = "image/jpeg";
                    else if (bytes.Length > 3 && bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E) mimeType = "image/png";
                    else if (bytes.Length > 5 && bytes[0] == 0x47 && bytes[1] == 0x49 && bytes[2] == 0x46) mimeType = "image/gif";
                    // etc. for other types if needed

                    return $"data:{mimeType};base64," + Convert.ToBase64String(bytes);
                }
                catch (FormatException fex) // Catch Base64 conversion errors
                {
                    System.Diagnostics.Trace.TraceError($"Base64 conversion error for avatar: {fex.Message}");
                    return defaultAvatar;
                }
            }
            // Return default if data is null, not byte[], or empty
            return defaultAvatar;
        }

        protected string FormatCommentText(object binhLuanData)
        {
            // Formats comment text safely for display within a 'prose' or pre-wrap container
            if (binhLuanData != DBNull.Value && binhLuanData != null)
            {
                // Encode HTML entities first to prevent XSS
                string encodedComment = HttpUtility.HtmlEncode(binhLuanData.ToString());
                // Let 'prose' class handle newline rendering. No need to replace with <br>.
                return encodedComment;
            }
            return string.Empty; // Return empty if comment is null/DBNull
        }

        protected string FormatRelativeTime(object ngayBinhLuanObj)
        {
            // Converts a DateTime object to a user-friendly relative time string
            if (ngayBinhLuanObj == DBNull.Value || ngayBinhLuanObj == null)
            {
                return string.Empty;
            }

            try
            {
                DateTime ngayBinhLuan = Convert.ToDateTime(ngayBinhLuanObj);
                TimeSpan timeDifference = DateTime.Now.Subtract(ngayBinhLuan);

                // Use thresholds for relative time display
                if (timeDifference.TotalSeconds < 60) return "vài giây trước";
                if (timeDifference.TotalMinutes < 60) return $"{(int)timeDifference.TotalMinutes} phút trước";
                if (timeDifference.TotalHours < 24) return $"{(int)timeDifference.TotalHours} giờ trước";
                if (timeDifference.TotalDays < 7) return $"{(int)timeDifference.TotalDays} ngày trước";
                if (timeDifference.TotalDays < 30) return $"{(int)(timeDifference.TotalDays / 7)} tuần trước";
                if (timeDifference.TotalDays < 365) return $"{(int)(timeDifference.TotalDays / 30)} tháng trước"; // Approximation

                // Fallback to specific date for older comments
                return ngayBinhLuan.ToString("dd/MM/yyyy");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceError($"Error formatting relative time for {ngayBinhLuanObj}: {ex.Message}");
                return "không xác định"; // Return placeholder on error
            }
        }

        private void ShowMessage(string message, bool isError)
        {
            // Displays a styled message to the user
            if (lblMessage == null) return; // Safety check

            // Base classes + margin
            string cssClass = "mb-6 p-4 rounded-md text-sm font-medium border "; // Added border base
            if (isError)
            {
                cssClass += "bg-red-50 border-red-300 text-red-700"; // Red alert styles
            }
            else
            {
                cssClass += "bg-green-50 border-green-300 text-green-700"; // Green success styles
            }

            lblMessage.Text = HttpUtility.HtmlEncode(message); // Always encode user message
            lblMessage.CssClass = cssClass;
            lblMessage.Visible = true;
        }

        // Logs error details and shows a user-friendly message
        private void LogErrorAndShowMessage(string detailedLogMessage, string userMessage)
        {
            System.Diagnostics.Trace.TraceError(detailedLogMessage); // Log detailed error
            ShowMessage(userMessage, true); // Show generic error to user
        }

        // Helper to hide main content panels, typically used on error
        private void DisableContentPanels()
        {
            if (pnlBookDetails != null) pnlBookDetails.Visible = false;
            // Add other panels here if needed
        }

        private void UpdateMasterCartCount()
        {
            // Updates the cart count displayed in the Master Page
            try
            {
                // Ensure the correct Master Page type name is used
                // IMPORTANT: Replace 'UserMaster' with the actual class name of your master page if different
                var master = Master as Webebook.WebForm.User.UserMaster;
                master?.UpdateCartCount(); // Use null-conditional operator
            }
            catch (Exception ex)
            {
                // Log error if Master Page access fails
                System.Diagnostics.Trace.TraceWarning($"Error accessing Master Page for cart count update: {ex.Message}");
            }
        }
    }
}