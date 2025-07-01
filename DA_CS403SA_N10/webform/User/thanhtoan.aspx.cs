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
using System.Web.UI.WebControls;
using System.Web.Script.Serialization; // ** Đảm bảo có using này **

namespace Webebook.WebForm.User
{
    public partial class thanhtoan : System.Web.UI.Page
    {
        // ... (Chuỗi kết nối và các thuộc tính giữ nguyên) ...
        string connectionString = ConfigurationManager.ConnectionStrings["datawebebookConnectionString"].ConnectionString;

        public bool IsBuyNowMode
        {
            get { return ViewState["IsBuyNowMode"] != null && (bool)ViewState["IsBuyNowMode"]; }
            protected set { ViewState["IsBuyNowMode"] = value; }
        }

        protected List<CartItemViewModel> SelectedItems
        {
            get { return ViewState["SelectedItems"] as List<CartItemViewModel>; }
            set { ViewState["SelectedItems"] = value; }
        }

        public decimal GrandTotal
        {
            get { return (ViewState["GrandTotal"] != null) ? (decimal)ViewState["GrandTotal"] : 0; }
            protected set { ViewState["GrandTotal"] = value; }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null)
            {
                Response.Redirect("~/WebForm/VangLai/dangnhap.aspx?returnUrl=" + Server.UrlEncode(Request.RawUrl), true);
                return;
            }

            if (!IsPostBack)
            {
                int buyNowId = 0;
                bool isBuyNowRequest = int.TryParse(Request.QueryString["buyNowId"], out buyNowId) && buyNowId > 0;

                if (isBuyNowRequest)
                {
                    this.IsBuyNowMode = true;
                    Session.Remove("SelectedCartItems");
                    LoadBuyNowItem(buyNowId);
                }
                else
                {
                    this.IsBuyNowMode = false;
                    if (Session["SelectedCartItems"] == null || !(Session["SelectedCartItems"] as List<int>).Any())
                    {
                        ShowMessage("Không có sản phẩm nào được chọn để thanh toán. Vui lòng quay lại giỏ hàng.", MessageType.Warning);
                        DisableCheckout("Không có sản phẩm.");
                        return;
                    }
                    LoadSelectedItems();
                }

                if (this.SelectedItems == null || !this.SelectedItems.Any())
                {
                    if (!lblMessage.Visible)
                    {
                        ShowMessage("Không có sản phẩm nào để hiển thị.", MessageType.Warning);
                        DisableCheckout("Không có sản phẩm.");
                    }
                }
                else
                {
                    BindDataAndDisplayTotal();
                    UpdateMasterCartCount();

                    // *** THAY ĐỔI: Thiết lập trạng thái hiển thị ban đầu cho các panel ***
                    // JavaScript sẽ xử lý việc chuyển đổi khi người dùng chọn
                    pnlBankInfo.Visible = true;  // Mặc định hiển thị Bank
                    //pnlCardForm.Visible = false;
                    //pnlWalletInfo.Visible = false;
                    // *** KẾT THÚC THAY ĐỔI ***

                    pnlOrderSummary.Visible = true;
                    pnlPaymentMethods.Visible = true;
                    btnXacNhan.Enabled = true;
                    btnXacNhan.CssClass = "w-full bg-indigo-600 hover:bg-indigo-700 text-white font-bold py-3 px-6 rounded-lg shadow-md hover:shadow-lg focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 transition duration-200 ease-in-out disabled:opacity-50 disabled:cursor-not-allowed";
                }
            }
            // Logic trong else của IsPostBack (nếu có) vẫn giữ nguyên
        }

        // ... (Các phương thức LoadBuyNowItem, LoadSelectedItems, BindDataAndDisplayTotal giữ nguyên) ...
        private void LoadBuyNowItem(int sachId)
        {
            this.SelectedItems = new List<CartItemViewModel>();
            decimal currentGrandTotal = 0;
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "SELECT TenSach, GiaSach FROM Sach WHERE IDSach = @IDSach";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@IDSach", sachId);
                    try
                    {
                        con.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                decimal donGia = reader["GiaSach"] != DBNull.Value ? Convert.ToDecimal(reader["GiaSach"]) : 0;
                                this.SelectedItems.Add(new CartItemViewModel
                                {
                                    IDGioHang = 0, // Không có ID giỏ hàng cho Mua Ngay
                                    IDSach = sachId,
                                    TenSach = reader["TenSach"].ToString(),
                                    SoLuong = 1, // Mua ngay mặc định 1
                                    DonGia = donGia
                                });
                                currentGrandTotal = this.SelectedItems[0].ThanhTien;
                            }
                            else
                            {
                                ShowMessage($"Không tìm thấy thông tin sách với ID={sachId}.", MessageType.Error);
                                DisableCheckout("Sách không tồn tại.");
                            }
                        }
                        this.GrandTotal = currentGrandTotal;
                        ViewState["SelectedItems"] = this.SelectedItems; // Lưu vào ViewState
                        ViewState["GrandTotal"] = this.GrandTotal;      // Lưu vào ViewState
                    }
                    catch (Exception ex)
                    {
                        ShowMessage("Lỗi tải thông tin sách mua ngay: " + ex.Message, MessageType.Error);
                        LogError($"LoadBuyNowItem Error for SachID {sachId}: {ex}");
                        this.SelectedItems = new List<CartItemViewModel>(); // Reset
                        this.GrandTotal = 0;
                        ViewState["SelectedItems"] = this.SelectedItems; // Cập nhật ViewState
                        ViewState["GrandTotal"] = this.GrandTotal;      // Cập nhật ViewState
                        DisableCheckout("Lỗi tải sách.");
                    }
                }
            }
        }

        private void LoadSelectedItems()
        {
            List<int> selectedCartItemIds = Session["SelectedCartItems"] as List<int>;
            if (selectedCartItemIds == null || !selectedCartItemIds.Any())
            {
                ShowMessage("Không có ID sản phẩm nào được chọn từ giỏ hàng.", MessageType.Warning);
                DisableCheckout("Lỗi dữ liệu giỏ hàng.");
                return;
            }

            this.SelectedItems = new List<CartItemViewModel>();
            decimal currentGrandTotal = 0;
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                StringBuilder sqlBuilder = new StringBuilder("SELECT gh.IDGioHang, gh.IDSach, s.TenSach, gh.SoLuong, s.GiaSach FROM GioHang gh JOIN Sach s ON gh.IDSach = s.IDSach WHERE gh.IDNguoiDung = @UserId AND gh.IDGioHang IN (");
                SqlCommand cmd = new SqlCommand { Connection = con };
                cmd.Parameters.AddWithValue("@UserId", Convert.ToInt32(Session["UserID"]));

                List<string> paramNames = new List<string>();
                for (int i = 0; i < selectedCartItemIds.Count; i++)
                {
                    string pName = "@IDGH" + i;
                    paramNames.Add(pName);
                    cmd.Parameters.AddWithValue(pName, selectedCartItemIds[i]);
                }
                sqlBuilder.Append(string.Join(",", paramNames));
                sqlBuilder.Append(")");
                cmd.CommandText = sqlBuilder.ToString();

                try
                {
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var item = new CartItemViewModel
                            {
                                IDGioHang = Convert.ToInt32(reader["IDGioHang"]),
                                IDSach = Convert.ToInt32(reader["IDSach"]),
                                TenSach = reader["TenSach"].ToString(),
                                SoLuong = reader["SoLuong"] != DBNull.Value ? Convert.ToInt32(reader["SoLuong"]) : 1,
                                DonGia = reader["GiaSach"] != DBNull.Value ? Convert.ToDecimal(reader["GiaSach"]) : 0
                            };
                            this.SelectedItems.Add(item);
                            currentGrandTotal += item.ThanhTien;
                        }
                    }
                    if (!this.SelectedItems.Any() && selectedCartItemIds.Any())
                    {
                        ShowMessage("Không thể tải thông tin các sản phẩm đã chọn. Có thể sản phẩm đã bị xóa khỏi giỏ hàng.", MessageType.Warning);
                        DisableCheckout("Lỗi tải sản phẩm.");
                    }
                    this.GrandTotal = currentGrandTotal;
                    ViewState["SelectedItems"] = this.SelectedItems;
                    ViewState["GrandTotal"] = this.GrandTotal;
                }
                catch (Exception ex)
                {
                    ShowMessage("Lỗi tải chi tiết giỏ hàng: " + ex.Message, MessageType.Error);
                    LogError($"LoadSelectedItems Error: {ex}");
                    this.SelectedItems = new List<CartItemViewModel>();
                    this.GrandTotal = 0;
                    ViewState["SelectedItems"] = this.SelectedItems;
                    ViewState["GrandTotal"] = this.GrandTotal;
                    DisableCheckout("Lỗi tải giỏ hàng.");
                }
            }
        }

        private void BindDataAndDisplayTotal()
        {
            rptSelectedItems.DataSource = this.SelectedItems;
            rptSelectedItems.DataBind();
        }

        // *** XÓA PHƯƠNG THỨC NÀY ***
        // protected void rblPaymentMethod_SelectedIndexChanged(object sender, EventArgs e)
        // {
        //     TogglePaymentDetailsVisibility();
        // }

        // *** XÓA PHƯƠNG THỨC NÀY ***
        // private void TogglePaymentDetailsVisibility() { ... }


        // ... (Các phương thức CheckAlreadyOwnedBooks, btnXacNhan_Click, và các hàm hỗ trợ khác giữ nguyên) ...
        private List<string> CheckAlreadyOwnedBooks(int userId, List<int> bookIdsToCheck)
        {
            List<string> ownedBookNames = new List<string>();
            if (bookIdsToCheck == null || !bookIdsToCheck.Any())
            {
                System.Diagnostics.Debug.WriteLine("CheckAlreadyOwnedBooks: No book IDs to check.");
                return ownedBookNames;
            }

            System.Diagnostics.Debug.WriteLine($"CheckAlreadyOwnedBooks: Checking UserID {userId} for BookIDs: {string.Join(",", bookIdsToCheck)}");

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                StringBuilder sqlBuilder = new StringBuilder("SELECT s.TenSach FROM TuSach ts JOIN Sach s ON ts.IDSach = s.IDSach WHERE ts.IDNguoiDung = @UserId AND ts.IDSach IN (");
                SqlCommand cmd = new SqlCommand { Connection = con };
                cmd.Parameters.AddWithValue("@UserId", userId);

                List<string> paramNames = new List<string>();
                for (int i = 0; i < bookIdsToCheck.Count; i++)
                {
                    string pName = "@IDS" + i;
                    paramNames.Add(pName);
                    cmd.Parameters.AddWithValue(pName, bookIdsToCheck[i]);
                }
                sqlBuilder.Append(string.Join(",", paramNames));
                sqlBuilder.Append(")");
                cmd.CommandText = sqlBuilder.ToString();
                System.Diagnostics.Debug.WriteLine($"CheckAlreadyOwnedBooks SQL: {cmd.CommandText}");

                try
                {
                    con.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            string bookName = reader["TenSach"].ToString();
                            ownedBookNames.Add(bookName);
                            System.Diagnostics.Debug.WriteLine($"CheckAlreadyOwnedBooks: Found owned book - {bookName}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    LogError($"CheckAlreadyOwnedBooks Error for User {userId}: {ex}");
                    System.Diagnostics.Debug.WriteLine($"CheckAlreadyOwnedBooks: Error - {ex.Message}");
                    return new List<string>(); // Trả về rỗng nếu lỗi
                }
            }
            System.Diagnostics.Debug.WriteLine($"CheckAlreadyOwnedBooks: Finished check. Found {ownedBookNames.Count} owned books.");
            return ownedBookNames;
        }

        protected void btnXacNhan_Click(object sender, EventArgs e)
        {
            bool isBuyNow = this.IsBuyNowMode;
            List<CartItemViewModel> itemsToProcess = ViewState["SelectedItems"] as List<CartItemViewModel>;
            decimal finalTotal = (ViewState["GrandTotal"] != null) ? (decimal)ViewState["GrandTotal"] : 0;
            List<int> selectedCartItemIds = null;
            if (!isBuyNow) { selectedCartItemIds = Session["SelectedCartItems"] as List<int>; }

            // --- KIỂM TRA ĐẦU VÀO ---
            if (Session["UserID"] == null)
            {
                ShowMessage("Phiên đăng nhập hết hạn. Vui lòng đăng nhập lại.", MessageType.Error);
                ScriptManager.RegisterStartupScript(this, GetType(), "EnableBtn", $"document.getElementById('{btnXacNhan.ClientID}')?.removeAttribute('disabled');document.getElementById('loadingOverlay')?.classList.remove('visible');", true);
                return;
            }
            int userId = Convert.ToInt32(Session["UserID"]);

            if (itemsToProcess == null || !itemsToProcess.Any() || finalTotal < 0)
            {
                ShowMessage("Không có sản phẩm hợp lệ trong đơn hàng.", MessageType.Error);
                ScriptManager.RegisterStartupScript(this, GetType(), "EnableBtn", $"document.getElementById('{btnXacNhan.ClientID}')?.removeAttribute('disabled');document.getElementById('loadingOverlay')?.classList.remove('visible');", true);
                DisableCheckout("Dữ liệu đơn hàng không hợp lệ.");
                return;
            }
            if (!isBuyNow && (selectedCartItemIds == null || !selectedCartItemIds.Any()))
            {
                ShowMessage("Lỗi xác định các sản phẩm trong giỏ hàng được chọn.", MessageType.Error);
                ScriptManager.RegisterStartupScript(this, GetType(), "EnableBtn", $"document.getElementById('{btnXacNhan.ClientID}')?.removeAttribute('disabled');document.getElementById('loadingOverlay')?.classList.remove('visible');", true);
                DisableCheckout("Lỗi ID giỏ hàng.");
                return;
            }
            // --- KẾT THÚC KIỂM TRA ĐẦU VÀO ---

            // ***** KIỂM TRA SÁCH ĐÃ MUA *****
            List<int> bookIdsToCheck = itemsToProcess.Select(item => item.IDSach).ToList();
            List<string> alreadyOwnedBooks = CheckAlreadyOwnedBooks(userId, bookIdsToCheck);

            if (alreadyOwnedBooks.Any())
            {
                // Chuẩn bị URL để chuyển hướng
                string tusachUrl = ResolveUrl("~/WebForm/User/tusach.aspx");
                string giohangUrl = ResolveUrl("~/WebForm/User/giohang_user.aspx");

                // Tạo danh sách sách bằng HTML để hiển thị đẹp hơn
                var htmlContent = new StringBuilder();
                htmlContent.Append("<p class='text-left mb-2'>Bạn không thể mua các sách đã có trong tủ sách:</p>");
                htmlContent.Append("<ul class='text-left list-disc list-inside bg-yellow-50 p-3 rounded-md border border-yellow-200'>");
                foreach (var bookName in alreadyOwnedBooks)
                {
                    // Mã hóa tên sách để tránh lỗi XSS
                    htmlContent.Append($"<li class='font-semibold'>{HttpUtility.HtmlEncode(bookName)}</li>");
                }
                htmlContent.Append("</ul>");

                // Sử dụng JavaScriptSerializer để chuyển chuỗi HTML thành chuỗi JavaScript an toàn
                var serializer = new JavaScriptSerializer();
                string serializedHtmlContent = serializer.Serialize(htmlContent.ToString());

                // Tạo kịch bản JavaScript để gọi SweetAlert2
                string script = $@"
                // Ẩn overlay loading trước khi hiện popup
                var overlay = document.getElementById('loadingOverlay');
                if (overlay) overlay.classList.remove('visible');

                // Gọi SweetAlert2
                Swal.fire({{
                    title: 'Sách đã có trong Tủ sách!',
                    html: {serializedHtmlContent},
                    icon: 'warning',
                    showCancelButton: true,
                    confirmButtonColor: '#3b82f6', // Màu xanh dương
                    cancelButtonColor: '#6b7280',  // Màu xám
                    confirmButtonText: '<i class=""fas fa-book-open""></i> Tới Tủ sách',
                    cancelButtonText: '<i class=""fas fa-shopping-cart""></i> Về Giỏ hàng',
                    reverseButtons: true // Đảo vị trí 2 nút cho hợp lý hơn (OK bên phải)
                }}).then((result) => {{
                    if (result.isConfirmed) {{
                        // Nếu người dùng chọn 'Tới Tủ sách'
                        window.location.href = '{tusachUrl}';
                    }} else {{
                        // Nếu người dùng chọn 'Về Giỏ hàng' hoặc đóng popup
                        window.location.href = '{giohangUrl}';
                    }}
                }});";

                // Đăng ký kịch bản để chạy trên client-side
                ScriptManager.RegisterStartupScript(this, GetType(), "AlreadyOwnedSweetAlert", script, true);
                return; // Dừng xử lý đơn hàng
            }
            // ***** KẾT THÚC KIỂM TRA SÁCH ĐÃ MUA *****

            // Lấy phương thức thanh toán từ control (vẫn cần thiết)
            string paymentMethod = rblPaymentMethod.SelectedValue;
            if (string.IsNullOrEmpty(paymentMethod))
            {
                ShowMessage("Vui lòng chọn một phương thức thanh toán.", MessageType.Warning);
                ScriptManager.RegisterStartupScript(this, GetType(), "EnableBtnPay", $"document.getElementById('{btnXacNhan.ClientID}')?.removeAttribute('disabled');document.getElementById('loadingOverlay')?.classList.remove('visible');", true);
                return;
            }

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                SqlTransaction transaction = null;
                try
                {
                    con.Open();
                    transaction = con.BeginTransaction();

                    // 1. Tạo Đơn Hàng
                    string donHangQuery = "INSERT INTO DonHang (IDNguoiDung, NgayDat, SoTien, TrangThaiThanhToan, PhuongThucThanhToan) OUTPUT INSERTED.IDDonHang VALUES (@IDNguoiDung, GETDATE(), @SoTien, @TrangThai, @PhuongThuc)";
                    int idDonHang;
                    string trangThaiThanhToan = "Pending"; // Luôn là Pending khi mới tạo
                    using (SqlCommand cmdDH = new SqlCommand(donHangQuery, con, transaction))
                    {
                        cmdDH.Parameters.AddWithValue("@IDNguoiDung", userId);
                        cmdDH.Parameters.AddWithValue("@SoTien", finalTotal);
                        cmdDH.Parameters.AddWithValue("@TrangThai", trangThaiThanhToan);
                        cmdDH.Parameters.AddWithValue("@PhuongThuc", paymentMethod);
                        idDonHang = (int)cmdDH.ExecuteScalar();
                    }
                    if (idDonHang <= 0)
                    {
                        throw new Exception("Không thể tạo đơn hàng trong cơ sở dữ liệu.");
                    }

                    // 2. Thêm Chi Tiết Đơn Hàng
                    string ctQuery = "INSERT INTO ChiTietDonHang (IDSach, IDDonHang, SoLuong, Gia) VALUES (@IDSach, @IDDonHang, @SoLuong, @Gia)";
                    foreach (var item in itemsToProcess)
                    {
                        using (SqlCommand cmdCT = new SqlCommand(ctQuery, con, transaction))
                        {
                            cmdCT.Parameters.AddWithValue("@IDSach", item.IDSach);
                            cmdCT.Parameters.AddWithValue("@IDDonHang", idDonHang);
                            cmdCT.Parameters.AddWithValue("@SoLuong", item.SoLuong);
                            cmdCT.Parameters.AddWithValue("@Gia", item.DonGia);
                            cmdCT.ExecuteNonQuery();
                        }
                    }

                    // 3. Xóa các mục đã chọn khỏi Giỏ Hàng (Nếu không phải Mua Ngay)
                    if (!isBuyNow && selectedCartItemIds != null && selectedCartItemIds.Any())
                    {
                        StringBuilder delSql = new StringBuilder("DELETE FROM GioHang WHERE IDGioHang IN (");
                        SqlCommand cmdDel = new SqlCommand { Connection = con, Transaction = transaction };
                        List<string> delParamNames = new List<string>();
                        for (int i = 0; i < selectedCartItemIds.Count; i++)
                        {
                            string pName = "@IDGHDel" + i;
                            delParamNames.Add(pName);
                            cmdDel.Parameters.AddWithValue(pName, selectedCartItemIds[i]);
                        }
                        delSql.Append(string.Join(",", delParamNames));
                        delSql.Append(")");
                        cmdDel.CommandText = delSql.ToString();
                        cmdDel.ExecuteNonQuery();

                        Session.Remove("SelectedCartItems");
                    }

                    transaction.Commit();
                    UpdateMasterCartCount();
                    Response.Redirect($"~/WebForm/User/xacnhandonhang.aspx?IDDonHang={idDonHang}", false);
                    Context.ApplicationInstance.CompleteRequest();
                }
                catch (Exception ex)
                {
                    if (transaction != null)
                    {
                        try { transaction.Rollback(); }
                        catch (Exception rbEx) { LogError($"Rollback Error: {rbEx}"); }
                    }
                    ShowMessage("Đã xảy ra lỗi trong quá trình xử lý đơn hàng: " + ex.Message, MessageType.Error);
                    LogError($"Checkout Error (UserID: {userId}): {ex}");
                    ScriptManager.RegisterStartupScript(this, GetType(), "EnableBtnErr", $"var btn = document.getElementById('{btnXacNhan.ClientID}'); if(btn) btn.removeAttribute('disabled'); var overlay = document.getElementById('loadingOverlay'); if(overlay) overlay.classList.remove('visible');", true);
                }
            }
        }
        private void DisableCheckout(string reason)
        {
            System.Diagnostics.Debug.WriteLine($"Checkout Disabled: {reason}");
            btnXacNhan.Enabled = false;
            btnXacNhan.ToolTip = reason;
            // Cập nhật class để hiển thị đúng trạng thái disabled
            btnXacNhan.CssClass = "w-full bg-indigo-600 hover:bg-indigo-700 text-white font-bold py-3 px-6 rounded-lg shadow-md hover:shadow-lg focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 transition duration-200 ease-in-out disabled:opacity-50 disabled:cursor-not-allowed";
            // Không ẩn panel
        }

        protected string FormatCurrency(object price)
        {
            if (price == null || price == DBNull.Value) return "0 VNĐ";
            try
            {
                return Convert.ToDecimal(price).ToString("N0", CultureInfo.GetCultureInfo("vi-VN")) + " VNĐ";
            }
            catch { return "0 VNĐ"; }
        }

        protected string TruncateString(object inputObject, int maxLength)
        {
            if (inputObject == null || inputObject == DBNull.Value) return string.Empty;
            string input = inputObject.ToString();
            if (string.IsNullOrEmpty(input)) return string.Empty;
            string truncated = input.Length <= maxLength ? input : input.Substring(0, maxLength).TrimEnd() + "...";
            return HttpUtility.HtmlEncode(truncated);
        }

        private void UpdateMasterCartCount()
        {
            if (Master is UserMaster master)
            {
                master.UpdateCartCount();
            }
            else { LogError("Không thể cập nhật số lượng giỏ hàng: Master Page không phải UserMaster."); }
        }

        protected enum MessageType { Success, Error, Warning, Info }

        private void ShowMessage(string message, MessageType type)
        {
            lblMessage.Text = HttpUtility.HtmlEncode(message);
            string cssClass = "block mb-6 text-sm p-4 rounded-lg border "; // Tăng mb
            switch (type)
            {
                case MessageType.Success: cssClass += "bg-green-100 border-green-300 text-green-800"; break; // Màu đậm hơn
                case MessageType.Error: cssClass += "bg-red-100 border-red-300 text-red-800"; break;
                case MessageType.Warning: cssClass += "bg-yellow-100 border-yellow-300 text-yellow-800"; break;
                case MessageType.Info: default: cssClass += "bg-blue-100 border-blue-300 text-blue-800"; break;
            }
            lblMessage.CssClass = cssClass;
            lblMessage.Visible = true;
        }

        private void LogError(string message)
        {
            System.Diagnostics.Trace.TraceError($"{DateTime.Now:yyyy-MM-dd HH:mm:ss} - {message}");
        }

        // Helper để kiểm tra xem có nên dùng colspan ít hơn cho mobile không
        protected bool IsMobile()
        {
            // Cách đơn giản, có thể cần thư viện hoặc cách phức tạp hơn để chính xác
            // string userAgent = Request.UserAgent;
            // return userAgent != null && (userAgent.Contains("Mobi") || userAgent.Contains("Android"));
            return false; // Tạm thời luôn trả về false, bạn có thể thêm logic phát hiện mobile nếu cần
        }
    }

    [Serializable]
    public class CartItemViewModel
    {
        public int IDGioHang { get; set; }
        public int IDSach { get; set; }
        public string TenSach { get; set; }
        public int SoLuong { get; set; }
        public decimal DonGia { get; set; }
        public decimal ThanhTien => SoLuong * DonGia;
    }
}