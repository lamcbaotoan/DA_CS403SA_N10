using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data; // Required for CommandType
using System.Data.SqlClient;
using System.Globalization;
using System.Linq; // Required for LINQ operations like Select
using System.Web.Script.Serialization; // Required for JavaScriptSerializer

namespace Webebook.WebForm.Admin
{
    public partial class BaoCaoDoanhThu : System.Web.UI.Page
    {
        string connectionString = ConfigurationManager.ConnectionStrings["datawebebookConnectionString"].ConnectionString;

        // Add hidden fields to the ASPX page first (see step 2)
        // These will be populated here
        protected string ChartLabelsJson = "[]";
        protected string ChartDataJson = "[]";
        protected bool HasChartData = false; // Flag to control chart rendering

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Master is Admin master)
                {
                    master.SetPageTitle("Báo Cáo Doanh Thu");
                }
                // Set default dates
                txtTuNgay.Text = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1).ToString("yyyy-MM-dd");
                txtDenNgay.Text = DateTime.Now.ToString("yyyy-MM-dd");
                LoadRevenue(); // Load initial data and chart
            }
        }

        protected void btnXemBaoCao_Click(object sender, EventArgs e)
        {
            LoadRevenue(); // Reload data and chart on button click
        }

        private void LoadRevenue()
        {
            DateTime tuNgay, denNgay;
            lblMessage.Text = ""; // Clear previous messages
            lblMessage.CssClass = "block mt-4 px-4 py-2 rounded-md text-sm"; // Reset message style

            if (!DateTime.TryParseExact(txtTuNgay.Text, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out tuNgay))
            {
                ShowError("Định dạng 'Từ Ngày' không hợp lệ (yyyy-MM-dd).");
                return;
            }
            if (!DateTime.TryParseExact(txtDenNgay.Text, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out denNgay))
            {
                ShowError("Định dạng 'Đến Ngày' không hợp lệ (yyyy-MM-dd).");
                return;
            }

            // Important: Adjust denNgay to include the *entire* day for comparison
            DateTime denNgayEndOfDay = denNgay.AddDays(1).AddTicks(-1);

            if (tuNgay > denNgay) // Compare original dates for validation
            {
                ShowError("'Từ Ngày' không thể lớn hơn 'Đến Ngày'.");
                return;
            }

            decimal tongDoanhThu = 0;
            var dailyRevenueData = new Dictionary<DateTime, decimal>();

            using (SqlConnection con = new SqlConnection(connectionString))
            {
                try
                {
                    con.Open();

                    // --- 1. Get Total Revenue (Existing Query) ---
                    string queryTotal = @"SELECT ISNULL(SUM(SoTien), 0) FROM DonHang
                                          WHERE TrangThaiThanhToan = 'Completed'
                                          AND NgayDat >= @TuNgay AND NgayDat <= @DenNgay";
                    using (SqlCommand cmdTotal = new SqlCommand(queryTotal, con))
                    {
                        cmdTotal.Parameters.AddWithValue("@TuNgay", tuNgay);
                        // Use the adjusted end date for querying
                        cmdTotal.Parameters.AddWithValue("@DenNgay", denNgayEndOfDay);
                        tongDoanhThu = Convert.ToDecimal(cmdTotal.ExecuteScalar());
                    }

                    // --- 2. Get Daily Revenue for Chart ---
                    string queryDaily = @"SELECT CAST(NgayDat AS DATE) AS Ngay, SUM(SoTien) AS DoanhThuNgay
                                          FROM DonHang
                                          WHERE TrangThaiThanhToan = 'Completed'
                                            AND NgayDat >= @TuNgay AND NgayDat <= @DenNgay
                                          GROUP BY CAST(NgayDat AS DATE)
                                          ORDER BY Ngay;";
                    using (SqlCommand cmdDaily = new SqlCommand(queryDaily, con))
                    {
                        cmdDaily.Parameters.AddWithValue("@TuNgay", tuNgay);
                        cmdDaily.Parameters.AddWithValue("@DenNgay", denNgayEndOfDay); // Use adjusted end date

                        using (SqlDataReader reader = cmdDaily.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                // Add data directly returned from DB
                                dailyRevenueData.Add((DateTime)reader["Ngay"], (decimal)reader["DoanhThuNgay"]);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ShowError("Lỗi khi truy vấn dữ liệu doanh thu: " + ex.Message);
                    return; // Stop processing if DB error
                }
            } // Connection closed automatically

            // --- Update UI Labels ---
            lblTongDoanhThu.Text = tongDoanhThu.ToString("N0") + " VNĐ";
            // Display the user-selected range, not the adjusted one
            lblKhoangThoiGian.Text = $"Từ {tuNgay:dd/MM/yyyy} đến {denNgay:dd/MM/yyyy}";

            // --- Prepare Data for Chart ---
            var chartLabels = new List<string>();
            var chartData = new List<decimal>();
            HasChartData = false; // Reset flag

            if (dailyRevenueData.Any()) // Only generate chart data if there's something to show
            {
                // Ensure all days in the range are present, even with 0 revenue
                for (DateTime date = tuNgay; date <= denNgay; date = date.AddDays(1))
                {
                    chartLabels.Add(date.ToString("dd/MM")); // Format for label
                    decimal revenueForDay = 0;
                    if (dailyRevenueData.TryGetValue(date, out revenueForDay))
                    {
                        chartData.Add(revenueForDay);
                    }
                    else
                    {
                        chartData.Add(0); // Add 0 for days with no sales
                    }
                }

                if (chartLabels.Any()) // Check again after processing range
                {
                    var serializer = new JavaScriptSerializer();
                    ChartLabelsJson = serializer.Serialize(chartLabels);
                    ChartDataJson = serializer.Serialize(chartData);
                    HasChartData = true;
                }
            }
            else
            {
                // Ensure chart variables are empty if no data
                ChartLabelsJson = "[]";
                ChartDataJson = "[]";
                HasChartData = false;
                if (tongDoanhThu == 0)
                {
                    // Optionally show a message if no revenue at all in the period
                    lblMessage.Text = "Không có dữ liệu doanh thu trong khoảng thời gian đã chọn.";
                    lblMessage.CssClass = "block mt-4 px-4 py-2 rounded-md text-sm bg-yellow-100 text-yellow-700 border border-yellow-200"; // Use a neutral/info style
                }
            }
        }

        // Helper method to display errors consistently
        private void ShowError(string message)
        {
            lblMessage.Text = message;
            lblMessage.CssClass = "block mt-4 px-4 py-2 rounded-md text-sm bg-red-100 text-red-700 border border-red-200";
            // Clear chart data on error
            ChartLabelsJson = "[]";
            ChartDataJson = "[]";
            HasChartData = false;
            // Optionally reset total revenue display
            //lblTongDoanhThu.Text = "0 VNĐ";
            //lblKhoangThoiGian.Text = "Vui lòng chọn lại khoảng thời gian";
        }
    }
}