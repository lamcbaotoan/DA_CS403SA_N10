using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls; // Needed for Panel

namespace Webebook.WebForm.Admin
{
    public partial class ChiTietDonHang_Admin : System.Web.UI.Page
    {
        // Store connection string as a readonly field
        private readonly string connectionString = ConfigurationManager.ConnectionStrings["datawebebookConnectionString"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Master is Admin master)
                {
                    master.SetPageTitle("Chi Tiết Đơn Hàng");
                }
                LoadOrderDetails();
            }
        }

        private void LoadOrderDetails()
        {
            string idDonHang = Request.QueryString["IDDonHang"];
            if (string.IsNullOrEmpty(idDonHang))
            {
                ShowMessage("Không tìm thấy ID đơn hàng được chỉ định.", isError: true);
                // Optionally hide the details sections if ID is missing
                // detailsPanel.Visible = false; // Assuming you wrap details in panels
                return;
            }

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                try
                {
                    con.Open();

                    // --- Load General Order Information ---
                    string query = @"SELECT
                                        dh.IDDonHang,
                                        ISNULL(nd.Ten, nd.Username) AS TenNguoiDung,
                                        dh.NgayDat,
                                        dh.SoTien,
                                        dh.PhuongThucThanhToan,
                                        dh.TrangThaiThanhToan
                                    FROM DonHang dh
                                    LEFT JOIN NguoiDung nd ON dh.IDNguoiDung = nd.IDNguoiDung
                                    WHERE dh.IDDonHang = @IDDonHang";

                    bool orderFound = false;
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        cmd.Parameters.AddWithValue("@IDDonHang", idDonHang);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                orderFound = true;
                                lblIDDonHang.Text = reader["IDDonHang"].ToString();
                                lblNguoiDat.Text = Server.HtmlEncode(reader["TenNguoiDung"].ToString()); // Encode user input
                                lblNgayDat.Text = Convert.ToDateTime(reader["NgayDat"]).ToString("dd/MM/yyyy HH:mm");
                                lblTongTien.Text = Convert.ToDecimal(reader["SoTien"]).ToString("N0") + " VNĐ";
                                lblPhuongThuc.Text = Server.HtmlEncode(reader["PhuongThucThanhToan"].ToString());

                                string trangThai = reader["TrangThaiThanhToan"].ToString();
                                lblTrangThai.Text = Server.HtmlEncode(trangThai);
                                SetStatusLabelStyle(trangThai); // Set the CSS class based on status
                            }
                        }
                    }

                    if (!orderFound)
                    {
                        ShowMessage($"Không tìm thấy đơn hàng với ID: {idDonHang}.", isError: true);
                        return; // Stop further processing if order not found
                    }


                    // --- Load Order Item Details ---
                    // Optional: Calculate total price per item in SQL for simplicity
                    string detailQuery = @"SELECT
                                            ctdh.IDSach,
                                            s.TenSach,
                                            ctdh.SoLuong,
                                            ctdh.Gia
                                            --, (ctdh.SoLuong * ctdh.Gia) AS ThanhTien -- Optional: Calculate in SQL
                                           FROM ChiTietDonHang ctdh
                                           JOIN Sach s ON ctdh.IDSach = s.IDSach
                                           WHERE ctdh.IDDonHang = @IDDonHang";
                    using (SqlCommand detailCmd = new SqlCommand(detailQuery, con))
                    {
                        detailCmd.Parameters.AddWithValue("@IDDonHang", idDonHang);
                        SqlDataAdapter da = new SqlDataAdapter(detailCmd);
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        // Optional: Calculate ThanhTien in C# if not done in SQL
                        // if (!dt.Columns.Contains("ThanhTien")) {
                        //    dt.Columns.Add("ThanhTien", typeof(decimal));
                        //    foreach (DataRow row in dt.Rows) {
                        //        row["ThanhTien"] = Convert.ToInt32(row["SoLuong"]) * Convert.ToDecimal(row["Gia"]);
                        //    }
                        // }


                        gvChiTiet.DataSource = dt;
                        gvChiTiet.DataBind();
                    }
                }
                catch (SqlException sqlEx) // More specific exception
                {
                    // Log the full exception details (sqlEx.ToString()) using a logging library
                    ShowMessage("Lỗi cơ sở dữ liệu khi tải chi tiết đơn hàng. Vui lòng thử lại sau.", isError: true);
                }
                catch (Exception ex) // Catch other potential exceptions
                {
                    // Log the full exception details (ex.ToString()) using a logging library
                    ShowMessage("Đã xảy ra lỗi không mong muốn. Vui lòng thử lại sau.", isError: true);
                }
            } // SqlConnection is automatically disposed here
        }

        /// <summary>
        /// Sets the visual style of the status label based on the status text.
        /// </summary>
        /// <param name="status">The order payment status string.</param>
        private void SetStatusLabelStyle(string status)
        {
            string baseCss = "text-base px-3 py-1 rounded-full font-semibold inline-block align-middle"; // Base style from ASPX
            string statusCss = "";

            // Normalize status for comparison (optional, but good practice)
            string normalizedStatus = status.Trim().ToLowerInvariant();

            // Determine CSS based on status value
            switch (normalizedStatus)
            {
                case "đã thanh toán": // Example: Vietnamese status
                case "completed":
                case "paid":
                case "thành công": // Example
                    statusCss = "bg-green-100 text-green-800";
                    break;

                case "chờ thanh toán": // Example: Vietnamese status
                case "pending":
                case "unpaid":
                case "đang xử lý": // Example
                    statusCss = "bg-yellow-100 text-yellow-800";
                    break;

                case "đã hủy": // Example: Vietnamese status
                case "cancelled":
                case "failed":
                case "thất bại": // Example
                    statusCss = "bg-red-100 text-red-800";
                    break;

                default: // Unknown or other statuses
                    statusCss = "bg-gray-100 text-gray-800";
                    break;
            }

            lblTrangThai.CssClass = $"{baseCss} {statusCss}";
        }

        /// <summary>
        /// Displays a message to the user in the designated panel.
        /// </summary>
        /// <param name="message">The text to display.</param>
        /// <param name="isError">True for error messages (red style), false for success/info (green/blue style).</param>
        private void ShowMessage(string message, bool isError)
        {
            pnlMessage.Visible = true;
            lblMessageText.Text = Server.HtmlEncode(message); // Encode message

            if (isError)
            {
                pnlMessage.CssClass = "mb-6 p-4 border border-red-300 bg-red-100 text-red-700 rounded-md flex items-center";
                lblMessageIcon.CssClass = "fas fa-exclamation-circle icon-prefix"; // Error icon
            }
            else
            {
                // Example for success message - you might need another color like blue for info
                pnlMessage.CssClass = "mb-6 p-4 border border-green-300 bg-green-100 text-green-700 rounded-md flex items-center";
                lblMessageIcon.CssClass = "fas fa-check-circle icon-prefix"; // Success icon
            }
        }
    }
}