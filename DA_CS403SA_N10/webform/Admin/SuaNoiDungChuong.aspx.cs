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
using System.Web.Script.Serialization;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Webebook.WebForm.Admin
{
    public partial class SuaNoiDungChuong : System.Web.UI.Page
    {
        // --- Constants ---
        private const string LoaiSach_TruyenTranh = "Truyện Tranh";
        private const string LoaiSach_TruyenChu = "Truyện Chữ";
        private const string BookContentVirtualBasePath = "~/BookContent/";
        private const string TempUploadVirtualPath = "~/Uploads/Temp/";
        protected const int MaxFileSizePerImageMb = 5;
        protected const int MaxFileSizeNovelMb = 10;
        protected readonly string[] AllowedNovelExtensions = { ".txt" };
        protected readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["datawebebookConnectionString"].ConnectionString;

        // --- Page Properties ---
        protected int CurrentIDNoiDung { get { return ViewState["Edit_IDNoiDung"] != null ? (int)ViewState["Edit_IDNoiDung"] : 0; } set { ViewState["Edit_IDNoiDung"] = value; } }
        protected int CurrentSachID { get { return ViewState["Edit_SachID"] != null ? (int)ViewState["Edit_SachID"] : 0; } set { ViewState["Edit_SachID"] = value; } }
        protected string CurrentLoaiSach { get { return ViewState["Edit_LoaiSach"] as string; } set { ViewState["Edit_LoaiSach"] = value; } }
        protected int CurrentSoChuong { get { return ViewState["Edit_SoChuong"] != null ? (int)ViewState["Edit_SoChuong"] : 0; } set { ViewState["Edit_SoChuong"] = value; } }

        // --- Page Lifecycle ---
        protected void Page_Load(object sender, EventArgs e)
        {
            int idNoiDung;
            if (!int.TryParse(Request.QueryString["id"], out idNoiDung) || idNoiDung <= 0) { if (!int.TryParse(hfIDNoiDung.Value, out idNoiDung) || idNoiDung <= 0) { ShowMessageAndRedirect("Thiếu ID Nội dung hợp lệ.", "QuanLySach.aspx", true); return; } }
            CurrentIDNoiDung = idNoiDung; hfIDNoiDung.Value = idNoiDung.ToString();

            int sachId;
            if (!int.TryParse(Request.QueryString["sachId"], out sachId) || sachId <= 0) { if (!int.TryParse(hfSachID.Value, out sachId) || sachId <= 0) { if (!IsPostBack) { } else { ShowMessage("Lỗi nghiêm trọng: Mất ID Sách.", true); DisableForm("Lỗi."); return; } } }
            if (sachId > 0) { CurrentSachID = sachId; hfSachID.Value = sachId.ToString(); }

            if (!IsPostBack) { if (!LoadChapterDataAndSetupForm(CurrentIDNoiDung)) { DisableForm("Lỗi tải dữ liệu chương."); } }

            if (CurrentSachID > 0) { hlBackToList.NavigateUrl = $"~/WebForm/Admin/SuaNoiDungSach.aspx?id={CurrentSachID}"; SetupValidatorsBasedOnBookType(CurrentLoaiSach); } else { DisableForm("Không xác định được sách."); }
        }

        // --- Data Loading ---
        private bool LoadChapterDataAndSetupForm(int idNoiDung)
        {
            using (var con = new SqlConnection(connectionString))
            {
                const string query = "SELECT nds.IDSach, nds.SoChuong, nds.TenChuong, nds.DuongDan, s.TenSach, s.LoaiSach FROM NoiDungSach nds JOIN Sach s ON nds.IDSach = s.IDSach WHERE nds.IDNoiDung = @IDNoiDung";
                using (var cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@IDNoiDung", idNoiDung);
                    try
                    {
                        con.Open();
                        using (var reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                CurrentSachID = reader.GetInt32(reader.GetOrdinal("IDSach")); CurrentLoaiSach = GetString(reader, "LoaiSach"); CurrentSoChuong = reader.GetInt32(reader.GetOrdinal("SoChuong"));
                                string tenChuong = GetString(reader, "TenChuong"); string duongDan = GetString(reader, "DuongDan"); string tenSach = GetString(reader, "TenSach");
                                hfSachID.Value = CurrentSachID.ToString(); hfLoaiSach.Value = CurrentLoaiSach; hfCurrentDuongDan.Value = duongDan; hfOriginalTenChuong.Value = tenChuong;
                                lblBookTitleContext.Text = HttpUtility.HtmlEncode(tenSach); lblSachIDContext.Text = CurrentSachID.ToString(); lblLoaiSachContext.Text = HttpUtility.HtmlEncode(CurrentLoaiSach);
                                txtSoChuong.Text = CurrentSoChuong.ToString(); txtTenChuong.Text = tenChuong;
                                SetPageTitle($"Sửa Chương {CurrentSoChuong} - {HttpUtility.HtmlEncode(tenSach)}");
                                SetupFormForEdit(CurrentLoaiSach, duongDan);
                                return true;
                            }
                        }
                    }
                    catch (Exception ex) { ShowMessage("Lỗi tải dữ liệu: " + ex.Message, true); Debug.WriteLine(ex); return false; }
                }
            }
            return false;
        }

        private void SetupFormForEdit(string loaiSach, string duongDan)
        {
            SetupContentPanels(loaiSach);
            if (loaiSach.Equals(LoaiSach_TruyenChu, StringComparison.OrdinalIgnoreCase))
            {
                string text = "";
                if (!string.IsNullOrEmpty(duongDan))
                {
                    try
                    {
                        text = File.ReadAllText(MapVirtualToPhysicalPath(duongDan), Encoding.UTF8);
                        pnlExistingNovelFile.Visible = true;
                        hlCurrentNovelFile.Text = Path.GetFileName(duongDan);
                        hlCurrentNovelFile.NavigateUrl = MapRelativePathToUrl(duongDan);
                    }
                    catch (Exception ex) { lblNovelFileReadError.Text = "Lỗi đọc file: " + ex.Message; lblNovelFileReadError.Visible = true; }
                }
                txtNoiDungChu.Text = text; hfOriginalNovelText.Value = text;
            }
            else if (loaiSach.Equals(LoaiSach_TruyenTranh, StringComparison.OrdinalIgnoreCase))
            {
                InitializeComicEditorClientScript(duongDan);
            }
        }

        private void InitializeComicEditorClientScript(string duongDan)
        {
            var imageList = string.IsNullOrWhiteSpace(duongDan) ? new List<object>() :
                duongDan.Split(',').Select(p => p.Trim()).Where(p => !string.IsNullOrEmpty(p))
                .Select(path => new { path = path, url = MapRelativePathToUrl(path), name = Path.GetFileName(path) })
                .Cast<object>().ToList();
            string json = new JavaScriptSerializer().Serialize(imageList);
            ScriptManager.RegisterStartupScript(this, GetType(), "InitComic", $"initializeComicEditor({json});", true);
        }

        // --- Event Handlers ---
        protected void btnSaveTenChuong_Click(object sender, EventArgs e)
        {
            if (CurrentIDNoiDung <= 0) { ShowMessage("Lỗi không xác định được ID chương.", true); return; }
            string newTenChuong = txtTenChuong.Text.Trim();
            if (newTenChuong == hfOriginalTenChuong.Value) { ShowMessage("Tên chương không có thay đổi.", false, true); return; }
            try
            {
                using (var con = new SqlConnection(connectionString))
                {
                    con.Open();
                    const string query = "UPDATE NoiDungSach SET TenChuong = @TenChuong, NgayTao = GETDATE() WHERE IDNoiDung = @IDNoiDung";
                    using (var cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@IDNoiDung", CurrentIDNoiDung);
                        cmd.Parameters.AddWithValue("@TenChuong", OrDBNull(newTenChuong));
                        if (cmd.ExecuteNonQuery() > 0) { ShowMessage("Cập nhật tên chương thành công.", false, true); hfOriginalTenChuong.Value = newTenChuong; }
                        else { ShowMessage("Không tìm thấy chương để cập nhật.", true); }
                    }
                }
            }
            catch (Exception ex) { ShowMessage("Lỗi CSDL khi lưu tên: " + ex.Message, true); Debug.WriteLine(ex); }
        }

        protected void btnSaveNoiDung_Click(object sender, EventArgs e)
        {
            if (CurrentIDNoiDung <= 0) { ShowMessage("Lỗi ID Chương.", true); EnableButtonsClientScript_Edit(); return; }
            Page.Validate("ChapterValidation");
            if (!Page.IsValid) { ShowMessage("Dữ liệu không hợp lệ.", true); vsChapterForm.Style["display"] = "block"; EnableButtonsClientScript_Edit(); return; }
            vsChapterForm.Style["display"] = "none";

            string opMessage = "";
            bool success = false;
            try { success = UpdateChapterNameAndContent(ref opMessage); }
            catch (Exception ex) { opMessage = "Lỗi hệ thống: " + ex.Message; Debug.WriteLine(ex); }

            if (success)
            {
                ShowMessage(opMessage, false, true);
                LoadChapterDataAndSetupForm(CurrentIDNoiDung);
            }
            else { ShowMessage(opMessage, true); }
            EnableButtonsClientScript_Edit();
            if (CurrentLoaiSach == LoaiSach_TruyenTranh && !success) { InitializeComicEditorClientScript(hfCurrentDuongDan.Value); }
        }

        protected void btnCancel_Click(object sender, EventArgs e)
        {
            string url = (CurrentSachID > 0) ? $"SuaNoiDungSach.aspx?id={CurrentSachID}" : "QuanLySach.aspx";
            Response.Redirect(url, false);
            Context.ApplicationInstance.CompleteRequest();
        }

        // --- Core Update Logic ---
        private bool UpdateChapterNameAndContent(ref string opMessage)
        {
            string newTenChuong = txtTenChuong.Text.Trim();
            bool tenChuongChanged = newTenChuong != (hfOriginalTenChuong.Value ?? "");
            ContentChanges changes = PrepareContentChanges(ref opMessage);
            if (changes == null) return false;
            if (!tenChuongChanged && !changes.HasContentChanged) { opMessage = "Không có thay đổi nào để lưu."; return true; }

            using (var con = new SqlConnection(connectionString))
            {
                con.Open();
                using (var trans = con.BeginTransaction())
                {
                    try
                    {
                        var sb = new StringBuilder("UPDATE NoiDungSach SET NgayTao = GETDATE()");
                        var cmd = new SqlCommand { Connection = con, Transaction = trans };
                        if (tenChuongChanged) { sb.Append(", TenChuong = @TenChuong"); cmd.Parameters.AddWithValue("@TenChuong", OrDBNull(newTenChuong)); }
                        if (changes.HasContentChanged) { sb.Append(", DuongDan = @DuongDan"); cmd.Parameters.AddWithValue("@DuongDan", OrDBNull(changes.NewDuongDanDb)); }
                        sb.Append(" WHERE IDNoiDung = @IDNoiDung");
                        cmd.CommandText = sb.ToString();
                        cmd.Parameters.AddWithValue("@IDNoiDung", CurrentIDNoiDung);
                        if (cmd.ExecuteNonQuery() > 0)
                        {
                            trans.Commit();
                            if (changes.HasContentChanged) FinalizeFileOperations(changes);
                            opMessage = "Lưu thành công!";
                            return true;
                        }
                        else { trans.Rollback(); opMessage = "Lỗi: Không tìm thấy chương trong CSDL."; CleanUpTempFiles(changes.ProcessedTempFileNames); return false; }
                    }
                    catch (Exception ex) { trans.Rollback(); opMessage = "Lỗi CSDL: " + ex.Message; Debug.WriteLine(ex); CleanUpTempFiles(changes.ProcessedTempFileNames); return false; }
                }
            }
        }

        private ContentChanges PrepareContentChanges(ref string errorMessage)
        {
            var changes = new ContentChanges();
            string physicalChapterPath = MapVirtualToPhysicalPath(BookContentVirtualBasePath + $"Sach_{CurrentSachID}/Chuong_{CurrentSoChuong}/");
            Directory.CreateDirectory(physicalChapterPath);

            if (CurrentLoaiSach.Equals(LoaiSach_TruyenChu, StringComparison.OrdinalIgnoreCase))
            {
                string oldText = hfOriginalNovelText.Value ?? ""; string newText = txtNoiDungChu.Text.Trim(); bool hasNewFile = fuFileTieuThuyet.HasFile; bool textChanged = newText != oldText;
                changes.HasContentChanged = hasNewFile || textChanged;
                if (!changes.HasContentChanged) return changes;
                if (!string.IsNullOrEmpty(hfCurrentDuongDan.Value)) changes.FilesToDelete.Add(hfCurrentDuongDan.Value);
                if (hasNewFile)
                {
                    string uniqueFileName = $"content_{Guid.NewGuid():N}{Path.GetExtension(fuFileTieuThuyet.FileName)}";
                    changes.NewDuongDanDb = $"/BookContent/Sach_{CurrentSachID}/Chuong_{CurrentSoChuong}/{uniqueFileName}";
                    changes.ActionToFinalize = () => fuFileTieuThuyet.SaveAs(Path.Combine(physicalChapterPath, uniqueFileName));
                }
                else if (textChanged)
                {
                    string uniqueFileName = $"content_{Guid.NewGuid():N}.txt";
                    changes.NewDuongDanDb = $"/BookContent/Sach_{CurrentSachID}/Chuong_{CurrentSoChuong}/{uniqueFileName}";
                    changes.ActionToFinalize = () => File.WriteAllText(Path.Combine(physicalChapterPath, uniqueFileName), newText, Encoding.UTF8);
                }
                return changes;
            }
            else if (CurrentLoaiSach.Equals(LoaiSach_TruyenTranh, StringComparison.OrdinalIgnoreCase))
            {
                string orderStr = hfComicImageOrder.Value ?? "";
                changes.HasContentChanged = (orderStr != hfCurrentDuongDan.Value);
                if (!changes.HasContentChanged) return changes;
                var finalIdentifiers = orderStr.Split(',').Where(s => !string.IsNullOrEmpty(s)).ToList();
                if (!finalIdentifiers.Any())
                {
                    changes.NewDuongDanDb = null;
                    changes.ActionToFinalize = () => { foreach (var ext in AllowedImageExtensions) foreach (var file in Directory.GetFiles(physicalChapterPath, $"*{ext}")) File.Delete(file); };
                    return changes;
                }
                string physicalTempUploadPath = MapVirtualToPhysicalPath(TempUploadVirtualPath);
                var sourceFiles = new List<string>();
                try
                {
                    foreach (string id in finalIdentifiers)
                    {
                        string tempPath = Path.Combine(physicalTempUploadPath, id);
                        if (File.Exists(tempPath)) { sourceFiles.Add(tempPath); changes.ProcessedTempFileNames.Add(id); }
                        else
                        {
                            string originalFileName = id.Contains("/") ? Path.GetFileName(id) : id;
                            string[] existingFiles = Directory.GetFiles(physicalChapterPath, originalFileName);
                            if (existingFiles.Any()) { sourceFiles.Add(existingFiles[0]); }
                            else { throw new FileNotFoundException($"Không tìm thấy file nguồn cho '{id}'"); }
                        }
                    }
                    var finalDbPaths = new List<string>();
                    string processingSubDir = Path.Combine(physicalChapterPath, "temp_processing_" + Guid.NewGuid().ToString("N"));
                    changes.ActionToFinalize = () =>
                    {
                        Directory.CreateDirectory(processingSubDir);
                        int counter = 1;
                        foreach (var sourceFile in sourceFiles) { string extension = Path.GetExtension(sourceFile); string newFileName = $"page_{counter:D3}{extension}"; File.Copy(sourceFile, Path.Combine(processingSubDir, newFileName), true); counter++; }
                        foreach (var ext in AllowedImageExtensions) foreach (var file in Directory.GetFiles(physicalChapterPath, $"*{ext}")) try { File.Delete(file); } catch (Exception ex) { Debug.WriteLine($"Could not delete old file {file}: {ex.Message}"); }
                        foreach (var file in Directory.GetFiles(processingSubDir)) { string finalName = Path.GetFileName(file); File.Move(file, Path.Combine(physicalChapterPath, finalName)); }
                        try { Directory.Delete(processingSubDir, true); } catch { }
                    };
                    for (int i = 0; i < sourceFiles.Count; i++)
                    {
                        string ext = Path.GetExtension(sourceFiles[i]);
                        // *** FIX: Thêm dấu / ở đầu ***
                        string newPath = $"/BookContent/Sach_{CurrentSachID}/Chuong_{CurrentSoChuong}/page_{i + 1:D3}{ext}";
                        finalDbPaths.Add(newPath);
                    }
                    changes.NewDuongDanDb = string.Join(",", finalDbPaths);
                }
                catch (Exception ex) { errorMessage = "Lỗi chuẩn bị file: " + ex.Message; Debug.WriteLine(ex); return null; }
                return changes;
            }
            errorMessage = "Loại sách không được hỗ trợ."; return null;
        }

        private void FinalizeFileOperations(ContentChanges changes)
        {
            if (changes == null) return;
            DeleteFilesSafe(changes.FilesToDelete);
            changes.ActionToFinalize?.Invoke();
            CleanUpTempFiles(changes.ProcessedTempFileNames);
        }

        // --- Helper Methods & Classes ---
        private void DeleteFilesSafe(List<string> paths) { if (paths == null) return; foreach (var p in paths.Distinct(StringComparer.OrdinalIgnoreCase)) { try { string physicalPath = MapVirtualToPhysicalPath(p); if (File.Exists(physicalPath)) File.Delete(physicalPath); } catch { } } }
        private void CleanUpTempFiles(List<string> tempFiles) { if (tempFiles == null) return; string tempDir = MapVirtualToPhysicalPath(TempUploadVirtualPath); foreach (var f in tempFiles.Distinct()) { try { string path = Path.Combine(tempDir, f); if (File.Exists(path)) File.Delete(path); } catch { } } }
        private void SetPageTitle(string title) { if (Master is Admin m) m.SetPageTitle(title); else Page.Title = title; }
        private string GetString(IDataRecord r, string c, string d = "") { int o = r.GetOrdinal(c); return r.IsDBNull(o) ? d : r.GetString(o).Trim(); }
        private object OrDBNull(string v) { return string.IsNullOrWhiteSpace(v) ? DBNull.Value : (object)v; }
        private string MapVirtualToPhysicalPath(string v) { return Server.MapPath(v); }
        private string MapRelativePathToUrl(string t) { try { return Page.ResolveClientUrl(t); } catch { return "#"; } }
        private void SetupContentPanels(string l) { pnlNovelContent.Visible = (l == LoaiSach_TruyenChu); pnlComicContent.Visible = (l == LoaiSach_TruyenTranh); }
        private void SetupValidatorsBasedOnBookType(string l) { bool n = (l == LoaiSach_TruyenChu); bool c = (l == LoaiSach_TruyenTranh); revFileTieuThuyet.Enabled = n; cvNovelContentRequired.Enabled = n; cvAnhTruyenRequired.Enabled = c; }
        private void DisableForm(string r) { txtTenChuong.Enabled = false; fuFileTieuThuyet.Enabled = false; txtNoiDungChu.Enabled = false; btnSaveNoiDung.Enabled = false; pnlComicContent.Visible = false; pnlNovelContent.Visible = false; }
        protected void cvNovelContentRequired_ServerValidate(object s, ServerValidateEventArgs a) { a.IsValid = !pnlNovelContent.Visible || (fuFileTieuThuyet.HasFile || !string.IsNullOrWhiteSpace(txtNoiDungChu.Text)); }
        protected void cvAnhTruyenRequired_ServerValidate(object s, ServerValidateEventArgs a) { a.IsValid = !pnlComicContent.Visible || !string.IsNullOrWhiteSpace(hfComicImageOrder.Value); }
        private void ShowMessage(string m, bool e, bool a = false) { pnlMessage.CssClass = "message-panel " + (e ? "message-error" : "message-success"); lblFormMessage.Text = HttpUtility.HtmlEncode(m); pnlMessage.Visible = true; if (a) ScriptManager.RegisterStartupScript(this, GetType(), "ClearMsg", $"setTimeout(function(){{var el=document.getElementById('{pnlMessage.ClientID}'); if(el)el.style.display='none';}}, 3000);", true); }
        private void ShowMessageAndRedirect(string m, string u, bool e) { ShowMessage(m + " Sẽ chuyển hướng...", e); ScriptManager.RegisterStartupScript(this, GetType(), "Redirect", $"setTimeout(function(){{window.location.href='{ResolveClientUrl(u)}';}}, 2000);", true); }
        private void EnableButtonsClientScript_Edit() { ScriptManager.RegisterStartupScript(this, GetType(), "EnableBtns", $"var btn=document.getElementById('{btnSaveNoiDung.ClientID}');if(btn){{btn.disabled=false;btn.classList.remove('loading-spinner');btn.value='Lưu Thay Đổi Nội Dung';}}var btn2=document.getElementById('{btnCancel.ClientID}');if(btn2)btn2.disabled=false;", true); }

        private class ContentChanges
        {
            public bool HasContentChanged { get; set; } = false;
            public string NewDuongDanDb { get; set; }
            public List<string> FilesToDelete { get; } = new List<string>();
            public List<string> ProcessedTempFileNames { get; } = new List<string>();
            public Action ActionToFinalize { get; set; }
        }
    }
}