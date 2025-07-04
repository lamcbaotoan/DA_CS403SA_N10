using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Webebook.WebForm.Admin
{
    public partial class ThemChuongMoi : System.Web.UI.Page
    {
        // --- Constants ---
        private const string LoaiSach_TruyenTranh = "Truyện Tranh";
        private const string LoaiSach_TruyenChu = "Truyện Chữ";
        private const string LoaiNoiDung_Image = "Image";
        private const string LoaiNoiDung_File = "File";
        private const string BookContentVirtualBasePath = "~/BookContent/";
        private const string TempUploadVirtualPath = "~/Uploads/Temp/"; // Thư mục tạm
        protected const int MaxFileSizePerImageMb = 500; // protected
        protected const int MaxFileSizeNovelMb = 500;  // protected
        protected readonly string[] AllowedNovelExtensions = { ".doc", ".docx", ".pdf", ".txt" };
        protected readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" }; // protected
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["datawebebookConnectionString"].ConnectionString;

        // --- Page Variables ---
        protected int CurrentSachID
        {
            get { return ViewState["CurrentSachID"] != null ? (int)ViewState["CurrentSachID"] : 0; }
            set { ViewState["CurrentSachID"] = value; }
        }
        protected string CurrentLoaiSach
        {
            get { return ViewState["CurrentLoaiSach"] as string; }
            set { ViewState["CurrentLoaiSach"] = value; }
        }

        // --- Page Lifecycle Events ---
        protected void Page_Init(object sender, EventArgs e) { /* ... */ }
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                int sachIdFromQuery = 0;
                if (!int.TryParse(Request.QueryString["sachId"], out sachIdFromQuery) || sachIdFromQuery <= 0) { ShowMessage("Thiếu hoặc không hợp lệ ID Sách.", true); DisableForm("Không xác định được Sách."); return; }
                CurrentSachID = sachIdFromQuery; hfSachID.Value = CurrentSachID.ToString();
                if (!LoadBookContextAndSetupForm(CurrentSachID)) { DisableForm("Lỗi tải thông tin sách."); }
            }
            if (CurrentSachID > 0 && !string.IsNullOrEmpty(CurrentLoaiSach)) { SetupValidatorsBasedOnBookType(CurrentLoaiSach); }
            else if (!IsPostBack) { } else { ShowMessage("Mất thông tin sách hoặc loại sách.", true); DisableForm("Lỗi trạng thái."); }
            if (CurrentSachID > 0) { hlBackToList.NavigateUrl = $"~/WebForm/Admin/SuaNoiDungSach.aspx?id={CurrentSachID}"; hlBackToList.Enabled = true; hlBackToList.CssClass = hlBackToList.CssClass.Replace(" opacity-50 cursor-not-allowed", "").Trim(); }
            else { hlBackToList.Enabled = false; if (!hlBackToList.CssClass.Contains("opacity-50")) hlBackToList.CssClass += " opacity-50 cursor-not-allowed"; }
        }

        // --- Data Loading & Setup ---
        private bool LoadBookContextAndSetupForm(int sachId)
        {
            string tenSach = null; string loaiSach = null;
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                const string query = "SELECT TOP 1 TenSach, LoaiSach FROM Sach WHERE IDSach = @IDSach";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@IDSach", sachId);
                    try { con.Open(); using (SqlDataReader reader = cmd.ExecuteReader()) { if (reader.Read()) { tenSach = GetString(reader, "TenSach"); loaiSach = GetString(reader, "LoaiSach"); if (string.IsNullOrEmpty(loaiSach) || (loaiSach != LoaiSach_TruyenChu && loaiSach != LoaiSach_TruyenTranh)) { ShowMessage($"Sách '{HttpUtility.HtmlEncode(tenSach)}' có Loại sách không hợp lệ ('{HttpUtility.HtmlEncode(loaiSach)}').", true); return false; } } else { ShowMessage($"Không tìm thấy sách ID={sachId}.", true); return false; } } }
                    catch (Exception ex) { ShowMessage("Lỗi tải thông tin sách: " + ex.Message, true); Debug.WriteLine($"ERROR Loading Book Context (ID: {sachId}): {ex}"); return false; }
                }
            }
            CurrentLoaiSach = loaiSach; hfLoaiSach.Value = CurrentLoaiSach; lblPageModeTitle.Text = "Thêm Chương Mới"; lblBookTitleContext.Text = HttpUtility.HtmlEncode(tenSach); lblSachIDContext.Text = sachId.ToString(); lblLoaiSachContext.Text = HttpUtility.HtmlEncode(CurrentLoaiSach);
            SetPageTitle($"Thêm Chương Mới - {HttpUtility.HtmlEncode(tenSach)}"); PrepareFormForAdd(sachId); SetupContentPanels(CurrentLoaiSach); SetupValidatorsBasedOnBookType(CurrentLoaiSach);
            return true;
        }
        private void PrepareFormForAdd(int sachId)
        {
            txtSoChuong.Enabled = true; cvSoChuongExists.Enabled = true;
            int nextChapter = GetMaxChapterNumber(sachId) + 1; txtSoChuong.Text = nextChapter > 0 ? nextChapter.ToString() : "1";
            txtTenChuong.Text = ""; txtNoiDungChu.Text = ""; hfComicImageOrder.Value = "";
            ScriptManager.RegisterStartupScript(this, this.GetType(), "ResetDropzoneState", "if(typeof myDropzoneInstance !== 'undefined' && myDropzoneInstance) { try { myDropzoneInstance.removeAllFiles(true); } catch(e) { console.error('Error resetting dropzone:', e); } uploadedFilesMap = {}; updateHiddenFieldOrder(); }", true);
        }
        private void SetupContentPanels(string loaiSach) { bool n = loaiSach == LoaiSach_TruyenChu; bool c = loaiSach == LoaiSach_TruyenTranh; pnlNovelContent.Visible = n; pnlComicContent.Visible = c; }
        private void SetupValidatorsBasedOnBookType(string loaiSach) { bool n = loaiSach == LoaiSach_TruyenChu; bool c = loaiSach == LoaiSach_TruyenTranh; rfvSoChuong.Enabled = true; cvSoChuongType.Enabled = true; cvSoChuongPositive.Enabled = true; cvSoChuongExists.Enabled = true; SetControlValidation(n, revFileTieuThuyet, cvNovelContentRequired); SetControlValidation(c, cvAnhTruyenRequired); }
        private void DisableForm(string reason) { /* Giữ nguyên */ }
        private void SetControlValidation(bool enabled, params BaseValidator[] validators) { foreach (var v in validators) { if (v != null && v.Enabled != enabled) v.Enabled = enabled; } }

        // --- Event Handlers ---
        protected void btnAdd_Click(object sender, EventArgs e)
        {
            int sachId = CurrentSachID; string loaiSach = CurrentLoaiSach; Debug.WriteLine($"--- btnAdd_Click START (SachID: {sachId}, LoaiSach: {loaiSach}) ---");
            if (sachId <= 0 || string.IsNullOrEmpty(loaiSach)) { ShowMessage("Lỗi: Không xác định sách/loại sách.", true); return; }
            SetupValidatorsBasedOnBookType(loaiSach); Page.Validate("AddChapterValidation");
            if (!Page.IsValid) { ShowMessage("Vui lòng kiểm tra lỗi.", true); if (vsChapterForm != null) vsChapterForm.Style["display"] = "block"; LogValidationErrors(); EnableButtonsClientScript(); return; }
            if (vsChapterForm != null) vsChapterForm.Style["display"] = "none"; int soChuong; if (!int.TryParse(txtSoChuong.Text.Trim(), out soChuong) || soChuong <= 0) { ShowMessage("Số chương không hợp lệ.", true); EnableButtonsClientScript(); return; }
            string tenChuong = txtTenChuong.Text.Trim(); bool success = false; string operationMessage = "";
            try { success = AddNewChapter(sachId, soChuong, tenChuong, loaiSach, ref operationMessage); }
            catch (Exception ex) { success = false; operationMessage = $"Lỗi hệ thống: {ex.Message}"; Debug.WriteLine($"CRITICAL ADD EXCEPTION (SachID: {sachId}, Chapter: {soChuong}): {ex.ToString()}"); }
            if (success) { string msg = $"Thêm chương {soChuong} thành công!"; string url = $"SuaNoiDungSach.aspx?id={sachId}&message={HttpUtility.UrlEncode(msg)}"; Debug.WriteLine($"--- btnAdd_Click SUCCESS. Redirecting ---"); Response.Redirect(url, false); Context.ApplicationInstance.CompleteRequest(); }
            else { ShowMessage(operationMessage ?? "Thêm chương thất bại.", true); Debug.WriteLine($"--- btnAdd_Click FAILED ---"); EnableButtonsClientScript(); }
        }
        protected void btnCancel_Click(object sender, EventArgs e) { /* Giữ nguyên */ }

        // --- Server-Side Validators ---
        protected void cvSoChuongExists_ServerValidate(object source, ServerValidateEventArgs args) { /* Giữ nguyên */ }
        protected void cvNovelContentRequired_ServerValidate(object source, ServerValidateEventArgs args) { /* Giữ nguyên */ }
        protected void cvAnhTruyenRequired_ServerValidate(object source, ServerValidateEventArgs args)
        { // Kiểm tra hidden field
            if (!pnlComicContent.Visible) { args.IsValid = true; return; }
            args.IsValid = !string.IsNullOrWhiteSpace(hfComicImageOrder.Value);
            if (!args.IsValid) { ((CustomValidator)source).ErrorMessage = "Bạn phải tải lên ít nhất một file ảnh hợp lệ."; }
        }

        // --- Core Logic: AddNewChapter (ĐÃ SỬA PHẦN TRUYỆN TRANH CHO DROPZONE) ---
        private bool AddNewChapter(int sachId, int soChuong, string tenChuong, string loaiSach, ref string operationMessage)
        {
            Debug.WriteLine($"--- AddNewChapter START (SachID: {sachId}, SoChuong: {soChuong}, LoaiSach: {loaiSach}) ---");
            string loaiNoiDungDb = ""; string duongDanDb = null; string physicalChapterPath = ""; string physicalTempPath = "";
            List<string> finalMovedFileRelativePaths = new List<string>(); List<string> processedTempFileNames = new List<string>();

            // 1. Tạo thư mục
            try
            {
                physicalChapterPath = Server.MapPath(BookContentVirtualBasePath + $"Sach_{sachId}/Chuong_{soChuong}/");
                if (!Directory.Exists(physicalChapterPath)) { Directory.CreateDirectory(physicalChapterPath); }
                physicalTempPath = Server.MapPath(TempUploadVirtualPath);
                if (!Directory.Exists(physicalTempPath)) { Directory.CreateDirectory(physicalTempPath); }
            }
            catch (Exception ex) { operationMessage = $"Lỗi tạo thư mục: {ex.Message}"; return false; }

            // 2. Xử lý file
            try
            {
                if (loaiSach.Equals(LoaiSach_TruyenChu, StringComparison.OrdinalIgnoreCase))
                {
                    #region Add Novel Logic (Không đổi)
                    loaiNoiDungDb = LoaiNoiDung_File;
                    bool fileUploaded = fuFileTieuThuyet.HasFiles && fuFileTieuThuyet.PostedFiles.Count > 0 && fuFileTieuThuyet.PostedFiles[0].ContentLength > 0;
                    string textEntered = txtNoiDungChu.Text.Trim();
                    if (fileUploaded)
                    {
                        HttpPostedFile file = fuFileTieuThuyet.PostedFiles[0];
                        if (!ValidateFile(file, (long)MaxFileSizeNovelMb * 1024 * 1024, AllowedNovelExtensions, out string validationError)) { operationMessage = $"File tiểu thuyết không hợp lệ: {validationError}"; DeleteDirectoryIfEmptySafe(physicalChapterPath); return false; }
                        string uniqueFileName = $"content_{Guid.NewGuid().ToString("N")}{Path.GetExtension(file.FileName)}";
                        string physicalSavePath = Path.Combine(physicalChapterPath, uniqueFileName);
                        string relativeSavePath = BookContentVirtualBasePath + $"Sach_{sachId}/Chuong_{soChuong}/{uniqueFileName}";
                        file.SaveAs(physicalSavePath); finalMovedFileRelativePaths.Add(relativeSavePath); duongDanDb = relativeSavePath.TrimStart('~');
                    }
                    else if (!string.IsNullOrWhiteSpace(textEntered))
                    {
                        string uniqueTextFileName = $"content_{Guid.NewGuid().ToString("N")}.txt";
                        string physicalSavePath = Path.Combine(physicalChapterPath, uniqueTextFileName);
                        string relativeSavePath = BookContentVirtualBasePath + $"Sach_{sachId}/Chuong_{soChuong}/{uniqueTextFileName}";
                        File.WriteAllText(physicalSavePath, textEntered, Encoding.UTF8); finalMovedFileRelativePaths.Add(relativeSavePath); duongDanDb = relativeSavePath.TrimStart('~');
                    }
                    else { operationMessage = "Không có nội dung truyện chữ."; DeleteDirectoryIfEmptySafe(physicalChapterPath); return false; }
                    #endregion
                }
                else if (loaiSach.Equals(LoaiSach_TruyenTranh, StringComparison.OrdinalIgnoreCase))
                {
                    #region Add Comic Logic (Dropzone)
                    loaiNoiDungDb = LoaiNoiDung_Image;
                    string orderedTempFileNamesString = hfComicImageOrder.Value ?? "";
                    List<string> orderedTempFileNames = orderedTempFileNamesString.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(f => f.Trim()).Where(f => !string.IsNullOrEmpty(f)).ToList();
                    if (!orderedTempFileNames.Any()) { operationMessage = "Không có file ảnh nào được tải lên."; DeleteDirectoryIfEmptySafe(physicalChapterPath); return false; }

                    List<string> finalRelativeDbPathsOrdered = new List<string>(); int pageCounter = 1;
                    foreach (string tempFileName in orderedTempFileNames)
                    {
                        if (string.IsNullOrWhiteSpace(tempFileName) || tempFileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 || tempFileName.Contains("..")) { continue; }
                        string sourceTempFilePath = Path.Combine(physicalTempPath, tempFileName);
                        if (!File.Exists(sourceTempFilePath)) { operationMessage = $"Lỗi: File tạm '{HttpUtility.HtmlEncode(tempFileName)}' không tồn tại."; DeleteFilesSafe(finalMovedFileRelativePaths); DeleteDirectoryIfEmptySafe(physicalChapterPath); return false; }

                        // Validate lại file tạm (an toàn hơn)
                        try { FileInfo fi = new FileInfo(sourceTempFilePath); if (fi.Length <= 0) throw new Exception($"File '{tempFileName}' 0 byte."); string extCheck = fi.Extension.ToLowerInvariant(); if (!AllowedImageExtensions.Contains(extCheck)) throw new Exception($"File '{tempFileName}' có định dạng không cho phép."); }
                        catch (Exception valEx) { operationMessage = $"Lỗi file tạm: {valEx.Message}"; DeleteFilesSafe(finalMovedFileRelativePaths); DeleteDirectoryIfEmptySafe(physicalChapterPath); try { File.Delete(sourceTempFilePath); } catch { } return false; }

                        string extension = Path.GetExtension(sourceTempFilePath);
                        string finalFileName = $"page_{pageCounter:D3}{extension}";
                        string finalPhysicalPath = Path.Combine(physicalChapterPath, finalFileName);
                        string finalRelativePathWithTilde = BookContentVirtualBasePath + $"Sach_{sachId}/Chuong_{soChuong}/{finalFileName}";

                        // Di chuyển file
                        try { if (File.Exists(finalPhysicalPath)) { File.Delete(finalPhysicalPath); } File.Move(sourceTempFilePath, finalPhysicalPath); finalMovedFileRelativePaths.Add(finalRelativePathWithTilde); finalRelativeDbPathsOrdered.Add(finalRelativePathWithTilde.TrimStart('~')); processedTempFileNames.Add(tempFileName); pageCounter++; }
                        catch (Exception moveEx) { operationMessage = $"Lỗi di chuyển file '{HttpUtility.HtmlEncode(tempFileName)}': {moveEx.Message}"; DeleteFilesSafe(finalMovedFileRelativePaths); DeleteDirectoryIfEmptySafe(physicalChapterPath); return false; }
                    }
                    if (!finalRelativeDbPathsOrdered.Any()) { operationMessage = "Không xử lý được file ảnh nào."; DeleteDirectoryIfEmptySafe(physicalChapterPath); return false; }
                    duongDanDb = string.Join(",", finalRelativeDbPathsOrdered); // Không có dấu phẩy cuối
                    #endregion
                }
                else { /* Loại sách không hỗ trợ */ }

                // 3. Lưu vào CSDL
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open(); const string query = @"INSERT INTO NoiDungSach (IDSach, SoChuong, TenChuong, LoaiNoiDung, DuongDan, NgayTao) VALUES (@IDSach, @SoChuong, @TenChuong, @LoaiNoiDung, @DuongDan, GETDATE());";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@IDSach", sachId); cmd.Parameters.AddWithValue("@SoChuong", soChuong); cmd.Parameters.AddWithValue("@TenChuong", OrDBNull(tenChuong));
                        cmd.Parameters.AddWithValue("@LoaiNoiDung", loaiNoiDungDb); cmd.Parameters.AddWithValue("@DuongDan", OrDBNull(duongDanDb));
                        int rows = cmd.ExecuteNonQuery();
                        if (rows > 0) { operationMessage = $"Thêm chương {soChuong} thành công!"; CleanUpTempFiles(processedTempFileNames); return true; } // Xóa file tạm
                        else { operationMessage = "Lỗi ghi CSDL."; DeleteFilesSafe(finalMovedFileRelativePaths); DeleteDirectoryIfEmptySafe(physicalChapterPath); return false; }
                    }
                }
            }
            // --- Xử lý lỗi chung và Rollback ---
            catch (SqlException sqlEx) when (sqlEx.Number == 2627 || sqlEx.Number == 2601) { operationMessage = $"Lỗi CSDL: Chương {soChuong} đã tồn tại."; if (cvSoChuongExists != null) cvSoChuongExists.IsValid = false; DeleteFilesSafe(finalMovedFileRelativePaths); DeleteDirectoryIfEmptySafe(physicalChapterPath); return false; }
            catch (Exception ex) { operationMessage = "Lỗi xử lý chương: " + ex.Message; Debug.WriteLine($"Error AddNewChapter: {ex}"); DeleteFilesSafe(finalMovedFileRelativePaths); DeleteDirectoryIfEmptySafe(physicalChapterPath); return false; }
        }

        // --- Helper Methods ---
        // --- Helper Methods ---

        private int GetMaxChapterNumber(int bookId)
        {
            if (bookId <= 0) return 0;
            int maxChapter = 0;
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                const string query = "SELECT ISNULL(MAX(SoChuong), 0) FROM NoiDungSach WHERE IDSach = @IDSach";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@IDSach", bookId);
                    try
                    {
                        con.Open();
                        object result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            int.TryParse(result.ToString(), out maxChapter);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error in GetMaxChapterNumber for BookID {bookId}: {ex.Message}");
                        // ShowMessage($"Lỗi đọc số chương lớn nhất: {ex.Message}", true); // Không nên ShowMessage trong hàm helper cấp thấp
                        maxChapter = -1; // Indicate error
                    }
                }
            }
            return maxChapter;
        }

        // Validate file (dùng cho cả Novel và Comic nếu cần validate lại file tạm)
        private bool ValidateFile(HttpPostedFile file, long maxSizeBytes, string[] allowedExtensions, out string errorMessage)
        {
            errorMessage = "";
            if (file == null || file.ContentLength <= 0)
            {
                errorMessage = "File không được rỗng hoặc không hợp lệ.";
                return false;
            }
            string fileName;
            try
            {
                fileName = Path.GetFileName(file.FileName);
                if (string.IsNullOrWhiteSpace(fileName)) { errorMessage = "Tên file không hợp lệ."; return false; }
            }
            catch (ArgumentException) { errorMessage = "Tên file chứa ký tự không hợp lệ."; return false; }

            if (file.ContentLength > maxSizeBytes)
            {
                errorMessage = $"File '{HttpUtility.HtmlEncode(fileName)}' quá lớn ({FormatFileSize(file.ContentLength)}). Tối đa: {FormatFileSize(maxSizeBytes)}.";
                return false;
            }
            string fileExtension = Path.GetExtension(fileName)?.ToLowerInvariant();
            if (string.IsNullOrEmpty(fileExtension) || !allowedExtensions.Contains(fileExtension, StringComparer.OrdinalIgnoreCase))
            {
                errorMessage = $"Định dạng file '{HttpUtility.HtmlEncode(fileExtension ?? "N/A")}' không cho phép. Chấp nhận: {string.Join(", ", allowedExtensions)}.";
                return false;
            }
            return true;
        }

        // Xóa danh sách các file (đường dẫn tương đối từ gốc web app, có ~)
        private void DeleteFilesSafe(List<string> relativePathsToDelete)
        {
            if (relativePathsToDelete == null || !relativePathsToDelete.Any()) return;
            Debug.WriteLine($"Attempting to rollback/delete {relativePathsToDelete.Count} files...");
            foreach (string relativePath in relativePathsToDelete.Distinct(StringComparer.OrdinalIgnoreCase))
            {
                DeleteSingleFileSafe(relativePath);
            }
        }

        // Xóa một file an toàn (đường dẫn tương đối từ gốc web app, có ~)
        private void DeleteSingleFileSafe(string relativePathWithTilde)
        {
            if (string.IsNullOrWhiteSpace(relativePathWithTilde)) return;
            if (!relativePathWithTilde.StartsWith("~"))
            { // Ensure tilde for MapPath
                relativePathWithTilde = "~" + (relativePathWithTilde.StartsWith("/") ? "" : "/") + relativePathWithTilde;
            }

            // Security Check: Only allow deletion within the BookContent or TempUpload path
            bool isAllowedPath = relativePathWithTilde.StartsWith(BookContentVirtualBasePath, StringComparison.OrdinalIgnoreCase) ||
                                 relativePathWithTilde.StartsWith(TempUploadVirtualPath, StringComparison.OrdinalIgnoreCase);

            if (!isAllowedPath)
            {
                Debug.WriteLine($"SECURITY WARNING (DeleteSingleFileSafe): Path '{relativePathWithTilde}' is outside allowed directories. Deletion skipped.");
                return;
            }

            string physicalPath = null;
            try
            {
                physicalPath = Server.MapPath(relativePathWithTilde);
                if (File.Exists(physicalPath))
                {
                    File.SetAttributes(physicalPath, FileAttributes.Normal);
                    File.Delete(physicalPath);
                    Debug.WriteLine($"Deleted file: {physicalPath}");
                }
                else
                {
                    // Debug.WriteLine($"File not found for deletion: {physicalPath}"); // Optional log
                }
            }
            catch (Exception ex) when (ex is FileNotFoundException || ex is DirectoryNotFoundException) { /* Ignore */ }
            catch (Exception ex) { Debug.WriteLine($"ERROR deleting file '{physicalPath ?? relativePathWithTilde}': {ex.Message}"); }
        }

        // Xóa thư mục nếu nó rỗng (nhận đường dẫn vật lý)
        private void DeleteDirectoryIfEmptySafe(string physicalDirectoryPath)
        {
            if (string.IsNullOrWhiteSpace(physicalDirectoryPath)) return;
            string physicalBaseContentPath = ""; try { physicalBaseContentPath = Server.MapPath(BookContentVirtualBasePath); } catch { return; }

            // Security Check
            if (string.IsNullOrEmpty(physicalBaseContentPath) || !physicalDirectoryPath.StartsWith(physicalBaseContentPath, StringComparison.OrdinalIgnoreCase) || physicalDirectoryPath.Equals(physicalBaseContentPath, StringComparison.OrdinalIgnoreCase))
            {
                Debug.WriteLine($"SECURITY WARNING (DeleteDirectoryIfEmptySafe): Path '{physicalDirectoryPath}' invalid. Deletion skipped."); return;
            }

            try
            {
                if (Directory.Exists(physicalDirectoryPath))
                {
                    if (!Directory.EnumerateFileSystemEntries(physicalDirectoryPath).Any())
                    {
                        Directory.Delete(physicalDirectoryPath, false); Debug.WriteLine($"Deleted empty directory: {physicalDirectoryPath}");
                    }
                    else { /* Debug.WriteLine($"Directory not empty: {physicalDirectoryPath}"); */ }
                }
            }
            catch (Exception ex) when (ex is DirectoryNotFoundException) { /* Ignore */}
            catch (Exception ex) { Debug.WriteLine($"ERROR checking/deleting directory '{physicalDirectoryPath}': {ex.Message}"); }
        }

        // Định dạng kích thước file
        private static string FormatFileSize(long bytes)
        {
            if (bytes < 0) return "N/A"; if (bytes == 0) return "0 Bytes"; const int k = 1024; string[] sizes = { "Bytes", "KB", "MB", "GB", "TB" };
            try { int i = Math.Max(0, Math.Min((int)Math.Floor(Math.Log(bytes) / Math.Log(k)), sizes.Length - 1)); return string.Format(CultureInfo.InvariantCulture, "{0:0.##} {1}", bytes / Math.Pow(k, i), sizes[i]); } catch { return "N/A"; }
        }

        // Hiển thị thông báo trên UI
        private void ShowMessage(string message, bool isError)
        {
            if (pnlMessage == null || lblFormMessage == null) return;
            pnlMessage.CssClass = "message-panel " + (isError ? "message-error" : "message-success");
            string iconClass = isError ? "fas fa-times-circle" : "fas fa-check-circle";
            lblFormMessage.Text = $"<i class='{iconClass}'></i> {HttpUtility.HtmlEncode(message ?? "Có lỗi xảy ra.")}";
            pnlMessage.Visible = true;
        }

        // Kích hoạt lại nút Add/Cancel bằng JS sau khi server xử lý lỗi
        private void EnableButtonsClientScript()
        {
            string script = @"var btnAdd = document.getElementById('" + btnAdd.ClientID + @"'); var btnCancel = document.getElementById('" + btnCancel.ClientID + @"'); if(btnAdd) { btnAdd.disabled = false; btnAdd.classList.remove('loading-spinner'); if(btnAdd.tagName === 'INPUT') { btnAdd.value = 'Thêm Chương'; } else { /* logic for button tag if needed */ } } if(btnCancel) { btnCancel.disabled = false; }";
            ScriptManager.RegisterStartupScript(this, this.GetType(), "EnableSubmitButtons_" + Guid.NewGuid(), script, true); // Thêm Guid để tránh key trùng lặp
        }

        // Ghi log chi tiết các lỗi validation server-side
        private void LogValidationErrors()
        {
            if (Page.IsValid) return;
            Debug.WriteLine("--- Server Validation Errors ---");
            foreach (IValidator validator in Page.GetValidators("AddChapterValidation")) // Chỉ lấy validator trong group cần thiết
            {
                if (!validator.IsValid)
                {
                    string controlToValidate = (validator is BaseValidator) ? ((BaseValidator)validator).ControlToValidate : "N/A";
                    string validatorId = (validator as Control)?.ID ?? "N/A";
                    Debug.WriteLine($"- Validator ({validatorId}): {validator.ErrorMessage} [Control: {controlToValidate}]");
                }
            }
            Debug.WriteLine("--- End Validation Errors ---");
        }

        // Đặt tiêu đề trang thông qua Master Page
        private void SetPageTitle(string title)
        {
            if (Master is Admin master)
            { // Đảm bảo tên class Admin là đúng
                master.SetPageTitle(title);
            }
            else { Page.Title = title; }
        }

        // Lấy giá trị string an toàn từ IDataRecord
        private string GetString(IDataRecord reader, string columnName, string defaultValue = "")
        {
            try { int ord = reader.GetOrdinal(columnName); return reader.IsDBNull(ord) ? defaultValue : reader.GetString(ord).Trim(); }
            catch (IndexOutOfRangeException) { Debug.WriteLine($"Warning: Column '{columnName}' not found."); return defaultValue; }
            catch (Exception ex) { Debug.WriteLine($"Error getting string '{columnName}': {ex.Message}"); return defaultValue; }
        }

        // Trả về DBNull.Value nếu string rỗng/null/whitespace
        private object OrDBNull(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? DBNull.Value : (object)value.Trim();
        }

        // Set header chống cache trình duyệt
        private void SetNoCacheHeaders()
        {
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.Cache.SetNoStore();
            Response.Cache.SetExpires(DateTime.UtcNow.AddYears(-1));
            Response.Cache.SetNoTransforms();
        }

        // Xóa các file trong thư mục tạm sau khi xử lý thành công
        private void CleanUpTempFiles(List<string> tempFileNames)
        {
            if (tempFileNames == null || !tempFileNames.Any()) return;
            string physicalTempPath = "";
            try { physicalTempPath = Server.MapPath(TempUploadVirtualPath); }
            catch (Exception mapEx) { Debug.WriteLine($"ERROR getting Temp Path for cleanup: {mapEx.Message}"); return; }

            Debug.WriteLine($"Cleaning up {tempFileNames.Count} temporary files...");
            foreach (string tempFileName in tempFileNames)
            {
                if (string.IsNullOrWhiteSpace(tempFileName)) continue;
                // Thêm kiểm tra an toàn cho tên file tạm
                if (tempFileName.Contains("..") || tempFileName.IndexOfAny(Path.GetInvalidPathChars()) >= 0 || tempFileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                {
                    Debug.WriteLine($"Skipping cleanup for potentially invalid temp filename: {tempFileName}");
                    continue;
                }

                string tempFilePath = Path.Combine(physicalTempPath, tempFileName);
                try
                {
                    if (File.Exists(tempFilePath))
                    {
                        File.Delete(tempFilePath);
                        // Debug.WriteLine($"Deleted temp file: {tempFilePath}"); // Có thể bật log này nếu cần
                    }
                }
                catch (Exception ex)
                {
                    // Log lỗi nhưng không dừng việc xóa các file khác
                    Debug.WriteLine($"ERROR deleting temp file '{tempFilePath}': {ex.Message}");
                }
            }
            Debug.WriteLine("Temp file cleanup finished.");
        }

    } // End Class
} // End Namespace