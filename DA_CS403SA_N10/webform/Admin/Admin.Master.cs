using System;
using System.Web;
using System.Web.Security; // Cần cho FormsAuthentication
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Diagnostics; // Giữ lại để dùng Debug.WriteLine

namespace Webebook.WebForm.Admin
{
    public partial class Admin : System.Web.UI.MasterPage
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            // --- Header kiểm soát Cache (Quan trọng cho các trang quản trị) ---
            Response.Cache.SetCacheability(HttpCacheability.NoCache); // Không cache ở client proxy
            Response.Cache.SetNoStore(); // Không lưu trữ ở bất kỳ đâu
            Response.Cache.SetExpires(DateTime.UtcNow.AddYears(-1)); // Hết hạn trong quá khứ
            // Response.Cache.SetMustRevalidate(true); // ĐÃ BỊ XÓA/GHI CHÚ - Không tồn tại trong mọi phiên bản Framework
            Response.Cache.SetNoTransforms(); // Không cho phép thay đổi nội dung
            // ---

            // --- Kiểm tra Xác thực và Phân quyền ---
            bool daXacThuc = Session["UserID"] != null && Session["VaiTro"] != null;
            bool laQuanTriVien = false;

            if (daXacThuc)
            {
                try
                {
                    // Đảm bảo chuyển đổi VaiTro sang số nguyên an toàn
                    if (int.TryParse(Session["VaiTro"]?.ToString(), out int giaTriVaiTro))
                    {
                        laQuanTriVien = (giaTriVaiTro == 0); // Giả sử 0 là vai trò Admin
                    }
                    else
                    {
                        // Ghi log cảnh báo nếu vai trò không phải là số nguyên hợp lệ
                        Debug.WriteLine($"CẢNH BÁO: Session['VaiTro'] không phải là số nguyên hợp lệ: {Session["VaiTro"]}");
                        daXacThuc = false; // Coi như chưa xác thực nếu vai trò không hợp lệ
                    }
                }
                catch (Exception ex)
                {
                    // Ghi log lỗi nếu có vấn đề trong quá trình xử lý Session
                    Debug.WriteLine($"LỖI xử lý Session['VaiTro']: {ex.Message}");
                    daXacThuc = false; // Coi như chưa xác thực nếu có lỗi
                }
            }

            // Quan trọng: Kiểm tra trên MỌI request (cả GET và POST)
            if (!daXacThuc || !laQuanTriVien)
            {
                // Ghi log chi tiết về việc xác thực/phân quyền thất bại
                Debug.WriteLine($"!!! XÁC THỰC/PHÂN QUYỀN THẤT BẠI khi tải trang. UserID Session: {Session["UserID"]}, VaiTro Session: {Session["VaiTro"]} !!!");

                // Dọn dẹp session và cookie trước khi chuyển hướng
                DangXuatNguoiDungHienTai(false); // false: Chỉ dọn dẹp, không redirect từ hàm này

                // Chuyển hướng người dùng về trang đăng nhập
                Response.Redirect("~/WebForm/VangLai/dangnhap.aspx", false); // false: không dừng thực thi ngay lập tức

                // Hoàn tất yêu cầu một cách sạch sẽ sau khi chuyển hướng
                Context.ApplicationInstance.CompleteRequest();
                return; // Dừng thực thi thêm mã trong Page_Load cho request này
            }
            else
            {
                // --- Người dùng đã đăng nhập và là Admin ---
                // Chỉ thiết lập thông tin người dùng và tiêu đề mặc định trong lần tải đầu tiên (GET request)
                if (!IsPostBack)
                {
                    // Lấy tên hiển thị, ưu tiên UsernameDisplay, sau đó Username, cuối cùng là mặc định
                    string tenHienThi = Session["UsernameDisplay"]?.ToString()
                                      ?? Session["Username"]?.ToString()
                                      ?? "Quản trị viên"; // Tên mặc định nếu không có session

                    // Luôn kiểm tra control có tồn tại không trước khi truy cập
                    if (lblUsername != null)
                    {
                        lblUsername.Text = "Xin chào, " + HttpUtility.HtmlEncode(tenHienThi);
                        lblUsername.ToolTip = "Người dùng: " + HttpUtility.HtmlEncode(tenHienThi); // Thêm tooltip rõ hơn
                    }

                    // Đặt tiêu đề trang mặc định nếu cần (sẽ bị ghi đè bởi trang con gọi SetPageTitle)
                    // Client-side script cũng cố gắng đặt tiêu đề này dựa trên link active nếu chưa được đặt ở đây.
                    if (lblPageTitle != null && (string.IsNullOrWhiteSpace(lblPageTitle.Text) || lblPageTitle.Text == "Trang Quản Trị"))
                    {
                        // Để trống ở đây và để JS xử lý, hoặc đặt tiêu đề mặc định nếu muốn
                    }
                    else if (lblPageTitle != null && !string.IsNullOrWhiteSpace(lblPageTitle.Text))
                    {
                        // Nếu tiêu đề ĐÃ được đặt, đảm bảo thẻ <title> HTML khớp.
                        Page.Title = HttpUtility.HtmlEncode(lblPageTitle.Text) + " - Quản trị Webebook";
                    }
                    else if (Page.Title == "Trang Quản Trị - Webebook" || string.IsNullOrWhiteSpace(Page.Title))
                    {
                        // Nếu chưa có tiêu đề nào được đặt, đặt tiêu đề mặc định cơ bản
                        Page.Title = "Quản trị Webebook";
                    }
                }
            }
        }

        // Xử lý sự kiện Click cho nút Đăng xuất (được kích hoạt bởi __doPostBack từ client-side)
        protected void btnLogout_Click(object sender, EventArgs e)
        {
            DangXuatNguoiDungHienTai(true); // true: Chuyển hướng sau khi đăng xuất
        }

        // Hàm thực hiện quá trình đăng xuất
        private void DangXuatNguoiDungHienTai(bool chuyenHuongSauDangXuat)
        {
            Debug.WriteLine($"Bắt đầu quá trình đăng xuất. Chuyển hướng: {chuyenHuongSauDangXuat}");
            try
            {
                // Xóa tất cả các biến trong Session hiện tại
                Session.Clear();
                // Hủy bỏ Session (giải phóng tài nguyên server và xóa session ID cookie)
                Session.Abandon();
                // Đăng xuất khỏi Forms Authentication (xóa vé xác thực)
                FormsAuthentication.SignOut();

                // --- Xóa Cookie Xác thực một cách tường minh ---
                if (Request.Cookies[FormsAuthentication.FormsCookieName] != null)
                {
                    HttpCookie authCookie = new HttpCookie(FormsAuthentication.FormsCookieName)
                    {
                        Value = string.Empty, // Xóa giá trị
                        Expires = DateTime.Now.AddDays(-1d), // Đặt ngày hết hạn trong quá khứ
                        Path = FormsAuthentication.FormsCookiePath // Đảm bảo đúng đường dẫn của cookie
                    };
                    Response.Cookies.Add(authCookie); // Ghi đè cookie cũ ở client
                    Debug.WriteLine("Cookie xác thực Forms Authentication đã được yêu cầu xóa.");
                }

                // --- Xóa Cookie Session ASP.NET một cách tường minh (Tùy chọn nhưng nên làm) ---
                if (Request.Cookies["ASP.NET_SessionId"] != null)
                {
                    HttpCookie sessionCookie = new HttpCookie("ASP.NET_SessionId")
                    {
                        Value = string.Empty,
                        Expires = DateTime.Now.AddDays(-1),
                        Path = "/" // Thường là đường dẫn gốc
                    };
                    Response.Cookies.Add(sessionCookie);
                    Debug.WriteLine("Cookie ASP.NET_SessionId đã được yêu cầu xóa.");
                }
            }
            catch (Exception ex)
            {
                // Ghi log bất kỳ lỗi nào xảy ra trong quá trình dọn dẹp
                Debug.WriteLine($"LỖI trong quá trình dọn dẹp khi đăng xuất: {ex.Message}");
            }

            // Chuyển hướng về trang đăng nhập nếu được yêu cầu
            if (chuyenHuongSauDangXuat)
            {
                try
                {
                    Response.Redirect("~/WebForm/VangLai/dangnhap.aspx", false);
                    Context.ApplicationInstance.CompleteRequest(); // Kết thúc request hiện tại một cách an toàn
                    Debug.WriteLine("Đã yêu cầu chuyển hướng đến trang đăng nhập sau khi đăng xuất.");
                }
                catch (System.Threading.ThreadAbortException)
                {
                    // Bắt ngoại lệ này để tránh lỗi không mong muốn
                    Debug.WriteLine("Bắt được ThreadAbortException trong quá trình chuyển hướng đăng xuất (mong đợi).");
                }
                catch (Exception ex)
                {
                    // Ghi log các lỗi khác có thể xảy ra trong quá trình chuyển hướng
                    Debug.WriteLine($"LỖI trong quá trình chuyển hướng sau đăng xuất: {ex.Message}");
                }
            }
        }

        // Hàm công khai để các trang con (.aspx) có thể gọi để đặt tiêu đề cho trang
        public void SetPageTitle(string title)
        {
            // Làm sạch và mã hóa tiêu đề để tránh XSS
            string tieuDeDaXuLy = string.IsNullOrWhiteSpace(title) ? "Trang Quản Trị" : HttpUtility.HtmlEncode(title);

            // Cập nhật Label tiêu đề trên header nếu control tồn tại
            if (lblPageTitle != null)
            {
                lblPageTitle.Text = tieuDeDaXuLy;
            }

            // Cập nhật thẻ <title> của trang HTML
            Page.Title = tieuDeDaXuLy + " - Quản trị Webebook";
        }
    }
}