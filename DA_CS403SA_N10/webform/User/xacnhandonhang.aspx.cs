using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Globalization; // Cần cho CultureInfo nếu dùng lại
using System.Web.UI;
using System.Web.UI.WebControls; // Cần cho Label

namespace Webebook.WebForm.User
{
    public partial class xacnhandonhang : System.Web.UI.Page
    {
        string connectionString = ConfigurationManager.ConnectionStrings["datawebebookConnectionString"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Load thông tin xác nhận
                LoadOrderConfirmation();

                // Cập nhật số lượng giỏ hàng trên master page (giờ sẽ là 0)
                if (Master is UserMaster master)
                {
                    master.UpdateCartCount();
                }
            }
        }

        /// <summary>
        /// Tải và hiển thị thông tin đơn hàng dựa trên ID từ QueryString.
        /// </summary>
        private void LoadOrderConfirmation()
        {
            string idDonHangStr = Request.QueryString["IDDonHang"];
            int idDonHang;

            // Kiểm tra ID đơn hàng hợp lệ
            if (string.IsNullOrEmpty(idDonHangStr) || !int.TryParse(idDonHangStr, out idDonHang))
            {
                ShowMessage("ID đơn hàng không hợp lệ hoặc không tìm thấy.", isError: true);
                pnlOrderDetails.Visible = false; // Ẩn panel chi tiết nếu lỗi
                return;
            }

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                // Chỉ lấy các cột cần thiết
                string query = @"SELECT NgayDat, SoTien, PhuongThucThanhToan, IDNguoiDung
                                 FROM DonHang
                                 WHERE IDDonHang = @IDDonHang";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@IDDonHang", idDonHang);
                    try
                    {
                        con.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                // Kiểm tra xem đơn hàng có thuộc về người dùng đang đăng nhập không (tăng cường bảo mật)
                                int orderUserId = Convert.ToInt32(reader["IDNguoiDung"]);
                                int currentUserId = (Session["UserID"] != null) ? Convert.ToInt32(Session["UserID"]) : -1;

                                if (currentUserId == -1 || orderUserId != currentUserId)
                                {
                                    ShowMessage("Bạn không có quyền xem đơn hàng này.", isError: true);
                                    pnlOrderDetails.Visible = false;
                                    return;
                                }

                                // Hiển thị thông tin
                                lblIDDonHang.Text = idDonHang.ToString();
                                lblNgayDat.Text = Convert.ToDateTime(reader["NgayDat"]).ToString("dd/MM/yyyy HH:mm");
                                lblTongTien.Text = Convert.ToDecimal(reader["SoTien"]).ToString("N0", CultureInfo.GetCultureInfo("vi-VN")) + " VNĐ";
                                lblPhuongThuc.Text = GetFriendlyPaymentMethodName(reader["PhuongThucThanhToan"].ToString());

                                // Hiển thị thông báo thành công
                                ShowMessage("Đơn hàng của bạn đã được đặt thành công!", isError: false);
                                pnlOrderDetails.Visible = true;
                            }
                            else
                            {
                                ShowMessage("Không tìm thấy thông tin đơn hàng.", isError: true);
                                pnlOrderDetails.Visible = false;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        ShowMessage("Lỗi khi tải thông tin đơn hàng: " + ex.Message, isError: true);
                        pnlOrderDetails.Visible = false;
                        LogError($"Load Order Confirmation Error (ID: {idDonHang}): {ex.ToString()}"); // Ghi log lỗi
                    }
                }
            }
        }

        /// <summary>
        /// Chuyển đổi mã phương thức thanh toán thành tên thân thiện.
        /// </summary>
        private string GetFriendlyPaymentMethodName(string paymentMethodCode)
        {
            switch (paymentMethodCode?.ToUpper()) // Thêm kiểm tra null và ToUpper cho chắc chắn
            {
                case "BANK":
                    return "Chuyển khoản ngân hàng";
                case "CARD":
                    return "Thẻ ngân hàng";
                case "WALLET":
                    return "Ví điện tử";
                case "COD": // Mặc dù đã xóa nhưng để lại phòng trường hợp dữ liệu cũ
                    return "Thanh toán khi nhận hàng";
                default:
                    return paymentMethodCode; // Trả về mã gốc nếu không khớp
            }
        }

        /// <summary>
        /// Hiển thị thông báo với style phù hợp.
        /// </summary>
        private void ShowMessage(string message, bool isError)
        {
            lblMessage.Text = message;
            lblMessage.CssClass = "block mb-6 text-center text-lg font-medium p-4 rounded-md border " +
                                  (isError ? "bg-red-50 border-red-300 text-red-700"
                                           : "bg-green-50 border-green-300 text-green-700");
            lblMessage.Visible = true;
        }

        /// <summary>
        /// Ghi log lỗi (thay thế bằng cơ chế log thực tế).
        /// </summary>
        private void LogError(string message)
        {
            System.Diagnostics.Trace.TraceError(message);
        }
    }
}