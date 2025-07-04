using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Webebook.WebForm.Admin
{
    public partial class ThemSach : System.Web.UI.Page
    {
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["datawebebookConnectionString"].ConnectionString;
        private const string UploadFolderPath = "~/Uploads/Covers/"; // Relative path
        private const int MaxFileSizeCoverMb = 5; // Max file size in MB

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                SetPageTitle("Thêm Sách Mới");
                // Xóa thông báo lỗi xem trước nếu có postback (ví dụ do validator khác)
                lblPreviewError.Text = "";
            }
        }

        protected void btnLuu_Click(object sender, EventArgs e)
        {
            // Xóa thông báo lỗi cũ
            lblMessage.Visible = false;
            lblPreviewError.Text = "";

            // Validate trang
            Page.Validate();
            if (!Page.IsValid)
            {
                ShowMessage("Vui lòng kiểm tra lại các trường thông tin.", MessageType.Error);
                Debug.WriteLine(">> ThemSach Validation Failed");
                return;
            }
            Debug.WriteLine(">> ThemSach Validation Passed");

            // --- Lấy dữ liệu từ Form ---
            string tenSach = txtTenSach.Text.Trim();
            string tacGia = txtTacGia.Text.Trim();
            string moTa = txtMoTa.Text.Trim();
            string trangThaiND = ddlTrangThaiNoiDung.SelectedValue;
            string nhaXuatBan = txtNhaXuatBan.Text.Trim();
            string nhomDich = txtNhomDich.Text.Trim();
            string loaiSach = ddlLoaiSach.SelectedValue;
            string theLoaiChuoi = txtTheLoaiChuoi.Text.Trim();

            // Validate Giá sách
            if (!decimal.TryParse(txtGiaSach.Text.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out decimal giaSach) || giaSach < 0)
            {
                // Validator nên bắt lỗi này, nhưng kiểm tra lại cho chắc
                ShowMessage("Giá sách nhập vào không hợp lệ hoặc là số âm.", MessageType.Error);
                return;
            }

            // --- Xử lý Upload Ảnh Bìa ---
            string relativeImagePath = null;
            string physicalSavePath = null; // Đổi tên để rõ ràng hơn là đường dẫn vật lý *để lưu*

            if (fuBiaSach.HasFile)
            {
                try
                {
                    HttpPostedFile postedFile = fuBiaSach.PostedFile;
                    string fileExtension = Path.GetExtension(postedFile.FileName).ToLowerInvariant();
                    string[] allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };

                    // Validate lại phía server (dù đã có validator)
                    if (!allowedExtensions.Contains(fileExtension))
                    {
                        ShowMessage($"Định dạng file ảnh bìa không hợp lệ. Chỉ chấp nhận: {string.Join(", ", allowedExtensions)}.", MessageType.Error);
                        return;
                    }
                    if (postedFile.ContentLength > MaxFileSizeCoverMb * 1024 * 1024)
                    {
                        ShowMessage($"Kích thước ảnh bìa ({FormatFileSize(postedFile.ContentLength)}) vượt quá giới hạn ({MaxFileSizeCoverMb}MB).", MessageType.Error);
                        return;
                    }

                    // Tạo tên file duy nhất
                    string uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
                    string physicalUploadFolder = Server.MapPath(UploadFolderPath);

                    // *** QUAN TRỌNG: Kiểm tra quyền ghi thư mục ***
                    // Nếu lỗi xảy ra ở bước SaveAs dưới đây, hãy đảm bảo tài khoản user của Application Pool (trong IIS)
                    // có quyền ghi (Write/Modify) trên thư mục vật lý: physicalUploadFolder
                    // Ví dụ: C:\inetpub\wwwroot\YourWebApp\Uploads\Covers\

                    // Tạo thư mục nếu chưa tồn tại
                    if (!Directory.Exists(physicalUploadFolder))
                    {
                        try
                        {
                            Directory.CreateDirectory(physicalUploadFolder);
                            Debug.WriteLine($"Created directory: {physicalUploadFolder}");
                        }
                        catch (Exception dirEx)
                        {
                            ShowMessage($"Lỗi nghiêm trọng: Không thể tạo thư mục upload. Vui lòng kiểm tra quyền ghi. Chi tiết: {dirEx.Message}", MessageType.Error);
                            Debug.WriteLine($"ERROR creating directory '{physicalUploadFolder}': {dirEx}");
                            return;
                        }
                    }

                    physicalSavePath = Path.Combine(physicalUploadFolder, uniqueFileName);

                    // Lưu file vật lý
                    postedFile.SaveAs(physicalSavePath);

                    // Lưu đường dẫn tương đối để dùng trong DB và hiển thị
                    relativeImagePath = VirtualPathUtility.Combine(UploadFolderPath, uniqueFileName);

                    Debug.WriteLine($"Image saved successfully to: {physicalSavePath}, Relative path: {relativeImagePath}");

                }
                catch (Exception ex)
                {
                    // Lỗi xảy ra trong quá trình xử lý hoặc lưu file
                    ShowMessage($"Lỗi khi lưu ảnh bìa: {ex.Message}. Vui lòng kiểm tra quyền ghi thư mục và thử lại.", MessageType.Error);
                    Debug.WriteLine($"ERROR saving cover image: {ex}");
                    // Không cần xóa file ở đây vì nếu SaveAs lỗi thì file chưa được tạo hoặc tạo không hoàn chỉnh.
                    return; // Dừng lại nếu không lưu được file
                }
            }
            else
            {
                // Trường hợp này validator đã bắt, nhưng để chắc chắn
                ShowMessage("Ảnh bìa là bắt buộc khi thêm sách mới.", MessageType.Error);
                return;
            }


            // --- Insert vào Database ---
            // Chỉ thực hiện nếu đã upload file thành công (relativeImagePath != null)
            if (relativeImagePath != null)
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    const string query = @"INSERT INTO Sach 
                                           (TenSach, TacGia, GiaSach, MoTa, TrangThaiNoiDung, DuongDanBiaSach, LoaiSach, TheLoaiChuoi, NhaXuatBan, NhomDich)
                                       OUTPUT INSERTED.IDSach 
                                       VALUES 
                                           (@TenSach, @TacGia, @GiaSach, @MoTa, @TrangThaiNoiDung, @DuongDanBiaSach, @LoaiSach, @TheLoaiChuoi, @NhaXuatBan, @NhomDich)";
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@TenSach", tenSach);
                        cmd.Parameters.AddWithValue("@TacGia", OrDBNull(tacGia));
                        cmd.Parameters.AddWithValue("@GiaSach", giaSach);
                        cmd.Parameters.AddWithValue("@MoTa", OrDBNull(moTa));
                        cmd.Parameters.AddWithValue("@TrangThaiNoiDung", OrDBNull(trangThaiND));
                        cmd.Parameters.AddWithValue("@DuongDanBiaSach", relativeImagePath); // Đã kiểm tra khác null ở trên
                        cmd.Parameters.AddWithValue("@LoaiSach", OrDBNull(loaiSach));
                        cmd.Parameters.AddWithValue("@TheLoaiChuoi", OrDBNull(theLoaiChuoi));
                        cmd.Parameters.AddWithValue("@NhaXuatBan", OrDBNull(nhaXuatBan));
                        cmd.Parameters.AddWithValue("@NhomDich", OrDBNull(nhomDich));

                        try
                        {
                            con.Open();
                            object result = cmd.ExecuteScalar();
                            if (result != null && int.TryParse(result.ToString(), out int newSachId))
                            {
                                Debug.WriteLine($"Book added successfully with ID: {newSachId}");
                                // Chuyển hướng với thông báo thành công
                                Response.Redirect($"QuanLySach.aspx?message=addsuccess&id={newSachId}", false); // <--- SỬA Ở ĐÂY
                            }
                            else
                            {
                                // Lỗi không mong muốn: Insert thành công nhưng không trả về ID?
                                ShowMessage("Thêm sách vào CSDL không thành công (không nhận được ID sách mới).", MessageType.Error);
                                Debug.WriteLine("ERROR adding book: ExecuteScalar returned null or non-integer.");
                                // Vì DB insert không thành công, xóa file đã upload trước đó
                                TryDeleteFile(physicalSavePath);
                            }
                        }
                        catch (SqlException sqlEx)
                        {
                            // Lỗi CSDL cụ thể
                            ShowMessage($"Lỗi CSDL khi thêm sách: {sqlEx.Message}. Mã lỗi: {sqlEx.Number}", MessageType.Error);
                            Debug.WriteLine($"SQL ERROR adding book: {sqlEx}");
                            // Vì DB insert lỗi, xóa file đã upload trước đó
                            TryDeleteFile(physicalSavePath);
                        }
                        catch (Exception ex)
                        {
                            // Lỗi chung khác
                            ShowMessage($"Lỗi không xác định khi thêm sách vào CSDL: {ex.Message}", MessageType.Error);
                            Debug.WriteLine($"ERROR adding book: {ex}");
                            // Vì DB insert lỗi, xóa file đã upload trước đó
                            TryDeleteFile(physicalSavePath);
                        }
                    } // End using SqlCommand
                } // End using SqlConnection
            }
            else
            {
                // Trường hợp này không nên xảy ra nếu logic ở trên đúng
                ShowMessage("Lỗi xử lý: Không có đường dẫn ảnh để lưu vào CSDL.", MessageType.Error);
                Debug.WriteLine("ERROR: relativeImagePath was null before database insert attempt.");
            }
        }

        protected void btnHuy_Click(object sender, EventArgs e)
        {
            Response.Redirect("QuanLySach.aspx", false);
        }

        // --- Các hàm hỗ trợ ---
        private void SetPageTitle(string title)
        {
            if (Master is Admin master) { master.SetPageTitle(title); }
            else { Page.Title = title; }
        }

        private object OrDBNull(string value)
        {
            return string.IsNullOrWhiteSpace(value) ? DBNull.Value : (object)value;
        }

        private enum MessageType { Success, Error, Warning, Info }

        private void ShowMessage(string message, MessageType type)
        {
            lblMessage.Text = HttpUtility.HtmlEncode(message); // Encode để tránh XSS
            lblMessage.Visible = true;
            string baseClass = "block mb-4 p-3 rounded-md border";
            switch (type)
            {
                case MessageType.Success: lblMessage.CssClass = $"{baseClass} bg-green-50 border-green-300 text-green-700"; break;
                case MessageType.Error: lblMessage.CssClass = $"{baseClass} bg-red-50 border-red-300 text-red-700"; break;
                case MessageType.Warning: lblMessage.CssClass = $"{baseClass} bg-yellow-50 border-yellow-300 text-yellow-700"; break;
                case MessageType.Info: default: lblMessage.CssClass = $"{baseClass} bg-blue-50 border-blue-300 text-blue-700"; break;
            }
        }

        private static string FormatFileSize(long bytes)
        {
            if (bytes < 0) return "N/A";
            if (bytes == 0) return "0 Bytes";
            const int k = 1024;
            string[] sizes = { "Bytes", "KB", "MB", "GB", "TB" };
            int i = (int)Math.Floor(Math.Log(bytes) / Math.Log(k));
            return string.Format(CultureInfo.InvariantCulture, "{0:0.#} {1}", bytes / Math.Pow(k, i), sizes[i]);
        }

        // Hàm xóa file an toàn
        private void TryDeleteFile(string physicalPath)
        {
            // Chỉ xóa nếu đường dẫn không rỗng và file tồn tại
            if (!string.IsNullOrEmpty(physicalPath) && File.Exists(physicalPath))
            {
                try
                {
                    File.Delete(physicalPath);
                    Debug.WriteLine($"Successfully deleted orphaned/temporary file: {physicalPath}");
                }
                catch (IOException ioEx)
                {
                    Debug.WriteLine($"IO Error deleting file '{physicalPath}': {ioEx.Message}");
                    // Có thể log lỗi này nhưng không cần báo cho người dùng trừ khi thật sự cần thiết
                }
                catch (UnauthorizedAccessException uaEx)
                {
                    Debug.WriteLine($"Permissions error deleting file '{physicalPath}': {uaEx.Message}");
                    // Có thể log lỗi này
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"General error deleting file '{physicalPath}': {ex.Message}");
                    // Có thể log lỗi này
                }
            }
            else if (!string.IsNullOrEmpty(physicalPath))
            {
                Debug.WriteLine($"Attempted to delete file, but it did not exist: {physicalPath}");
            }
        }
    }
}