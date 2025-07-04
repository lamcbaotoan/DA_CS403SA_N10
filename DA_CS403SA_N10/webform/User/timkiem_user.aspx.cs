using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Diagnostics; // Cho Debug.WriteLine

namespace Webebook.WebForm.User
{
    // KHÔNG ĐỊNH NGHĨA LẠI ENUM Ở ĐÂY

    public partial class timkiem_user : System.Web.UI.Page
    {
        string connectionString = ConfigurationManager.ConnectionStrings["datawebebookConnectionString"].ConnectionString;
        int userId = 0;

        protected void Page_Load(object sender, EventArgs e)
        {
            // Kiểm tra đăng nhập trước
            if (Session["UserID"] == null || !int.TryParse(Session["UserID"].ToString(), out userId) || userId <= 0)
            {
                Response.Redirect("~/WebForm/VangLai/dangnhap.aspx?message=notloggedin", true);
                return;
            }

            if (!IsPostBack)
            {
                LoadSearchResults();
            }
            // Ẩn message nếu không phải postback từ nút Thêm giỏ
            if (!IsPostBack || !(ScriptManager.GetCurrent(Page)?.AsyncPostBackSourceElementID?.Contains("btnAddToCart") ?? false))
            {
                lblMessage.Visible = false;
            }
        }

        private void LoadSearchResults()
        {
            string keyword = Request.QueryString["q"];
            litKeyword.Text = HttpUtility.HtmlEncode(keyword);
            if (pnlNoResults.FindControl("litNoResultKeyword") is Literal litNRK)
            {
                litNRK.Text = HttpUtility.HtmlEncode(keyword); // Hiển thị keyword trong thông báo no result
            }
            pnlNoResults.Visible = false;

            if (string.IsNullOrWhiteSpace(keyword)) { ShowMessage("Vui lòng nhập từ khóa tìm kiếm.", true, true); rptKetQuaUser.DataSource = null; rptKetQuaUser.DataBind(); pnlNoResults.Visible = true; return; }

            DataTable dt = new DataTable();
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                string query = @"SELECT IDSach, TenSach, TacGia, GiaSach, DuongDanBiaSach FROM Sach WHERE TenSach LIKE @Keyword OR TacGia LIKE @Keyword OR MoTa LIKE @Keyword OR TheLoaiChuoi LIKE @Keyword OR LoaiSach LIKE @Keyword ORDER BY CASE WHEN TenSach LIKE @ExactKeyword THEN 0 ELSE 1 END, TenSach";
                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Keyword", "%" + keyword + "%");
                    cmd.Parameters.AddWithValue("@ExactKeyword", keyword);
                    try { con.Open(); SqlDataAdapter da = new SqlDataAdapter(cmd); da.Fill(dt); rptKetQuaUser.DataSource = dt; rptKetQuaUser.DataBind(); bool hasData = dt.Rows.Count > 0; pnlNoResults.Visible = !hasData; if (hasData && IsPostBack) { ScriptManager.RegisterStartupScript(this, GetType(), "InitFadeIn", "setTimeout(initializeCardFadeInSearch, 100);", true); } }
                    catch (Exception ex) { ShowMessage("Lỗi khi tìm kiếm: " + ex.Message, true); LogError($"Lỗi LoadSearchResults (User): {ex.ToString()}"); rptKetQuaUser.DataSource = null; rptKetQuaUser.DataBind(); pnlNoResults.Visible = true; }
                }
            }
        }

        // Sử dụng Repeater's ItemCommand event
        protected void rptKetQuaUser_ItemCommand(object source, RepeaterCommandEventArgs e)
        {
            if (e.CommandName == "AddToCart")
            {
                if (userId <= 0) { Response.Redirect("~/WebForm/VangLai/dangnhap.aspx?message=sessionexpired&returnUrl=" + HttpUtility.UrlEncode(Request.Url.PathAndQuery), true); return; }
                try
                {
                    int idSach = Convert.ToInt32(e.CommandArgument);
                    HiddenField hfBookName = (HiddenField)e.Item.FindControl("hfBookName");
                    string bookName = hfBookName != null && !string.IsNullOrEmpty(hfBookName.Value) ? hfBookName.Value : GetBookName(idSach);

                    CartAddResultd result = AddToCart(userId, idSach); // Sử dụng Enum gốc

                    switch (result)
                    {
                        case CartAddResultd.Success: ShowMessage($"Đã thêm '{HttpUtility.HtmlEncode(bookName)}' vào giỏ hàng!", false); if (Master is UserMaster master) { master.UpdateCartCount(); } break;
                        case CartAddResultd.AlreadyExists: string encodedBookNameJs = HttpUtility.JavaScriptStringEncode(bookName); string script = $"showAlreadyInCartPopup('{encodedBookNameJs}');"; ScriptManager.RegisterStartupScript(this, this.GetType(), $"ShowAlreadyInCartPopup_{idSach}", script, true); break;
                        case CartAddResultd.Error: break;
                    }
                }
                catch (FormatException) { ShowMessage("Lỗi: ID sách không hợp lệ.", true); LogError("Lỗi FormatException trong rptKetQuaUser_ItemCommand, ID: " + e.CommandArgument?.ToString()); }
                catch (Exception ex) { LogError($"Lỗi rptKetQuaUser_ItemCommand: {ex.ToString()}"); ShowMessage("Lỗi khi thêm vào giỏ hàng.", true); }
            }
        }

        // --- HÀM HELPER ĐẦY ĐỦ ---
        private CartAddResultd AddToCart(int currentUserId, int idSach) { using (SqlConnection con = new SqlConnection(connectionString)) { string checkQuery = "SELECT COUNT(*) FROM GioHang WHERE IDNguoiDung = @UserId AND IDSach = @IDSach"; try { con.Open(); using (SqlCommand checkCmd = new SqlCommand(checkQuery, con)) { checkCmd.Parameters.AddWithValue("@UserId", currentUserId); checkCmd.Parameters.AddWithValue("@IDSach", idSach); int existingCount = (int)checkCmd.ExecuteScalar(); if (existingCount > 0) { return CartAddResultd.AlreadyExists; } } string insertQuery = "INSERT INTO GioHang (IDNguoiDung, IDSach, SoLuong) VALUES (@UserId, @IDSach, 1)"; using (SqlCommand insertCmd = new SqlCommand(insertQuery, con)) { insertCmd.Parameters.AddWithValue("@UserId", currentUserId); insertCmd.Parameters.AddWithValue("@IDSach", idSach); int rowsAffected = insertCmd.ExecuteNonQuery(); if (rowsAffected > 0) { return CartAddResultd.Success; } else { ShowMessage("Không thể thêm sách vào giỏ hàng.", true); return CartAddResultd.Error; } } } catch (SqlException sqlEx) { ShowMessage("Lỗi cơ sở dữ liệu khi thao tác với giỏ hàng.", true); LogError($"SQL Lỗi AddToCart User {currentUserId}, Sach {idSach} (Search Page): {sqlEx}"); return CartAddResultd.Error; } catch (Exception ex) { ShowMessage("Lỗi khi thêm vào giỏ hàng.", true); LogError($"Lỗi AddToCart User {currentUserId}, Sach {idSach} (Search Page): {ex}"); return CartAddResultd.Error; } } }
        private string GetBookName(int idSach) { string bookName = "Sách này"; using (SqlConnection con = new SqlConnection(connectionString)) { string query = "SELECT TenSach FROM Sach WHERE IDSach = @IDSach"; using (SqlCommand cmd = new SqlCommand(query, con)) { cmd.Parameters.AddWithValue("@IDSach", idSach); try { con.Open(); object result = cmd.ExecuteScalar(); if (result != null && result != DBNull.Value) { bookName = result.ToString(); } } catch (Exception ex) { LogError($"Lỗi GetBookName ({idSach}) (Search Page): {ex.Message}"); } } } return bookName; }
        private void ShowMessage(string message, bool isError, bool useYellow = false) { if (lblMessage == null) return; lblMessage.Text = HttpUtility.HtmlEncode(message); string cssClass = "block w-full p-4 mb-6 text-sm rounded-lg border "; if (isError) { cssClass += useYellow ? "bg-yellow-50 border-yellow-300 text-yellow-800" : "bg-red-50 border-red-300 text-red-800"; } else { cssClass += "bg-green-50 border-green-300 text-green-800"; } lblMessage.CssClass = cssClass; lblMessage.Visible = true; }
        protected string GetImageUrl(object pathData) { string placeholder = ResolveUrl("~/Images/placeholder_cover.png"); if (pathData != DBNull.Value && !string.IsNullOrEmpty(pathData?.ToString())) { string path = pathData.ToString(); if (path.StartsWith("~") || path.StartsWith("/")) { try { return ResolveUrl(path); } catch { return placeholder; } } else if (Uri.IsWellFormedUriString(path, UriKind.Absolute)) { return path; } } return placeholder; }
        private void LogError(string errorMessage) { Debug.WriteLine($"[UserSearch][{DateTime.Now:HH:mm:ss.fff}] {errorMessage}"); }
    }
}