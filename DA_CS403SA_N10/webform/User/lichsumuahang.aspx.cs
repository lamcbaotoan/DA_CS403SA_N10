// File: lichsumuahang.aspx.cs (Hoàn thiện)
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.HtmlControls; // Cần cho HtmlGenericControl (span)
using System.Web.UI.WebControls;
using System.Diagnostics; // Cho logging

// --- ViewModels --- (Giữ nguyên)
[Serializable]
public class OrderItemViewModel
{
    public int IDSach { get; set; }
    public string TenSach { get; set; }
    public string DuongDanBiaSach { get; set; }
    public int IDDonHang { get; set; }
}

[Serializable]
public class OrderHistoryViewModel
{
    public int IDDonHang { get; set; }
    public DateTime NgayDat { get; set; }
    public decimal SoTien { get; set; }
    public string TrangThaiThanhToan { get; set; }
    public List<OrderItemViewModel> Items { get; set; }

    public OrderHistoryViewModel()
    {
        Items = new List<OrderItemViewModel>();
    }
}
// --- End ViewModels ---

namespace Webebook.WebForm.User
{
    public partial class lichsumuahang : System.Web.UI.Page
    {
        string connectionString = ConfigurationManager.ConnectionStrings["datawebebookConnectionString"].ConnectionString;
        int userId = 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Check User Authentication
            if (Session["UserID"] == null || !int.TryParse(Session["UserID"].ToString(), out userId) || userId <= 0)
            {
                string returnUrl = Server.UrlEncode(Request.Url.PathAndQuery);
                Response.Redirect(ResolveUrl("~/WebForm/VangLai/dangnhap.aspx") + "?returnUrl=" + returnUrl + "&message=loginrequired", true);
                return;
            }

            if (!IsPostBack)
            {
                LoadOrderHistoryWithDetails();
                UpdateMasterCartCount(); // Cập nhật giỏ hàng trên master page nếu cần
            }
            // Không ẩn message trên postback để giữ lại thông báo lỗi nếu có từ các hàm khác
            // if (IsPostBack && !ScriptManager.GetCurrent(Page).IsInAsyncPostBack) { lblMessage.Visible = false; }
        }

        // Tải lịch sử đơn hàng và chi tiết
        private void LoadOrderHistoryWithDetails()
        {
            List<OrderHistoryViewModel> orderHistory = new List<OrderHistoryViewModel>();
            Dictionary<int, List<OrderItemViewModel>> orderItemsDict = new Dictionary<int, List<OrderItemViewModel>>();

            try
            {
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    con.Open();

                    // 1. Lấy tất cả đơn hàng của user
                    string orderQuery = @"SELECT IDDonHang, NgayDat, SoTien, TrangThaiThanhToan FROM DonHang WHERE IDNguoiDung = @UserId ORDER BY NgayDat DESC";
                    using (SqlCommand cmdOrder = new SqlCommand(orderQuery, con))
                    {
                        cmdOrder.Parameters.AddWithValue("@UserId", this.userId);
                        using (SqlDataReader orderReader = cmdOrder.ExecuteReader())
                        {
                            while (orderReader.Read())
                            {
                                orderHistory.Add(new OrderHistoryViewModel
                                {
                                    IDDonHang = Convert.ToInt32(orderReader["IDDonHang"]),
                                    NgayDat = Convert.ToDateTime(orderReader["NgayDat"]),
                                    SoTien = Convert.ToDecimal(orderReader["SoTien"]),
                                    TrangThaiThanhToan = orderReader["TrangThaiThanhToan"]?.ToString() ?? string.Empty
                                });
                            }
                        } // orderReader dispose
                    } // cmdOrder dispose

                    // 2. Lấy tất cả chi tiết đơn hàng cho các đơn hàng đã tải (nếu có)
                    if (orderHistory.Any())
                    {
                        List<int> orderIds = orderHistory.Select(o => o.IDDonHang).ToList();
                        StringBuilder detailSqlBuilder = new StringBuilder(@"SELECT ct.IDDonHang, ct.IDSach, s.TenSach, s.DuongDanBiaSach FROM ChiTietDonHang ct JOIN Sach s ON ct.IDSach = s.IDSach WHERE ct.IDDonHang IN (");
                        SqlCommand cmdDetail = new SqlCommand { Connection = con }; // Tạo command mới, dùng lại connection

                        for (int i = 0; i < orderIds.Count; i++)
                        {
                            string paramName = "@IDDonHangParam" + i;
                            detailSqlBuilder.Append(paramName);
                            if (i < orderIds.Count - 1) detailSqlBuilder.Append(",");
                            cmdDetail.Parameters.AddWithValue(paramName, orderIds[i]);
                        }
                        detailSqlBuilder.Append(")");
                        cmdDetail.CommandText = detailSqlBuilder.ToString();

                        using (SqlDataReader detailReader = cmdDetail.ExecuteReader())
                        {
                            while (detailReader.Read())
                            {
                                int currentOrderId = Convert.ToInt32(detailReader["IDDonHang"]);
                                var item = new OrderItemViewModel
                                {
                                    IDDonHang = currentOrderId,
                                    IDSach = Convert.ToInt32(detailReader["IDSach"]),
                                    TenSach = detailReader["TenSach"]?.ToString() ?? "N/A",
                                    DuongDanBiaSach = detailReader["DuongDanBiaSach"]?.ToString()
                                };
                                if (!orderItemsDict.ContainsKey(currentOrderId))
                                {
                                    orderItemsDict[currentOrderId] = new List<OrderItemViewModel>();
                                }
                                orderItemsDict[currentOrderId].Add(item);
                            }
                        } // detailReader dispose
                    } // End if (orderHistory.Any())
                } // Connection dispose

                // 3. Gán chi tiết vào từng đơn hàng
                foreach (var order in orderHistory)
                {
                    if (orderItemsDict.ContainsKey(order.IDDonHang))
                    {
                        order.Items = orderItemsDict[order.IDDonHang];
                    }
                    // Nếu không có key thì order.Items sẽ là list rỗng (đã khởi tạo)
                }

                // 4. Bind dữ liệu và cập nhật UI
                rptOrders.DataSource = orderHistory;
                rptOrders.DataBind();
                pnlNoOrders.Visible = !orderHistory.Any(); // Hiển thị panel nếu không có đơn hàng

            }
            catch (Exception ex)
            {
                ShowMessage("Lỗi tải lịch sử đơn hàng: " + ex.Message, true);
                LogError($"LoadOrderHistoryWithDetails Error for User {this.userId}: {ex.ToString()}");
                rptOrders.DataSource = null; rptOrders.DataBind();
                pnlNoOrders.Visible = true; // Hiển thị panel không có đơn hàng khi lỗi
            }
        }

        // Xử lý ItemDataBound cho Repeater đơn hàng
        protected void rptOrders_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                var order = e.Item.DataItem as OrderHistoryViewModel;
                if (order != null)
                {
                    // Bind Nested Repeater
                    Repeater rptOrderItems = e.Item.FindControl("rptOrderItems") as Repeater;
                    if (rptOrderItems != null) { rptOrderItems.DataSource = order.Items; rptOrderItems.DataBind(); }

                    // Set Order Status Badge
                    Literal ltrStatus = e.Item.FindControl("ltrStatus") as Literal;
                    if (ltrStatus != null)
                    {
                        string statusText = GetVietnameseStatusText(order.TrangThaiThanhToan);
                        string statusColorClass = GetStatusCssClass(order.TrangThaiThanhToan); // Chỉ lấy class màu
                        // Kết hợp class base và class màu
                        ltrStatus.Text = $"<span class='status-badge-base {statusColorClass}'>{statusText}</span>";
                    }
                }
            }
        }

        // Xử lý ItemDataBound cho Repeater chi tiết đơn hàng
        protected void rptOrderItems_ItemDataBound(object sender, RepeaterItemEventArgs e)
        {
            if (e.Item.ItemType == ListItemType.Item || e.Item.ItemType == ListItemType.AlternatingItem)
            {
                HyperLink hlReview = (HyperLink)e.Item.FindControl("hlReview");
                HtmlGenericControl spnCannotReview = (HtmlGenericControl)e.Item.FindControl("spnCannotReview");
                HyperLink hlBookImageLink = (HyperLink)e.Item.FindControl("hlBookImageLink");
                Image imgBookCover = (Image)e.Item.FindControl("imgBookCover");
                HyperLink hlBookTitleLink = (HyperLink)e.Item.FindControl("hlBookTitleLink");

                var currentItem = e.Item.DataItem as OrderItemViewModel;
                if (currentItem == null) return;

                // Lấy thông tin đơn hàng cha
                RepeaterItem parentRepeaterItem = (RepeaterItem)e.Item.NamingContainer.NamingContainer;
                var parentOrder = parentRepeaterItem?.DataItem as OrderHistoryViewModel;

                if (parentOrder != null && hlReview != null && spnCannotReview != null && hlBookImageLink != null && imgBookCover != null && hlBookTitleLink != null)
                {
                    imgBookCover.ImageUrl = GetBookImageUrl(currentItem.DuongDanBiaSach); // Dùng hàm helper GetBookImageUrl
                    hlBookTitleLink.Text = Server.HtmlEncode(currentItem.TenSach); // Encode tên sách
                    hlBookTitleLink.ToolTip = currentItem.TenSach; // Tooltip hiển thị tên đầy đủ

                    // Kiểm tra trạng thái đơn hàng để hiển thị nút Đánh giá
                    bool isCompletedOrPaid = parentOrder.TrangThaiThanhToan.Equals("Completed", StringComparison.OrdinalIgnoreCase) ||
                                             parentOrder.TrangThaiThanhToan.Equals("Paid", StringComparison.OrdinalIgnoreCase);

                    hlReview.Visible = isCompletedOrPaid;
                    spnCannotReview.Visible = !isCompletedOrPaid;
                    if (isCompletedOrPaid)
                    {
                        hlReview.NavigateUrl = ResolveUrl($"~/WebForm/User/danhgia.aspx?IDSach={currentItem.IDSach}&orderId={currentItem.IDDonHang}");
                    }

                    // Đặt link cho ảnh và tiêu đề sách
                    // Nếu đã hoàn thành -> link tới tủ sách, ngược lại -> link tới chi tiết sách
                    string bookLinkUrl = isCompletedOrPaid
                                        ? ResolveUrl("~/WebForm/User/tusach.aspx") // Hoặc link cụ thể hơn nếu có
                                        : ResolveUrl($"~/WebForm/User/chitietsach_user.aspx?IDSach={currentItem.IDSach}");
                    hlBookImageLink.NavigateUrl = bookLinkUrl;
                    hlBookTitleLink.NavigateUrl = bookLinkUrl;
                }
                else
                {
                    // Xử lý lỗi nếu không tìm thấy control hoặc dữ liệu cha
                    if (hlReview != null) hlReview.Visible = false; if (spnCannotReview != null) spnCannotReview.Visible = false;
                    LogError($"Error finding controls or parent data in rptOrderItems_ItemDataBound for OrderID: {currentItem?.IDDonHang}, BookID: {currentItem?.IDSach}");
                }
            }
        }

        // --- Helper Functions ---

        // Trả về text tiếng Việt
        protected string GetVietnameseStatusText(string status)
        {
            switch (status?.ToLowerInvariant()) { case "completed": case "paid": return "Hoàn thành"; case "pending": return "Chờ xử lý"; case "processing": return "Đang xử lý"; case "shipping": return "Đang giao"; case "cancelled": return "Đã hủy"; case "failed": return "Thất bại"; default: return string.IsNullOrEmpty(status) ? "Không xác định" : status; }
        }

        // Chỉ trả về class màu sắc Tailwind
        protected string GetStatusCssClass(string status)
        {
            switch (status?.ToLowerInvariant()) { case "completed": case "paid": return "bg-green-100 text-green-800 border-green-200"; case "pending": return "bg-yellow-100 text-yellow-800 border-yellow-200"; case "processing": return "bg-sky-100 text-sky-800 border-sky-200"; case "shipping": return "bg-blue-100 text-blue-800 border-blue-200"; case "cancelled": case "failed": return "bg-red-100 text-red-800 border-red-200"; default: return "bg-gray-100 text-gray-800 border-gray-200"; }
        }

        private void ShowMessage(string message, bool isError)
        {
            if (lblMessage == null) return; lblMessage.Text = HttpUtility.HtmlEncode(message); string cssClass = "block w-full p-4 mb-6 text-sm rounded-lg border "; if (isError) { cssClass += "bg-red-50 border-red-300 text-red-800"; } else { cssClass += "bg-green-50 border-green-300 text-green-800"; }
            lblMessage.CssClass = cssClass; lblMessage.Visible = true;
        }

        // Cập nhật giỏ hàng trên MasterPage
        private void UpdateMasterCartCount()
        {
            if (Master is UserMaster master) { master.UpdateCartCount(); } else { LogError("Could not find Master Page of type UserMaster to update cart count."); }
        }

        // Format tiền tệ
        protected string FormatCurrency(object price) { if (price == null || price == DBNull.Value) { return "0 VNĐ"; } try { decimal amount = Convert.ToDecimal(price); CultureInfo vietnamCulture = CultureInfo.GetCultureInfo("vi-VN"); return amount.ToString("N0", vietnamCulture) + " VNĐ"; } catch (Exception ex) { LogError($"FormatCurrency Error: Could not convert '{price}'. {ex.Message}"); return "Lỗi giá"; } }

        // Lấy URL ảnh sách
        protected string GetBookImageUrl(object imageUrl) { string url = imageUrl?.ToString(); string placeholder = ResolveUrl("~/Images/placeholder_cover.png"); if (string.IsNullOrWhiteSpace(url)) { return placeholder; } if (url.StartsWith("~/")) { try { return ResolveUrl(url); } catch (Exception ex) { LogError($"Error resolving image URL '{url}': {ex.Message}"); return placeholder; } } if (url.StartsWith("/") || url.StartsWith("http://") || url.StartsWith("https://")) { return url; } try { return ResolveUrl("~/" + url.TrimStart('/')); } catch (Exception ex) { LogError($"Error guessing resolution for image URL '{url}': {ex.Message}"); return placeholder; } }

        // Ghi log lỗi
        private void LogError(string message) { Debug.WriteLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] ERROR (LichSuMuaHang): {message}"); }

        // Hàm GetData (Cần thiết cho LoadOrderHistoryWithDetails)
        private DataTable GetData(string query, params SqlParameter[] parameters) { DataTable dt = new DataTable(); try { using (SqlConnection con = new SqlConnection(connectionString)) { using (SqlCommand cmd = new SqlCommand(query, con)) { if (parameters != null) { cmd.Parameters.AddRange(parameters); } con.Open(); SqlDataAdapter da = new SqlDataAdapter(cmd); da.Fill(dt); } } } catch (SqlException sqlEx) { LogError($"SQL ERROR in GetData: {sqlEx.Message} (Query: {query})"); ShowMessage("Lỗi truy vấn cơ sở dữ liệu.", true); return null; } catch (Exception ex) { LogError($"General ERROR in GetData: {ex}"); ShowMessage("Đã xảy ra lỗi không mong muốn.", true); return null; } return dt; }

    } // End class lichsumuahang
} // End namespace