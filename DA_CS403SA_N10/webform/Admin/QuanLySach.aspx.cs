using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Webebook.WebForm.Admin
{
    public partial class QuanLySach : System.Web.UI.Page
    {
        string connectionString = ConfigurationManager.ConnectionStrings["datawebebookConnectionString"].ConnectionString;
        private const string UploadFolderPath = "~/Uploads/Covers/";

        #region Filter State Properties (using ViewState)
        private string CurrentSearchTerm
        {
            get { return ViewState["CurrentSearchTerm"] as string ?? string.Empty; }
            set { ViewState["CurrentSearchTerm"] = value; }
        }
        private string CurrentStatusFilter
        {
            get { return ViewState["CurrentStatusFilter"] as string ?? string.Empty; }
            set { ViewState["CurrentStatusFilter"] = value; }
        }
        #endregion

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Master is Admin master)
                {
                    master.SetPageTitle("Quản Lý Sách");
                }
                InitializeFiltersAndBindGrid();
                if (!string.IsNullOrEmpty(Request.QueryString["message"]))
                {
                    HandleMessage(Request.QueryString["message"]);
                }
            }
        }

        /// <summary>
        /// Khởi tạo giá trị bộ lọc trong ViewState và tải dữ liệu ban đầu.
        /// </summary>
        private void InitializeFiltersAndBindGrid()
        {
            // Đặt giá trị mặc định cho bộ lọc trong ViewState (là rỗng để hiển thị tất cả)
            CurrentSearchTerm = string.Empty;
            CurrentStatusFilter = string.Empty;

            // Gắn giá trị mặc định vào các control lọc
            txtSearchTerm.Text = string.Empty;
            ddlStatusFilter.ClearSelection(); // Xóa lựa chọn cũ
            // *** SỬA LỖI CS0131 ***
            // Tìm item "-- Tất cả --" và chọn nó nếu tìm thấy
            ListItem defaultStatusItem = ddlStatusFilter.Items.FindByValue(string.Empty);
            if (defaultStatusItem != null)
            {
                defaultStatusItem.Selected = true;
            }
            // *** HẾT SỬA LỖI ***

            BindGrid(); // Tải dữ liệu lần đầu
        }

        /// <summary>
        /// Gắn dữ liệu từ bảng Sach vào control GridViewSach, áp dụng bộ lọc hiện tại.
        /// </summary>
        private void BindGrid()
        {
            string searchTerm = CurrentSearchTerm;
            string statusFilter = CurrentStatusFilter;
            DataTable dt = new DataTable();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string baseQuery = @"SELECT
                                    IDSach, TenSach, TacGia, GiaSach, NhaXuatBan,
                                    DuongDanBiaSach, TrangThaiNoiDung
                                FROM Sach";
                StringBuilder whereClause = new StringBuilder();
                List<SqlParameter> parameters = new List<SqlParameter>();

                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    if (whereClause.Length > 0) whereClause.Append(" AND");
                    whereClause.Append(" (TenSach LIKE @SearchTerm OR TacGia LIKE @SearchTerm)");
                    parameters.Add(new SqlParameter("@SearchTerm", $"%{searchTerm}%"));
                }
                if (!string.IsNullOrWhiteSpace(statusFilter))
                {
                    if (whereClause.Length > 0) whereClause.Append(" AND");
                    whereClause.Append(" TrangThaiNoiDung = @StatusFilter");
                    parameters.Add(new SqlParameter("@StatusFilter", statusFilter));
                }

                string finalQuery = baseQuery;
                if (whereClause.Length > 0)
                {
                    finalQuery += " WHERE" + whereClause.ToString();
                }
                finalQuery += " ORDER BY IDSach DESC";

                Debug.WriteLine($"Executing Query: {finalQuery}");

                using (SqlCommand cmd = new SqlCommand(finalQuery, con))
                {
                    if (parameters.Count > 0)
                    {
                        cmd.Parameters.AddRange(parameters.ToArray());
                    }
                    try
                    {
                        con.Open();
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        da.Fill(dt);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Lỗi khi gắn dữ liệu vào grid: {ex.ToString()}");
                        ShowMessage("Lỗi tải danh sách sách.", isError: true);
                        dt = new DataTable();
                    }
                }
            }

            GridViewSach.DataSource = dt;
            GridViewSach.DataBind();
            UpdateEmptyDataTemplateMessages(dt.Rows.Count == 0, !string.IsNullOrWhiteSpace(CurrentSearchTerm) || !string.IsNullOrWhiteSpace(CurrentStatusFilter));
        }

        /// <summary>
        /// Cập nhật thông báo trong EmptyDataTemplate.
        /// </summary>
        private void UpdateEmptyDataTemplateMessages(bool isEmpty, bool hasFilters)
        {
            if (isEmpty && GridViewSach.Controls.Count > 0 && GridViewSach.Controls[0] is Table)
            {
                GridViewRow emptyRow = GridViewSach.Controls[0].Controls[0] as GridViewRow;
                if (emptyRow != null && emptyRow.RowType == DataControlRowType.EmptyDataRow)
                {
                    PlaceHolder phEmptyDataMessage = (PlaceHolder)emptyRow.FindControl("phEmptyDataMessage");
                    PlaceHolder phNoDataMessage = (PlaceHolder)emptyRow.FindControl("phNoDataMessage");
                    if (phEmptyDataMessage != null && phNoDataMessage != null)
                    {
                        phEmptyDataMessage.Visible = hasFilters;
                        phNoDataMessage.Visible = !hasFilters;
                    }
                }
            }
        }

        /// <summary>
        /// Xử lý sự kiện click cho nút "Lọc / Tìm".
        /// </summary>
        protected void btnFilter_Click(object sender, EventArgs e)
        {
            CurrentSearchTerm = txtSearchTerm.Text.Trim();
            CurrentStatusFilter = ddlStatusFilter.SelectedValue;
            GridViewSach.PageIndex = 0;
            BindGrid();
        }

        /// <summary>
        /// Xử lý sự kiện click cho nút "Bỏ lọc".
        /// </summary>
        protected void btnClearFilter_Click(object sender, EventArgs e)
        {
            // Xóa trắng các control lọc
            txtSearchTerm.Text = string.Empty;
            ddlStatusFilter.ClearSelection(); // Xóa lựa chọn cũ
            // *** SỬA LỖI CS0131 ***
            // Tìm item "-- Tất cả --" và chọn nó nếu tìm thấy
            ListItem defaultStatusItem = ddlStatusFilter.Items.FindByValue(string.Empty);
            if (defaultStatusItem != null)
            {
                defaultStatusItem.Selected = true;
            }
            // *** HẾT SỬA LỖI ***


            // Xóa các giá trị lọc đã lưu trong ViewState
            CurrentSearchTerm = string.Empty;
            CurrentStatusFilter = string.Empty;

            // Reset về trang đầu tiên
            GridViewSach.PageIndex = 0;

            // Tải lại dữ liệu GridView để hiển thị tất cả sách
            BindGrid();
        }

        /// <summary>
        /// Xử lý sự kiện thay đổi trang cho control GridViewSach.
        /// </summary>
        protected void GridViewSach_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            GridViewSach.PageIndex = e.NewPageIndex;
            BindGrid();
        }

        // --- Các hàm xử lý sự kiện RowCommand, RowDeleting, RowDataBound, GetStatusBadge, DeleteSach, TryDeleteFile, ShowMessage, HandleMessage, GetDeleteConfirmationScript, btnThemSachMoi_Click giữ nguyên như phiên bản trước ---
        // ... (Copy các hàm này từ phiên bản trước vào đây) ...
        protected void btnThemSachMoi_Click(object sender, EventArgs e)
        {
            Response.Redirect("ThemSach.aspx", false);
            Context.ApplicationInstance.CompleteRequest();
        }
        protected void GridViewSach_RowCommand(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName == "EditBook")
            {
                try
                {
                    int idSach = Convert.ToInt32(e.CommandArgument);
                    Response.Redirect($"SuaSach.aspx?id={idSach}", false);
                    Context.ApplicationInstance.CompleteRequest();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Lỗi RowCommand 'EditBook': {ex.Message}");
                    ShowMessage("Lỗi khi chuyển đến trang sửa sách.", isError: true);
                }
            }
        }
        protected void GridViewSach_RowDeleting(object sender, GridViewDeleteEventArgs e)
        {
            if (e.RowIndex >= 0 && GridViewSach.DataKeys != null && e.RowIndex < GridViewSach.DataKeys.Count)
            {
                try
                {
                    int idSach = Convert.ToInt32(GridViewSach.DataKeys[e.RowIndex].Value);
                    DeleteSach(idSach);
                    BindGrid();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Lỗi RowDeleting: {ex.ToString()}");
                    ShowMessage("Lỗi khi chuẩn bị xóa sách.", isError: true);
                    BindGrid();
                }
            }
            else
            {
                ShowMessage("Lỗi: Không thể xác định dòng sách cần xóa.", isError: true);
            }
        }
        protected void GridViewSach_RowDataBound(object sender, GridViewRowEventArgs e)
        {
            if (e.Row.RowType == DataControlRowType.DataRow)
            {
                LinkButton lnkXoa = (LinkButton)e.Row.FindControl("lnkXoa");
                DataRowView rowView = (DataRowView)e.Row.DataItem;

                if (lnkXoa != null && rowView != null)
                {
                    string tenSach = rowView["TenSach"]?.ToString() ?? "Không có tên";
                    string idSach = rowView["IDSach"]?.ToString() ?? "N/A";

                    // Dòng code này sẽ gán OnClientClick một cách chính xác
                    string tenSachEncoded = HttpUtility.JavaScriptStringEncode(tenSach);
                    lnkXoa.OnClientClick = $"showDeleteConfirmation('{idSach}', '{tenSachEncoded}', '{lnkXoa.UniqueID}'); return false;";
                }
            }
        }
        /*
                protected string GetDeleteConfirmationScript(object tenSachObject, object idSachObject)
                {
                    string tenSach = tenSachObject?.ToString() ?? "Không có tên";
                    string idSach = idSachObject?.ToString() ?? "N/A";
                    string encodedTenSach = HttpUtility.JavaScriptStringEncode(tenSach);
                    string message = $"CẢNH BÁO:\\n\\n" +
                                     $"Việc xóa sách \\\"{encodedTenSach}\\\" (ID: {idSach}) cũng sẽ xóa nó khỏi:\\n" +
                                     $"  - Tủ Sách của tất cả người dùng\\n" +
                                     $"  - Giỏ hàng của tất cả người dùng\\n" +
                                     $"  - Chi tiết đơn hàng liên quan\\n" +
                                     $"  - Bình luận (TuongTac) liên quan\\n" +
                                     $"  - Nội dung và Đánh giá sách (tự động bởi CSDL nếu có ON DELETE CASCADE)\\n\\n" +
                                     $"Bạn có chắc chắn muốn xóa không? Hành động này không thể hoàn tác.";
                    return $"return confirm('{message}');";
                }*/
        private void DeleteSach(int idSach)
        {
            string imagePathToDelete = null;
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                try
                {
                    con.Open();
                    string getPathQuery = "SELECT DuongDanBiaSach FROM Sach WHERE IDSach = @IDSach";
                    using (SqlCommand cmdGetPath = new SqlCommand(getPathQuery, con))
                    {
                        cmdGetPath.Parameters.AddWithValue("@IDSach", idSach);
                        object result = cmdGetPath.ExecuteScalar();
                        if (result != null && result != DBNull.Value && !string.IsNullOrWhiteSpace(result.ToString())) { imagePathToDelete = result.ToString(); }
                    }

                    using (SqlTransaction transaction = con.BeginTransaction())
                    {
                        try
                        {
                            string[] deleteQueries = {
                                 "DELETE FROM ChiTietDonHang WHERE IDSach = @IDSach",
                                 "DELETE FROM GioHang WHERE IDSach = @IDSach",
                                 "DELETE FROM TuongTac WHERE IDSach = @IDSach",
                                 "DELETE FROM TuSach WHERE IDSach = @IDSach",
                                 "DELETE FROM Sach WHERE IDSach = @IDSach"
                             };
                            int rowsAffected = 0;
                            foreach (string query in deleteQueries)
                            {
                                using (SqlCommand cmd = new SqlCommand(query, con, transaction))
                                {
                                    cmd.Parameters.AddWithValue("@IDSach", idSach);
                                    if (query.StartsWith("DELETE FROM Sach")) { rowsAffected = cmd.ExecuteNonQuery(); } else { cmd.ExecuteNonQuery(); }
                                }
                            }

                            if (rowsAffected > 0)
                            {
                                transaction.Commit();
                                ShowMessage("Xóa sách và tất cả dữ liệu liên quan thành công!", isError: false);
                                if (!string.IsNullOrWhiteSpace(imagePathToDelete)) { TryDeleteFile(imagePathToDelete); }
                            }
                            else
                            {
                                transaction.Rollback();
                                ShowMessage("Không tìm thấy sách để xóa (ID: " + idSach + ").", isError: true);
                            }
                        }
                        catch (SqlException sqlEx)
                        {
                            transaction.Rollback(); Debug.WriteLine($"SQL Error deleting: {sqlEx}");
                            ShowMessage(sqlEx.Number == 547 ? "Lỗi CSDL (Khóa ngoại): Không thể xóa sách vì còn dữ liệu liên quan." : $"Lỗi CSDL khi xóa: {sqlEx.Message}", isError: true);
                        }
                        catch (Exception ex)
                        {
                            transaction.Rollback(); Debug.WriteLine($"Error deleting: {ex}");
                            ShowMessage("Lỗi không xác định khi xóa: " + ex.Message, isError: true);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Outer Error deleting: {ex}");
                    ShowMessage("Lỗi hệ thống khi chuẩn bị xóa: " + ex.Message, isError: true);
                }
            }
        }
        private void TryDeleteFile(string relativePath)
        {
            if (string.IsNullOrWhiteSpace(relativePath)) return;
            string physicalPath = null;
            try
            {
                physicalPath = Server.MapPath(relativePath);
                if (File.Exists(physicalPath)) { File.Delete(physicalPath); Debug.WriteLine($"Deleted file: {physicalPath}"); }
                else { Debug.WriteLine($"File not found: {physicalPath}"); }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deleting file '{physicalPath}': {ex.Message}");
                if (ex is IOException || ex is UnauthorizedAccessException || ex is System.Security.SecurityException) { ShowMessage($"Lỗi khi xóa file ảnh: {ex.Message}", isError: true); }
            }
        }
        private void ShowMessage(string message, bool isError)
        {
            lblMessage.Text = HttpUtility.HtmlEncode(message);
            lblMessage.CssClass = isError
                ? "block mb-4 p-3 rounded-md border bg-red-50 border-red-300 text-red-700"
                : "block mb-4 p-3 rounded-md border bg-green-50 border-green-300 text-green-700";
            lblMessage.Visible = true;
        }
        protected string GetStatusBadge(object statusObj)
        {
            string status = statusObj?.ToString() ?? string.Empty;
            string badgeClass = "px-2 inline-flex text-xs leading-5 font-semibold rounded-full";
            string text = HttpUtility.HtmlEncode(status);
            switch (status.ToLowerInvariant().Trim())
            {
                case "hoàn thành":
                    badgeClass += " bg-green-100 text-green-800"; text = "Hoàn thành"; break;
                case "đang cập nhật":
                    badgeClass += " bg-yellow-100 text-yellow-800"; text = "Đang cập nhật"; break;
                case "tạm dừng":
                    badgeClass += " bg-red-100 text-red-800"; text = "Tạm dừng"; break;
                default:
                    badgeClass += " bg-gray-100 text-gray-800"; break;
            }
            return $"<span class='{badgeClass}'>{text}</span>";
        }
        private void HandleMessage(string messageKey)
        {
            switch (messageKey.ToLowerInvariant())
            {
                case "addsuccess": ShowMessage("Thêm sách mới thành công!", isError: false); break;
                case "editsuccess": ShowMessage("Cập nhật thông tin sách thành công!", isError: false); break;
                case "invalidid": ShowMessage("ID sách không hợp lệ hoặc không được cung cấp.", isError: true); break;
                case "notfound": ShowMessage("Không tìm thấy sách được yêu cầu.", isError: true); break;
                case "dberror": ShowMessage("Lỗi cơ sở dữ liệu khi thực hiện thao tác.", isError: true); break;
                case "saveerror": ShowMessage("Lỗi khi lưu thông tin sách.", isError: true); break;
                default: Debug.WriteLine($"Received unknown message key: {messageKey}"); break;
            }
        }

    }
}