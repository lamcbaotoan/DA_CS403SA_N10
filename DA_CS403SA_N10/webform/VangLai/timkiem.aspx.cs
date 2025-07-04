using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Diagnostics; // Giữ lại để debug

namespace Webebook.WebForm.VangLai
{
    public partial class timkiem : System.Web.UI.Page
    {
        string connectionString = ConfigurationManager.ConnectionStrings["datawebebookConnectionString"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadSearchResults();
            }
            // Giữ nguyên logic ẩn/hiện message
            if (lblMessage.Text == string.Empty) // Ẩn nếu không có text, bất kể postback
            {
                lblMessage.Visible = false;
            }
        }

        private void LoadSearchResults()
        {
            string keyword = Request.QueryString["q"];
            // Sử dụng HtmlEncode để tránh XSS khi hiển thị lại keyword
            litKeyword.Text = HttpUtility.HtmlEncode(keyword ?? "..."); // Hiển thị '...' nếu keyword null
            pnlNoResults.Visible = false; // Ẩn panel no results ban đầu

            if (string.IsNullOrWhiteSpace(keyword))
            {
                // Hiển thị thông báo cảnh báo (màu vàng)
                ShowMessage("Vui lòng nhập từ khóa tìm kiếm.", true, true);
                rptKetQua.DataSource = null;
                rptKetQua.DataBind();
                pnlNoResults.Visible = true; // Hiện panel no results vì không có keyword
                return;
            }

            DataTable dt = new DataTable();
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                // Giữ nguyên logic truy vấn (LIKE hoặc CONTAINSTABLE)
                // === Original LIKE Query (Fallback) ===
                string query = @"SELECT IDSach, TenSach, TacGia, GiaSach, DuongDanBiaSach
                                 FROM Sach
                                 WHERE TenSach LIKE @Keyword
                                    OR TacGia LIKE @Keyword
                                    OR MoTa LIKE @Keyword       -- Cân nhắc hiệu năng khi LIKE trên cột lớn như MoTa
                                    OR TheLoaiChuoi LIKE @Keyword
                                    OR LoaiSach LIKE @Keyword   -- Giả sử LoaiSach là tên loại (text)
                                 ORDER BY TenSach"; // Hoặc ORDER BY phù hợp hơn

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Keyword", "%" + keyword + "%");

                    try
                    {
                        con.Open();
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        da.Fill(dt);
                        rptKetQua.DataSource = dt;
                        rptKetQua.DataBind();

                        if (dt.Rows.Count == 0)
                        {
                            pnlNoResults.Visible = true;
                            lblMessage.Visible = false; // Không cần hiển thị message lỗi nếu đã có panel 'no results'
                        }
                        else
                        {
                            pnlNoResults.Visible = false; // Đảm bảo ẩn nếu có kết quả
                        }
                    }
                    catch (Exception ex)
                    {
                        // Hiển thị thông báo lỗi (màu đỏ)
                        ShowMessage("Đã xảy ra lỗi trong quá trình tìm kiếm. Vui lòng thử lại.", true);
                        Debug.WriteLine($"Lỗi LoadSearchResults (VangLai): {ex.ToString()}"); // Log lỗi chi tiết
                        rptKetQua.DataSource = null; // Xóa dữ liệu cũ nếu có lỗi
                        rptKetQua.DataBind();
                        pnlNoResults.Visible = true; // Có thể hiển thị panel no results khi lỗi
                    }
                } // End using SqlCommand
            } // End using SqlConnection
        }

        // Hàm hiển thị thông báo - Cập nhật CSS classes nếu cần
        private void ShowMessage(string message, bool isErrorOrWarning, bool useYellow = false)
        {
            lblMessage.Text = HttpUtility.HtmlEncode(message); // Encode để an toàn
            string baseClasses = "block mb-6 p-4 rounded-lg border text-sm"; // Lớp cơ bản cho thông báo

            if (isErrorOrWarning)
            {
                lblMessage.CssClass = useYellow
                    ? $"{baseClasses} bg-yellow-50 border-yellow-300 text-yellow-800" // Warning (Yellow)
                    : $"{baseClasses} bg-red-50 border-red-300 text-red-800";       // Error (Red)
            }
            else // Success (Green) - Ít dùng ở trang tìm kiếm trừ khi có hành động khác
            {
                lblMessage.CssClass = $"{baseClasses} bg-green-50 border-green-300 text-green-800";
            }
            lblMessage.Visible = true;
        }

        // Nếu có phân trang, cần xử lý sự kiện PageIndexChanging ở đây
        // protected void rptKetQua_PageIndexChanging(...) { ... }
    }
}