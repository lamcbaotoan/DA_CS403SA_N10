    using System;
    using System.Configuration;
    using System.Data.SqlClient;
    using System.Globalization;
    using System.Collections.Generic;
    using System.Text.Json; // Hoặc dùng Newtonsoft.Json

    namespace Webebook.WebForm.Admin
    {
        // Lớp ChartData giữ nguyên
        public class ChartData
        {
            public List<string> Labels { get; set; } = new List<string>();
            // Dùng decimal hoặc int đều được cho số lượng, decimal linh hoạt hơn
            public List<decimal> Data { get; set; } = new List<decimal>();
        }

        public partial class adminhome : System.Web.UI.Page
        {
            string connectionString = ConfigurationManager.ConnectionStrings["datawebebookConnectionString"].ConnectionString;

            public string RevenueChartJson { get; private set; } = "{}";
            // UserChartJson giờ sẽ chứa dữ liệu Trạng thái Đơn hàng
            public string UserChartJson { get; private set; } = "{}";

            protected void Page_Load(object sender, EventArgs e)
            {
                if (!IsPostBack)
                {
                    if (Master is Admin master)
                    {
                        master.SetPageTitle("Bảng Điều Khiển");
                    }
                    SetWelcomeMessage();
                    LoadDashboardStats();
                    LoadChartData(); // Gọi hàm tải dữ liệu biểu đồ
                }
            }

            // --- Giữ nguyên SetWelcomeMessage và LoadDashboardStats ---
            private void SetWelcomeMessage()
            {
                // ... (code giữ nguyên) ...
                int currentHour = DateTime.Now.Hour;
                string greeting;

                if (currentHour >= 5 && currentHour < 11) greeting = "Chào buổi sáng";
                else if (currentHour >= 11 && currentHour < 14) greeting = "Chào buổi trưa";
                else if (currentHour >= 14 && currentHour < 18) greeting = "Chào buổi chiều";
                else greeting = "Chào buổi tối";

                // Lấy tên Admin - Thay thế bằng logic lấy tên thực tế (ví dụ: từ Session, Identity)
                string adminName = "Admin"; // << THAY THẾ CHỖ NÀY
                if (Session["AdminUsername"] != null) // Ví dụ lấy từ Session
                {
                    adminName = Session["AdminUsername"].ToString();
                }

                litWelcomeMessage.Text = $"{greeting}, {adminName}!";
            }

            private void LoadDashboardStats()
            {
                // ... (code giữ nguyên) ...
                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    try
                    {
                        con.Open();
                        // Get Total Books
                        using (SqlCommand cmdSach = new SqlCommand("SELECT COUNT(*) FROM Sach", con))
                        {
                            lblTongSach.Text = cmdSach.ExecuteScalar()?.ToString() ?? "0";
                        }
                        // Get New Orders (Pending or Placed Today)
                        // CẬP NHẬT: Có thể bạn muốn định nghĩa "Đơn hàng mới" chính xác hơn
                        // Ví dụ: Chỉ tính 'Pending', hoặc đơn hàng trong 24h qua?
                        using (SqlCommand cmdDonHang = new SqlCommand("SELECT COUNT(*) FROM DonHang WHERE TrangThaiThanhToan = 'Pending' OR CONVERT(date, NgayDat) = CONVERT(date, GETDATE())", con))
                        {
                            lblDonHangMoi.Text = cmdDonHang.ExecuteScalar()?.ToString() ?? "0";
                        }
                        // Get Total Users
                        using (SqlCommand cmdNguoiDung = new SqlCommand("SELECT COUNT(*) FROM NguoiDung WHERE IDNguoiDung > 0", con)) // Giả sử có bảng NguoiDung
                        {
                            lblNguoiDung.Text = cmdNguoiDung.ExecuteScalar()?.ToString() ?? "0";
                        }
                        // Get Monthly Revenue (Completed Orders This Month)
                        using (SqlCommand cmdDoanhThu = new SqlCommand("SELECT ISNULL(SUM(SoTien), 0) FROM DonHang WHERE TrangThaiThanhToan = 'Completed' AND MONTH(NgayDat) = MONTH(GETDATE()) AND YEAR(NgayDat) = YEAR(GETDATE())", con))
                        {
                            decimal doanhThu = Convert.ToDecimal(cmdDoanhThu.ExecuteScalar() ?? 0);
                            lblDoanhThuThang.Text = doanhThu.ToString("N0", CultureInfo.GetCultureInfo("vi-VN")) + " VNĐ";
                        }
                    }
                    catch (Exception ex)
                    {
                        LogError(ex, "Error loading dashboard stats.");
                        ShowError("Không thể tải dữ liệu thống kê thẻ.");
                        lblTongSach.Text = "Lỗi";
                        lblDonHangMoi.Text = "Lỗi";
                        lblNguoiDung.Text = "Lỗi";
                        lblDoanhThuThang.Text = "Lỗi";
                    }
                }
            }

            private void LoadChartData()
            {
                RevenueChartJson = GetRevenueChartData();
                // Gọi hàm lấy dữ liệu trạng thái đơn hàng cho UserChartJson
                UserChartJson = GetOrderStatusChartData();
            }

            // --- Giữ nguyên GetRevenueChartData ---
            private string GetRevenueChartData()
            {
                var chartData = new ChartData();
                string query = @"
                    SELECT
                        FORMAT(d.MonthStart, 'MM/yyyy') AS MonthLabel,
                        ISNULL(SUM(dh.SoTien), 0) AS MonthlyRevenue
                    FROM
                        (VALUES
                            (DATEADD(month, -5, GETDATE())),
                            (DATEADD(month, -4, GETDATE())),
                            (DATEADD(month, -3, GETDATE())),
                            (DATEADD(month, -2, GETDATE())),
                            (DATEADD(month, -1, GETDATE())),
                            (GETDATE())
                        ) AS Months(MonthDate)
                    CROSS APPLY
                        (SELECT DATEADD(day, 1 - DAY(MonthDate), CAST(MonthDate AS DATE)) AS MonthStart) AS d
                    LEFT JOIN
                        DonHang dh ON dh.TrangThaiThanhToan = 'Completed'
                                   AND YEAR(dh.NgayDat) = YEAR(d.MonthStart)
                                   AND MONTH(dh.NgayDat) = MONTH(d.MonthStart)
                    GROUP BY
                        FORMAT(d.MonthStart, 'MM/yyyy'), d.MonthStart
                    ORDER BY
                        d.MonthStart;";

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        try
                        {
                            con.Open();
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    chartData.Labels.Add(reader["MonthLabel"].ToString());
                                    chartData.Data.Add(Convert.ToDecimal(reader["MonthlyRevenue"]));
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            LogError(ex, "Error loading revenue chart data.");
                            ShowError("Không thể tải dữ liệu biểu đồ doanh thu.");
                            return "{}";
                        }
                    }
                }
                return JsonSerializer.Serialize(chartData);
            }


            // *** SỬA HÀM NÀY ***
            // Hàm lấy dữ liệu Phân bổ Trạng thái Đơn hàng
            private string GetOrderStatusChartData() // Đổi tên hàm cho rõ ràng (hoặc giữ nguyên GetUserChartData)
            {
                var chartData = new ChartData();
                // Lấy số lượng đơn hàng theo từng trạng thái
                string query = @"
                    SELECT
                        ISNULL(TrangThaiThanhToan, 'Unknown') AS OrderStatus, -- Đặt tên trạng thái nếu bị NULL
                        COUNT(*) AS StatusCount
                    FROM DonHang
                    GROUP BY TrangThaiThanhToan
                    ORDER BY StatusCount DESC; -- Sắp xếp tùy chọn";

                using (SqlConnection con = new SqlConnection(connectionString))
                {
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        try
                        {
                            con.Open();
                            using (SqlDataReader reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    chartData.Labels.Add(reader["OrderStatus"].ToString());
                                    // Dùng Convert.ToDecimal hoặc Convert.ToInt32 đều được
                                    chartData.Data.Add(Convert.ToDecimal(reader["StatusCount"]));
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            LogError(ex, "Error loading order status chart data.");
                            ShowError("Không thể tải dữ liệu biểu đồ trạng thái đơn hàng.");
                            return "{}"; // Trả về JSON trống nếu có lỗi
                        }
                    }
                }
                // Serialize sang JSON
                return JsonSerializer.Serialize(chartData);
            }

            // --- Giữ nguyên LogError và ShowError ---
            private void LogError(Exception ex, string context)
            {
                System.Diagnostics.Trace.WriteLine($"ERROR in {context}: {ex.Message}");
            }

            private void ShowError(string message)
            {
                // Cân nhắc dùng cách khác thay vì alert
                // ClientScript.RegisterStartupScript(this.GetType(), "alert", $"alert('{message.Replace("'", "\\'")}');", true);
            }

        }
    }