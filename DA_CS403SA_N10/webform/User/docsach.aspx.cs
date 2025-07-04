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
using System.Text.RegularExpressions;
using System.Reflection;
using System.Text; // Thêm cho Encoding

namespace Webebook.WebForm.User
{
    public partial class docsach : System.Web.UI.Page
    {
        string connectionString = ConfigurationManager.ConnectionStrings["datawebebookConnectionString"].ConnectionString;
        int userId = 0;
        int sachId = 0;
        public int currentChuong = 0; // Public để ASPX truy cập highlight chương
        string tenSach = "";

        protected void Page_Load(object sender, EventArgs e)
        {
            // Kiểm tra đăng nhập
            if (Session["UserID"] == null || !int.TryParse(Session["UserID"].ToString(), out userId) || userId <= 0)
            {
                Response.Redirect(ResolveUrl("~/WebForm/VangLai/dangnhap.aspx") + "?returnUrl=" + Server.UrlEncode(Request.Url.PathAndQuery), false);
                Context.ApplicationInstance.CompleteRequest();
                return;
            }

            // Lấy và kiểm tra ID Sách từ QueryString
            if (!int.TryParse(Request.QueryString["IDSach"], out sachId) || sachId <= 0)
            {
                ShowMessage("ID Sách không hợp lệ.", true);
                HideReaderUI();
                return;
            }

            // Xác định chương cần hiển thị
            int requestedChapter = 0;
            bool hasRequestedChapter = int.TryParse(Request.QueryString["SoChuong"], out requestedChapter) && requestedChapter > 0;

            if (hasRequestedChapter) // Nếu có số chương trên URL
            {
                currentChuong = requestedChapter;
            }
            else // Nếu không có số chương trên URL
            {
                string lastRead = GetLastReadChapter(sachId, userId); // Lấy chương đọc lần cuối
                if (!string.IsNullOrEmpty(lastRead) && int.TryParse(lastRead, out int lr) && lr > 0)
                {
                    currentChuong = lr; // Dùng chương đọc lần cuối
                }
                else
                {
                    currentChuong = GetFirstChapterNumber(sachId); // Lấy chương đầu tiên
                }

                // Nếu không tìm được chương nào hợp lệ (sách mới, chưa có chương)
                if (currentChuong <= 0)
                {
                    ShowMessage("Sách này chưa có chương nào hoặc không xác định được chương cần đọc.", true);
                    HideReaderUI();
                    return;
                }

                // Nếu không phải postback và chưa có số chương trên URL, chuyển hướng để thêm số chương vào URL
                if (!IsPostBack)
                {
                    Response.Redirect(ResolveUrl($"~/WebForm/User/docsach.aspx?IDSach={sachId}&SoChuong={currentChuong}"), false);
                    Context.ApplicationInstance.CompleteRequest();
                    return; // Dừng xử lý trang hiện tại sau khi chuyển hướng
                }
            }

            // Chỉ tải dữ liệu lần đầu (không phải postback từ UpdatePanel)
            if (!IsPostBack)
            {
                if (LoadChapterContent()) // Tải nội dung chương
                {
                    UpdateReadingProgress(); // Cập nhật vị trí đọc
                    LoadComments();         // Tải bình luận
                    SetupNavigation();      // Cài đặt các nút điều hướng
                    pnlNavigation.Visible = true; // Hiển thị thanh điều hướng
                }
                else // Nếu không tải được nội dung chương
                {
                    HideReaderUI(); // Ẩn giao diện đọc
                }
                UpdateMasterCartCount(); // Cập nhật giỏ hàng trên MasterPage
            }

            // Ẩn thông báo khi có postback (ví dụ: sau khi gửi bình luận)
            if (lblMessage != null && IsPostBack)
            {
                lblMessage.Visible = false;
            }
        }

        // Tải nội dung chương và xác định loại nội dung
        private bool LoadChapterContent()
        {
            string loaiNoiDung = ""; string noiDungText = ""; string duongDan = "";
            string tenChuong = ""; bool chapterExists = false;

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                // Lấy thông tin chương và tên sách
                string query = @"SELECT nds.LoaiNoiDung, nds.NoiDungText, nds.DuongDan, nds.TenChuong, s.TenSach
                                 FROM NoiDungSach nds JOIN Sach s ON nds.IDSach = s.IDSach
                                 WHERE nds.IDSach = @IDSach AND nds.SoChuong = @SoChuong";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@IDSach", sachId);
                    cmd.Parameters.AddWithValue("@SoChuong", currentChuong);
                    try
                    {
                        con.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read()) // Nếu tìm thấy chương
                            {
                                chapterExists = true;
                                loaiNoiDung = reader["LoaiNoiDung"]?.ToString() ?? "";
                                noiDungText = reader["NoiDungText"] != DBNull.Value ? reader["NoiDungText"].ToString() : "";
                                duongDan = reader["DuongDan"] != DBNull.Value ? reader["DuongDan"].ToString() : "";
                                tenChuong = reader["TenChuong"] != DBNull.Value ? reader["TenChuong"].ToString() : "";
                                tenSach = reader["TenSach"]?.ToString() ?? "Không có tên sách";
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ShowMessage($"Lỗi khi tải nội dung chương: {ex.Message}", true);
                        System.Diagnostics.Debug.WriteLine($"LoadChapterContent Error: {ex.ToString()}");
                        return false; // Trả về false nếu lỗi
                    }
                }
            }

            if (!chapterExists) // Nếu không tìm thấy chương trong DB
            {
                ShowMessage($"Không tìm thấy nội dung cho chương {currentChuong} của sách này.", true);
                return false;
            }

            // Cập nhật tiêu đề trang và label
            lblBookTitleRead.Text = Server.HtmlEncode(tenSach);
            lblChapterInfoRead.Text = $"Chương {currentChuong}" + (!string.IsNullOrEmpty(tenChuong) ? $": {Server.HtmlEncode(tenChuong)}" : "");

            // Sửa dòng này:
            // Trong docsach.aspx.cs -> LoadChapterContent(), sau khi đọc reader thành công:
            string displayTenSach = string.IsNullOrWhiteSpace(tenSach) ? "Sách không tên" : tenSach;
            string displayChuong = currentChuong > 0 ? $"Chương {currentChuong}" : "Chương không xác định";
            this.Title = $"Đọc {Server.HtmlEncode(displayTenSach)} - {displayChuong}";

            // Ẩn tất cả các panel nội dung trước khi hiển thị panel phù hợp
            HideContentPanels();
            bool success = true; // Biến đánh dấu thành công

            // Xử lý hiển thị dựa trên loại nội dung
            switch (loaiNoiDung.ToLower())
            {
                case "text": // Nội dung dạng text thuần
                    litTextContent.Text = ConvertUrlsToLinks(FormatNewlinesForDisplay(Server.HtmlEncode(noiDungText)));
                    pnlTextContent.Visible = true;
                    break;
                case "file": // Nội dung là file (TXT hoặc file khác cần tải)
                    success = ProcessFileContent(duongDan);
                    break;
                case "image": // Nội dung là ảnh (truyện tranh)
                    success = LoadImageContent(duongDan);
                    break;
                case "pdf": // Nội dung là file PDF
                    success = EmbedPdfContent(duongDan);
                    break;
                default: // Loại nội dung không xác định hoặc không hỗ trợ hiển thị trực tiếp
                    success = HandleUnknownContent(loaiNoiDung, duongDan);
                    break;
            }
            return success;
        }

        // Xử lý nội dung dạng file
        private bool ProcessFileContent(string duongDan)
        {
            try
            {
                if (!string.IsNullOrEmpty(duongDan) && IsPathSafe(duongDan))
                {
                    string physicalPath = Server.MapPath("~" + duongDan);
                    if (File.Exists(physicalPath))
                    {
                        string fileExtension = Path.GetExtension(physicalPath).ToLowerInvariant();
                        if (fileExtension == ".txt") // Nếu là file TXT, đọc và hiển thị như text
                        {
                            string fileContent = File.ReadAllText(physicalPath, System.Text.Encoding.UTF8); // Chỉ định UTF8
                            litTextContent.Text = ConvertUrlsToLinks(FormatNewlinesForDisplay(Server.HtmlEncode(fileContent)));
                            pnlTextContent.Visible = true; // Hiển thị trong panel text
                            return true;
                        }
                        else // Các loại file khác, hiển thị link tải
                        {
                            DisplayDownloadLink(duongDan, fileExtension);
                            pnlFileViewer.Visible = true;
                            return true;
                        }
                    }
                    else
                    {
                        ShowMessage($"Lỗi: Không tìm thấy tệp: {Server.HtmlEncode(physicalPath)}", true);
                        return false;
                    }
                }
                else
                {
                    ShowMessage($"Lỗi: Đường dẫn tệp không hợp lệ hoặc không an toàn: {Server.HtmlEncode(duongDan)}", true);
                    return false;
                }
            }
            catch (Exception ex)
            {
                ShowMessage($"Lỗi khi đọc tệp: {ex.Message}", true);
                System.Diagnostics.Debug.WriteLine($"File Read Error: {ex.ToString()}");
                return false;
            }
        }

        // Tải và hiển thị nội dung ảnh
        private bool LoadImageContent(string duongDan)
        {
            if (!string.IsNullOrEmpty(duongDan))
            {
                List<string> imageUrls = duongDan.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(path => path.Trim())
                    .Where(path => !string.IsNullOrEmpty(path) && IsPathSafe(path)) // Kiểm tra an toàn
                    .Select(path => ResolveUrl("~" + path)) // Chuyển thành URL client
                    .ToList();

                if (imageUrls.Any())
                {
                    rptImageContent.DataSource = imageUrls;
                    rptImageContent.DataBind();
                    pnlImageContent.Visible = true; // Hiển thị panel ảnh
                    return true;
                }
                else
                {
                    ShowMessage("Lỗi: Không tìm thấy đường dẫn ảnh hợp lệ.", true);
                    return false;
                }
            }
            else
            {
                ShowMessage("Lỗi: Không có đường dẫn ảnh cho chương này.", true);
                return false;
            }
        }

        // Nhúng nội dung PDF
        private bool EmbedPdfContent(string duongDan)
        {
            if (!string.IsNullOrEmpty(duongDan) && IsPathSafe(duongDan))
            {
                DisplayPdfEmbed(ResolveUrl("~" + duongDan)); // Hiển thị PDF nhúng
                pnlFileViewer.Visible = true;
                return true;
            }
            else
            {
                ShowMessage($"Lỗi: Đường dẫn PDF không hợp lệ hoặc không an toàn: {Server.HtmlEncode(duongDan)}", true);
                return false;
            }
        }

        // Xử lý loại nội dung không xác định
        private bool HandleUnknownContent(string loaiNoiDung, string duongDan)
        {
            ShowMessage($"Lỗi: Loại nội dung '{Server.HtmlEncode(loaiNoiDung)}' không được hỗ trợ hiển thị.", true);
            if (!string.IsNullOrEmpty(duongDan) && IsPathSafe(duongDan))
            {
                DisplayDownloadLink(duongDan, Path.GetExtension(duongDan));
                pnlFileViewer.Visible = true;
                return true; // Vẫn coi là thành công vì đã hiện link tải
            }
            return false; // Thất bại nếu không có cả link tải
        }

        // Kiểm tra đường dẫn ảo có an toàn không (chỉ cho phép trong thư mục BookContent)
        private bool IsPathSafe(string virtualPath)
        {
            return !string.IsNullOrEmpty(virtualPath) &&
                   virtualPath.StartsWith("/BookContent/", StringComparison.OrdinalIgnoreCase) &&
                   !virtualPath.Contains(".."); // Ngăn chặn Path Traversal
        }

        // Hiển thị link tải file khi không xem trực tiếp được
        private void DisplayDownloadLink(string virtualPath, string fileExtension)
        {
            string fileName = Path.GetFileName(virtualPath);
            string fileUrl = ResolveUrl("~" + virtualPath);
            litFileViewerContent.Text = $@"
                 <div class='not-prose mt-6 p-4 bg-gray-100 border border-gray-300 rounded-md text-center'>
                     <p class='mb-3 text-gray-700 text-base'>Không thể hiển thị trực tiếp nội dung <strong class='font-medium'>{Server.HtmlEncode(fileName)}</strong> ({fileExtension.ToUpper()}).</p>
                     <a href='{fileUrl}' download='{Server.HtmlEncode(fileName)}' target='_blank' rel='noopener noreferrer'
                         class='inline-flex items-center px-5 py-2 bg-indigo-600 border border-transparent rounded-md font-semibold text-xs text-white uppercase tracking-widest hover:bg-indigo-700 active:bg-indigo-800 focus:outline-none focus:border-indigo-900 focus:ring focus:ring-indigo-300 disabled:opacity-25 transition'>
                         <i class='fas fa-download mr-2'></i> Tải xuống tệp
                     </a>
                 </div>";
        }

        // Hiển thị trình xem PDF nhúng
        // Helper: Hiển thị trình xem PDF nhúng
        private void DisplayPdfEmbed(string fileUrl)
        {
            // Thêm #toolbar=0 vào URL để cố gắng ẩn thanh công cụ của trình duyệt
            string embedUrl = fileUrl + "#toolbar=0";

            litFileViewerContent.Text = $@"
        <div class='not-prose file-viewer-container mt-4' style='height: 80vh; border: 1px solid #ccc;'>
            <iframe src='{embedUrl}' type='application/pdf' width='100%' height='100%'>
                <p>Trình duyệt của bạn không hỗ trợ xem PDF trực tiếp.
                    <a href='{fileUrl}' target='_blank' class='text-indigo-600 hover:underline'>Nhấn vào đây để tải về</a>.
                </p>
            </iframe>
        </div>";
        }

        // Cập nhật vị trí đọc cuối cùng của người dùng cho sách này
        private void UpdateReadingProgress()
        {
            if (userId <= 0 || sachId <= 0 || currentChuong <= 0) return;

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                // Dùng MERGE để INSERT nếu chưa có hoặc UPDATE nếu đã có trong TuSach
                string query = @"
                    MERGE TuSach AS target
                    USING (SELECT @UserId AS IDNguoiDung, @IDSach AS IDSach) AS source
                    ON target.IDNguoiDung = source.IDNguoiDung AND target.IDSach = source.IDSach
                    WHEN MATCHED THEN
                        UPDATE SET ViTriDoc = @SoChuong -- Chỉ cập nhật vị trí đọc
                    WHEN NOT MATCHED THEN
                        INSERT (IDNguoiDung, IDSach, NgayThem, ViTriDoc)
                        VALUES (source.IDNguoiDung, source.IDSach, GETDATE(), @SoChuong);";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@SoChuong", currentChuong.ToString()); // Lưu vị trí đọc là string trong DB
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    cmd.Parameters.AddWithValue("@IDSach", sachId);
                    try
                    {
                        con.Open();
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"UpdateReadingProgress Error: {ex.ToString()}");
                        // Không hiển thị lỗi cho người dùng để tránh làm phiền
                    }
                }
            }
        }

        // Lấy chương đọc lần cuối từ TuSach
        private string GetLastReadChapter(int bookId, int usrId)
        {
            string lastRead = null;
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "SELECT ViTriDoc FROM TuSach WHERE IDNguoiDung = @UserId AND IDSach = @IDSach";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@UserId", usrId);
                    cmd.Parameters.AddWithValue("@IDSach", bookId);
                    try
                    {
                        con.Open();
                        object result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            lastRead = result.ToString();
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"GetLastReadChapter Error (IDSach={bookId}, IDUser={usrId}): {ex.ToString()}");
                    }
                }
            }
            return lastRead;
        }

        // Lấy số chương đầu tiên của sách (lớn hơn 0)
        private int GetFirstChapterNumber(int bookId)
        {
            int firstChapter = 0;
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "SELECT MIN(SoChuong) FROM NoiDungSach WHERE IDSach = @IDSach AND SoChuong > 0";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@IDSach", bookId);
                    try
                    {
                        con.Open();
                        object result = cmd.ExecuteScalar();
                        if (result != null && result != DBNull.Value)
                        {
                            if (int.TryParse(result.ToString(), out int tempChapter) && tempChapter > 0)
                            {
                                firstChapter = tempChapter;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"GetFirstChapterNumber Error (IDSach={bookId}): {ex.ToString()}");
                    }
                }
            }
            return firstChapter > 0 ? firstChapter : 0; // Trả về 0 nếu không tìm thấy
        }

        // Thiết lập các nút điều hướng (Trước, Sau, Danh sách chương,...)
        private void SetupNavigation()
        {
            int minChuong = 0;
            int maxChuong = 0;
            DataTable dtChapters = new DataTable(); // Bảng chứa danh sách chương cho popup

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                try
                {
                    con.Open();
                    // Lấy chương nhỏ nhất và lớn nhất
                    string queryMinMax = "SELECT MIN(SoChuong), MAX(SoChuong) FROM NoiDungSach WHERE IDSach = @IDSach AND SoChuong > 0";
                    using (SqlCommand cmdMinMax = new SqlCommand(queryMinMax, con))
                    {
                        cmdMinMax.Parameters.AddWithValue("@IDSach", sachId);
                        using (SqlDataReader readerMinMax = cmdMinMax.ExecuteReader())
                        {
                            if (readerMinMax.Read())
                            {
                                if (readerMinMax[0] != DBNull.Value) minChuong = Convert.ToInt32(readerMinMax[0]);
                                if (readerMinMax[1] != DBNull.Value) maxChuong = Convert.ToInt32(readerMinMax[1]);
                            }
                        }
                    }
                    // Lấy danh sách chương cho popup
                    string queryList = "SELECT IDSach, SoChuong, TenChuong FROM NoiDungSach WHERE IDSach = @IDSach AND SoChuong > 0 ORDER BY SoChuong";
                    using (SqlCommand cmdList = new SqlCommand(queryList, con))
                    {
                        cmdList.Parameters.AddWithValue("@IDSach", sachId);
                        using (SqlDataAdapter da = new SqlDataAdapter(cmdList)) { da.Fill(dtChapters); }
                    }
                }
                catch (Exception ex)
                {
                    ShowMessage($"Lỗi khi lấy thông tin chương: {ex.Message}", true);
                    System.Diagnostics.Debug.WriteLine($"SetupNavigation Error: {ex.ToString()}");
                    pnlNavigation.Visible = false; // Ẩn nav nếu lỗi
                    return;
                }
            }

            // Thiết lập URL cho các nút cố định
            hlInfo.NavigateUrl = ResolveUrl($"~/WebForm/User/chitietsach_chap.aspx?IDSach={sachId}");
            hlBookshelf.NavigateUrl = ResolveUrl("~/WebForm/User/tusach.aspx");

            // CSS classes chung cho nút nav và trạng thái disable
            string baseNavButtonClass = "flex items-center justify-center w-11 h-11 hover:bg-gray-700/80 text-white rounded-full transition duration-200 ease-in-out";
            string disabledClass = " disabled opacity-50 pointer-events-none cursor-not-allowed";

            // Nút Chương Trước
            bool canGoPrev = minChuong > 0 && currentChuong > minChuong;
            hlPrevChap.Enabled = canGoPrev;
            hlPrevChap.NavigateUrl = canGoPrev ? ResolveUrl($"~/WebForm/User/docsach.aspx?IDSach={sachId}&SoChuong={currentChuong - 1}") : "#";
            hlPrevChap.CssClass = baseNavButtonClass + (canGoPrev ? "" : disabledClass);
            if (!canGoPrev) hlPrevChap.Attributes["disabled"] = "disabled"; else hlPrevChap.Attributes.Remove("disabled");

            // Nút Chương Sau
            bool canGoNext = maxChuong > 0 && currentChuong < maxChuong;
            hlNextChap.Enabled = canGoNext;
            hlNextChap.NavigateUrl = canGoNext ? ResolveUrl($"~/WebForm/User/docsach.aspx?IDSach={sachId}&SoChuong={currentChuong + 1}") : "#";
            hlNextChap.CssClass = baseNavButtonClass + (canGoNext ? "" : disabledClass);
            if (!canGoNext) hlNextChap.Attributes["disabled"] = "disabled"; else hlNextChap.Attributes.Remove("disabled");

            // Bind dữ liệu cho popup danh sách chương
            if (dtChapters.Rows.Count > 0)
            {
                rptChapterListPopup.DataSource = dtChapters;
                rptChapterListPopup.DataBind();
                btnToggleChapterList.Visible = true; // Hiện nút mở popup
            }
            else
            {
                btnToggleChapterList.Visible = false; // Ẩn nếu không có chương
            }
        }

        // Tải danh sách bình luận cho chương hiện tại
        private void LoadComments()
        {
            DataTable dtComments = new DataTable();
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                // Lấy bình luận, tên hiển thị (Tên hoặc Username) và avatar
                string query = @"
                    SELECT
                        t.BinhLuan,
                        t.NgayBinhLuan,
                        ISNULL(nd.Ten, nd.Username) AS TenHienThi,
                        nd.AnhNen
                    FROM TuongTac t
                    LEFT JOIN NguoiDung nd ON t.IDNguoiDung = nd.IDNguoiDung
                    WHERE t.IDSach = @IDSach AND t.SoChap = @SoChuong AND t.BinhLuan IS NOT NULL AND LTRIM(RTRIM(t.BinhLuan)) <> ''
                    ORDER BY t.NgayBinhLuan DESC"; // Mới nhất lên đầu

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@IDSach", sachId);
                    cmd.Parameters.AddWithValue("@SoChuong", currentChuong);
                    try
                    {
                        con.Open();
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        da.Fill(dtComments);

                        rptComments.DataSource = dtComments;
                        rptComments.DataBind();

                        lblCommentCount.Text = dtComments.Rows.Count.ToString(); // Cập nhật số lượng
                        lblNoComments.Visible = (dtComments.Rows.Count == 0); // Hiện/ẩn thông báo "chưa có bình luận"
                    }
                    catch (Exception ex)
                    {
                        // Hiển thị lỗi nếu không tải được bình luận
                        lblNoComments.Text = $"Lỗi tải bình luận: {ex.Message}";
                        lblNoComments.CssClass = "text-red-600 text-sm italic block text-center";
                        lblNoComments.Visible = true;
                        lblCommentCount.Text = "0";
                        System.Diagnostics.Debug.WriteLine($"LoadComments Error: {ex.ToString()}");
                    }
                }
            }
        }

        // Xử lý sự kiện khi người dùng gửi bình luận
        protected void btnSubmitComment_Click(object sender, EventArgs e)
        {
            Page.Validate("CommentValidation"); // Kiểm tra RequiredFieldValidator
            if (!Page.IsValid) return;

            string commentText = txtCommentInput.Text.Trim();

            // Kiểm tra lại nội dung không được rỗng hoặc chỉ có khoảng trắng
            if (string.IsNullOrWhiteSpace(commentText))
            {
                rfvComment.ErrorMessage = "Nội dung bình luận không được để trống hoặc chỉ chứa khoảng trắng.";
                rfvComment.IsValid = false;
                return; // Dừng lại nếu không hợp lệ
            }

            // Kiểm tra thông tin cần thiết
            if (userId <= 0 || sachId <= 0 || currentChuong <= 0)
            {
                // Thông báo lỗi bằng script nếu thông tin không đủ
                ScriptManager.RegisterStartupScript(UpdatePanelComments, UpdatePanelComments.GetType(), "commentErrorAlert", "alert('Lỗi: Thông tin không hợp lệ để gửi bình luận.');", true);
                return;
            }

            // Thêm bình luận vào CSDL
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"
                    INSERT INTO TuongTac (IDNguoiDung, IDSach, SoChap, BinhLuan, NgayBinhLuan)
                    VALUES (@IDNguoiDung, @IDSach, @SoChap, @BinhLuan, GETDATE())";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@IDNguoiDung", userId);
                    cmd.Parameters.AddWithValue("@IDSach", sachId);
                    cmd.Parameters.AddWithValue("@SoChap", currentChuong);
                    cmd.Parameters.AddWithValue("@BinhLuan", commentText); // Lưu text đã trim

                    try
                    {
                        con.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0) // Nếu thêm thành công
                        {
                            txtCommentInput.Text = ""; // Xóa nội dung đã nhập
                            LoadComments(); // Tải lại danh sách bình luận (UpdatePanel sẽ cập nhật)
                        }
                        else // Nếu không có dòng nào được thêm (lỗi không mong muốn)
                        {
                            ScriptManager.RegisterStartupScript(UpdatePanelComments, UpdatePanelComments.GetType(), "commentFailAlert", "alert('Gửi bình luận không thành công. Vui lòng thử lại.');", true);
                        }
                    }
                    catch (Exception ex) // Xử lý lỗi CSDL
                    {
                        System.Diagnostics.Debug.WriteLine($"Submit Comment Error: {ex.ToString()}");
                        // Mã hóa thông báo lỗi để hiển thị an toàn trong alert JS
                        string escapedErrorMessage = Server.HtmlEncode(ex.Message.Replace("'", "\\'").Replace("\r\n", " ").Replace("\n", " "));
                        ScriptManager.RegisterStartupScript(UpdatePanelComments, UpdatePanelComments.GetType(), "commentExceptionAlert", $"alert('Đã xảy ra lỗi khi gửi bình luận: {escapedErrorMessage}');", true);
                    }
                }
            }
        }

        // Helper: Lấy URL avatar (Base64 hoặc ảnh mặc định)
        protected string GetAvatarUrl(object anhNenData)
        {
            string defaultAvatar = ResolveUrl("~/Images/default_avatar.png"); // Đường dẫn ảnh mặc định
            if (anhNenData != DBNull.Value && anhNenData is byte[] avatarBytes && avatarBytes.Length > 0)
            {
                try
                {
                    // Chuyển đổi byte array thành Base64 để hiển thị trực tiếp
                    return "data:image/jpeg;base64," + Convert.ToBase64String(avatarBytes);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error converting avatar bytes: {ex.Message}");
                    return defaultAvatar; // Trả về mặc định nếu lỗi chuyển đổi
                }
            }
            return defaultAvatar; // Trả về mặc định nếu không có avatar hoặc dữ liệu không hợp lệ
        }

        // Helper: Định dạng text bình luận (HTML Encode -> Chuyển URL -> Chuyển xuống dòng)
        protected string FormatCommentText(object binhLuanData)
        {
            if (binhLuanData != DBNull.Value && binhLuanData != null)
            {
                string encodedText = Server.HtmlEncode(binhLuanData.ToString()); // Mã hóa HTML entities
                string textWithLinks = ConvertUrlsToLinks(encodedText);        // Chuyển đổi URL thành link
                string finalFormattedText = FormatNewlinesForDisplay(textWithLinks); // Chuyển \n thành <br />
                return finalFormattedText;
            }
            return ""; // Trả về chuỗi rỗng nếu null hoặc DBNull
        }

        // Helper: Chuyển đổi các URL trong text thành thẻ <a>
        private string ConvertUrlsToLinks(string encodedText)
        {
            if (string.IsNullOrEmpty(encodedText)) return "";
            // Biểu thức chính quy tìm URL (http hoặc https)
            string pattern = @"(https?:\/\/[^\s<]+)";
            // Thay thế URL tìm thấy bằng thẻ <a>
            return Regex.Replace(encodedText, pattern,
                "<a href=\"$0\" target=\"_blank\" rel=\"noopener noreferrer\" class=\"text-indigo-600 hover:text-indigo-800 hover:underline\">$0</a>",
                RegexOptions.IgnoreCase | RegexOptions.Multiline);
        }

        // Helper: Chuyển đổi ký tự xuống dòng (\n) thành thẻ <br />
        private string FormatNewlinesForDisplay(string text)
        {
            return text?.Replace("\n", "<br />") ?? ""; // An toàn với null
        }

        // Helper: Ẩn tất cả các panel chứa nội dung chính
        private void HideContentPanels()
        {
            pnlTextContent.Visible = false;
            pnlImageContent.Visible = false;
            pnlFileViewer.Visible = false;
        }

        // Helper: Ẩn toàn bộ giao diện đọc (khi có lỗi nghiêm trọng)
        private void HideReaderUI()
        {
            HideContentPanels(); // Ẩn các panel nội dung
            if (pnlNavigation != null) pnlNavigation.Visible = false; // Ẩn thanh điều hướng
            if (UpdatePanelComments != null) UpdatePanelComments.Visible = false; // Ẩn khu vực bình luận
        }

        // Helper: Hiển thị thông báo (lỗi hoặc thành công)
        private void ShowMessage(string message, bool isError)
        {
            if (lblMessage == null) return;
            lblMessage.Text = Server.HtmlEncode(message); // Mã hóa thông báo
            // CSS classes cơ bản và màu sắc dựa trên loại thông báo
            string baseClasses = "fixed top-5 right-5 max-w-md z-[1060] px-4 py-3 rounded-md shadow-lg text-sm font-medium";
            string colorClasses = isError
                ? "bg-red-100 border border-red-400 text-red-700" // Lỗi
                : "bg-green-100 border border-green-400 text-green-700"; // Thành công
            lblMessage.CssClass = $"{baseClasses} {colorClasses}";
            lblMessage.Visible = true;

            // Tự động ẩn thông báo thành công sau vài giây
            if (!isError)
            {
                ScriptManager.RegisterStartupScript(this, this.GetType(), "AutoHideSuccessMessage",
                    "setTimeout(function() { var lbl = document.getElementById('" + lblMessage.ClientID + "'); if(lbl) { lbl.style.transition = 'opacity 0.5s ease-out'; lbl.style.opacity = '0'; setTimeout(function() { lbl.style.display='none'; lbl.style.opacity = '1'; }, 500); } }, 4000);", // 4 giây
                    true);
            }
        }

        // Helper: Cập nhật số lượng giỏ hàng trên Master Page (nếu có)
        private void UpdateMasterCartCount()
        {
            try
            {
                // Thử cast Master Page sang kiểu cụ thể
                if (Master is Webebook.WebForm.User.UserMaster master)
                {
                    master.UpdateCartCount(); // Gọi phương thức public trên Master
                }
                // Cách khác: Dùng Reflection (ít ưu tiên hơn)
                else if (Master != null)
                {
                    MethodInfo updateMethod = Master.GetType().GetMethod("UpdateCartCount");
                    if (updateMethod != null)
                    {
                        updateMethod.Invoke(Master, null);
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine("Không tìm thấy phương thức UpdateCartCount trên Master Page.");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi truy cập Master Page hoặc phương thức UpdateCartCount: {ex.Message}");
            }
        }

    } // Kết thúc class docsach
} // Kết thúc namespace Webebook.WebForm.User