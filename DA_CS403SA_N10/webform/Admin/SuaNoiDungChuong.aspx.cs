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
using System.Web.Script.Serialization; // Cần cho JSON parsing
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Webebook.WebForm.Admin
{
    public partial class SuaNoiDungChuong : System.Web.UI.Page
    {
        // --- Constants ---
        private const string LoaiSach_TruyenTranh = "Truyện Tranh";
        private const string LoaiSach_TruyenChu = "Truyện Chữ";
        private const string LoaiNoiDung_Image = "Image";
        private const string LoaiNoiDung_File = "File";
        private const string BookContentVirtualBasePath = "~/BookContent/";
        private const string TempUploadVirtualPath = "~/Uploads/Temp/";
        protected const int MaxFileSizePerImageMb = 500; // protected để ASPX truy cập
        protected const int MaxFileSizeNovelMb = 500;    // protected
        protected readonly string[] AllowedNovelExtensions = { ".doc", ".docx", ".pdf", ".txt" };
        protected readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" }; // protected
        private readonly string[] AllowedTextFileExtension = { ".txt" };

        private readonly string connectionString = ConfigurationManager.ConnectionStrings["datawebebookConnectionString"].ConnectionString;

        // --- Page Variables (Dùng ViewState) ---
        protected int CurrentIDNoiDung
        {
            get { return ViewState["Edit_IDNoiDung"] != null ? (int)ViewState["Edit_IDNoiDung"] : 0; }
            set { ViewState["Edit_IDNoiDung"] = value; }
        }
        protected int CurrentSachID
        {
            get { return ViewState["Edit_SachID"] != null ? (int)ViewState["Edit_SachID"] : 0; }
            set { ViewState["Edit_SachID"] = value; }
        }
        protected string CurrentLoaiSach
        {
            get { return ViewState["Edit_LoaiSach"] as string; }
            set { ViewState["Edit_LoaiSach"] = value; }
        }
        protected int CurrentSoChuong
        {
            get { return ViewState["Edit_SoChuong"] != null ? (int)ViewState["Edit_SoChuong"] : 0; }
            set { ViewState["Edit_SoChuong"] = value; }
        }

        // --- Page Lifecycle ---
        protected void Page_Init(object sender, EventArgs e) { /* Có thể debug Request ở đây */ }

        protected void Page_Load(object sender, EventArgs e)
        {
            int idNoiDung = 0; int sachId = 0;
            if (!int.TryParse(Request.QueryString["id"], out idNoiDung) || idNoiDung <= 0) { if (!int.TryParse(hfIDNoiDung.Value, out idNoiDung) || idNoiDung <= 0) { ShowMessageAndRedirect("Thiếu ID Nội dung.", "QuanLySach.aspx", true); return; } }
            CurrentIDNoiDung = idNoiDung; hfIDNoiDung.Value = idNoiDung.ToString();

            if (!int.TryParse(Request.QueryString["sachId"], out sachId) || sachId <= 0) { if (!IsPostBack) { /* Sẽ lấy từ DB */ } else if (!int.TryParse(hfSachID.Value, out sachId) || sachId <= 0) { ShowMessage("Thiếu ID Sách.", true); DisableForm("Lỗi."); return; } }
            if (sachId > 0) { CurrentSachID = sachId; hfSachID.Value = sachId.ToString(); }

            if (!IsPostBack)
            {
                if (!LoadChapterDataAndSetupForm(CurrentIDNoiDung)) { if (btnSaveNoiDung.Enabled) DisableForm("Lỗi tải dữ liệu."); }
                else { btnSaveTenChuong.Enabled = true; btnSaveNoiDung.Enabled = true; btnCancel.Enabled = true; }
            }
            if (CurrentSachID > 0)
            {
                hlBackToList.NavigateUrl = $"~/WebForm/Admin/SuaNoiDungSach.aspx?id={CurrentSachID}"; hlBackToList.Enabled = true;
                if (!string.IsNullOrEmpty(CurrentLoaiSach)) { SetupValidatorsBasedOnBookType(CurrentLoaiSach); } else if (IsPostBack) { ShowMessage("Mất Loại sách.", true); DisableForm("Lỗi."); }
            }
            else { hlBackToList.Enabled = false; if (btnSaveNoiDung.Enabled) DisableForm("Không xác định Sách."); }
        }

        // --- Data Loading & Setup ---
        private bool LoadChapterDataAndSetupForm(int idNoiDung)
        {
            string tenSach = null; string loaiSach = null; int sachId = 0; int soChuong = 0;
            string tenChuong = null; string currentDuongDan = null; string noiDungText = null;
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                const string query = @"SELECT TOP 1 nds.IDSach, nds.SoChuong, nds.TenChuong, nds.DuongDan, nds.NoiDungText, s.TenSach, s.LoaiSach FROM NoiDungSach nds INNER JOIN Sach s ON nds.IDSach = s.IDSach WHERE nds.IDNoiDung = @IDNoiDung";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@IDNoiDung", idNoiDung);
                    try
                    {
                        con.Open(); using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                sachId = reader.GetInt32(reader.GetOrdinal("IDSach")); soChuong = reader.GetInt32(reader.GetOrdinal("SoChuong")); tenChuong = GetString(reader, "TenChuong"); currentDuongDan = GetString(reader, "DuongDan"); noiDungText = GetString(reader, "NoiDungText"); tenSach = GetString(reader, "TenSach"); loaiSach = GetString(reader, "LoaiSach");
                                if (sachId <= 0 || string.IsNullOrEmpty(loaiSach) || (loaiSach != LoaiSach_TruyenChu && loaiSach != LoaiSach_TruyenTranh)) { ShowMessage("Dữ liệu không hợp lệ.", true); return false; }
                                CurrentSachID = sachId; CurrentLoaiSach = loaiSach; CurrentSoChuong = soChuong; hfSachID.Value = sachId.ToString(); hfLoaiSach.Value = loaiSach; hfCurrentDuongDan.Value = currentDuongDan ?? ""; hfOriginalNovelText.Value = noiDungText ?? ""; hfOriginalTenChuong.Value = tenChuong ?? "";
                                lblBookTitleContext.Text = HttpUtility.HtmlEncode(tenSach); lblSachIDContext.Text = sachId.ToString(); lblLoaiSachContext.Text = HttpUtility.HtmlEncode(loaiSach);
                                txtSoChuong.Text = soChuong.ToString(); txtSoChuong.Enabled = false; txtSoChuong.CssClass += " bg-gray-100 cursor-not-allowed"; txtTenChuong.Text = tenChuong ?? "";
                                SetPageTitle($"Sửa Chương {soChuong} - {HttpUtility.HtmlEncode(tenSach)}"); SetupFormForEdit(loaiSach, currentDuongDan, noiDungText); return true;
                            }
                            else { ShowMessage($"Không tìm thấy chương ID={idNoiDung}.", true); return false; }
                        }
                    }
                    catch (Exception ex) { ShowMessage("Lỗi tải dữ liệu chương: " + ex.Message, true); Debug.WriteLine($"ERROR Loading Chapter Data (IDNoiDung: {idNoiDung}): {ex}"); return false; }
                }
            }
        }
        private void SetupFormForEdit(string loaiSach, string currentDuongDan, string noiDungTextDb)
        {
            SetupContentPanels(loaiSach); lblNovelFileReadError.Visible = false;
            if (loaiSach.Equals(LoaiSach_TruyenChu, StringComparison.OrdinalIgnoreCase))
            {
                string displayedText = noiDungTextDb ?? ""; bool isFileContent = false; pnlExistingNovelFile.Visible = false;
                if (string.IsNullOrEmpty(displayedText) && !string.IsNullOrEmpty(currentDuongDan))
                {
                    string fullRelativePath = "~/" + currentDuongDan.TrimStart('/'); string fileName = Path.GetFileName(currentDuongDan); string extension = Path.GetExtension(currentDuongDan)?.ToLowerInvariant();
                    bool isLikelyTextFile = AllowedTextFileExtension.Contains(extension) || fileName.StartsWith("content_", StringComparison.OrdinalIgnoreCase);
                    if (isLikelyTextFile) { Debug.WriteLine($"Reading novel file: {fullRelativePath}"); try { string physicalPath = MapVirtualToPhysicalPath(fullRelativePath); Debug.WriteLine($"Physical path: {physicalPath}"); if (File.Exists(physicalPath)) { Debug.WriteLine("File exists. Reading..."); displayedText = File.ReadAllText(physicalPath, Encoding.UTF8); isFileContent = true; Debug.WriteLine($"Read success. Length: {displayedText.Length}"); } else { lblNovelFileReadError.Text = $"Lỗi: File '{HttpUtility.HtmlEncode(fileName)}' không tồn tại."; lblNovelFileReadError.Visible = true; Debug.WriteLine($"ERROR: File not found at {physicalPath}"); pnlExistingNovelFile.Visible = true; hlCurrentNovelFile.Text = HttpUtility.HtmlEncode(fileName) + " (Không tìm thấy)"; hlCurrentNovelFile.NavigateUrl = "#notfound"; } } catch (Exception ex) { lblNovelFileReadError.Text = $"Lỗi đọc file: {HttpUtility.HtmlEncode(ex.Message)}"; lblNovelFileReadError.Visible = true; Debug.WriteLine($"ERROR reading novel text file ({currentDuongDan}): {ex.ToString()}"); pnlExistingNovelFile.Visible = true; hlCurrentNovelFile.Text = HttpUtility.HtmlEncode(fileName) + " (Lỗi đọc)"; hlCurrentNovelFile.NavigateUrl = "#readerror"; } }
                    else { pnlExistingNovelFile.Visible = true; hlCurrentNovelFile.Text = HttpUtility.HtmlEncode(fileName); try { hlCurrentNovelFile.NavigateUrl = MapRelativePathToUrl(fullRelativePath); } catch { hlCurrentNovelFile.NavigateUrl = "#error"; } }
                }
                else { pnlExistingNovelFile.Visible = !string.IsNullOrEmpty(currentDuongDan); if (pnlExistingNovelFile.Visible) { hlCurrentNovelFile.Text = HttpUtility.HtmlEncode(Path.GetFileName(currentDuongDan)); try { hlCurrentNovelFile.NavigateUrl = MapRelativePathToUrl("~/" + currentDuongDan.TrimStart('/')); } catch { hlCurrentNovelFile.NavigateUrl = "#error"; } } }
                txtNoiDungChu.Text = displayedText; hfOriginalNovelText.Value = displayedText; if (isFileContent) { pnlExistingNovelFile.Visible = false; }
            }
            else if (loaiSach.Equals(LoaiSach_TruyenTranh, StringComparison.OrdinalIgnoreCase)) { InitializeComicEditorClientScript(currentDuongDan); }
            SetupValidatorsBasedOnBookType(loaiSach);
        }
        private void InitializeComicEditorClientScript(string currentDuongDan) { List<object> imageList = new List<object>(); if (!string.IsNullOrWhiteSpace(currentDuongDan)) { imageList = currentDuongDan.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).Where(p => !string.IsNullOrEmpty(p)).Select(path => new { path = path, url = MapRelativePathToUrl("~/" + path.TrimStart('/')), name = Path.GetFileName(path) }).Cast<object>().ToList(); } string initialImagesJson = "null"; if (imageList.Any()) { try { initialImagesJson = new JavaScriptSerializer().Serialize(imageList); } catch (Exception ex) { Debug.WriteLine($"ERROR serializing comic images: {ex.Message}"); ShowMessage("Lỗi tạo danh sách ảnh.", true); } } string script = $"if(typeof initializeComicEditor === 'function'){{ try{{ initializeComicEditor({HttpUtility.JavaScriptStringEncode(initialImagesJson, true)}); }} catch(e){{ console.error('JS Error calling initializeComicEditor:', e); }} }} else {{ console.error('initializeComicEditor function not found!'); }}"; ScriptManager.RegisterStartupScript(this, GetType(), "InitComicEditorScript_" + Guid.NewGuid(), script, true); }
        private void SetupContentPanels(string loaiSach) { bool n = loaiSach == LoaiSach_TruyenChu; bool c = loaiSach == LoaiSach_TruyenTranh; pnlNovelContent.Visible = n; pnlComicContent.Visible = c; }
        private void SetupValidatorsBasedOnBookType(string loaiSach) { bool n = loaiSach == LoaiSach_TruyenChu; bool c = loaiSach == LoaiSach_TruyenTranh; SetControlValidation(n, revFileTieuThuyet, cvNovelContentRequired); SetControlValidation(c, cvAnhTruyenRequired); }
        private void DisableForm(string reason) { txtSoChuong.Enabled = false; txtTenChuong.Enabled = false; fuFileTieuThuyet.Enabled = false; txtNoiDungChu.Enabled = false; btnSaveTenChuong.Enabled = false; btnSaveNoiDung.Enabled = false; btnSaveNoiDung.ToolTip = reason; btnCancel.Enabled = true; revFileTieuThuyet.Enabled = false; cvNovelContentRequired.Enabled = false; cvAnhTruyenRequired.Enabled = false; pnlNovelContent.Visible = false; pnlComicContent.Visible = false; Debug.WriteLine($"Form disabled (Edit). Reason: {reason}"); }
        private void SetControlValidation(bool enabled, params BaseValidator[] validators) { foreach (var v in validators) { if (v != null && v.Enabled != enabled) v.Enabled = enabled; } }

        // --- Event Handlers ---
        protected void btnSaveTenChuong_Click(object sender, EventArgs e)
        {
            int idNoiDung = CurrentIDNoiDung; int soChuong = CurrentSoChuong; string tenSach = lblBookTitleContext.Text; if (idNoiDung <= 0) { ShowMessage("Lỗi ID Chương.", true); return; }
            string newTenChuong = txtTenChuong.Text.Trim(); string originalTenChuong = hfOriginalTenChuong.Value ?? "";
            if (newTenChuong != originalTenChuong)
            {
                Debug.WriteLine($"Updating TenChuong for IDNoiDung {idNoiDung}"); try
                {
                    using (SqlConnection con = new SqlConnection(connectionString)) { con.Open(); string query = "UPDATE NoiDungSach SET TenChuong = @TenChuong, NgayTao = GETDATE() WHERE IDNoiDung = @IDNoiDung"; using (SqlCommand cmd = new SqlCommand(query, con)) { cmd.Parameters.AddWithValue("@IDNoiDung", idNoiDung); cmd.Parameters.AddWithValue("@TenChuong", OrDBNull(newTenChuong)); int rows = cmd.ExecuteNonQuery(); if (rows > 0) { ShowMessage("Đã cập nhật tên chương.", false, true); hfOriginalTenChuong.Value = newTenChuong; SetPageTitle($"Sửa Chương {soChuong} - {tenSach}"); } else { ShowMessage("Lỗi cập nhật tên chương.", true); } } }
                }
                catch (Exception ex) { ShowMessage("Lỗi CSDL: " + ex.Message, true); Debug.WriteLine($"Error saving TenChuong: {ex}"); }
            }
            else { ShowMessage("Tên chương không thay đổi.", false, true); }
        }
        protected void btnSaveNoiDung_Click(object sender, EventArgs e)
        {
            int idNoiDung = CurrentIDNoiDung; int sachId = CurrentSachID; string loaiSach = CurrentLoaiSach; int soChuong = CurrentSoChuong;
            Debug.WriteLine($"--- btnSaveNoiDung_Click STARTED (IDNoiDung: {idNoiDung}) ---");
            if (idNoiDung <= 0 || sachId <= 0 || string.IsNullOrEmpty(loaiSach) || soChuong <= 0) { ShowMessage("Lỗi: Thiếu thông tin.", true); EnableButtonsClientScript_Edit(); return; }
            SetupValidatorsBasedOnBookType(loaiSach); Page.Validate("ChapterValidation");
            if (!Page.IsValid) { ShowMessage("Vui lòng kiểm tra lỗi.", true); if (vsChapterForm != null) vsChapterForm.Style["display"] = "block"; LogValidationErrors(); EnableButtonsClientScript_Edit(); return; }
            if (vsChapterForm != null) vsChapterForm.Style["display"] = "none";
            bool success = false; string operationMessage = "";
            try { success = UpdateChapterContent(idNoiDung, sachId, soChuong, loaiSach, ref operationMessage); } // Gọi hàm cập nhật nội dung
            catch (Exception ex) { success = false; operationMessage = $"Lỗi hệ thống: {ex.Message}"; Debug.WriteLine($"CRITICAL UPDATE CONTENT EXCEPTION (IDNoiDung: {idNoiDung}): {ex.ToString()}"); }
            if (success)
            {
                string msg = operationMessage ?? $"Cập nhật nội dung chương {soChuong} thành công!"; ShowMessage(msg, false, true);
                if (!LoadChapterDataAndSetupForm(idNoiDung)) { DisableForm("Lỗi tải lại dữ liệu."); } else { EnableButtonsClientScript_Edit(); }
            }
            else { ShowMessage(operationMessage ?? "Cập nhật nội dung thất bại.", true); Debug.WriteLine($"--- btnSaveNoiDung_Click FAILED ---"); EnableButtonsClientScript_Edit(); if (loaiSach == LoaiSach_TruyenTranh) { InitializeComicEditorClientScript(hfCurrentDuongDan.Value); } }
        }
        protected void btnCancel_Click(object sender, EventArgs e) { string url = (CurrentSachID > 0) ? $"SuaNoiDungSach.aspx?id={CurrentSachID}" : "QuanLySach.aspx"; Response.Redirect(url, false); Context.ApplicationInstance.CompleteRequest(); }

        // --- Server-Side Validators ---
        protected void cvSoChuongExists_ServerValidate(object source, ServerValidateEventArgs args) { args.IsValid = true; }
        protected void cvNovelContentRequired_ServerValidate(object source, ServerValidateEventArgs args) { if (!pnlNovelContent.Visible) { args.IsValid = true; return; } bool hasNewFile = fuFileTieuThuyet.HasFiles && fuFileTieuThuyet.PostedFiles.Count > 0 && fuFileTieuThuyet.PostedFiles[0].ContentLength > 0; bool hasText = !string.IsNullOrWhiteSpace(txtNoiDungChu.Text); args.IsValid = hasNewFile || hasText; if (!args.IsValid) { ((CustomValidator)source).ErrorMessage = "Nội dung chương không được để trống."; } }
        protected void cvAnhTruyenRequired_ServerValidate(object source, ServerValidateEventArgs args) { if (!pnlComicContent.Visible) { args.IsValid = true; return; } args.IsValid = !string.IsNullOrWhiteSpace(hfComicImageOrder.Value); if (!args.IsValid) { ((CustomValidator)source).ErrorMessage = "Phải còn lại ít nhất một ảnh."; } }

        // --- Core Logic: UpdateChapterContent ---
        private bool UpdateChapterContent(int idNoiDung, int sachId, int soChuong, string loaiSach, ref string operationMessage)
        {
            Debug.WriteLine($"--- UpdateChapterContent START (IDNoiDung: {idNoiDung}) ---");
           string loaiNoiDungDb = "";
            string updatedDuongDanDb = null; string physicalChapterPath = ""; string physicalTempPath = "";
            List<string> finalPathsCreatedOrMoved = new List<string>(); List<string> processedTempFileNames = new List<string>(); List<string> originalFilesToDeletePhysically = new List<string>();
            bool contentDataChanged = false;

            // A. Chuẩn bị đường dẫn
            try { physicalChapterPath = Server.MapPath(BookContentVirtualBasePath + $"Sach_{sachId}/Chuong_{soChuong}/"); if (!Directory.Exists(physicalChapterPath)) Directory.CreateDirectory(physicalChapterPath); physicalTempPath = Server.MapPath(TempUploadVirtualPath); if (!Directory.Exists(physicalTempPath)) Directory.CreateDirectory(physicalTempPath); }
            catch (Exception ex) { operationMessage = $"Lỗi tạo thư mục: {ex.Message}"; return false; }

            // B. Xử lý Nội dung
            try
            {
                string originalDbDuongDan = hfCurrentDuongDan.Value ?? "";
                if (loaiSach.Equals(LoaiSach_TruyenChu, StringComparison.OrdinalIgnoreCase))
                {
                    #region Update Novel Logic
                   loaiNoiDungDb = LoaiNoiDung_File; 
                    string oldText = hfOriginalNovelText.Value ?? ""; string newText = txtNoiDungChu.Text.Trim(); bool hasNewFileUpload = fuFileTieuThuyet.HasFiles && fuFileTieuThuyet.PostedFiles.Count > 0 && fuFileTieuThuyet.PostedFiles[0].ContentLength > 0; bool textHasChanged = newText != oldText.Trim(); string currentTildePathToDelete = null; if (!string.IsNullOrWhiteSpace(originalDbDuongDan)) { currentTildePathToDelete = "~/" + originalDbDuongDan.TrimStart('/'); }
                    contentDataChanged = hasNewFileUpload || textHasChanged;
                    if (hasNewFileUpload) { HttpPostedFile file = fuFileTieuThuyet.PostedFiles[0]; if (!ValidateFile(file, (long)MaxFileSizeNovelMb * 1024 * 1024, AllowedNovelExtensions, out string valError)) { operationMessage = $"File mới không hợp lệ: {valError}"; return false; } if (currentTildePathToDelete != null) { originalFilesToDeletePhysically.Add(currentTildePathToDelete); } string uniqueFileName = $"content_{Guid.NewGuid().ToString("N")}{Path.GetExtension(file.FileName)}"; string physicalSavePath = Path.Combine(physicalChapterPath, uniqueFileName); string relativeSavePath = BookContentVirtualBasePath + $"Sach_{sachId}/Chuong_{soChuong}/{uniqueFileName}"; file.SaveAs(physicalSavePath); finalPathsCreatedOrMoved.Add(relativeSavePath); updatedDuongDanDb = relativeSavePath.TrimStart('~'); }
                    else if (textHasChanged) { if (currentTildePathToDelete != null) { originalFilesToDeletePhysically.Add(currentTildePathToDelete); } if (!string.IsNullOrWhiteSpace(newText)) { string uniqueTextFileName = $"content_{Guid.NewGuid().ToString("N")}.txt"; string physicalSavePath = Path.Combine(physicalChapterPath, uniqueTextFileName); string relativeSavePath = BookContentVirtualBasePath + $"Sach_{sachId}/Chuong_{soChuong}/{uniqueTextFileName}"; File.WriteAllText(physicalSavePath, newText, Encoding.UTF8); finalPathsCreatedOrMoved.Add(relativeSavePath); updatedDuongDanDb = relativeSavePath.TrimStart('~'); } else { operationMessage = "Nội dung không được trống."; return false; } }
                    else { updatedDuongDanDb = originalDbDuongDan; }
                    #endregion
                }
                else if (loaiSach.Equals(LoaiSach_TruyenTranh, StringComparison.OrdinalIgnoreCase))
                {
                    #region Update Comic Logic (Dropzone - Two Stage Rename)
                    loaiNoiDungDb = LoaiNoiDung_Image; 
                    string finalOrderString = hfComicImageOrder.Value ?? ""; string deleteString = hfComicImagesToDelete.Value ?? ""; string replaceJson = hfComicImagesToReplace.Value ?? "[]";
                    List<string> finalOrderIdentifiers = finalOrderString.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList(); List<string> pathsToDelete = deleteString.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).ToList(); List<ComicReplacement> replacements = new List<ComicReplacement>(); try { replacements = new JavaScriptSerializer().Deserialize<List<ComicReplacement>>(replaceJson) ?? new List<ComicReplacement>(); } catch { }
                    contentDataChanged = (pathsToDelete.Any() || replacements.Any() || finalOrderIdentifiers.Any(id => !originalDbDuongDan.Contains(id)) || finalOrderString != originalDbDuongDan);
                    if (!contentDataChanged) { updatedDuongDanDb = originalDbDuongDan; Debug.WriteLine("Comic unchanged."); }
                    else
                    {
                        Debug.WriteLine("Comic changed, processing..."); List<string> finalRelativeDbPaths = new List<string>(); int pageCounter = 1; bool fileProcessingError = false; Dictionary<string, string> sourceToFinalMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase); Dictionary<string, string> sourceToTempMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase); List<string> tempPhysicalFilesCreatedForRename = new List<string>();
                        foreach (string ptd in pathsToDelete) { originalFilesToDeletePhysically.Add("~/" + ptd.TrimStart('/')); }
                        foreach (var rep in replacements) { originalFilesToDeletePhysically.Add("~/" + rep.originalPath.TrimStart('/')); }
                        // Stage 1
                        foreach (string identifier in finalOrderIdentifiers) { try { string finalFileName = ""; string finalPhysicalPath = ""; string sourcePhysicalPath = ""; bool isTempFile = false; string tempFileName = null; ComicReplacement replacement = replacements.FirstOrDefault(r => r.tempFileName == identifier); bool isOriginalPath = originalDbDuongDan.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Any(p => p.Trim().Equals(identifier, StringComparison.OrdinalIgnoreCase)); if (replacement != null) { isTempFile = true; tempFileName = replacement.tempFileName; sourcePhysicalPath = Path.Combine(physicalTempPath, tempFileName); } else if (isOriginalPath && !pathsToDelete.Contains(identifier, StringComparer.OrdinalIgnoreCase)) { isTempFile = false; sourcePhysicalPath = MapVirtualToPhysicalPath("~/" + identifier.TrimStart('/')); } else { isTempFile = true; tempFileName = identifier; sourcePhysicalPath = Path.Combine(physicalTempPath, tempFileName); } if (!File.Exists(sourcePhysicalPath)) throw new FileNotFoundException($"Nguồn '{Path.GetFileName(sourcePhysicalPath)}' ko tồn tại.", sourcePhysicalPath); FileInfo fi = new FileInfo(sourcePhysicalPath); if (fi.Length <= 0) throw new Exception($"File '{tempFileName ?? identifier}' 0 byte."); string extCheck = fi.Extension.ToLowerInvariant(); if (!AllowedImageExtensions.Contains(extCheck)) throw new Exception($"File '{tempFileName ?? identifier}' định dạng sai."); if (fi.Length > (long)MaxFileSizePerImageMb * 1024 * 1024) throw new Exception($"File '{tempFileName ?? identifier}' quá lớn."); string extension = Path.GetExtension(sourcePhysicalPath); finalFileName = $"page_{pageCounter:D3}{extension}"; finalPhysicalPath = Path.Combine(physicalChapterPath, finalFileName); sourceToFinalMap[sourcePhysicalPath] = finalPhysicalPath; if (!string.Equals(sourcePhysicalPath, finalPhysicalPath, StringComparison.OrdinalIgnoreCase)) { string tempRenamePath = Path.Combine(physicalChapterPath, $"temp_{Guid.NewGuid():N}{extension}"); File.Move(sourcePhysicalPath, tempRenamePath); sourceToTempMap[sourcePhysicalPath] = tempRenamePath; tempPhysicalFilesCreatedForRename.Add(tempRenamePath); if (isTempFile) processedTempFileNames.Add(tempFileName); } else { string finalTildePath = BookContentVirtualBasePath + $"Sach_{sachId}/Chuong_{soChuong}/{finalFileName}"; finalPathsCreatedOrMoved.Add(finalTildePath); finalRelativeDbPaths.Add(finalTildePath.TrimStart('~')); if (isTempFile) processedTempFileNames.Add(tempFileName); } pageCounter++; } catch (Exception stage1Ex) { operationMessage = $"Lỗi GĐ1 file '{identifier}': {stage1Ex.Message}"; fileProcessingError = true; break; } }
                        if (fileProcessingError) { RollbackRenamesFromTemp(sourceToTempMap); DeletePhysicalFilesOnError(tempPhysicalFilesCreatedForRename); DeleteFilesSafe(finalPathsCreatedOrMoved); return false; }
                        // Stage 2
                        foreach (var kvp in sourceToTempMap) { string originalSource = kvp.Key; string tempRenamePath = kvp.Value; string finalTargetPath = sourceToFinalMap[originalSource]; string finalTildePath = BookContentVirtualBasePath + $"Sach_{sachId}/Chuong_{soChuong}/{Path.GetFileName(finalTargetPath)}"; try { if (File.Exists(finalTargetPath)) File.Delete(finalTargetPath); File.Move(tempRenamePath, finalTargetPath); finalPathsCreatedOrMoved.Add(finalTildePath); finalRelativeDbPaths.Add(finalTildePath.TrimStart('~')); } catch (Exception stage2Ex) { operationMessage = $"Lỗi GĐ2 đổi tên thành '{Path.GetFileName(finalTargetPath)}': {stage2Ex.Message}"; fileProcessingError = true; break; } }
                        if (fileProcessingError) { DeleteFilesSafe(finalPathsCreatedOrMoved); DeletePhysicalFilesOnError(tempPhysicalFilesCreatedForRename); return false; }
                        if (!finalRelativeDbPaths.Any()) { operationMessage = "Lỗi: Không còn ảnh hợp lệ."; DeleteDirectoryIfEmptySafe(physicalChapterPath); return false; }
                        finalRelativeDbPaths.Sort((a, b) => { try { int numA = int.Parse(Path.GetFileNameWithoutExtension(a).Split('_').Last()); int numB = int.Parse(Path.GetFileNameWithoutExtension(b).Split('_').Last()); return numA.CompareTo(numB); } catch { return 0; } }); updatedDuongDanDb = string.Join(",", finalRelativeDbPaths);
                    } // End if(contentDataChanged) for Comic
                    #endregion
                }
                else { /* Loại sách không hỗ trợ */ }

                // --- C. Cập nhật CSDL ---
                // *** Bỏ kiểm tra metadataChanged ở đây ***
                if (!contentDataChanged)
                { // Chỉ kiểm tra nội dung
                    operationMessage = "Nội dung không có thay đổi."; CleanUpTempFiles(processedTempFileNames); DeleteFilesSafe(originalFilesToDeletePhysically.Distinct(StringComparer.OrdinalIgnoreCase).ToList()); return true;
                }

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open(); using (SqlTransaction transaction = con.BeginTransaction())
                    {
                        try
                        {
                            // *** Chỉ UPDATE DuongDan và NgayTao ***
                            string updateSql = "UPDATE NoiDungSach SET DuongDan = @DuongDan, NgayTao = GETDATE() WHERE IDNoiDung = @IDNoiDung";
                            using (SqlCommand cmd = new SqlCommand(updateSql, con, transaction))
                            {
                                cmd.Parameters.AddWithValue("@IDNoiDung", idNoiDung);
                                cmd.Parameters.AddWithValue("@DuongDan", OrDBNull(updatedDuongDanDb));
                                int rows = cmd.ExecuteNonQuery();
                                if (rows > 0) { transaction.Commit(); Debug.WriteLine($"DB Content Update Success."); DeleteFilesSafe(originalFilesToDeletePhysically.Distinct(StringComparer.OrdinalIgnoreCase).ToList()); CleanUpTempFiles(processedTempFileNames); operationMessage = $"Cập nhật nội dung chương {soChuong} thành công!"; hfCurrentDuongDan.Value = updatedDuongDanDb ?? ""; /* Cập nhật hidden field sau thành công */ return true; }
                                else { transaction.Rollback(); operationMessage = "Lỗi cập nhật CSDL."; DeleteFilesSafe(finalPathsCreatedOrMoved); return false; }
                            }
                        }
                        catch (Exception dbEx) { try { transaction.Rollback(); } catch { } operationMessage = "Lỗi CSDL: " + dbEx.Message; DeleteFilesSafe(finalPathsCreatedOrMoved); return false; }
                    }
                } // End using con
            }
            catch (Exception ex) { operationMessage = "Lỗi xử lý nội dung: " + ex.Message; Debug.WriteLine($"Error UpdateChapterContent: {ex}"); DeleteFilesSafe(finalPathsCreatedOrMoved); return false; }
        }


        // --- Helper Methods ---
        private int GetMaxChapterNumber(int bookId) { if (bookId <= 0) return 0; int m = 0; using (var c = new SqlConnection(connectionString)) { const string q = "SELECT ISNULL(MAX(SoChuong), 0) FROM NoiDungSach WHERE IDSach=@I"; using (var k = new SqlCommand(q, c)) { k.Parameters.AddWithValue("@I", bookId); try { c.Open(); var r = k.ExecuteScalar(); if (r != null && r != DBNull.Value) int.TryParse(r.ToString(), out m); } catch { m = -1; } } } return m; }
        private int GetChapterNumberById(int idNoiDung) { if (idNoiDung <= 0) return 0; using (var c = new SqlConnection(connectionString)) { const string q = "SELECT SoChuong FROM NoiDungSach WHERE IDNoiDung=@I"; using (var k = new SqlCommand(q, c)) { k.Parameters.AddWithValue("@I", idNoiDung); try { c.Open(); var r = k.ExecuteScalar(); return (r == DBNull.Value || r == null) ? 0 : Convert.ToInt32(r); } catch { return 0; } } } }
        private string GetChapterNameById(int idNoiDung) { if (idNoiDung <= 0) return ""; using (var c = new SqlConnection(connectionString)) { const string q = "SELECT TenChuong FROM NoiDungSach WHERE IDNoiDung=@I"; using (var k = new SqlCommand(q, c)) { k.Parameters.AddWithValue("@I", idNoiDung); try { c.Open(); var r = k.ExecuteScalar(); return r == DBNull.Value || r == null ? "" : r.ToString().Trim(); } catch { return ""; } } } }
        private bool ValidateFile(HttpPostedFile f, long maxSz, string[] allowExt, out string err) { err = ""; if (f == null || f.ContentLength <= 0) { err = "File rỗng."; return false; } string fn; try { fn = Path.GetFileName(f.FileName); if (string.IsNullOrWhiteSpace(fn)) { err = "Tên file lỗi."; return false; } } catch { err = "Tên file lỗi."; return false; } if (f.ContentLength > maxSz) { err = $"File '{HttpUtility.HtmlEncode(fn)}' > {FormatFileSize(maxSz)}."; return false; } string ext = Path.GetExtension(fn)?.ToLowerInvariant(); if (string.IsNullOrEmpty(ext) || !allowExt.Contains(ext, StringComparer.OrdinalIgnoreCase)) { err = $"Định dạng '{ext ?? "N/A"}' không hợp lệ."; return false; } return true; }
        private void DeleteFilesSafe(List<string> paths) { if (paths == null || !paths.Any()) return; foreach (var p in paths.Distinct(StringComparer.OrdinalIgnoreCase)) DeleteSingleFileSafe(p); }
        private void DeleteSingleFileSafe(string pathTilde) { if (string.IsNullOrWhiteSpace(pathTilde)) return; string pT = pathTilde.Trim(); if (!pT.StartsWith("~")) pT = "~" + (pT.StartsWith("/") ? "" : "/") + pT; bool allow = pT.StartsWith(BookContentVirtualBasePath, StringComparison.OrdinalIgnoreCase) || pT.StartsWith(TempUploadVirtualPath, StringComparison.OrdinalIgnoreCase); if (!allow) { Debug.WriteLine($"SEC WARN DeleteSingle: Denied '{pT}'"); return; } string pp = null; try { pp = Server.MapPath(pT); if (File.Exists(pp)) { File.SetAttributes(pp, FileAttributes.Normal); File.Delete(pp); Debug.WriteLine($"Deleted: {pp}"); } } catch (Exception ex) when (ex is FileNotFoundException || ex is DirectoryNotFoundException) { } catch (Exception ex) { Debug.WriteLine($"ERR Delete '{pp ?? pT}': {ex.Message}"); } }
        private void DeleteDirectoryIfEmptySafe(string physDir) { if (string.IsNullOrWhiteSpace(physDir)) return; string physBase = ""; try { physBase = Server.MapPath(BookContentVirtualBasePath); } catch { return; } if (string.IsNullOrEmpty(physBase) || !physDir.StartsWith(physBase, StringComparison.OrdinalIgnoreCase) || physDir.Equals(physBase, StringComparison.OrdinalIgnoreCase)) { Debug.WriteLine($"SEC WARN DeleteDir: Denied '{physDir}'"); return; } try { if (Directory.Exists(physDir) && !Directory.EnumerateFileSystemEntries(physDir).Any()) { Directory.Delete(physDir, false); Debug.WriteLine($"Deleted empty dir: {physDir}"); } } catch (Exception ex) when (ex is DirectoryNotFoundException) { } catch (Exception ex) { Debug.WriteLine($"ERR Check/Del dir '{physDir}': {ex.Message}"); } }
        private static string FormatFileSize(long b) { if (b < 0) return "N/A"; if (b == 0) return "0 Bytes"; int i = 0; double d = (double)b; while (d >= 1024 && i < 4) { d /= 1024; i++; } return $"{d:0.#} {new[] { "Bytes", "KB", "MB", "GB", "TB" }[i]}"; }
        private void ShowMessage(string msg, bool isError, bool autoClear = false, int delay = 3000) { if (pnlMessage == null || lblFormMessage == null) return; pnlMessage.CssClass = "message-panel " + (isError ? "message-error" : "message-success"); lblFormMessage.Text = $"<i class='fas fa-{(isError ? "times" : "check")}-circle'></i> {HttpUtility.HtmlEncode(msg ?? "...")}"; pnlMessage.Visible = true; if (autoClear && ScriptManager.GetCurrent(Page) != null) { var k = $"ClearMsg_{Guid.NewGuid()}"; var s = $"window.setTimeout(function(){{var el=document.getElementById('{pnlMessage.ClientID}'); if(el){{el.style.transition='opacity 0.3s ease-out'; el.style.opacity='0'; window.setTimeout(function(){{el.style.display='none'; el.style.opacity='1';}},300);}} }}, {delay});"; ScriptManager.RegisterStartupScript(this, GetType(), k, s, true); } } // Đảm bảo gọi static
        private void ShowMessageAndRedirect(string msg, string url, bool isError) { ShowMessage(msg + " Đang chuyển hướng...", isError); DisableForm("Chuyển hướng..."); string rUrl = "#"; try { rUrl = ResolveClientUrl(url); } catch { } string s = $"window.setTimeout(function(){{window.location.href='{rUrl}';}},2000);"; var sm = ScriptManager.GetCurrent(Page); if (sm != null) ScriptManager.RegisterStartupScript(this, GetType(), "Redirect_" + Guid.NewGuid(), s, true); else Page.ClientScript.RegisterStartupScript(GetType(), "Redirect_" + Guid.NewGuid(), s, true); } // Đảm bảo gọi static
        private void EnableButtonsClientScript_Edit() { string script = $"var btnSaveContent=document.getElementById('{btnSaveNoiDung?.ClientID}');var btnCancel=document.getElementById('{btnCancel?.ClientID}');var btnSaveName=document.getElementById('{btnSaveTenChuong?.ClientID}'); if(btnSaveContent){{btnSaveContent.disabled=false;btnSaveContent.classList.remove('loading-spinner');btnSaveContent.value='Lưu Nội Dung';}} if(btnCancel){{btnCancel.disabled=false;}} if(btnSaveName){{btnSaveName.disabled=false;}}"; var sm = ScriptManager.GetCurrent(Page); if (sm != null) ScriptManager.RegisterStartupScript(this, GetType(), "EnableBtns_" + Guid.NewGuid(), script, true); else Page.ClientScript.RegisterStartupScript(GetType(), "EnableBtns_" + Guid.NewGuid(), script, true); } // Đảm bảo gọi static
        private void LogValidationErrors() { if (Page.IsValid) return; Debug.WriteLine("--- Server Validation Errors ---"); foreach (IValidator v in Page.GetValidators("ChapterValidation")) { if (!v.IsValid) { string c = (v is BaseValidator b) ? b.ControlToValidate : "N/A"; string i = (v as Control)?.ID ?? "N/A"; Debug.WriteLine($"- V ({i}): {v.ErrorMessage} [Ctrl:{c}]"); } } Debug.WriteLine("--- End Validation Errors ---"); }
        private void SetPageTitle(string title) { if (Master is Admin m) m.SetPageTitle(title); else Page.Title = title; }
        private string GetString(IDataRecord r, string col, string d = "") { try { int o = r.GetOrdinal(col); return r.IsDBNull(o) ? d : r.GetString(o).Trim(); } catch { return d; } }
        private object OrDBNull(string v) { return string.IsNullOrWhiteSpace(v) ? DBNull.Value : (object)v.Trim(); }
        private void SetNoCacheHeaders() { Response.Cache.SetCacheability(HttpCacheability.NoCache); Response.Cache.SetNoStore(); Response.Cache.SetExpires(DateTime.UtcNow.AddYears(-1)); Response.Cache.SetNoTransforms(); }
        private string MapVirtualToPhysicalPath(string virtPath) { if (string.IsNullOrWhiteSpace(virtPath)) throw new ArgumentNullException(nameof(virtPath)); if (!virtPath.StartsWith("~")) virtPath = "~" + (virtPath.StartsWith("/") ? "" : "/") + virtPath; try { string p = Server.MapPath(virtPath); if (p == null) throw new InvalidOperationException($"MapPath null for '{virtPath}'."); return p; } catch (Exception ex) { throw new InvalidOperationException($"MapPath error '{virtPath}': {ex.Message}", ex); } }
        private string MapRelativePathToUrl(string pathTilde) { try { return Page.ResolveClientUrl(pathTilde); } catch { return "#error"; } }
        private void CleanUpTempFiles(List<string> tempFiles) { if (tempFiles == null || !tempFiles.Any()) return; string physTemp = ""; try { physTemp = Server.MapPath(TempUploadVirtualPath); } catch { return; } if (!Directory.Exists(physTemp)) return; Debug.WriteLine($"Cleaning {tempFiles.Count} temp files..."); foreach (string fName in tempFiles) { if (string.IsNullOrWhiteSpace(fName) || fName.Contains("..") || fName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) continue; string fp = Path.Combine(physTemp, fName); try { if (File.Exists(fp)) File.Delete(fp); } catch (Exception ex) { Debug.WriteLine($"ERR deleting temp '{fp}':{ex.Message}"); } } Debug.WriteLine("Cleanup done."); }
        private void RollbackRenamesFromTemp(Dictionary<string, string> map) { Debug.WriteLine($"Rolling back {map.Count} renames..."); foreach (var kvp in map) { string src = kvp.Key; string tmp = kvp.Value; try { if (File.Exists(tmp)) { if (!File.Exists(src)) File.Move(tmp, src); else File.Delete(tmp); } } catch (Exception ex) { Debug.WriteLine($"ERR Rollback '{tmp}' to '{src}': {ex.Message}"); } } }
        private void DeletePhysicalFilesOnError(List<string> physPaths) { if (physPaths == null || !physPaths.Any()) return; foreach (string p in physPaths.Distinct(StringComparer.OrdinalIgnoreCase)) { try { if (File.Exists(p)) { File.SetAttributes(p, FileAttributes.Normal); File.Delete(p); } } catch (Exception ex) { Debug.WriteLine($"ERR deleting phys '{p}': {ex.Message}"); } } }

        // --- Class phụ trợ ---
        [Serializable] private class ComicReplacement { public string originalPath { get; set; } public string tempFileName { get; set; } }

    } // End Class
} // End Namespace