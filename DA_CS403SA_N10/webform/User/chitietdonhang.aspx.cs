// Generated on: 06/04/2025 (YYYY-MM-DD Format for reference)
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.HtmlControls; // Keep this if needed elsewhere

// --- ViewModels ---
[Serializable]
public class OrderItemDetailViewModel
{
    public int IDSach { get; set; }
    public string TenSach { get; set; }
    public string DuongDanBiaSach { get; set; }
    public int SoLuong { get; set; }
    public decimal Gia { get; set; } // Giá tại thời điểm mua
    public decimal ThanhTien => SoLuong * Gia;
}
// --- End ViewModels ---

namespace Webebook.WebForm.User
{
    public partial class chitietdonhang : System.Web.UI.Page
    {
        string connectionString = ConfigurationManager.ConnectionStrings["datawebebookConnectionString"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                LoadOrderDetails();
            }
            // Clear message on postback if any actions were added later
            if (IsPostBack)
            {
                lblMessage.Visible = false;
            }
        }

        /// <summary>
        /// Tải và hiển thị chi tiết đơn hàng.
        /// </summary>
        private void LoadOrderDetails()
        {
            string idDonHangStr = Request.QueryString["IDDonHang"];
            int idDonHang;

            // --- Xác thực ---
            if (Session["UserID"] == null)
            {
                // Redirect to login, preserving the return URL
                Response.Redirect("~/WebForm/VangLai/dangnhap.aspx?returnUrl=" + Server.UrlEncode(Request.Url.PathAndQuery), true); // Added true to end response
                return;
            }
            int userId = Convert.ToInt32(Session["UserID"]);

            if (string.IsNullOrEmpty(idDonHangStr) || !int.TryParse(idDonHangStr, out idDonHang))
            {
                ShowMessage("ID đơn hàng không hợp lệ hoặc bị thiếu.", true);
                HideDetails();
                return;
            }

            // --- Truy vấn dữ liệu ---
            List<OrderItemDetailViewModel> orderItems = new List<OrderItemDetailViewModel>();
            bool orderFoundAndOwned = false;
            decimal totalAmountFromDb = 0; // To store the total amount from DonHang table

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                try
                {
                    con.Open();

                    // 1. Lấy thông tin đơn hàng và kiểm tra quyền sở hữu
                    string orderQuery = @"SELECT NgayDat, SoTien, PhuongThucThanhToan, TrangThaiThanhToan
                                          FROM DonHang
                                          WHERE IDDonHang = @IDDonHang AND IDNguoiDung = @UserId";
                    using (SqlCommand cmd = new SqlCommand(orderQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@IDDonHang", idDonHang);
                        cmd.Parameters.AddWithValue("@UserId", userId);
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                orderFoundAndOwned = true;
                                lblIDDonHang.Text = idDonHang.ToString();
                                lblNgayDat.Text = Convert.ToDateTime(reader["NgayDat"]).ToString("dd/MM/yyyy HH:mm");

                                // Store total amount from DB and format it
                                totalAmountFromDb = Convert.ToDecimal(reader["SoTien"]);
                                lblTongTienValue.Text = FormatCurrency(totalAmountFromDb);

                                lblPhuongThuc.Text = GetFriendlyPaymentMethodName(reader["PhuongThucThanhToan"]?.ToString());
                                string trangThai = reader["TrangThaiThanhToan"]?.ToString() ?? string.Empty;
                                // Use Literal to render HTML for status badge
                                ltrTrangThai.Text = $"<span class='{GetStatusCssClass(trangThai)}'>{GetVietnameseStatusText(trangThai)}</span>";
                            }
                        } // End reader using
                    } // End command using

                    if (!orderFoundAndOwned)
                    {
                        ShowMessage("Không tìm thấy đơn hàng này hoặc bạn không có quyền xem chi tiết.", true);
                        HideDetails();
                        return;
                    }

                    // 2. Lấy chi tiết các mục trong đơn hàng
                    // Include Sach table join for book details
                    string detailQuery = @"SELECT ctdh.IDSach, s.TenSach, s.DuongDanBiaSach, ctdh.SoLuong, ctdh.Gia
                                           FROM ChiTietDonHang ctdh
                                           INNER JOIN Sach s ON ctdh.IDSach = s.IDSach
                                           WHERE ctdh.IDDonHang = @IDDonHang";
                    using (SqlCommand detailCmd = new SqlCommand(detailQuery, con))
                    {
                        detailCmd.Parameters.AddWithValue("@IDDonHang", idDonHang);
                        using (SqlDataReader itemReader = detailCmd.ExecuteReader())
                        {
                            while (itemReader.Read())
                            {
                                orderItems.Add(new OrderItemDetailViewModel
                                {
                                    IDSach = Convert.ToInt32(itemReader["IDSach"]),
                                    TenSach = itemReader["TenSach"]?.ToString() ?? "N/A",
                                    DuongDanBiaSach = itemReader["DuongDanBiaSach"]?.ToString(), // Handle potential null
                                    SoLuong = Convert.ToInt32(itemReader["SoLuong"]),
                                    Gia = Convert.ToDecimal(itemReader["Gia"]) // Price at the time of order
                                });
                            }
                        } // End itemReader using
                    } // End detailCmd using

                    // --- Binding Data and Setting Visibility ---
                    bool hasItems = orderItems.Any();
                    rptOrderItems.DataSource = orderItems;
                    rptOrderItems.DataBind();

                    // Show Repeater table structure *only* if there are items
                    // The table tags are inside Header/Footer, so Repeater handles this implicitly if DataSource is empty.
                    // We need to control the visibility of the surrounding panel parts based on items.

                    // Show/hide the "no items" message
                    pnlNoOrderItemsMessage.Visible = !hasItems;

                    // Ensure the panels themselves are visible if the order was found
                    pnlOrderInfo.Visible = true;
                    pnlOrderItems.Visible = true; // Keep the panel visible, the content inside adjusts

                }
                catch (SqlException dbEx) // Catch specific SQL errors
                {
                    ShowMessage("Lỗi cơ sở dữ liệu khi tải chi tiết đơn hàng. Vui lòng thử lại sau.", true);
                    HideDetails();
                    LogError($"SQL Error Loading Order Details (ID: {idDonHang}, User: {userId}): {dbEx.Message}\n{dbEx.StackTrace}");
                }
                catch (Exception ex) // Catch general errors
                {
                    ShowMessage("Đã xảy ra lỗi không mong muốn khi tải chi tiết đơn hàng.", true);
                    HideDetails();
                    LogError($"General Error Loading Order Details (ID: {idDonHang}, User: {userId}): {ex.Message}\n{ex.ToString()}"); // Log full details
                }
            } // End connection using
        }

        /// <summary>
        /// Ẩn các panel chi tiết khi có lỗi hoặc không tìm thấy đơn hàng.
        /// </summary>
        private void HideDetails()
        {
            pnlOrderInfo.Visible = false;
            pnlOrderItems.Visible = false;
            // Optionally clear labels if needed
            // lblIDDonHang.Text = "";
            // ... etc.
        }

        // --- CÁC HÀM HỖ TRỢ ---

        /// <summary>
        /// Trả về tên trạng thái tiếng Việt.
        /// </summary>
        protected string GetVietnameseStatusText(string status)
        {
            // Added some more potential statuses and improved handling
            switch (status?.ToLowerInvariant()) // Use ToLowerInvariant for consistency
            {
                case "completed":
                case "paid":
                case "delivered": // Example: If you add delivery status
                    return "Hoàn thành";
                case "pending":
                case "unpaid":
                    return "Chờ thanh toán";
                case "processing":
                    return "Đang xử lý";
                case "confirmed": // Example
                    return "Đã xác nhận";
                case "shipping":
                case "shipped":
                    return "Đang giao hàng";
                case "cancelled":
                case "canceled": // Common alternative spelling
                    return "Đã hủy";
                case "failed":
                    return "Thất bại";
                case "refunded":
                    return "Đã hoàn tiền";
                default:
                    return string.IsNullOrWhiteSpace(status) ? "Không xác định" : status; // Handle empty/whitespace
            }
        }

        /// <summary>
        /// Trả về lớp CSS Tailwind cho trạng thái đơn hàng (phù hợp với badge style).
        /// </summary>
        protected string GetStatusCssClass(string status)
        {
            // Refined base class and color combinations for modern Tailwind
            string baseClass = "px-2.5 py-0.5 inline-flex text-xs leading-5 font-semibold rounded-full";
            switch (status?.ToLowerInvariant())
            {
                case "completed":
                case "paid":
                case "delivered":
                    return $"{baseClass} bg-green-100 text-green-800 border border-green-200"; // Softer border
                case "pending":
                case "unpaid":
                    return $"{baseClass} bg-yellow-100 text-yellow-800 border border-yellow-200";
                case "processing":
                case "confirmed":
                    return $"{baseClass} bg-blue-100 text-blue-800 border border-blue-200";
                case "shipping":
                case "shipped":
                    return $"{baseClass} bg-indigo-100 text-indigo-800 border border-indigo-200";
                case "cancelled":
                case "canceled":
                case "failed":
                    return $"{baseClass} bg-red-100 text-red-800 border border-red-200";
                case "refunded":
                    return $"{baseClass} bg-gray-100 text-gray-800 border border-gray-300"; // Neutral for refunded
                default:
                    return $"{baseClass} bg-gray-100 text-gray-600 border border-gray-200"; // Default/Unknown
            }
        }

        /// <summary>
        /// Trả về tên phương thức thanh toán thân thiện hơn.
        /// </summary>
        protected string GetFriendlyPaymentMethodName(string paymentMethodCode)
        {
            // Keep this logic as is, seems reasonable
            switch (paymentMethodCode?.ToUpperInvariant()) // Use ToUpperInvariant
            {
                case "BANK": return "Chuyển khoản ngân hàng";
                case "CARD": return "Thẻ ATM/Visa/Mastercard"; // More specific
                case "WALLET": return "Ví điện tử (MoMo/ZaloPay/...)"; // Example specifics
                case "COD": return "Thanh toán khi nhận hàng (COD)";
                default:
                    return string.IsNullOrWhiteSpace(paymentMethodCode) ? "Chưa xác định" : paymentMethodCode;
            }
        }

        /// <summary>
        /// Hiển thị thông báo cho người dùng.
        /// </summary>
        private void ShowMessage(string message, bool isError)
        {
            // Using more descriptive CSS classes based on Tailwind palette
            lblMessage.Text = Server.HtmlEncode(message); // Encode to prevent XSS
            lblMessage.CssClass = isError
                ? "block mb-6 p-4 rounded-md border bg-red-50 border-red-300 text-red-700 text-sm"
                : "block mb-6 p-4 rounded-md border bg-blue-50 border-blue-300 text-blue-700 text-sm"; // Or green for success
            lblMessage.Visible = true;
        }

        /// <summary>
        /// Ghi log lỗi (nên sử dụng một thư viện logging chuyên dụng trong ứng dụng thực tế).
        /// </summary>
        private void LogError(string message)
        {
            // Simple trace logging, replace with a proper logging framework (NLog, Serilog) if possible
            System.Diagnostics.Trace.TraceError(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " | " + message);
            // TODO: Implement proper logging (e.g., write to file, database, logging service)
        }

        /// <summary>
        /// Định dạng tiền tệ Việt Nam.
        /// </summary>
        protected string FormatCurrency(object price)
        {
            if (price == null || price == DBNull.Value) return "0 VNĐ";
            try
            {
                // Use CultureInfo for reliable formatting
                return Convert.ToDecimal(price).ToString("N0", CultureInfo.GetCultureInfo("vi-VN")) + " VNĐ";
            }
            catch (FormatException) // Catch specific error
            {
                LogError($"Currency Format Error for value: {price}");
                return "Lỗi giá";
            }
            catch (InvalidCastException) // Catch specific error
            {
                LogError($"Currency Cast Error for value: {price}");
                return "Lỗi giá";
            }
        }

        /// <summary>
        /// Lấy URL hình ảnh sách, sử dụng placeholder nếu không có.
        /// </summary>
        protected string GetBookImageUrl(object imageUrl)
        {
            string url = imageUrl?.ToString();
            // Use IsNullOrWhiteSpace for robust check
            if (string.IsNullOrWhiteSpace(url))
            {
                // Use ResolveUrl for relative path from application root
                return ResolveUrl("~/Images/placeholder_cover.png"); // Ensure this placeholder exists
            }
            // Resolve URL if it's a relative path starting with ~/
            if (url.StartsWith("~/"))
            {
                return ResolveUrl(url);
            }
            // Assume it's an absolute URL or handled correctly otherwise
            return url;
        }

        /// <summary>
        /// Tính thành tiền cho từng dòng trong Repeater.
        /// </summary>
        protected string CalculateLineTotal(object quantity, object unitPrice)
        {
            // Check for null or DBNull before conversion
            if (quantity == null || quantity == DBNull.Value || unitPrice == null || unitPrice == DBNull.Value)
            {
                return FormatCurrency(0);
            }
            try
            {
                int qty = Convert.ToInt32(quantity);
                decimal price = Convert.ToDecimal(unitPrice);
                // Ensure non-negative results
                if (qty < 0 || price < 0) return FormatCurrency(0);
                return FormatCurrency(qty * price);
            }
            catch (Exception ex) when (ex is FormatException || ex is OverflowException || ex is InvalidCastException)
            {
                LogError($"Line Total Calculation Error (Qty: {quantity}, Price: {unitPrice}): {ex.Message}");
                return "Lỗi tính";
            }
        }

        /// <summary>
        /// Hàm hỗ trợ rút gọn chuỗi và thêm dấu "..." nếu quá dài.
        /// </summary>
        protected string TruncateString(object inputObject, int maxLength)
        {
            if (inputObject == null || inputObject == DBNull.Value) return string.Empty;

            string input = inputObject.ToString();
            if (maxLength <= 0) return input; // Return full string if max length is not positive

            if (string.IsNullOrEmpty(input) || input.Length <= maxLength)
            {
                return input;
            }
            // Trim potentially partial words and add ellipsis
            return input.Substring(0, maxLength).TrimEnd() + "...";
        }

    } // End class
} // End namespace0