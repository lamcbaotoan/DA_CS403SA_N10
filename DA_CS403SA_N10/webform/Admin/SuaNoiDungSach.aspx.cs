using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Webebook.WebForm.Admin
{
    public partial class SuaNoiDungSach : System.Web.UI.Page
    {
        // Ensure the connection string name matches your Web.config
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["datawebebookConnectionString"]?.ConnectionString;
        private int _sachId;
        private string _loaiSach;
        // Define base path relative to the web application root
        private const string BookContentBasePath = "~/BookContent/";

        protected void Page_Load(object sender, EventArgs e)
        {
            // Check connection string early
            if (string.IsNullOrEmpty(_connectionString))
            {
                ShowMessage("Lỗi cấu hình: Không tìm thấy chuỗi kết nối 'datawebebookConnectionString'.", true);
                // Disable controls that require DB access if needed
                btnAddNewChapter.Enabled = false;
                gvContent.Visible = false;
                return;
            }

            // Initialize essential page data (SachID)
            if (!InitializePageData())
            {
                // Initialization failed (e.g., invalid ID, book not found), message already shown.
                // Disable controls or redirect if necessary
                btnAddNewChapter.Enabled = false;
                gvContent.Visible = false;
                return;
            }

            if (!IsPostBack)
            {
                // Load book context details and bind the grid only on initial load
                if (LoadBookContext(_sachId))
                {
                    BindContentGrid();
                }
                else
                {
                    // Book not found, disable controls
                    btnAddNewChapter.Enabled = false;
                    gvContent.Visible = false;
                }
            }
            // _sachId is now available for postback events like button clicks or grid commands
        }

        // Attempts to get SachID from QueryString or HiddenField
        private bool InitializePageData()
        {
            // Try getting ID from QueryString first, then fallback to HiddenField (for postbacks)
            if (!int.TryParse(Request.QueryString["id"] ?? hfSachID.Value, out _sachId) || _sachId <= 0)
            {
                // If even the hidden field is invalid/empty on postback, redirect.
                if (!string.IsNullOrEmpty(Request.QueryString["id"]))
                    ShowMessage("ID sách không hợp lệ được cung cấp.", true);
                else
                    Session["AdminMessage"] = "Không thể xác định ID sách. Vui lòng thử lại."; // Use Session for message across redirect

                Response.Redirect("QuanLySach.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
                return false;
            }

            // Store valid SachID in hidden field for postbacks
            hfSachID.Value = _sachId.ToString();
            return true; // Sach ID is valid, proceed to load context
        }

        // Loads Book Title and Type into header labels and fields
        private bool LoadBookContext(int sachId)
        {
            using (var con = new SqlConnection(_connectionString))
            {
                var cmd = new SqlCommand("SELECT TenSach, LoaiSach FROM Sach WHERE IDSach = @IDSach", con);
                cmd.Parameters.AddWithValue("@IDSach", sachId);
                try
                {
                    con.Open();
                    using (var reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            lblBookTitleContext.Text = HttpUtility.HtmlEncode(reader["TenSach"].ToString()); // Encode output
                            lblSachIDContext.Text = sachId.ToString();
                            _loaiSach = reader["LoaiSach"].ToString();
                            lblLoaiSachContext.Text = HttpUtility.HtmlEncode(_loaiSach); // Encode output
                            hfLoaiSach.Value = _loaiSach; // Store LoaiSach for potential use

                            // Set Master Page Title if Master is of correct type
                            if (Master is Admin masterPage)
                            {
                                masterPage.SetPageTitle($"Quản Lý Nội Dung Sách: {HttpUtility.HtmlEncode(reader["TenSach"].ToString())} (ID: {sachId})");
                            }
                            else // Handle case where Master is not the expected type or null
                            {
                                Page.Title = $"Quản Lý Nội Dung Sách: {HttpUtility.HtmlEncode(reader["TenSach"].ToString())}";
                            }
                            return true;
                        }
                        else
                        {
                            ShowMessage($"Không tìm thấy sách với ID {sachId}.", true);
                            return false;
                        }
                    }
                }
                catch (SqlException ex)
                {
                    ShowMessage($"Lỗi cơ sở dữ liệu khi tải thông tin sách: {ex.Message}", true);
                    // Log the exception details here (ex.ToString())
                    return false;
                }
                catch (Exception ex) // Catch other potential exceptions
                {
                    ShowMessage($"Lỗi không xác định khi tải thông tin sách: {ex.Message}", true);
                    // Log the exception details here (ex.ToString())
                    return false;
                }
            }
        }

        // Binds the GridView with chapter data
        private void BindContentGrid()
        {
            using (var con = new SqlConnection(_connectionString))
            {
                // Select necessary columns for display and operations
                var cmd = new SqlCommand("SELECT IDNoiDung, SoChuong, TenChuong, LoaiNoiDung, DuongDan FROM NoiDungSach WHERE IDSach = @IDSach ORDER BY SoChuong ASC", con);
                cmd.Parameters.AddWithValue("@IDSach", _sachId);
                var dt = new DataTable();
                try
                {
                    con.Open();
                    new SqlDataAdapter(cmd).Fill(dt);
                    gvContent.DataSource = dt;
                    gvContent.DataBind();
                }
                catch (SqlException ex)
                {
                    ShowMessage($"Lỗi cơ sở dữ liệu khi tải danh sách chương: {ex.Message}", true);
                    // Log the exception details here (ex.ToString())
                    gvContent.DataSource = null; // Clear datasource on error
                    gvContent.DataBind();
                }
                catch (Exception ex) // Catch other potential exceptions
                {
                    ShowMessage($"Lỗi không xác định khi tải danh sách chương: {ex.Message}", true);
                    // Log the exception details here (ex.ToString())
                    gvContent.DataSource = null; // Clear datasource on error
                    gvContent.DataBind();
                }
            }
        }

        protected void gvContent_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                var drv = (DataRowView)e.Row.DataItem;
                var pnlImages = (Panel)e.Row.FindControl("pnlImages");
                var rptImages = (Repeater)e.Row.FindControl("rptChapterImages"); // Giữ lại cho logic ảnh
                var litMore = (Literal)e.Row.FindControl("litMoreImagesIndicator"); // Giữ lại cho logic ảnh
                var lblError = (Label)e.Row.FindControl("lblContentError");
                // Sử dụng lblContentText để hiển thị tên file cho truyện chữ/file
                var lblText = (Label)e.Row.FindControl("lblContentText");

                // Kiểm tra các control có tồn tại không
                if (pnlImages == null || rptImages == null || litMore == null || lblError == null || lblText == null)
                {
                    // Ghi log lỗi nếu cần
                    return;
                }

                // Đặt lại trạng thái hiển thị ban đầu cho tất cả các control nội dung
                pnlImages.Visible = false;
                lblError.Visible = false;
                lblText.Visible = false; // Mặc định ẩn
                litMore.Visible = false; // Mặc định ẩn

                string loaiNoiDung = drv["LoaiNoiDung"]?.ToString();
                string duongDan = drv["DuongDan"]?.ToString();

                if (loaiNoiDung == "Image")
                {
                    // --- Logic hiển thị ảnh (Giữ nguyên như cũ) ---
                    if (!string.IsNullOrWhiteSpace(duongDan))
                    {
                        var imagePaths = duongDan.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                                .Select(p => p.Trim())
                                                .Where(p => !string.IsNullOrEmpty(p))
                                                .ToList();

                        if (imagePaths.Any())
                        {
                            const int maxImagesToShow = 4;
                            pnlImages.Visible = true; // Hiện panel ảnh
                            rptImages.DataSource = imagePaths.Take(maxImagesToShow);
                            rptImages.DataBind();

                            if (imagePaths.Count > maxImagesToShow)
                            {
                                litMore.Visible = true;
                                litMore.Text = $" (+{imagePaths.Count - maxImagesToShow})";
                            }
                        }
                        else
                        {
                            lblError.Text = "[Lỗi: Định dạng đường dẫn ảnh không hợp lệ]";
                            lblError.Visible = true;
                        }
                    }
                    else
                    {
                        lblError.Text = "[Chưa có ảnh cho chương này]";
                        lblError.Visible = true;
                    }
                    // --- Kết thúc logic hiển thị ảnh ---
                }
                // --- BEGIN SỬA ĐỔI: Xử lý cho Truyện Chữ/File ---
                else if (loaiNoiDung == "File" || loaiNoiDung == "Text") // Xử lý cả loại 'File' và 'Text' nếu có đường dẫn
                {
                    if (!string.IsNullOrWhiteSpace(duongDan))
                    {
                        try
                        {
                            // Lấy tên file từ đường dẫn
                            string fileName = Path.GetFileName(duongDan);
                            lblText.Text = HttpUtility.HtmlEncode(fileName); // Hiển thị tên file đã mã hóa HTML
                            lblText.CssClass = "text-sm text-gray-700"; // Bỏ class italic, có thể chỉnh style nếu muốn
                            lblText.Visible = true; // Hiển thị label chứa tên file
                        }
                        catch (ArgumentException) // Xử lý nếu đường dẫn chứa ký tự không hợp lệ
                        {
                            lblError.Text = "[Lỗi: Đường dẫn file không hợp lệ]";
                            lblError.Visible = true;
                        }
                    }
                    // Trường hợp đặc biệt: Nếu là LoaiNoiDung='Text' và không có DuongDan nhưng có NoiDungText
                    else if (loaiNoiDung == "Text" && drv["NoiDungText"] != DBNull.Value && !string.IsNullOrWhiteSpace(drv["NoiDungText"].ToString()))
                    {
                        // Bạn có thể hiển thị thông báo khác ở đây nếu muốn phân biệt text lưu trực tiếp trong DB
                        lblText.Text = "[Nội dung dạng Text]";
                        lblText.CssClass = "text-gray-600 italic";
                        lblText.Visible = true;
                    }
                    else
                    {
                        // Nếu LoaiNoiDung là File/Text nhưng DuongDan trống
                        lblError.Text = "[Chưa có file nội dung]";
                        lblError.Visible = true;
                    }
                }
                // --- END SỬA ĐỔI ---
                else // Xử lý các loại nội dung khác hoặc không xác định
                {
                    // Kiểm tra xem có bất kỳ nội dung nào không (đường dẫn hoặc text)
                    if (string.IsNullOrWhiteSpace(duongDan) && (drv["NoiDungText"] == DBNull.Value || string.IsNullOrWhiteSpace(drv["NoiDungText"]?.ToString())))
                    {
                        lblError.Text = "[Chưa có nội dung]";
                        lblError.Visible = true;
                    }
                    else
                    {
                        // Có nội dung nhưng loại không phải Image/File/Text hoặc không xác định
                        lblError.Text = $"[Loại nội dung không rõ: {HttpUtility.HtmlEncode(loaiNoiDung)}]";
                        // Thử hiển thị tên file nếu có đường dẫn, ngay cả khi loại không xác định
                        if (!string.IsNullOrWhiteSpace(duongDan))
                        {
                            try
                            {
                                lblError.Text += $" ({HttpUtility.HtmlEncode(Path.GetFileName(duongDan))})";
                            }
                            catch { }
                        }
                        lblError.Visible = true;
                    }
                }
            }
        }


        protected void rptChapterImages_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                var img = (Image)e.Item.FindControl("imgChapterPage");
                var path = e.Item.DataItem as string; // DataItem is the string path

                if (img != null && !string.IsNullOrWhiteSpace(path))
                {
                    try
                    {
                        // Use ResolveUrl to handle application-relative paths correctly
                        img.ImageUrl = ResolveUrl(path);
                        img.AlternateText = $"Ảnh trang: {Path.GetFileName(path)}";
                        // Add onerror handler for broken images (optional but good practice)
                        img.Attributes["onerror"] = "this.style.display='none'; this.parentElement.appendChild(document.createTextNode('[Ảnh lỗi]'));";
                    }
                    catch (Exception ex) // Catch potential errors during URL resolving or path handling
                    {
                        img.Visible = false; // Hide the image control on error
                                             // Log error: Failed to set image source for path
                                             // Optionally add a literal control here to show an error message in place of the image
                    }
                }
                else if (img != null)
                {
                    img.Visible = false; // Hide if path is invalid or control not found properly
                }
            }
        }

        protected void btnAddNewChapter_Click(object sender, EventArgs e)
        {
            // Ensure _sachId is valid before redirecting
            if (_sachId > 0)
            {
                Response.Redirect($"ThemChuongMoi.aspx?sachId={_sachId}", false);
                Context.ApplicationInstance.CompleteRequest();
            }
            else
            {
                ShowMessage("Không thể thêm chương mới: ID sách không hợp lệ.", true);
            }
        }

        protected void btnBackToEditInfo_Click(object sender, EventArgs e)
        {
            // Ensure _sachId is valid before redirecting
            if (_sachId > 0)
            {
                Response.Redirect($"SuaSach.aspx?id={_sachId}", false);
                Context.ApplicationInstance.CompleteRequest();
            }
            else
            {
                ShowMessage("Không thể quay lại sửa thông tin sách: ID sách không hợp lệ.", true);
            }
        }

        protected void gvContent_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            // Check for Edit command
            if (e.CommandName == "EditChapter")
            {
                // Ensure _sachId is still valid (e.g. from Hidden Field if needed on complex postbacks)
                if (_sachId <= 0 && !int.TryParse(hfSachID.Value, out _sachId))
                {
                    ShowMessage("Không thể sửa chương: ID sách không hợp lệ.", true);
                    return;
                }

                if (!int.TryParse(e.CommandArgument?.ToString(), out int idNoiDung) || idNoiDung <= 0)
                {
                    ShowMessage("ID nội dung chương không hợp lệ.", true);
                    return;
                }

                // Redirect to the chapter edit page
                // Parameter name "id" for chapter content ID, "sachId" for book context
                Response.Redirect($"SuaNoiDungChuong.aspx?id={idNoiDung}&sachId={_sachId}", false);
                Context.ApplicationInstance.CompleteRequest();
            }
            // Add other CommandName handlers here if needed
        }

        protected void gvContent_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            // Ensure keys exist and are convertible
            if (gvContent.DataKeys == null || e.RowIndex >= gvContent.DataKeys.Count)
            {
                ShowMessage("Lỗi: Không thể lấy khóa dữ liệu để xóa.", true);
                e.Cancel = true; // Prevent the GridView from trying to enter delete mode internally
                return;
            }

            int idNoiDung = 0;
            int soChuong = 0;
            bool success = true;

            try
            {
                idNoiDung = Convert.ToInt32(gvContent.DataKeys[e.RowIndex]?["IDNoiDung"]);
                // Use TryGetValue if SoChuong might not always be present, or handle potential null
                var soChuongValue = gvContent.DataKeys[e.RowIndex]?["SoChuong"];
                if (soChuongValue != null && soChuongValue != DBNull.Value)
                {
                    soChuong = Convert.ToInt32(soChuongValue);
                }
                else
                {
                    // Handle case where SoChuong might be missing or null if needed
                    // For now, assume it's required for directory deletion logic
                    ShowMessage("Lỗi: Không thể lấy số chương để xóa.", true);
                    success = false;
                }

            }
            catch (Exception ex) // Catch conversion or index errors
            {
                ShowMessage($"Lỗi khi lấy thông tin chương cần xóa: {ex.Message}", true);
                // Log exception details
                success = false;
            }


            if (success && idNoiDung > 0) // Proceed only if ID and SoChuong were successfully retrieved
            {
                if (DeleteChapter(idNoiDung, soChuong)) // Check if delete was successful
                {
                    BindContentGrid(); // Rebind grid only on successful deletion
                }
                // Message (success or error) is shown within DeleteChapter method
            }
            else if (success && idNoiDung <= 0) // Handle case where ID is invalid after conversion
            {
                ShowMessage("Lỗi: ID nội dung không hợp lệ để xóa.", true);
            }

            // We handle rebinding manually, so cancel the GridView's default delete operation
            e.Cancel = true;
        }


        // Deletes the chapter record and associated files/directory
        // Returns true on success, false on failure.
        private bool DeleteChapter(int idNoiDung, int soChuong)
        {
            // Ensure _sachId is valid (might need reloading from HiddenField in edge cases)
            if (_sachId <= 0 && !int.TryParse(hfSachID.Value, out _sachId))
            {
                ShowMessage("Không thể xóa chương: ID sách không hợp lệ.", true);
                return false;
            }

            string duongDan = null;
            List<string> imagePhysicalPaths = new List<string>();
            string chapterDirectoryPath = null;

            using (var con = new SqlConnection(_connectionString))
            {
                SqlTransaction transaction = null;
                try
                {
                    con.Open();
                    transaction = con.BeginTransaction();

                    // 1. Get the path(s) before deleting the record
                    var cmdGet = new SqlCommand("SELECT DuongDan, LoaiNoiDung FROM NoiDungSach WHERE IDNoiDung = @IDNoiDung AND IDSach = @IDSach", con, transaction);
                    cmdGet.Parameters.AddWithValue("@IDNoiDung", idNoiDung);
                    cmdGet.Parameters.AddWithValue("@IDSach", _sachId); // Ensure we only delete from the correct book

                    string loaiNoiDung = null;
                    using (SqlDataReader reader = cmdGet.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            duongDan = reader["DuongDan"]?.ToString();
                            loaiNoiDung = reader["LoaiNoiDung"]?.ToString();
                        }
                        else
                        {
                            // Record doesn't exist or doesn't belong to this book
                            transaction.Rollback();
                            ShowMessage($"Lỗi: Không tìm thấy chương (ID: {idNoiDung}) thuộc sách (ID: {_sachId}) để xóa.", true);
                            return false;
                        }
                    } // Reader is closed here

                    // 2. Delete the database record
                    var cmdDelete = new SqlCommand("DELETE FROM NoiDungSach WHERE IDNoiDung = @IDNoiDung", con, transaction);
                    cmdDelete.Parameters.AddWithValue("@IDNoiDung", idNoiDung);
                    int rowsAffected = cmdDelete.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        // Should not happen if the SELECT found it, but check anyway
                        transaction.Rollback();
                        ShowMessage($"Lỗi: Không thể xóa bản ghi chương (ID: {idNoiDung}) khỏi cơ sở dữ liệu.", true);
                        return false;
                    }

                    // 3. Commit the transaction *before* attempting file operations
                    transaction.Commit();

                    // 4. Prepare file/directory paths *after* successful commit
                    if (loaiNoiDung == "Image" && !string.IsNullOrWhiteSpace(duongDan))
                    {
                        // Construct physical paths for images
                        imagePhysicalPaths = duongDan.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                                                   .Select(p => p.Trim())
                                                   .Where(p => !string.IsNullOrEmpty(p))
                                                   .Select(p => {
                                                       try { return Server.MapPath(p); } // Map relative web path to physical path
                                                       catch { return null; } // Handle invalid paths gracefully
                                                   })
                                                   .Where(p => p != null) // Filter out any nulls from failed MapPath
                                                   .ToList();

                        // Construct physical path for the chapter directory *if* SoChuong is valid
                        if (soChuong > 0) // Assuming 0 is not a valid chapter number for directory naming
                        {
                            try
                            {
                                // Combine base path, book-specific folder, and chapter-specific folder
                                chapterDirectoryPath = Server.MapPath(Path.Combine(BookContentBasePath, $"Sach_{_sachId}", $"Chuong_{soChuong}"));
                            }
                            catch (Exception ex)
                            {
                                // Log error: Failed to map chapter directory path
                                ShowMessage($"Lưu ý: Đã xóa bản ghi chương {soChuong}, nhưng không thể xác định đường dẫn thư mục để dọn dẹp: {ex.Message}", false); // Show as warning/info
                                chapterDirectoryPath = null; // Prevent directory deletion attempt
                            }
                        }
                    }

                    // 5. Perform file and directory deletion *after* successful commit
                    bool fileDeleteError = false;
                    if (imagePhysicalPaths.Any())
                    {
                        foreach (var path in imagePhysicalPaths)
                        {
                            try
                            {
                                if (File.Exists(path))
                                {
                                    File.Delete(path);
                                }
                            }
                            catch (IOException ex)
                            {
                                // Log error: Failed to delete file 'path'
                                fileDeleteError = true;
                            }
                            catch (UnauthorizedAccessException ex)
                            {
                                // Log error: No permission to delete file 'path'
                                fileDeleteError = true;
                            }
                            catch (Exception ex) // Catch other potential file deletion errors
                            {
                                // Log error: Generic error deleting file 'path'
                                fileDeleteError = true;
                            }
                        }
                    }

                    // 6. Attempt to delete the chapter directory if it exists and is empty
                    bool dirDeleteError = false;
                    if (chapterDirectoryPath != null && Directory.Exists(chapterDirectoryPath))
                    {
                        try
                        {
                            // Check if directory is empty (safer)
                            if (!Directory.EnumerateFileSystemEntries(chapterDirectoryPath).Any())
                            {
                                Directory.Delete(chapterDirectoryPath);
                            }
                            else
                            {
                                // Log info: Directory not empty, skipping deletion
                                // Or ShowMessage($"Lưu ý: Thư mục chương {soChuong} không trống nên không được xóa.", false);
                            }
                        }
                        catch (IOException ex)
                        {
                            // Log error: Failed to delete directory
                            dirDeleteError = true;
                        }
                        catch (UnauthorizedAccessException ex)
                        {
                            // Log error: No permission to delete directory
                            dirDeleteError = true;
                        }
                        catch (Exception ex)
                        {
                            // Log error: Generic error deleting directory
                            dirDeleteError = true;
                        }
                    }

                    // 7. Show appropriate success/warning message
                    if (fileDeleteError || dirDeleteError)
                    {
                        ShowMessage($"Đã xóa chương {soChuong} khỏi CSDL, nhưng đã xảy ra lỗi khi xóa một số file hoặc thư mục liên quan trên máy chủ.", false); // Show as non-error
                    }
                    else
                    {
                        ShowMessage($"Đã xóa chương {soChuong} và các file liên quan (nếu có) thành công.", false);
                    }
                    return true; // Database delete was successful

                }
                catch (SqlException ex)
                {
                    transaction?.Rollback(); // Rollback on SQL error
                    ShowMessage($"Lỗi cơ sở dữ liệu khi xóa chương: {ex.Message}", true);
                    // Log the exception details here (ex.ToString())
                    return false;
                }
                catch (Exception ex)
                {
                    transaction?.Rollback(); // Rollback on general error during DB phase
                    ShowMessage($"Lỗi không xác định khi xóa chương: {ex.Message}", true);
                    // Log the exception details here (ex.ToString())
                    return false;
                }
                // File/Dir deletion happens outside the transaction, errors there don't rollback DB changes.
            }
        }


        // Helper method to display messages to the user
        private void ShowMessage(string message, bool isError)
        {
            pnlMessage.CssClass = "message-panel " + (isError ? "message-error" : "message-success");
            // Use Font Awesome icons for visual feedback
            string iconClass = isError ? "fa-times-circle" : "fa-check-circle";
            lblContentMessage.Text = $"<i class='fas {iconClass}'></i> {HttpUtility.HtmlEncode(message)}"; // Encode message
            pnlMessage.Visible = true;

            // Optional: Use ScriptManager to focus on the message after postback
            // if (ScriptManager.GetCurrent(this.Page) != null) {
            //    ScriptManager.RegisterStartupScript(this.Page, this.GetType(), "FocusMessagePanel", "document.getElementById('" + pnlMessage.ClientID + "').focus();", true);
            // }
        }
    }
}