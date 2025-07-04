using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;

namespace Webebook.WebForm.User
{
    public partial class giohang_user : System.Web.UI.Page
    {
        string connectionString = ConfigurationManager.ConnectionStrings["datawebebookConnectionString"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null)
            {
                Response.Redirect("~/WebForm/VangLai/dangnhap.aspx?returnUrl=" + Server.UrlEncode(Request.RawUrl), true);
                return;
            }

            if (!IsPostBack)
            {
                LoadCart();
                UpdateMasterCartCount();
            }
        }

        // === PHƯƠNG THỨC ĐÃ SỬA LỖI ===
        protected void gvGioHang_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                // Khai báo drv MỘT LẦN ở đây để sử dụng cho toàn bộ phương thức
                DataRowView drv = (DataRowView)e.Row.DataItem;
                if (drv == null) return; // Thêm kiểm tra an toàn

                // --- Logic cho Checkbox ---
                CheckBox chkSelect = (CheckBox)e.Row.FindControl("chkSelect");
                if (chkSelect != null)
                {
                    // KHÔNG khai báo lại "DataRowView drv" ở đây nữa
                    if (drv.Row.Table.Columns.Contains("GiaSach") && drv["GiaSach"] != DBNull.Value)
                    {
                        try
                        {
                            decimal price = Convert.ToDecimal(drv["GiaSach"]);
                            chkSelect.Attributes["data-price"] = price.ToString(CultureInfo.InvariantCulture);

                            var parentDiv = chkSelect.Parent as System.Web.UI.HtmlControls.HtmlGenericControl;
                            if (parentDiv != null) parentDiv.Attributes["data-price"] = price.ToString(CultureInfo.InvariantCulture);
                        }
                        catch (Exception ex)
                        {
                            LogError($"RowDataBound Price Error Row {e.Row.RowIndex}: {ex.Message}");
                            chkSelect.Attributes["data-price"] = "0";
                        }
                    }
                    else { chkSelect.Attributes["data-price"] = "0"; }

                    if (!chkSelect.CssClass.Contains("item-checkbox")) { chkSelect.CssClass += " item-checkbox"; }
                    if (!chkSelect.CssClass.Contains("form-checkbox")) { chkSelect.CssClass += " form-checkbox"; }
                }
                else { LogError($"Could not find chkSelect in row {e.Row.RowIndex}"); }

                // --- GÁN SỰ KIỆN CHO NÚT XÓA ---
                var lnkXoa = e.Row.FindControl("lnkXoa") as LinkButton;
                if (lnkXoa != null)
                {
                    string cartItemId = drv["IDGioHang"].ToString();
                    string bookTitle = drv["TenSach"]?.ToString() ?? "[Sách không tên]";
                    string encodedBookTitle = HttpUtility.JavaScriptStringEncode(bookTitle);

                    lnkXoa.OnClientClick = $"showCartItemDeleteConfirmation('{cartItemId}', '{encodedBookTitle}', '{lnkXoa.UniqueID}'); return false;";
                }
            }
            else if (e.Row.RowType == DataControlRowType.Header)
            {
                CheckBox chkHeader = e.Row.FindControl("chkHeader") as CheckBox;
                if (chkHeader != null)
                {
                    if (!chkHeader.CssClass.Contains("header-checkbox")) { chkHeader.CssClass += " header-checkbox"; }
                    if (!chkHeader.CssClass.Contains("form-checkbox")) { chkHeader.CssClass += " form-checkbox"; }
                }
                else { LogError($"Could not find chkHeader in header row"); }
            }
        }

        private void LoadCart()
        {
            pnlCart.Visible = false;
            pnlEmptyCart.Visible = true;
            btnThanhToan.Enabled = false;
            lblSelectedTotal.Text = "0 VNĐ";
            if (lblSelectedItemCount != null) { lblSelectedItemCount.Text = "0"; }

            int userId;
            if (!int.TryParse(Session["UserID"]?.ToString(), out userId))
            {
                ShowMessage("Không thể xác định người dùng. Vui lòng đăng nhập lại.", true);
                LogError("UserID session variable is missing or invalid in LoadCart.");
                return;
            }

            DataTable dt = new DataTable();
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"SELECT gh.IDGioHang, gh.IDSach, s.TenSach, s.DuongDanBiaSach, s.GiaSach
                                 FROM GioHang gh
                                 INNER JOIN Sach s ON gh.IDSach = s.IDSach
                                 WHERE gh.IDNguoiDung = @UserId
                                 ORDER BY gh.IDGioHang DESC";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    try
                    {
                        con.Open();
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        da.Fill(dt);

                        gvGioHang.DataSource = dt;
                        gvGioHang.DataBind();

                        bool hasItems = dt.Rows.Count > 0;
                        pnlCart.Visible = hasItems;
                        pnlEmptyCart.Visible = !hasItems;

                        if (hasItems)
                        {
                            ClientScript.RegisterStartupScript(this.GetType(), "CartLoad_" + DateTime.Now.Ticks, "initializeCartEvents();", true);
                        }
                    }
                    catch (SqlException sqlEx) { HandleLoadError(userId, $"SQL Error Loading Cart: {sqlEx.Message}", "Lỗi cơ sở dữ liệu khi tải giỏ hàng."); }
                    catch (Exception ex) { HandleLoadError(userId, $"General Error Loading Cart: {ex.Message}", "Lỗi không xác định khi tải giỏ hàng."); }
                }
            }
        }

        private void HandleLoadError(int userId, string logMessage, string userMessage)
        {
            ShowMessage(userMessage, true);
            LogError(logMessage + $" (UserID: {userId})");
            if (gvGioHang != null) { gvGioHang.DataSource = null; gvGioHang.DataBind(); }
            pnlCart.Visible = false;
            pnlEmptyCart.Visible = true;
        }

        protected void gvGioHang_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            int userId;
            if (!int.TryParse(Session["UserID"]?.ToString(), out userId)) { Response.Redirect("~/WebForm/VangLai/dangnhap.aspx", true); return; }

            if (e.CommandName == "Xoa")
            {
                if (!int.TryParse(e.CommandArgument?.ToString(), out int idGioHang) || idGioHang <= 0)
                {
                    ShowMessage("ID mục giỏ hàng không hợp lệ.", true);
                    LogError($"Invalid CommandArgument in gvGioHang_RowCommand: '{e.CommandArgument}'");
                    return;
                }
                DeleteItem(idGioHang, userId);
            }
        }

        private void DeleteItem(int idGioHang, int userId)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "DELETE FROM GioHang WHERE IDGioHang = @IDGioHang AND IDNguoiDung = @UserId";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@IDGioHang", idGioHang);
                    cmd.Parameters.AddWithValue("@UserId", userId);
                    try
                    {
                        con.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            ShowMessage("Đã xóa sách khỏi giỏ hàng.", false);
                            LoadCart();
                            UpdateMasterCartCount();
                        }
                        else
                        {
                            ShowMessage("Không tìm thấy mục để xóa hoặc bạn không có quyền.", true);
                            LogError($"Failed to delete cart item. Rows affected: 0. IDGioHang: {idGioHang}, UserID: {userId}");
                        }
                    }
                    catch (Exception ex)
                    {
                        ShowMessage("Lỗi khi xóa sách.", true);
                        LogError($"Error Deleting Cart Item {idGioHang} User {userId}: {ex.Message}");
                    }
                }
            }
        }

        protected void btnThanhToan_Click(object sender, EventArgs e)
        {
            if (Session["UserID"] == null) { Response.Redirect("~/WebForm/VangLai/dangnhap.aspx", true); return; }

            List<int> selectedCartItemIds = new List<int>();

            foreach (GridViewRow row in gvGioHang.Rows)
            {
                if (row.RowType == DataControlRowType.DataRow)
                {
                    CheckBox chkSelect = (CheckBox)row.FindControl("chkSelect");
                    if (chkSelect != null && chkSelect.Checked)
                    {
                        int idGioHang = Convert.ToInt32(gvGioHang.DataKeys[row.RowIndex]["IDGioHang"]);
                        selectedCartItemIds.Add(idGioHang);
                    }
                }
            }

            if (selectedCartItemIds.Any())
            {
                Session["SelectedCartItems"] = selectedCartItemIds;
                Response.Redirect("~/WebForm/User/thanhtoan.aspx", false);
                Context.ApplicationInstance.CompleteRequest();
            }
            else
            {
                ShowMessage("Vui lòng chọn ít nhất một sản phẩm để thanh toán.", true);
            }
        }

        private void UpdateMasterCartCount()
        {
            if (Master is Webebook.WebForm.User.UserMaster master)
            {
                master.UpdateCartCount();
            }
            else
            {
                LogError($"Could not find Master Page of expected type (Webebook.WebForm.User.UserMaster). Actual type: {Master?.GetType().FullName ?? "null"}");
            }
        }

        private void ShowMessage(string message, bool isError)
        {
            if (lblMessage != null)
            {
                lblMessage.Text = Server.HtmlEncode(message);
                lblMessage.CssClass = "block mb-4 p-3 rounded-md border text-sm " +
                                  (isError ? "bg-red-50 border-red-300 text-red-700"
                                           : "bg-green-50 border-green-300 text-green-700");
                lblMessage.Visible = true;
            }
            else { LogError($"lblMessage control not found on page when trying to show: '{message}'"); }
        }

        private void LogError(string message)
        {
            System.Diagnostics.Debug.WriteLine("GIOHANG_ERROR: " + DateTime.Now + " | " + message);
        }
    }
}