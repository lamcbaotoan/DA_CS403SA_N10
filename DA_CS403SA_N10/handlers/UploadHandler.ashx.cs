using System;
using System.Web;
using System.IO;
using System.Web.SessionState; // Thêm nếu cần truy cập Session
using System.Diagnostics;     // Thêm để dùng Debug
using System.Linq; // Thêm để dùng Linq

// *** Đảm bảo namespace này đúng với cấu trúc thư mục của bạn ***
namespace Webebook.Handlers
{
    // Tên class phải khớp với thuộc tính Class trong file .ashx
    public class UploadHandler : IHttpHandler, IRequiresSessionState // Thêm IRequiresSessionState nếu cần Session
    {
        // Thư mục lưu file tạm thời (so với gốc web app)
        private const string TempDirectory = "~/Uploads/Temp/";
        // Giới hạn dung lượng file phía server (bytes) - Nên khớp hoặc lớn hơn client-side Dropzone một chút
        private const long MaxFileSizeServer = 500 * 1024 * 1024; // 500MB
        // Các đuôi file cho phép phía server
        private readonly string[] AllowedExtensionsServer = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

       public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json"; // Trả về JSON
            string tempFileName = null;
            string errorMessage = null;
            string originalFileNameForResponse = null; // Lưu tên gốc để trả về nếu cần

            // --- Optional: Thêm kiểm tra xác thực/quyền ở đây nếu cần ---
            // Ví dụ: kiểm tra Session admin
            /*
            if (context.Session["UserID"] == null || context.Session["VaiTro"] == null || Convert.ToInt32(context.Session["VaiTro"]) != 0) // 0 là Admin
            {
                context.Response.StatusCode = 401; // Unauthorized
                context.Response.Write("{\"error\": \"Không có quyền upload.\"}");
                Debug.WriteLine("UploadHandler: Unauthorized access attempt.");
                return;
            }
            */
            // --- Kết thúc kiểm tra xác thực ---

            try
            {
                // Kiểm tra xem có file được gửi lên không
                if (context.Request.Files.Count > 0)
                {
                    HttpPostedFile postedFile = context.Request.Files[0]; // Dropzone thường gửi 1 file/request
                    originalFileNameForResponse = Path.GetFileName(postedFile.FileName);

                    // --- Validate file phía Server ---
                    if (postedFile.ContentLength <= 0)
                    {
                        errorMessage = "File bị rỗng (0 byte).";
                        context.Response.StatusCode = 400; // Bad Request
                    }
                    else if (postedFile.ContentLength > MaxFileSizeServer)
                    {
                        errorMessage = $"File quá lớn (tối đa {MaxFileSizeServer / 1024 / 1024}MB).";
                        context.Response.StatusCode = 400;
                    }
                    else
                    {
                        string fileExtension = Path.GetExtension(postedFile.FileName)?.ToLowerInvariant();
                        if (string.IsNullOrEmpty(fileExtension) || !AllowedExtensionsServer.Contains(fileExtension))
                        {
                            errorMessage = $"Định dạng file không được chấp nhận ({string.Join(", ", AllowedExtensionsServer)}).";
                            context.Response.StatusCode = 400;
                        }
                    }
                    // --- Kết thúc Validate ---

                    // Nếu không có lỗi validation
                    if (string.IsNullOrEmpty(errorMessage))
                    {
                        // Tạo tên file tạm duy nhất (giữ lại phần mở rộng gốc)
                        string uniqueSuffix = Guid.NewGuid().ToString("N").Substring(0, 10); // Chuỗi ngẫu nhiên ngắn
                        // Làm sạch tên file gốc để dùng trong tên file tạm
                        string safeOriginalName = Path.GetFileNameWithoutExtension(originalFileNameForResponse) ?? "file";
                        safeOriginalName = System.Text.RegularExpressions.Regex.Replace(safeOriginalName, @"[^\w\.-]", "_"); // Thay ký tự không hợp lệ bằng _
                        safeOriginalName = safeOriginalName.Length > 50 ? safeOriginalName.Substring(0, 50) : safeOriginalName; // Giới hạn độ dài
                        string extension = Path.GetExtension(originalFileNameForResponse)?.ToLowerInvariant() ?? ".jpg"; // Lấy đuôi gốc, fallback .jpg
                        tempFileName = $"{uniqueSuffix}_{safeOriginalName}{extension}"; // Ví dụ: abc123xyz_ten_file_goc.jpg

                        // Lấy đường dẫn vật lý thư mục tạm
                        string tempPath = context.Server.MapPath(TempDirectory);

                        // Đảm bảo thư mục tạm tồn tại
                        if (!Directory.Exists(tempPath))
                        {
                            Directory.CreateDirectory(tempPath);
                        }

                        // Lưu file
                        string savePath = Path.Combine(tempPath, tempFileName);
                        postedFile.SaveAs(savePath);
                        Debug.WriteLine($"Uploaded temporary file: {savePath}");
                    }
                }
                else
                {
                    errorMessage = "Không nhận được file nào từ request.";
                    context.Response.StatusCode = 400;
                }
            }
            // Xử lý lỗi truy cập file (thường do xung đột khi lưu nhanh)
            catch (HttpException httpEx) when (httpEx.ErrorCode == -2147024864)
            {
                errorMessage = "Lỗi lưu file tạm (xung đột ghi file). Vui lòng thử lại.";
                context.Response.StatusCode = 500;
                Debug.WriteLine($"ERROR saving temporary file (Conflict): {httpEx.Message}");
            }
            // Xử lý các lỗi khác
            catch (Exception ex)
            {
                errorMessage = "Lỗi server khi xử lý upload."; // Không nên hiển thị chi tiết lỗi cho client
                context.Response.StatusCode = 500; // Internal Server Error
                Debug.WriteLine($"ERROR processing upload: {ex.ToString()}");
                // Cố gắng dọn dẹp file tạm nếu đã tạo tên mà lưu lỗi
                if (!string.IsNullOrEmpty(tempFileName))
                {
                    try
                    {
                        string errorSavePath = Path.Combine(context.Server.MapPath(TempDirectory), tempFileName);
                        if (File.Exists(errorSavePath)) File.Delete(errorSavePath);
                    }
                    catch { } // Bỏ qua lỗi khi xóa
                }
                tempFileName = null; // Đảm bảo không trả về tên file lỗi
            }

            // Trả về kết quả JSON cho Dropzone
            if (string.IsNullOrEmpty(errorMessage) && !string.IsNullOrEmpty(tempFileName))
            {
                // Thành công: trả về tên file tạm đã lưu và tên gốc
                context.Response.Write($"{{\"fileName\": \"{HttpUtility.JavaScriptStringEncode(tempFileName)}\", \"originalName\": \"{HttpUtility.JavaScriptStringEncode(originalFileNameForResponse)}\"}}");
            }
            else
            {
                // Thất bại: trả về thông báo lỗi
                // Nếu không set StatusCode ở trên thì mặc định là 200 OK, Dropzone sẽ coi là success nhưng không có fileName -> gây lỗi
                if (context.Response.StatusCode == 200) context.Response.StatusCode = 400; // Đặt lỗi mặc định nếu chưa có
                context.Response.Write($"{{\"error\": \"{HttpUtility.JavaScriptStringEncode(errorMessage ?? "Lỗi không xác định.")}\"}}");
            }
        }

        // Chỉ định rằng handler này không thể tái sử dụng cho request khác
        public bool IsReusable
        {
            get { return false; }
        }
    }
}