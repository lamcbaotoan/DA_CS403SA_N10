using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text; // For StringBuilder
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Webebook.WebForm.Admin
{
    public partial class SuaSach : System.Web.UI.Page
    {
        // --- Class Members ---
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["datawebebookConnectionString"].ConnectionString;
        private const string UploadFolderPath = "~/Uploads/Covers/"; // Relative path to the upload folder
        private const int MaxFileSizeCoverMb = 5;     // Server-side limit for cover image (MB)
        private int _sachId = 0;

        // --- Page Lifecycle Events ---
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (!int.TryParse(Request.QueryString["id"], out _sachId) || _sachId <= 0)
                {
                    ShowMessageAndRedirect("ID sách không hợp lệ.", "QuanLySach.aspx?message=invalidid", MessageType.Error);
                    return;
                }
                hfSachID.Value = _sachId.ToString();
                SetPageTitle("Sửa Thông Tin Sách");
                LoadSachData(_sachId);
            }
            else
            {
                // Retrieve SachID from HiddenField on PostBack
                if (!int.TryParse(hfSachID.Value, out _sachId) || _sachId <= 0)
                {
                    ShowMessage("Lỗi nghiêm trọng: Không thể xác định ID sách khi tải lại trang.", MessageType.Error);
                    DisableActionButtons();
                    return;
                }
            }
        }

        // --- Data Loading ---
        private void LoadSachData(int currentSachId)
        {
            Debug.WriteLine($"Loading data for Sach ID: {currentSachId}");
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                // Updated Query to fetch new columns
                const string query = @"SELECT IDSach, TenSach, TacGia, GiaSach, MoTa, NhaXuatBan, NhomDich,
                                              TrangThaiNoiDung, DuongDanBiaSach, LoaiSach, TheLoaiChuoi
                                       FROM Sach WHERE IDSach = @IDSach";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@IDSach", currentSachId);
                    try
                    {
                        con.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                lblSachID.Text = reader["IDSach"].ToString();
                                txtTenSach.Text = GetString(reader, "TenSach");
                                txtTacGia.Text = GetString(reader, "TacGia");
                                // Use F0 to format without decimals for display, but keep precision for saving
                                txtGiaSach.Text = GetDecimal(reader, "GiaSach").ToString("F0", CultureInfo.InvariantCulture);
                                txtMoTa.Text = GetString(reader, "MoTa");
                                txtNhaXuatBan.Text = GetString(reader, "NhaXuatBan");
                                txtNhomDich.Text = GetString(reader, "NhomDich");

                                // Load new fields
                                string loaiSachValue = GetString(reader, "LoaiSach");
                                SetSelectedValue(ddlLoaiSach, loaiSachValue);
                                txtTheLoaiChuoi.Text = GetString(reader, "TheLoaiChuoi");

                                string trangThaiValue = GetString(reader, "TrangThaiNoiDung", "Hoàn thành");
                                SetSelectedValue(ddlTrangThaiNoiDung, trangThaiValue);

                                // Handle Cover Image Path
                                string imagePath = GetString(reader, "DuongDanBiaSach");
                                if (!string.IsNullOrWhiteSpace(imagePath))
                                {
                                    try
                                    {
                                        // Check if file exists physically (optional but good for robustness)
                                        string physicalPath = Server.MapPath(imagePath);
                                        if (File.Exists(physicalPath))
                                        {
                                            imgCurrentBiaSach.ImageUrl = ResolveUrl(imagePath); // Use ResolveUrl for ~ paths
                                            imgCurrentBiaSach.Visible = true;
                                            lblNoCurrentImage.Visible = false;
                                            Debug.WriteLine($"Displaying image from: {imagePath}");
                                        }
                                        else
                                        {
                                            imgCurrentBiaSach.ImageUrl = ""; // Clear if file missing
                                            imgCurrentBiaSach.Visible = false;
                                            lblNoCurrentImage.Visible = true;
                                            lblNoCurrentImage.Text = "Ảnh bìa không tồn tại trên máy chủ.";
                                            Debug.WriteLine($"Image file not found at: {physicalPath}");
                                        }
                                        hfCurrentDuongDanBiaSach.Value = imagePath; // Store current path anyway
                                    }
                                    catch (Exception pathEx) // Catch potential Server.MapPath errors etc.
                                    {
                                        Debug.WriteLine($"Error processing image path '{imagePath}': {pathEx.Message}");
                                        ShowMessage("Lỗi khi xử lý đường dẫn ảnh bìa.", MessageType.Warning);
                                        imgCurrentBiaSach.Visible = false;
                                        lblNoCurrentImage.Visible = true;
                                    }
                                }
                                else
                                {
                                    imgCurrentBiaSach.ImageUrl = "";
                                    imgCurrentBiaSach.Visible = false;
                                    lblNoCurrentImage.Visible = true;
                                    lblNoCurrentImage.Text = "Chưa có ảnh bìa";
                                    hfCurrentDuongDanBiaSach.Value = "";
                                    Debug.WriteLine("No cover image path found in database.");
                                }
                            }
                            else
                            {
                                Debug.WriteLine($"Book with ID {currentSachId} not found.");
                                ShowMessageAndRedirect($"Không tìm thấy sách với ID = {currentSachId}.", "QuanLySach.aspx?message=notfound", MessageType.Error);
                            }
                        }
                    }
                    catch (SqlException sqlEx) { ShowMessage("Lỗi CSDL khi tải dữ liệu sách.", MessageType.Error); Debug.WriteLine($"SQL ERROR loading book ID {currentSachId}: {sqlEx}"); DisableActionButtons(); }
                    catch (Exception ex) { ShowMessage("Lỗi không xác định khi tải dữ liệu sách.", MessageType.Error); Debug.WriteLine($"ERROR loading book ID {currentSachId}: {ex}"); DisableActionButtons(); }
                }
            }
        }

        // --- Info Saving ---
        protected void btnLuuThongTin_Click(object sender, EventArgs e)
        {
            Debug.WriteLine("btnLuuThongTin_Click triggered.");
            if (_sachId <= 0) { ShowMessage("Lỗi: Không xác định được ID sách để lưu.", MessageType.Error); return; }

            Page.Validate(); // Trigger validation controls
            if (!Page.IsValid)
            {
                ShowMessage("Vui lòng kiểm tra lại các trường thông tin sách.", MessageType.Warning);
                Debug.WriteLine(">> LuuThongTin Validation Failed");
                return;
            }
            Debug.WriteLine(">> LuuThongTin Validation Passed");

            // --- Get Data from Form ---
            string tenSach = txtTenSach.Text.Trim();
            string tacGia = txtTacGia.Text.Trim();
            string moTa = txtMoTa.Text.Trim();
            string trangThaiND = ddlTrangThaiNoiDung.SelectedValue;
            string nhaXuatBan = txtNhaXuatBan.Text.Trim();
            string nhomDich = txtNhomDich.Text.Trim();
            string loaiSach = ddlLoaiSach.SelectedValue; // New
            string theLoaiChuoi = txtTheLoaiChuoi.Text.Trim(); // New

            // Validate Price
            if (!decimal.TryParse(txtGiaSach.Text.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal giaSach) || giaSach < 0)
            {
                ShowMessage("Giá sách nhập vào không hợp lệ hoặc là số âm.", MessageType.Error);
                return;
            }

            // --- Handle Optional File Upload (Replace Existing Image) ---
            string newRelativeImagePath = null; // Path of the *new* image, if uploaded
            string oldPhysicalImagePath = null; // Physical path of the *old* image to delete
            bool updateImage = false;

            if (fuBiaSach.HasFile)
            {
                Debug.WriteLine("New cover image upload detected.");
                try
                {
                    HttpPostedFile postedFile = fuBiaSach.PostedFile;
                    string fileExtension = Path.GetExtension(postedFile.FileName).ToLowerInvariant();
                    string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };

                    // Validate file type and size
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        ShowMessage($"Định dạng file ảnh bìa mới không hợp lệ. Chỉ chấp nhận: {string.Join(", ", allowedExtensions)}.", MessageType.Error);
                        return;
                    }
                    if (postedFile.ContentLength > MaxFileSizeCoverMb * 1024 * 1024)
                    {
                        ShowMessage($"Kích thước ảnh bìa mới ({FormatFileSize(postedFile.ContentLength)}) vượt quá giới hạn ({MaxFileSizeCoverMb}MB).", MessageType.Error);
                        return;
                    }

                    // Generate unique filename
                    string uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
                    string physicalUploadFolder = Server.MapPath(UploadFolderPath);

                    // Ensure directory exists
                    if (!Directory.Exists(physicalUploadFolder))
                    {
                        Directory.CreateDirectory(physicalUploadFolder);
                        Debug.WriteLine($"Created directory: {physicalUploadFolder}");
                    }

                    string newFullSavePath = Path.Combine(physicalUploadFolder, uniqueFileName);
                    postedFile.SaveAs(newFullSavePath); // Save the new file

                    newRelativeImagePath = VirtualPathUtility.Combine(UploadFolderPath, uniqueFileName); // e.g., ~/Uploads/Covers/guid.jpg
                    updateImage = true; // Flag that we need to update the path in DB
                    Debug.WriteLine($"New image saved successfully: {newFullSavePath}, New relative path: {newRelativeImagePath}");

                    // Get the physical path of the OLD image for later deletion
                    if (!string.IsNullOrWhiteSpace(hfCurrentDuongDanBiaSach.Value))
                    {
                        try
                        {
                            oldPhysicalImagePath = Server.MapPath(hfCurrentDuongDanBiaSach.Value);
                            Debug.WriteLine($"Old image physical path identified for potential deletion: {oldPhysicalImagePath}");
                        }
                        catch (Exception mapPathEx)
                        {
                            Debug.WriteLine($"Warning: Could not map old image path '{hfCurrentDuongDanBiaSach.Value}' for deletion: {mapPathEx.Message}");
                            oldPhysicalImagePath = null; // Ensure it's null if mapping fails
                        }
                    }

                }
                catch (Exception ex)
                {
                    ShowMessage("Lỗi khi lưu ảnh bìa mới: " + ex.Message, MessageType.Error);
                    Debug.WriteLine($"ERROR saving new cover image: {ex}");
                    // Don't proceed with DB update if new image saving failed
                    return;
                }
            }
            else
            {
                Debug.WriteLine("No new cover image uploaded, keeping the existing one (if any).");
                // Keep the existing image path from the hidden field if no new file is uploaded.
                newRelativeImagePath = hfCurrentDuongDanBiaSach.Value; // This will be used if !updateImage
            }


            // --- Update Database ---
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                // Build the query dynamically based on whether the image is updated
                StringBuilder queryBuilder = new StringBuilder();
                queryBuilder.Append(@"UPDATE Sach SET
                                        TenSach = @TenSach,
                                        TacGia = @TacGia,
                                        GiaSach = @GiaSach,
                                        MoTa = @MoTa,
                                        TrangThaiNoiDung = @TrangThaiNoiDung,
                                        LoaiSach = @LoaiSach,
                                        TheLoaiChuoi = @TheLoaiChuoi,
                                        NhaXuatBan = @NhaXuatBan,
                                        NhomDich = @NhomDich");

                // Only add the image path update if a new file was successfully uploaded
                if (updateImage)
                {
                    queryBuilder.Append(", DuongDanBiaSach = @DuongDanBiaSach");
                }

                queryBuilder.Append(" WHERE IDSach = @IDSach");

                using (SqlCommand cmd = new SqlCommand(queryBuilder.ToString(), con))
                {
                    // Add Common Parameters
                    cmd.Parameters.AddWithValue("@TenSach", tenSach);
                    cmd.Parameters.AddWithValue("@TacGia", OrDBNull(tacGia));
                    cmd.Parameters.AddWithValue("@GiaSach", giaSach);
                    cmd.Parameters.AddWithValue("@MoTa", OrDBNull(moTa));
                    cmd.Parameters.AddWithValue("@TrangThaiNoiDung", OrDBNull(trangThaiND));
                    cmd.Parameters.AddWithValue("@LoaiSach", OrDBNull(loaiSach));
                    cmd.Parameters.AddWithValue("@TheLoaiChuoi", OrDBNull(theLoaiChuoi));
                    cmd.Parameters.AddWithValue("@NhaXuatBan", OrDBNull(nhaXuatBan));
                    cmd.Parameters.AddWithValue("@NhomDich", OrDBNull(nhomDich));
                    cmd.Parameters.AddWithValue("@IDSach", _sachId);

                    // Add Image Path Parameter ONLY if updating
                    if (updateImage)
                    {
                        cmd.Parameters.AddWithValue("@DuongDanBiaSach", OrDBNull(newRelativeImagePath));
                        Debug.WriteLine($"Adding @DuongDanBiaSach parameter with value: {newRelativeImagePath ?? "NULL"}");
                    }
                    else
                    {
                        Debug.WriteLine($"Not adding @DuongDanBiaSach parameter (updateImage is false).");
                    }


                    try
                    {
                        con.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();
                        Debug.WriteLine($"Rows affected by UPDATE Sach: {rowsAffected}");

                        if (rowsAffected > 0)
                        {
                            ShowMessage("Cập nhật thông tin sách thành công!", MessageType.Success);

                            // --- Post-Update Actions ---
                            if (updateImage)
                            {
                                // 1. Update the UI Image and Hidden Field
                                imgCurrentBiaSach.ImageUrl = ResolveUrl(newRelativeImagePath);
                                imgCurrentBiaSach.Visible = !string.IsNullOrEmpty(newRelativeImagePath);
                                lblNoCurrentImage.Visible = string.IsNullOrEmpty(newRelativeImagePath);
                                hfCurrentDuongDanBiaSach.Value = newRelativeImagePath ?? ""; // Update hidden field
                                Debug.WriteLine($"UI updated with new image path: {newRelativeImagePath}");

                                // 2. Delete the OLD image file (if it existed and is different from the new one)
                                if (!string.IsNullOrWhiteSpace(oldPhysicalImagePath) && oldPhysicalImagePath != Server.MapPath(newRelativeImagePath ?? string.Empty))
                                {
                                    TryDeleteFile(oldPhysicalImagePath);
                                }
                            }
                        }
                        else
                        {
                            ShowMessage($"Không có thông tin nào được cập nhật (Sách ID: {_sachId}). Có thể dữ liệu không thay đổi.", MessageType.Warning);
                            // If update failed but we saved a new image, delete the newly saved image.
                            if (updateImage && !string.IsNullOrEmpty(newRelativeImagePath))
                            {
                                TryDeleteFile(Server.MapPath(newRelativeImagePath));
                                Debug.WriteLine($"Deleted newly uploaded file because DB update affected 0 rows: {newRelativeImagePath}");
                            }
                        }
                    }
                    catch (SqlException sqlEx)
                    {
                        ShowMessage("Lỗi CSDL khi cập nhật thông tin sách.", MessageType.Error);
                        Debug.WriteLine($"SQL ERROR updating book ID {_sachId}: {sqlEx}");
                        // If DB update fails, try to delete the newly uploaded image to prevent orphans
                        if (updateImage && !string.IsNullOrEmpty(newRelativeImagePath))
                        {
                            TryDeleteFile(Server.MapPath(newRelativeImagePath));
                            Debug.WriteLine($"Deleted newly uploaded file due to DB error: {newRelativeImagePath}");
                        }
                    }
                    catch (Exception ex)
                    {
                        ShowMessage("Lỗi không xác định khi cập nhật thông tin sách.", MessageType.Error);
                        Debug.WriteLine($"ERROR updating book ID {_sachId}: {ex}");
                        // If DB update fails, try to delete the newly uploaded image
                        if (updateImage && !string.IsNullOrEmpty(newRelativeImagePath))
                        {
                            TryDeleteFile(Server.MapPath(newRelativeImagePath));
                            Debug.WriteLine($"Deleted newly uploaded file due to general error: {newRelativeImagePath}");
                        }
                    }
                }
            }
        }

        // --- Event Handlers ---
        protected void btnHuy_Click(object sender, EventArgs e)
        {
            Response.Redirect("QuanLySach.aspx", true); // Use true to prevent further execution
        }

        protected void btnManageContent_Click(object sender, EventArgs e)
        {
            if (_sachId > 0)
            {
                Response.Redirect($"SuaNoiDungSach.aspx?id={_sachId}", true); // Use true
            }
            else
            {
                ShowMessage("Không thể xác định ID sách để quản lý nội dung.", MessageType.Error);
            }
        }

        // --- Helper Methods ---
        #region Data Access Helpers
        // GetString, GetDecimal, GetInt32 (if needed), OrDBNull remain the same as before
        private string GetString(IDataRecord record, string columnName, string defaultValue = "") { try { int ordinal = record.GetOrdinal(columnName); return record.IsDBNull(ordinal) ? defaultValue : record.GetString(ordinal).Trim(); } catch { return defaultValue; } }
        private decimal GetDecimal(IDataRecord record, string columnName, decimal defaultValue = 0) { try { int ordinal = record.GetOrdinal(columnName); if (!record.IsDBNull(ordinal)) { object value = record.GetValue(ordinal); if (value is decimal decValue) return decValue; if (decimal.TryParse(value.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal parsedValue)) return parsedValue; } } catch { } return defaultValue; }
        // private int GetInt32(IDataRecord record, string columnName, int defaultValue = 0) { try { int ordinal = record.GetOrdinal(columnName); if (!record.IsDBNull(ordinal)) { object value = record.GetValue(ordinal); if (value is int intValue) return intValue; try { return Convert.ToInt32(value); } catch { if (int.TryParse(value.ToString(), out int parsedValue)) return parsedValue; } } } catch { } return defaultValue; }
        private object OrDBNull(string value) { return string.IsNullOrWhiteSpace(value) ? DBNull.Value : (object)value; }
        #endregion

        #region UI Helpers
        private void SetPageTitle(string title) { if (Master is Admin master) { master.SetPageTitle(title); } else { Page.Title = title; } }
        private void SetSelectedValue(ListControl listControl, string valueToSelect) { if (listControl != null) { listControl.ClearSelection(); if (!string.IsNullOrEmpty(valueToSelect)) { ListItem item = listControl.Items.FindByValue(valueToSelect); if (item != null) item.Selected = true; else Debug.WriteLine($"Warning: Value '{valueToSelect}' not found in ListControl '{listControl.ID}'."); } } }
        private void DisableActionButtons() { if (btnLuuThongTin != null) btnLuuThongTin.Enabled = false; if (btnManageContent != null) btnManageContent.Enabled = false; btnLuuThongTin.CssClass += " opacity-50 cursor-not-allowed"; btnManageContent.CssClass += " opacity-50 cursor-not-allowed"; } // Added styling disable

        // Updated ShowMessage methods to use the single lblMessage and enum
        private void ShowMessage(string message, MessageType type)
        {
            lblMessage.Text = HttpUtility.HtmlEncode(message);
            lblMessage.Visible = true;
            string baseClasses = "block mb-4 p-3 rounded-md border";
            switch (type)
            {
                case MessageType.Success: lblMessage.CssClass = $"{baseClasses} bg-green-50 border-green-300 text-green-700"; break;
                case MessageType.Error: lblMessage.CssClass = $"{baseClasses} bg-red-50 border-red-300 text-red-700"; break;
                case MessageType.Warning: lblMessage.CssClass = $"{baseClasses} bg-yellow-50 border-yellow-300 text-yellow-700"; break;
                case MessageType.Info: default: lblMessage.CssClass = $"{baseClasses} bg-blue-50 border-blue-300 text-blue-700"; break;
            }
            Debug.WriteLine($"{type}: {message}");
        }
        private enum MessageType { Success, Error, Warning, Info }

        private void ShowMessageAndRedirect(string message, string redirectUrl, MessageType type)
        {
            ShowMessage(message + " Đang chuyển hướng...", type);
            DisableActionButtons();
            string script = $"window.setTimeout(function(){{ window.location.href = '{ResolveUrl(redirectUrl)}'; }}, 2500);"; // Increased delay slightly
            ScriptManager.RegisterStartupScript(this, this.GetType(), "RedirectScript", script, true);
        }
        #endregion

        #region File Handling Helpers
        // ValidateAndReadImage is NO LONGER NEEDED as we don't read bytes for saving path
        // TryDeleteFile helper
        private void TryDeleteFile(string physicalPath)
        {
            if (!string.IsNullOrWhiteSpace(physicalPath))
            {
                try
                {
                    if (File.Exists(physicalPath))
                    {
                        File.Delete(physicalPath);
                        Debug.WriteLine($"Successfully deleted file: {physicalPath}");
                    }
                    else
                    {
                        Debug.WriteLine($"File not found for deletion: {physicalPath}");
                    }
                }
                catch (IOException ioEx)
                {
                    // Log or handle file locking issues etc.
                    Debug.WriteLine($"IO Error deleting file '{physicalPath}': {ioEx.Message}");
                    ShowMessage($"Không thể xóa file ảnh cũ ({Path.GetFileName(physicalPath)}). Có thể file đang được sử dụng.", MessageType.Warning);
                }
                catch (UnauthorizedAccessException uaEx)
                {
                    // Log or handle permissions issues
                    Debug.WriteLine($"Permissions Error deleting file '{physicalPath}': {uaEx.Message}");
                    ShowMessage($"Không có quyền xóa file ảnh cũ ({Path.GetFileName(physicalPath)}).", MessageType.Warning);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"General Error deleting file '{physicalPath}': {ex.Message}");
                    ShowMessage($"Lỗi không xác định khi xóa file ảnh cũ ({Path.GetFileName(physicalPath)}).", MessageType.Warning);
                }
            }
            else
            {
                Debug.WriteLine("TryDeleteFile called with null or empty path.");
            }
        }

        // FormatFileSize remains the same
        private static string FormatFileSize(long bytes) { if (bytes < 0) return "N/A"; if (bytes == 0) return "0 Bytes"; const int k = 1024; string[] sizes = { "Bytes", "KB", "MB", "GB", "TB" }; int i = (int)Math.Floor(Math.Log(bytes) / Math.Log(k)); return string.Format(CultureInfo.InvariantCulture, "{0:0.#} {1}", bytes / Math.Pow(k, i), sizes[i]); }
        #endregion
    }
}

