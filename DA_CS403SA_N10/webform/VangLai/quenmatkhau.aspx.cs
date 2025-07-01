using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Net.Mail;
using System.Web.UI;
using System.Web.UI.WebControls; // Đảm bảo có using này

namespace Webebook.WebForm.VangLai
{
    public partial class quenmatkhau : System.Web.UI.Page
    {
        string connectionString = ConfigurationManager.ConnectionStrings["datawebebookConnectionString"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                lblMessage.Visible = false;
                lblResetMessage.Visible = false;
            }
        }

        protected void btnGui_Click(object sender, EventArgs e)
        {
            Page.Validate("RequestGroup");
            if (!Page.IsValid) return;

            string email = txtEmail.Text.Trim();
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                // Giữ nguyên câu truy vấn gốc và logic kiểm tra email
                // Nên thêm kiểm tra TrangThai = 1 nếu cần tài khoản hoạt động
                string query = "SELECT COUNT(*) FROM NguoiDung WHERE Email = @Email";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Email", email);
                    try
                    {
                        con.Open();
                        int count = (int)cmd.ExecuteScalar();
                        if (count > 0)
                        {
                            // Giữ nguyên logic kiểm tra rate limit bằng Session
                            if (Session["LastRequestTime"] != null)
                            {
                                DateTime lastRequestTime = (DateTime)Session["LastRequestTime"];
                                if ((DateTime.Now - lastRequestTime).TotalSeconds < 60)
                                {
                                    ShowMessage(lblMessage, "Vui lòng đợi 60 giây trước khi yêu cầu mã mới.", MessageType.Warning);
                                    return;
                                }
                            }

                            // Giữ nguyên logic tạo mã, lưu Session
                            string maXacNhan = GenerateConfirmationCode();
                            Session["MaXacNhan"] = maXacNhan;
                            Session["EmailReset"] = email;
                            Session["LastRequestTime"] = DateTime.Now;
                            // Có thể thêm thời gian hết hạn cho mã nếu muốn:
                            // Session["CodeExpiry"] = DateTime.Now.AddMinutes(10);

                            try
                            {
                                SendConfirmationEmail(email, maXacNhan);
                                ShowMessage(lblMessage, "Mã xác nhận đã được gửi tới email của bạn.", MessageType.Info);
                            }
                            catch
                            {
                                ShowMessage(lblMessage, "Không thể gửi email. Vui lòng thử lại sau.", MessageType.Error);
                                return;
                            }
                            // divRequestEmail.Visible = false; // Ẩn form email nếu muốn
                            divResetPassword.Visible = true; // Hiện form reset
                                                             // ShowMessage(lblResetMessage, "Mã đã được tạo...", MessageType.Info); // Bỏ thông báo này nếu không cần

                            // Giữ nguyên logic gọi JS để hiện popup
                            ScriptManager.RegisterStartupScript(this, this.GetType(), "ShowConfirmationPopup", "showPopup();", true);
                        }
                        else
                        {
                            // Email không tồn tại
                            ShowMessage(lblMessage, "Email không tồn tại.", MessageType.Error);
                        }
                    }
                    // Catch lỗi gốc, hiển thị thông báo chung
                    catch (Exception ex)
                    {
                        // Log lỗi chi tiết để debug (quan trọng)
                        Debug.WriteLine($"Lỗi btnGui_Click: {ex.ToString()}");
                        // Hiển thị lỗi chung cho người dùng
                        ShowMessage(lblMessage, "Đã xảy ra lỗi khi kiểm tra email. Vui lòng thử lại.", MessageType.Error);
                    }
                } // cmd Dispose
            } // con Dispose
        }

        protected void btnXacNhan_Click(object sender, EventArgs e)
        {
            Page.Validate("ResetGroup");
            if (!Page.IsValid) return;

            // Giữ nguyên logic kiểm tra Session
            if (Session["MaXacNhan"] == null || Session["EmailReset"] == null)
            {
                ShowMessage(lblResetMessage, "Phiên làm việc không hợp lệ hoặc đã hết hạn. Vui lòng yêu cầu lại mã.", MessageType.Error);
                // Có thể ẩn form reset và hiện lại form email ở đây nếu muốn
                // divResetPassword.Visible = false;
                // divRequestEmail.Visible = true;
                return;
            }
            // Kiểm tra thời gian hết hạn nếu bạn có đặt Session["CodeExpiry"]
            /*
            if (Session["CodeExpiry"] == null || DateTime.Now > (DateTime)Session["CodeExpiry"])
            {
                 ShowMessage(lblResetMessage, "Mã xác nhận đã hết hạn. Vui lòng yêu cầu lại.", MessageType.Error);
                 // Nên ẩn form reset và hiện lại form email
                 return;
            }
            */

            // Giữ nguyên logic kiểm tra mã xác nhận
            if (txtMaXacNhan.Text.Trim() != Session["MaXacNhan"].ToString())
            {
                ShowMessage(lblResetMessage, "Mã xác nhận không đúng.", MessageType.Error);
                return;
            }

            // Mật khẩu khớp đã được kiểm tra bằng CompareValidator

            string email = Session["EmailReset"].ToString();
            // **QUAN TRỌNG: PHẢI BĂM MẬT KHẨU TRƯỚC KHI LƯU**
            string newPasswordPlainText = txtMatKhauMoi.Text;
            // string hashedPassword = YourPasswordHashingFunction(newPasswordPlainText);

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                // Giữ nguyên logic cập nhật mật khẩu
                string query = "UPDATE NguoiDung SET MatKhau = @MatKhau WHERE Email = @Email";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    // cmd.Parameters.AddWithValue("@MatKhau", hashedPassword); // Dùng mật khẩu đã băm
                    cmd.Parameters.AddWithValue("@MatKhau", newPasswordPlainText); // TẠM DÙNG - **KHÔNG AN TOÀN**
                    cmd.Parameters.AddWithValue("@Email", email);
                    try
                    {
                        con.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            ShowMessage(lblMessage, "Đặt lại mật khẩu thành công! Bạn có thể đăng nhập.", MessageType.Success);
                            divResetPassword.Visible = false; // Ẩn form reset
                            divRequestEmail.Visible = true;   // Hiện lại form email (để thấy thông báo thành công)
                            txtEmail.Text = email;           // Có thể điền lại email
                           // ClearResetSession(); // Xóa session
                            Response.Redirect("dangnhap.aspx", false);
                        }
                        else
                        {
                            ShowMessage(lblResetMessage, "Không thể cập nhật mật khẩu. Email không tồn tại hoặc có lỗi xảy ra.", MessageType.Error);
                        }
                    }
                    // Catch lỗi gốc khi cập nhật
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Lỗi btnXacNhan_Click: {ex.ToString()}");
                        ShowMessage(lblResetMessage, "Đã xảy ra lỗi khi cập nhật mật khẩu.", MessageType.Error);
                    }
                } // cmd Dispose
            } // con Dispose
        }

        private void SendConfirmationEmail(string toEmail, string maXacNhan)
        {
            string subject = "Mã xác nhận đặt lại mật khẩu";
            string body = $"Mã xác nhận của bạn là: <b>{maXacNhan}</b><br/>Mã này có hiệu lực trong 10 phút.";

            MailMessage message = new MailMessage();
            message.To.Add(toEmail);
            message.Subject = subject;
            message.Body = body;
            message.IsBodyHtml = true;
            message.From = new MailAddress("webebookrecreate@gmail.com"); // phải trùng với web.config

            SmtpClient smtp = new SmtpClient();
            try
            {
                smtp.Send(message);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi gửi email: {ex.Message}");
                throw new Exception("Không thể gửi email xác nhận.");
            }
        }

        protected void btnHuy_Click(object sender, EventArgs e)
        {
            // Giữ nguyên logic nút Hủy
            divResetPassword.Visible = false;
            divRequestEmail.Visible = true;
            lblMessage.Visible = false;
            lblResetMessage.Visible = false;
            txtMaXacNhan.Text = "";
            txtMatKhauMoi.Text = "";
            txtXacNhanMatKhau.Text = "";
            ClearResetSession();
        }

        private string GenerateConfirmationCode()
        {
            // Giữ nguyên hàm tạo mã
            Random random = new Random();
            return random.Next(1000, 10000).ToString("D4"); // Đảm bảo 4 chữ số
        }

        private enum MessageType { Success, Error, Warning, Info }

        // Hàm hiển thị thông báo đã sửa lỗi cú pháp
        private void ShowMessage(Label labelControl, string message, MessageType type)
        {
            labelControl.Text = message;
            string baseClasses = "block w-full p-4 mb-4 text-sm rounded-lg border";
            string specificClasses = "";

            switch (type)
            {
                case MessageType.Success: specificClasses = "bg-green-50 border-green-300 text-green-800"; break;
                case MessageType.Error: specificClasses = "bg-red-50 border-red-300 text-red-800"; break;
                case MessageType.Warning: specificClasses = "bg-yellow-50 border-yellow-300 text-yellow-800"; break;
                case MessageType.Info: specificClasses = "bg-blue-50 border-blue-300 text-blue-800"; break;
            }
            labelControl.CssClass = $"{baseClasses} {specificClasses}";
            labelControl.Visible = true;
        }

        // Hàm xóa session gốc
        private void ClearResetSession()
        {
            Session.Remove("MaXacNhan");
            Session.Remove("EmailReset");
            Session.Remove("LastRequestTime");
            // Session.Remove("CodeExpiry"); // Xóa nếu bạn có dùng
        }

        // **NHẮC LẠI QUAN TRỌNG:**
        // 1. Băm Mật Khẩu: Phần cập nhật mật khẩu đang lưu plain text. Bạn BẮT BUỘC phải thay thế bằng việc gọi hàm băm mật khẩu trước khi lưu vào CSDL.
        // 2. Bảo mật mã xác nhận: Hiển thị mã trực tiếp không phải là cách làm an toàn trong thực tế. Mã nên được gửi qua email/SMS.
    }
}