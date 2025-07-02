using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections.Generic;
using System.Globalization;
using System.Linq; // Keep this for potential future use
using System.Web;

namespace Webebook.WebForm.User
{
    public partial class giohang_user : System.Web.UI.Page
    {
        string connectionString = ConfigurationManager.ConnectionStrings["datawebebookConnectionString"].ConnectionString;

        // Page_Load remains the same
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

        // LoadCart, HandleLoadError, gvGioHang_RowCommand, DeleteItem, btnThanhToan_Click, UpdateMasterCartCount, ShowMessage, LogError
        // remain the same as previous version.

        // === MODIFIED: gvGioHang_RowDataBound ===
        protected void gvGioHang_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                // --- Find Checkbox and Set Data Price ---
                CheckBox chkSelect = (CheckBox)e.Row.FindControl("chkSelect");
                if (chkSelect != null)
                {
                    DataRowView drv = (DataRowView)e.Row.DataItem;
                    if (drv != null && drv.Row.Table.Columns.Contains("GiaSach") && drv["GiaSach"] != DBNull.Value)
                    {
                        try
                        {
                            decimal price = Convert.ToDecimal(drv["GiaSach"]);
                            // Use InvariantCulture to ensure decimal point is '.' for JavaScript parseFloat
                            chkSelect.Attributes["data-price"] = price.ToString(CultureInfo.InvariantCulture);
                            // Also add to parent div for easier access in responsive view if needed
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

                    // Ensure CSS classes are present (though they should be from markup)
                    if (!chkSelect.CssClass.Contains("item-checkbox")) { chkSelect.CssClass += " item-checkbox"; }
                    if (!chkSelect.CssClass.Contains("form-checkbox")) { chkSelect.CssClass += " form-checkbox"; }
                }
                else { LogError($"Could not find chkSelect in row {e.Row.RowIndex}"); }

                // --- Remove Name Span Tooltip Logic ---
                // The HyperLink in ASPX now handles the tooltip directly via Eval.
                /*
                var nameSpan = e.Row.FindControl("TenSach") as Label; // Assuming you might change the span to a label or find it by class
                // ... (rest of the old finding logic for nameSpan) ...
                if (nameSpan != null)
                {
                    DataRowView drv = (DataRowView)e.Row.DataItem;
                    if (drv != null && drv.Row.Table.Columns.Contains("TenSach"))
                    {
                        nameSpan.ToolTip = drv["TenSach"].ToString();
                    }
                }
                */
            }
            else if (e.Row.RowType == DataControlRowType.Header)
            {
                // --- Ensure Header Checkbox CSS ---
                CheckBox chkHeader = e.Row.FindControl("chkHeader") as CheckBox;
                if (chkHeader != null)
                {
                    if (!chkHeader.CssClass.Contains("header-checkbox")) { chkHeader.CssClass += " header-checkbox"; }
                    if (!chkHeader.CssClass.Contains("form-checkbox")) { chkHeader.CssClass += " form-checkbox"; }
                }
                else { LogError($"Could not find chkHeader in header row"); }
            }
        }
        // === END MODIFIED ===

        // Other methods (LoadCart, DeleteItem, btnThanhToan_Click etc. remain the same)
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
                // Consider redirecting here if user ID is essential and missing.
                // Response.Redirect("~/WebForm/VangLai/dangnhap.aspx", true);
                // return;
                LogError("UserID session variable is missing or invalid in LoadCart.");
                return; // Stop loading if no valid user ID
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
                            // Reregister script after databind to ensure elements exist
                            // Use different key to avoid conflicts if LoadCart is called multiple times
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

            if (!int.TryParse(e.CommandArgument?.ToString(), out int idGioHang) || idGioHang <= 0)
            {
                ShowMessage("ID mục giỏ hàng không hợp lệ.", true);
                LogError($"Invalid CommandArgument in gvGioHang_RowCommand: '{e.CommandArgument}'");
                return;
            }

            if (e.CommandName == "Xoa")
            {
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
                            LoadCart(); // Reload data
                            UpdateMasterCartCount(); // Update count in master page
                                                     // Reregister script after databind
                            ClientScript.RegisterStartupScript(this.GetType(), "UpdateCartAfterDelete_" + DateTime.Now.Ticks, "initializeCartEvents();", true);
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
            int userId;
            if (!int.TryParse(Session["UserID"]?.ToString(), out userId)) { Response.Redirect("~/WebForm/VangLai/dangnhap.aspx", true); return; }

            List<int> selectedCartItemIds = new List<int>();
            bool itemSelected = false;

            if (gvGioHang == null || gvGioHang.Rows.Count == 0)
            {
                ShowMessage("Giỏ hàng trống hoặc không thể truy cập.", true);
                LogError("btnThanhToan_Click called but GridView is null or empty.");
                return;
            }


            foreach (GridViewRow row in gvGioHang.Rows)
            {
                if (row.RowType == DataControlRowType.DataRow)
                {
                    CheckBox chkSelect = (CheckBox)row.FindControl("chkSelect");
                    // Check if DataKeys exists and index is valid before accessing
                    if (chkSelect != null && chkSelect.Checked && gvGioHang.DataKeys != null && row.RowIndex < gvGioHang.DataKeys.Count)
                    {
                        try
                        {
                            // Validate DataKey structure before accessing
                            if (gvGioHang.DataKeys[row.RowIndex].Values.Contains("IDGioHang"))
                            {
                                int idGioHang = Convert.ToInt32(gvGioHang.DataKeys[row.RowIndex]["IDGioHang"]);
                                selectedCartItemIds.Add(idGioHang);
                                itemSelected = true;
                            }
                            else
                            {
                                LogError($"DataKey 'IDGioHang' not found for Row {row.RowIndex} during checkout.");
                                ShowMessage("Lỗi cấu trúc dữ liệu giỏ hàng.", true);
                                return; // Stop processing
                            }
                        }
                        catch (Exception ex)
                        {
                            LogError($"Checkout Error Get Key Row {row.RowIndex}: {ex.Message}");
                            ShowMessage("Lỗi xử lý chọn hàng.", true);
                            return; // Stop processing on error
                        }
                    }
                    else if (chkSelect == null)
                    {
                        LogError($"chkSelect is null in row {row.RowIndex} during checkout.");
                    }
                }
            }

            if (itemSelected)
            {
                Session["SelectedCartItems"] = selectedCartItemIds;
                Response.Redirect("~/WebForm/User/thanhtoan.aspx", false); // Navigate to checkout page
                Context.ApplicationInstance.CompleteRequest(); // Recommended after Response.Redirect(..., false)
            }
            else
            {
                ShowMessage("Vui lòng chọn ít nhất một sản phẩm.", true);
                // Ensure JS is re-initialized if needed after postback with message
                ClientScript.RegisterStartupScript(this.GetType(), "CheckoutErrorReinit_" + DateTime.Now.Ticks, "initializeCartEvents();", true);
            }
        }

        private void UpdateMasterCartCount()
        {
            // IMPORTANT: Ensure 'Webebook.WebForm.User.UserMaster' matches the EXACT namespace and class name of your Master Page code-behind file.
            if (Master is Webebook.WebForm.User.UserMaster master)
            {
                master.UpdateCartCount();
            }
            else
            {
                // Log this error, it indicates a potential problem with Master Page type casting
                LogError($"Could not find Master Page of expected type (Webebook.WebForm.User.UserMaster). Actual type: {Master?.GetType().FullName ?? "null"}");
                // Optionally show a generic error or handle gracefully
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
            // Simple logging to Debug output. Replace/extend with a robust logging framework.
            System.Diagnostics.Debug.WriteLine("GIOHANG_ERROR: " + DateTime.Now + " | " + message);
            // Example: try { System.IO.File.AppendAllText(Server.MapPath("~/App_Data/ErrorLog_Cart.txt"), DateTime.Now + ": " + message + Environment.NewLine); } catch { /* Ignore logging errors */ }
        }

    }
}