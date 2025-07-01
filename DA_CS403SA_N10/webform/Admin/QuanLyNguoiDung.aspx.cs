using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web;
using System.Web.Script.Services;
using System.Web.Services;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Webebook.WebForm.Admin
{
    public partial class QuanLyNguoiDung : System.Web.UI.Page
    {
        string connectionString = ConfigurationManager.ConnectionStrings["datawebebookConnectionString"].ConnectionString;
        private const string DeletedStatus = "Deleted";

        protected void Page_Load(object sender, EventArgs e)
        {
            EnsureScriptManagerIsEnabled();

            if (!IsPostBack)
            {
                if (Master is Admin master)
                {
                    master.SetPageTitle("Quản Lý Người Dùng");
                }

                if (!string.IsNullOrEmpty(Request.QueryString["message"]))
                {
                    HandleQueryStringMessage(Request.QueryString["message"]);
                }

                BindGrid();
            }
        }

        private void EnsureScriptManagerIsEnabled()
        {
            ScriptManager scriptManager = ScriptManager.GetCurrent(this.Page);
            if (scriptManager != null && !scriptManager.EnablePageMethods)
            {
                scriptManager.EnablePageMethods = true;
                System.Diagnostics.Debug.WriteLine("Info: PageMethods enabled on existing ScriptManager.");
            }
        }

        private void HandleQueryStringMessage(string messageKey)
        {
            switch (messageKey)
            {
                case "addusersuccess":
                    DisplayMessage("Thêm người dùng mới thành công!", MessageType.Success);
                    break;
                default:
                    DisplayMessage("Thao tác thành công.", MessageType.Success); 
                    break;
            }
        }

        private enum MessageType { Success, Error, Warning, Info }

        private void DisplayMessage(string messageText, MessageType type = MessageType.Info)
        {
            pnlMessageArea.Visible = true;
            successAlert.Visible = false;
            errorAlert.Visible = false;
            warningAlert.Visible = false;
            infoAlert.Visible = false;

            switch (type)
            {
                case MessageType.Success:
                    litSuccessMessage.Text = messageText;
                    successAlert.Visible = true;
                    break;
                case MessageType.Error:
                    litErrorMessage.Text = messageText;
                    errorAlert.Visible = true;
                    break;
                case MessageType.Warning:
                    litWarningMessage.Text = messageText;
                    warningAlert.Visible = true;
                    break;
                case MessageType.Info:
                default:
                    litInfoMessage.Text = messageText;
                    infoAlert.Visible = true;
                    break;
            }
        }

        private void DisplayModalMessage(string messageText, MessageType type = MessageType.Error)
        {
            pnlModalMessageArea.Visible = true;
            modalSuccessAlert.Visible = false;
            modalErrorAlert.Visible = false;
            modalWarningAlert.Visible = false;

            switch (type)
            {
                case MessageType.Success:
                    litModalSuccessMessage.Text = messageText;
                    modalSuccessAlert.Visible = true;
                    break;
                case MessageType.Error:
                    litModalErrorMessage.Text = messageText;
                    modalErrorAlert.Visible = true;
                    break;
                case MessageType.Warning:
                    litModalWarningMessage.Text = messageText;
                    modalWarningAlert.Visible = true;
                    break;
            }
            UpdatePanelModal.Update();
        }

        private void BindGrid()
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                StringBuilder queryBuilder = new StringBuilder("SELECT IDNguoiDung, Username, Email, Ten, DienThoai, VaiTro, ISNULL(TrangThai, 'Active') AS TrangThai FROM NguoiDung");
                List<string> conditions = new List<string>();
                SqlCommand cmd = new SqlCommand();

                if (!string.IsNullOrEmpty(ddlFilterRole.SelectedValue))
                {
                    conditions.Add("VaiTro = @VaiTro");
                    cmd.Parameters.AddWithValue("@VaiTro", Convert.ToInt32(ddlFilterRole.SelectedValue));
                }

                string selectedStatus = ddlFilterStatus.SelectedValue;
                if (!string.IsNullOrEmpty(selectedStatus))
                {
                    if (selectedStatus.Equals("All", StringComparison.OrdinalIgnoreCase)) { /* No condition needed */ }
                    else if (selectedStatus.Equals("ActiveOrLocked", StringComparison.OrdinalIgnoreCase))
                    {
                        conditions.Add("ISNULL(TrangThai, 'Active') <> @DeletedStatus");
                        cmd.Parameters.AddWithValue("@DeletedStatus", DeletedStatus);
                    }
                    else
                    {
                        conditions.Add("ISNULL(TrangThai, 'Active') = @TrangThai");
                        cmd.Parameters.AddWithValue("@TrangThai", selectedStatus);
                    }
                }
                else
                {
                    conditions.Add("ISNULL(TrangThai, 'Active') <> @DeletedStatusDefault");
                    cmd.Parameters.AddWithValue("@DeletedStatusDefault", DeletedStatus);
                }

                if (conditions.Count > 0)
                {
                    queryBuilder.Append(" WHERE ").Append(string.Join(" AND ", conditions));
                }

                queryBuilder.Append(" ORDER BY VaiTro ASC, IDNguoiDung DESC");

                cmd.CommandText = queryBuilder.ToString();
                cmd.Connection = con;

                try
                {
                    con.Open();
                    SqlDataAdapter da = new SqlDataAdapter(cmd);
                    DataTable dt = new DataTable();
                    da.Fill(dt);
                    GridViewNguoiDung.DataSource = dt;
                    GridViewNguoiDung.DataBind();
                }
                catch (Exception ex)
                {
                    DisplayMessage("Lỗi tải danh sách người dùng: " + ex.Message, MessageType.Error);
                    GridViewNguoiDung.DataSource = null;
                    GridViewNguoiDung.DataBind();
                    System.Diagnostics.Debug.WriteLine($"BindGrid Error: {ex}");
                }
            }
        }

        protected void btnFilter_Click(object sender, EventArgs e)
        {
            GridViewNguoiDung.PageIndex = 0;
            BindGrid();
            pnlMessageArea.Visible = false;
        }

        protected void btnClearFilter_Click(object sender, EventArgs e)
        {
            ddlFilterRole.SelectedIndex = 0;
            ddlFilterStatus.ClearSelection();
            ddlFilterStatus.Items.FindByValue("ActiveOrLocked").Selected = true;
            GridViewNguoiDung.PageIndex = 0;
            BindGrid();
            pnlMessageArea.Visible = false;
        }

        protected void GridViewNguoiDung_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            GridViewNguoiDung.PageIndex = e.NewPageIndex;
            BindGrid();
            pnlMessageArea.Visible = false;
        }

        protected void GridViewNguoiDung_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandArgument == null || !int.TryParse(e.CommandArgument.ToString(), out int userId)) return;

            int loggedInUserId = HttpContext.Current.Session["UserID"] != null ? Convert.ToInt32(HttpContext.Current.Session["UserID"]) : -999;

            if (!CanPerformActionOnUser(userId, loggedInUserId, out string permissionMessage))
            {
                DisplayMessage(permissionMessage, MessageType.Warning);
                return;
            }

            pnlMessageArea.Visible = false;

            switch (e.CommandName)
            {
                case "LockUser":
                case "UnlockUser":
                    string newStatus = e.CommandName == "LockUser" ? "Locked" : "Active";
                    UpdateUserStatus(userId, newStatus);
                    BindGrid();
                    break;
                case "DeleteUser":
                    SoftDeleteUser(userId);
                    BindGrid();
                    break;
            }
        }

        private void UpdateUserStatus(int userId, string newStatus)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string updateQuery = "UPDATE NguoiDung SET TrangThai = @TrangThai WHERE IDNguoiDung = @IDNguoiDung AND ISNULL(TrangThai, 'Active') <> @DeletedStatus";
                try
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand(updateQuery, con))
                    {
                        cmd.Parameters.AddWithValue("@TrangThai", newStatus);
                        cmd.Parameters.AddWithValue("@IDNguoiDung", userId);
                        cmd.Parameters.AddWithValue("@DeletedStatus", DeletedStatus);

                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            DisplayMessage($"Cập nhật trạng thái người dùng #{userId} thành '{newStatus}' thành công!", MessageType.Success);
                        }
                        else
                        {
                            DisplayMessage($"Không tìm thấy người dùng #{userId} hoặc người dùng đã bị xóa.", MessageType.Warning);
                        }
                    }
                }
                catch (Exception ex)
                {
                    DisplayMessage("Lỗi khi cập nhật trạng thái: " + ex.Message, MessageType.Error);
                    System.Diagnostics.Debug.WriteLine($"UpdateUserStatus Error: {ex}");
                }
            }
        }

        private void SoftDeleteUser(int userId)
        {
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "UPDATE NguoiDung SET TrangThai = @NewStatus WHERE IDNguoiDung = @IDNguoiDung AND ISNULL(TrangThai, 'Active') <> @CurrentStatus";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@NewStatus", DeletedStatus);
                    cmd.Parameters.AddWithValue("@CurrentStatus", DeletedStatus);
                    cmd.Parameters.AddWithValue("@IDNguoiDung", userId);
                    try
                    {
                        con.Open();
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            DisplayMessage($"Vô hiệu hóa (xóa mềm) người dùng #{userId} thành công!", MessageType.Success);
                        }
                        else
                        {
                            DisplayMessage($"Không tìm thấy người dùng #{userId} hoặc người dùng đã được vô hiệu hóa.", MessageType.Warning);
                        }
                    }
                    catch (Exception ex)
                    {
                        DisplayMessage("Lỗi khi vô hiệu hóa người dùng: " + ex.Message, MessageType.Error);
                        System.Diagnostics.Debug.WriteLine($"SoftDeleteUser Error: {ex}");
                    }
                }
            }
        }

        protected void btnSaveChanges_Click(object sender, EventArgs e)
        {
            int userId;
            if (!int.TryParse(hfEditUserID.Value, out userId) || userId <= 0)
            {
                DisplayModalMessage("UserID không hợp lệ để lưu.", MessageType.Error);
                return;
            }

            int loggedInUserId = Session["UserID"] != null ? Convert.ToInt32(Session["UserID"]) : -999;

            if (!CanPerformActionOnUser(userId, loggedInUserId, out string permissionMessage))
            {
                DisplayModalMessage(permissionMessage, MessageType.Warning);
                return;
            }

            string matKhauMoi = txtEditMatKhauMoi.Text;
            string xacNhanMatKhau = txtEditXacNhanMatKhau.Text;
            if (!string.IsNullOrEmpty(matKhauMoi) && matKhauMoi != xacNhanMatKhau)
            {
                DisplayModalMessage("Mật khẩu mới và xác nhận mật khẩu không khớp.", MessageType.Error);
                return;
            }

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                StringBuilder queryBuilder = new StringBuilder("UPDATE NguoiDung SET Email = @Email, Ten = @Ten, DienThoai = @DienThoai, VaiTro = @VaiTro, TrangThai = @TrangThai ");
                SqlCommand cmd = new SqlCommand();

                try
                {
                    con.Open();
                    cmd.Parameters.AddWithValue("@Email", txtEditEmail.Text.Trim());
                    cmd.Parameters.AddWithValue("@Ten", string.IsNullOrWhiteSpace(txtEditTen.Text) ? (object)DBNull.Value : txtEditTen.Text.Trim());
                    cmd.Parameters.AddWithValue("@DienThoai", string.IsNullOrWhiteSpace(txtEditDienThoai.Text) ? (object)DBNull.Value : txtEditDienThoai.Text.Trim());
                    cmd.Parameters.AddWithValue("@VaiTro", Convert.ToInt32(ddlEditVaiTro.SelectedValue));
                    string newStatus = ddlEditTrangThai.SelectedValue;
                    cmd.Parameters.AddWithValue("@TrangThai", newStatus);
                    cmd.Parameters.AddWithValue("@IDNguoiDung", userId);

                    if (!string.IsNullOrEmpty(matKhauMoi))
                    {
                        queryBuilder.Append(", MatKhau = @MatKhau ");
                        cmd.Parameters.AddWithValue("@MatKhau", matKhauMoi);
                    }

                    queryBuilder.Append("WHERE IDNguoiDung = @IDNguoiDung AND ISNULL(TrangThai, 'Active') <> @DeletedStatus");
                    cmd.Parameters.AddWithValue("@DeletedStatus", DeletedStatus);

                    cmd.CommandText = queryBuilder.ToString();
                    cmd.Connection = con;

                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        ScriptManager.RegisterStartupScript(this, GetType(), "CloseModalScript", "closeEditModal();", true);
                        DisplayMessage($"Cập nhật thông tin người dùng #{userId} thành công!", MessageType.Success);
                        BindGrid();
                    }
                    else
                    {
                        DisplayModalMessage("Không có thay đổi nào được lưu hoặc không tìm thấy người dùng (có thể đã bị xóa).", MessageType.Warning);
                    }
                }
                catch (SqlException sqlEx)
                {
                    string errorMsg = "Lỗi CSDL khi cập nhật: ";
                    if (sqlEx.Number == 2627 || sqlEx.Number == 2601)
                    {
                        if (sqlEx.Message.ToLower().Contains("email")) errorMsg += "Email này đã tồn tại.";
                        else errorMsg += "Vi phạm ràng buộc dữ liệu duy nhất (unique constraint).";
                    }
                    else
                    {
                        errorMsg += sqlEx.Message;
                    }
                    DisplayModalMessage(errorMsg, MessageType.Error);
                    System.Diagnostics.Debug.WriteLine($"btnSaveChanges SQL Error: {sqlEx}");
                }
                catch (Exception ex)
                {
                    DisplayModalMessage("Lỗi không xác định khi cập nhật người dùng: " + ex.Message, MessageType.Error);
                    System.Diagnostics.Debug.WriteLine($"btnSaveChanges Error: {ex}");
                }
            }
        }

        [WebMethod]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public static object GetUserData(int userId)
        {
            if (HttpContext.Current.Session["UserID"] == null || HttpContext.Current.Session["VaiTro"] == null || Convert.ToInt32(HttpContext.Current.Session["VaiTro"]) != 0)
            {
                System.Diagnostics.Debug.WriteLine($"Unauthorized attempt to call GetUserData for UserID: {userId}");
                return new { error = "Truy cập không được phép." };
            }

            string connString = ConfigurationManager.ConnectionStrings["datawebebookConnectionString"].ConnectionString;
            NguoiDungData user = null;

            using (SqlConnection con = new SqlConnection(connString))
            {
                string query = "SELECT Username, Email, Ten, DienThoai, VaiTro, ISNULL(TrangThai, 'Active') as TrangThai FROM NguoiDung WHERE IDNguoiDung = @IDNguoiDung";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@IDNguoiDung", userId);
                    try
                    {
                        con.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string status = reader["TrangThai"]?.ToString();
                                if (status == DeletedStatus)
                                {
                                    return new { error = "Người dùng này đã bị xóa và không thể chỉnh sửa." };
                                }

                                user = new NguoiDungData
                                {
                                    Username = reader["Username"]?.ToString(),
                                    Email = reader["Email"]?.ToString(),
                                    Ten = reader["Ten"] != DBNull.Value ? reader["Ten"].ToString() : null,
                                    DienThoai = reader["DienThoai"] != DBNull.Value ? reader["DienThoai"].ToString() : null,
                                    VaiTro = reader["VaiTro"] != DBNull.Value ? Convert.ToInt32(reader["VaiTro"]) : 1,
                                    TrangThai = status
                                };
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"Error fetching user data for ID {userId}: {ex.Message}");
                        return new { error = $"Lỗi máy chủ khi lấy dữ liệu: {ex.Message}" };
                    }
                }
            }

            if (user == null)
            {
                return new { error = $"Không tìm thấy người dùng với ID {userId}." };
            }
            return user;
        }

        public class NguoiDungData
        {
            public string Username { get; set; }
            public string Email { get; set; }
            public string Ten { get; set; }
            public string DienThoai { get; set; }
            public int VaiTro { get; set; }
            public string TrangThai { get; set; }
        }

        protected void GridViewNguoiDung_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                // Lấy thông tin cần thiết từ dòng dữ liệu
                int rowUserId = Convert.ToInt32(GridViewNguoiDung.DataKeys[e.Row.RowIndex].Value);
                string status = DataBinder.Eval(e.Row.DataItem, "TrangThai")?.ToString() ?? "Active";
                string username = DataBinder.Eval(e.Row.DataItem, "Username")?.ToString() ?? "[không tên]";
                string usernameEncoded = HttpUtility.JavaScriptStringEncode(username); // Mã hóa để an toàn trong JS

                // Tìm các control
                LinkButton lnkToggleLock = (LinkButton)e.Row.FindControl("lnkToggleLock");
                LinkButton lnkEditUser = (LinkButton)e.Row.FindControl("lnkEditUser");
                LinkButton lnkDeleteUser = (LinkButton)e.Row.FindControl("lnkDeleteUser");

                // Logic ẩn/hiện nút (giữ nguyên)
                int loggedInUserId = Session["UserID"] != null ? Convert.ToInt32(Session["UserID"]) : -999;
                bool canModify = CanModifyUserCheck(rowUserId, loggedInUserId) && status != DeletedStatus;

                if (lnkEditUser != null) lnkEditUser.Visible = canModify;

                // ================= THAY ĐỔI BẮT ĐẦU =================
                if (lnkToggleLock != null)
                {
                    lnkToggleLock.Visible = canModify;
                    if (canModify) // Chỉ gán OnClientClick nếu nút được hiển thị
                    {
                        if (status.Equals("Active", StringComparison.OrdinalIgnoreCase))
                        {
                            lnkToggleLock.OnClientClick = $"showLockConfirmation('{usernameEncoded}', '{lnkToggleLock.UniqueID}'); return false;";
                        }
                        else
                        {
                            lnkToggleLock.OnClientClick = $"showUnlockConfirmation('{usernameEncoded}', '{lnkToggleLock.UniqueID}'); return false;";
                        }
                    }
                }

                if (lnkDeleteUser != null)
                {
                    lnkDeleteUser.Visible = canModify;
                    if (canModify) // Chỉ gán OnClientClick nếu nút được hiển thị
                    {
                        lnkDeleteUser.OnClientClick = $"showDisableConfirmation('{usernameEncoded}', '{lnkDeleteUser.UniqueID}'); return false;";
                    }
                }
                // ================= THAY ĐỔI KẾT THÚC =================
            }
        }

        protected bool CanModifyUser(int targetUserId)
        {
            int loggedInUserId = Session["UserID"] != null ? Convert.ToInt32(Session["UserID"]) : -999;
            return CanModifyUserCheck(targetUserId, loggedInUserId);
        }

        private bool CanModifyUserCheck(int targetUserId, int loggedInUserId)
        {
            if (targetUserId <= 0) return false;

            if (targetUserId == loggedInUserId) return false; // Cannot modify self

            return true;
        }

        private bool CanPerformActionOnUser(int targetUserId, int currentUserId, out string message)
        {
            message = "";

            if (!CanModifyUserCheck(targetUserId, currentUserId))
            {
                message = "Không thể thực hiện thao tác này trên tài khoản của chính bạn (hoặc tài khoản không hợp lệ).";
                return false;
            }

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = "SELECT TrangThai FROM NguoiDung WHERE IDNguoiDung = @IDNguoiDung";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@IDNguoiDung", targetUserId);
                    try
                    {
                        con.Open();
                        object result = cmd.ExecuteScalar();
                        if (result != null && result.ToString().Equals(DeletedStatus, StringComparison.OrdinalIgnoreCase))
                        {
                            message = "Không thể thực hiện thao tác trên người dùng đã bị xóa.";
                            return false;
                        }
                        if (result == null || result == DBNull.Value)
                        {
                            message = "Không tìm thấy người dùng.";
                            return false;
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"CanPerformActionOnUser Status Check Error: {ex}");
                        message = "Lỗi kiểm tra trạng thái người dùng: " + ex.Message;
                        return false;
                    }
                }
            }
            return true;
        }

        protected void btnThemNguoiDungMoi_Click(object sender, EventArgs e)
        {
            Response.Redirect("ThemNguoiDung.aspx");
        }

        protected string GetStatusCssClass(object statusObj)
        {
            string status = statusObj?.ToString() ?? "Active";
            switch (status.ToLower())
            {
                case "active":
                    return "px-2 inline-flex text-xs leading-5 font-semibold rounded-full bg-green-100 text-green-800";
                case "locked":
                    return "px-2 inline-flex text-xs leading-5 font-semibold rounded-full bg-red-100 text-red-800";
                case "deleted":
                    return "status-deleted";
                default:
                    return "";
            }
        }
    }
}