using System;
using System.Data.SqlClient;
using System.Configuration;
using System.Web.UI; // Thêm using này để dùng Page.IsValid
using System.Web.UI.WebControls; // Thêm using này để dùng Label

namespace Webebook.WebForm.VangLai
{
    public partial class dangky : System.Web.UI.Page
    {
        string connectionString = ConfigurationManager.ConnectionStrings["datawebebookConnectionString"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Reset lỗi khi load lần đầu (có thể giữ hoặc bỏ nếu không cần)
            if (!IsPostBack)
            {
                lblUsernameError.Visible = false;
                lblEmailError.Visible = false;
                lblPasswordError.Visible = false;
            }
        }

        protected void btnDangKy_Click(object sender, EventArgs e)
        {
            // Reset các label lỗi backend trước mỗi lần submit
            lblUsernameError.Text = "";
            lblUsernameError.Visible = false;
            lblEmailError.Text = "";
            lblEmailError.Visible = false;
            lblPasswordError.Text = ""; // Dùng cho lỗi mật khẩu hoặc lỗi chung
            lblPasswordError.Visible = false;


            // Kích hoạt validation phía client/server
            Page.Validate();
            if (!Page.IsValid)
            {
                return; // Dừng nếu validation cơ bản thất bại
            }

            // Lấy dữ liệu từ form
            string username = txtUsername.Text.Trim();
            string email = txtEmail.Text.Trim();
            string password = txtPassword.Text; // **LẤY PLAIN TEXT**

            // Kiểm tra độ dài mật khẩu thủ công (logic gốc của bạn)
            // Lưu ý: RegularExpressionValidator đã làm việc này rồi.
            if (password.Length < 6)
            {
                // Hiển thị lỗi vào Label (theo logic gốc)
                // Lưu ý: Gán Text vào label sẽ không có icon tự động như validator
                // Nên có thể dùng validator vẫn tốt hơn.
                lblPasswordError.Text = "Mật khẩu phải có ít nhất 6 ký tự.";
                lblPasswordError.Visible = true; // Hiển thị label lỗi này
                return;
            }

            // Bắt đầu kiểm tra và insert vào DB
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();

                    // --- Bước 1: Kiểm tra Username hoặc Email đã tồn tại (logic gốc) ---
                    string checkQuery = "SELECT COUNT(*) FROM NguoiDung WHERE Username = @Username OR Email = @Email";
                    using (SqlCommand checkCommand = new SqlCommand(checkQuery, connection))
                    {
                        checkCommand.Parameters.AddWithValue("@Username", username);
                        checkCommand.Parameters.AddWithValue("@Email", email);
                        int existingUserCount = (int)checkCommand.ExecuteScalar();

                        if (existingUserCount > 0)
                        {
                            // Đã tồn tại -> Kiểm tra xem cái nào trùng (logic gốc)
                            string checkSpecificQuery = "SELECT CASE WHEN Username = @Username THEN 'Username' WHEN Email = @Email THEN 'Email' ELSE '' END AS DuplicateField FROM NguoiDung WHERE Username = @Username OR Email = @Email";
                            using (SqlCommand specificCommand = new SqlCommand(checkSpecificQuery, connection))
                            {
                                specificCommand.Parameters.AddWithValue("@Username", username);
                                specificCommand.Parameters.AddWithValue("@Email", email);

                                // Gán lỗi vào các label tương ứng (logic gốc)
                                // Sử dụng icon và span để giống validator
                                string duplicateField = specificCommand.ExecuteScalar()?.ToString();
                                if (duplicateField == "Username")
                                {
                                    lblUsernameError.Text = "<i class='fas fa-exclamation-circle'></i> <span>Tên đăng nhập đã tồn tại.</span>";
                                    lblUsernameError.Visible = true;
                                }
                                else if (duplicateField == "Email")
                                {
                                    lblEmailError.Text = "<i class='fas fa-exclamation-circle'></i> <span>Email đã được sử dụng.</span>";
                                    lblEmailError.Visible = true;
                                }
                                else // Trường hợp hiếm gặp khác
                                {
                                    lblUsernameError.Text = "<i class='fas fa-exclamation-circle'></i> <span>Tên đăng nhập hoặc email đã tồn tại.</span>";
                                    lblUsernameError.Visible = true;
                                }
                            }
                            return; // Dừng lại vì đã có lỗi trùng lặp
                        }
                    } // checkCommand Dispose

                    // --- Bước 2: Nếu không trùng, tiến hành INSERT (logic gốc) ---

                    // **CẢNH BÁO BẢO MẬT NGHIÊM TRỌNG: KHÔNG BAO GIỜ LƯU MẬT KHẨU DẠNG NÀY**
                    // **BẠN PHẢI THAY THẾ BẰNG VIỆC MÃ HÓA/BĂM MẬT KHẨU**
                    string hashedPassword = password; // **CHỈ LÀ GIẢ ĐỊNH - THAY BẰNG HASH THỰC TẾ**

                    // Câu lệnh INSERT gốc (chỉ có Username, Email, MatKhau, VaiTro)
                    string insertQuery = "INSERT INTO NguoiDung (Username, Email, MatKhau, VaiTro) VALUES (@Username, @Email, @MatKhau, @VaiTro)";
                    using (SqlCommand insertCommand = new SqlCommand(insertQuery, connection))
                    {
                        insertCommand.Parameters.AddWithValue("@Username", username);
                        insertCommand.Parameters.AddWithValue("@Email", email);
                        insertCommand.Parameters.AddWithValue("@MatKhau", hashedPassword); // **PHẢI LƯU HASHED PASSWORD**
                        insertCommand.Parameters.AddWithValue("@VaiTro", 1); // Mặc định là User thường

                        int rowsAffected = insertCommand.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            // Đăng ký thành công, chuyển hướng về trang đăng nhập (logic gốc)
                            Response.Redirect("~/WebForm/VangLai/dangnhap.aspx?registered=true", false);
                            // Ngăn code chạy tiếp sau redirect
                            Context.ApplicationInstance.CompleteRequest();
                        }
                        else
                        {
                            // Lỗi không xác định khi insert
                            lblPasswordError.Text = "<i class='fas fa-exclamation-circle'></i> <span>Đăng ký không thành công do lỗi hệ thống.</span>";
                            lblPasswordError.Visible = true;
                        }
                    } // insertCommand Dispose
                }
                catch (Exception ex) // Bắt lỗi chung (logic gốc)
                {
                    // Ghi log lỗi để debug
                    System.Diagnostics.Debug.WriteLine($"ERROR Registering: {ex.ToString()}");
                    // Hiển thị lỗi chung vào một label (ví dụ: lblPasswordError)
                    lblPasswordError.Text = "<i class='fas fa-exclamation-circle'></i> <span>Lỗi hệ thống khi đăng ký: " + ex.Message + "</span>";
                    lblPasswordError.Visible = true;
                }
            } // connection Dispose
        } // End btnDangKy_Click
    } // End class
} // End namespace