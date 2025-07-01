using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
// KHÔNG CẦN using System.Security.Cryptography;
// KHÔNG CẦN using System.Text;
using System.Web.UI;

namespace Webebook.WebForm.Admin
{
    public partial class ThemNguoiDung : System.Web.UI.Page
    {
        string connectionString = ConfigurationManager.ConnectionStrings["datawebebookConnectionString"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Master is Admin master)
                {
                    master.SetPageTitle("Thêm Người Dùng Mới");
                }
            }
            lblUsernameError.Text = "";
            lblEmailError.Text = "";
            lblMessage.Text = "";
        }

        protected void btnLuu_Click(object sender, EventArgs e)
        {
            if (!Page.IsValid)
            {
                return;
            }

            string username = txtUsername.Text.Trim();
            string email = txtEmail.Text.Trim();
            string password = txtPassword.Text; // Lấy mật khẩu gốc (plain text)
            string ten = txtTen.Text.Trim();
            string dienThoai = txtDienThoai.Text.Trim();
            int vaiTro = Convert.ToInt32(ddlVaiTro.SelectedValue);
            string trangThai = "Active";

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                try
                {
                    con.Open();

                    // 1. Kiểm tra Username hoặc Email đã tồn tại chưa?
                    string checkQuery = "SELECT COUNT(*) FROM NguoiDung WHERE Username = @Username OR Email = @Email";
                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, con))
                    {
                        checkCmd.Parameters.AddWithValue("@Username", username);
                        checkCmd.Parameters.AddWithValue("@Email", email);
                        int userCount = (int)checkCmd.ExecuteScalar();

                        if (userCount > 0)
                        {
                            string checkSpecificQuery = "SELECT CASE WHEN Username = @Username THEN 'Username' WHEN Email = @Email THEN 'Email' ELSE '' END FROM NguoiDung WHERE Username = @Username OR Email = @Email";
                            using (SqlCommand specificCmd = new SqlCommand(checkSpecificQuery, con))
                            {
                                specificCmd.Parameters.AddWithValue("@Username", username);
                                specificCmd.Parameters.AddWithValue("@Email", email);
                                string duplicateField = specificCmd.ExecuteScalar()?.ToString();
                                if (duplicateField == "Username")
                                    lblUsernameError.Text = "Tên đăng nhập đã tồn tại.";
                                else if (duplicateField == "Email")
                                    lblEmailError.Text = "Email đã được sử dụng.";
                                else
                                {
                                    lblUsernameError.Text = "Tên đăng nhập hoặc Email đã tồn tại.";
                                    lblEmailError.Text = "Tên đăng nhập hoặc Email đã tồn tại.";
                                }
                            }
                            return;
                        }
                    }

                    // 2. KHÔNG mã hóa mật khẩu

                    // 3. Thêm người dùng mới vào CSDL với mật khẩu plain text
                    string insertQuery = @"INSERT INTO NguoiDung (Username, Email, MatKhau, Ten, DienThoai, VaiTro, TrangThai)
                                         VALUES (@Username, @Email, @MatKhau, @Ten, @DienThoai, @VaiTro, @TrangThai)";
                    using (SqlCommand insertCmd = new SqlCommand(insertQuery, con))
                    {
                        insertCmd.Parameters.AddWithValue("@Username", username);
                        insertCmd.Parameters.AddWithValue("@Email", email);
                        // LƯU MẬT KHẨU PLAIN TEXT - RẤT NGUY HIỂM!
                        insertCmd.Parameters.AddWithValue("@MatKhau", password);
                        insertCmd.Parameters.AddWithValue("@Ten", string.IsNullOrEmpty(ten) ? (object)DBNull.Value : ten);
                        insertCmd.Parameters.AddWithValue("@DienThoai", string.IsNullOrEmpty(dienThoai) ? (object)DBNull.Value : dienThoai);
                        insertCmd.Parameters.AddWithValue("@VaiTro", vaiTro);
                        insertCmd.Parameters.AddWithValue("@TrangThai", trangThai);

                        int rowsAffected = insertCmd.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            Response.Redirect("QuanLyNguoiDung.aspx?message=addusersuccess");
                        }
                        else
                        {
                            lblMessage.Text = "Thêm người dùng không thành công. Vui lòng thử lại.";
                            lblMessage.CssClass = "block mb-4 text-red-600";
                        }
                    }
                }
                catch (Exception ex)
                {
                    lblMessage.Text = "Đã xảy ra lỗi: " + ex.Message;
                    lblMessage.CssClass = "block mb-4 text-red-600";
                }
            }
        }

        protected void btnHuy_Click(object sender, EventArgs e)
        {
            Response.Redirect("QuanLyNguoiDung.aspx");
        }

        // ĐÃ XÓA HÀM HASHPASSWORD
    }
}