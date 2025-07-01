using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections.Generic; // Required for List
using System.Linq; // Required for LINQ methods like ToList, Take
using Newtonsoft.Json; // Required for JSON serialization (Install Newtonsoft.Json via NuGet)


namespace Webebook.WebForm.Admin
{
    public partial class ThongKeSach : System.Web.UI.Page
    {
        string connectionString = ConfigurationManager.ConnectionStrings["datawebebookConnectionString"].ConnectionString;

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                if (Master is Admin master)
                {
                    master.SetPageTitle("Thống Kê Sách Bán Chạy");
                }
                BindData(); // Renamed BindGrid to BindData as it now binds more than the grid
            }
        }

        private void BindData()
        {
            DataTable dt = GetStatisticsData(); // Fetch data once

            if (dt != null && dt.Rows.Count > 0)
            {
                // Bind GridView
                GridViewThongKe.DataSource = dt;
                GridViewThongKe.DataBind();

                // Prepare and Bind Chart Data (Top 5 for charts)
                PrepareChartData(dt, 5); // Fetch Top 5 books for charts
            }
            else
            {
                // Handle case with no data
                GridViewThongKe.DataSource = null;
                GridViewThongKe.DataBind(); // This will show the EmptyDataText
                lblMessage.Text = "Chưa có dữ liệu thống kê để hiển thị.";
                lblMessage.CssClass = "block mb-4 text-center text-yellow-600 font-semibold";
            }
        }

        private DataTable GetStatisticsData()
        {
            DataTable dt = new DataTable();
            using (SqlConnection con = new SqlConnection(connectionString))
            {
                // Fetch Top 100 or more if needed, ordering is important
                string query = @"SELECT
                                    s.IDSach,
                                    s.TenSach,
                                    s.TacGia,
                                    s.DuongDanBiaSach,
                                    SUM(ctdh.SoLuong) AS TongSoLuongBan,
                                    SUM(ctdh.SoLuong * ctdh.Gia) AS TongDoanhThu
                                FROM Sach s
                                JOIN ChiTietDonHang ctdh ON s.IDSach = ctdh.IDSach
                                JOIN DonHang dh ON ctdh.IDDonHang = dh.IDDonHang
                                WHERE dh.TrangThaiThanhToan = 'Completed' /*Ensure only completed orders are counted [cite: 4, 19, 20, 21, 22, 23, 24, 25, 26, 27, 28, 29, 30, 31, 32, 33, 34] */
                                GROUP BY s.IDSach, s.TenSach, s.TacGia, s.DuongDanBiaSach
                                ORDER BY TongSoLuongBan DESC, TongDoanhThu DESC"; // Order by Sales first, then Revenue

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    try
                    {
                        con.Open();
                        SqlDataAdapter da = new SqlDataAdapter(cmd);
                        da.Fill(dt);
                    }
                    catch (Exception ex)
                    {
                        lblMessage.Text = "Lỗi tải dữ liệu thống kê: " + ex.Message;
                        lblMessage.CssClass = "block mb-4 text-center text-red-600 font-semibold";
                        // Optionally log the full exception details
                        return null; // Return null or empty DataTable on error
                    }
                }
            }
            return dt;
        }

        private void PrepareChartData(DataTable dt, int topN)
        {
            // Take only the top N rows from the already sorted DataTable
            var topBooks = dt.AsEnumerable().Take(topN).ToList();

            if (topBooks.Any())
            {
                // Data for Sales Quantity Chart (Bar Chart)
                List<string> labels = topBooks.Select(row => TruncateString(row.Field<string>("TenSach"), 30)).ToList(); // Truncate long names
                List<decimal> salesData = topBooks.Select(row => Convert.ToDecimal(row["TongSoLuongBan"])).ToList(); // Use Convert for safety

                // Data for Revenue Chart (Pie or Doughnut Chart)
                // Re-order by revenue for the revenue chart if desired, or use the same order
                var topBooksByRevenue = dt.AsEnumerable()
                                        .OrderByDescending(row => Convert.ToDecimal(row["TongDoanhThu"]))
                                        .Take(topN)
                                        .ToList();
                List<string> revenueLabels = topBooksByRevenue.Select(row => TruncateString(row.Field<string>("TenSach"), 30)).ToList();
                List<decimal> revenueData = topBooksByRevenue.Select(row => Convert.ToDecimal(row["TongDoanhThu"])).ToList();


                // Store data in hidden fields for JavaScript access
                hfChartLabels.Value = JsonConvert.SerializeObject(labels);
                hfSalesData.Value = JsonConvert.SerializeObject(salesData);
                // Use separate labels and data for the revenue chart
                // hfRevenueLabels.Value = JsonConvert.SerializeObject(revenueLabels); // Need another hidden field if labels differ
                // Using same labels for simplicity here, adjust if needed
                hfRevenueData.Value = JsonConvert.SerializeObject(revenueData);

                // Register the JavaScript to render the charts
                RegisterChartScript();
            }
        }

        // Helper function to truncate strings for chart labels
        private string TruncateString(string value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return value;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength) + "...";
        }


        private void RegisterChartScript()
        {
            // Use double curly braces {{ }} to escape JavaScript braces within the C# interpolated string
            string script = $@"
        <script type='text/javascript'>
            document.addEventListener('DOMContentLoaded', function() {{
                function getRandomColor() {{
                    var r = Math.floor(Math.random() * 200);
                    var g = Math.floor(Math.random() * 200);
                    var b = Math.floor(Math.random() * 200);
                    // Correctly escaped string interpolation for CSS rgba function
                    return `rgba(${{r}}, ${{g}}, ${{b}}, 0.7)`;
                }}

                function generateColors(count) {{
                    let colors = [];
                    for(let i = 0; i < count; i++) {{
                        colors.push(getRandomColor());
                    }}
                    return colors;
                }}

                const chartLabels = JSON.parse(document.getElementById('{hfChartLabels.ClientID}').value || '[]');
                const salesData = JSON.parse(document.getElementById('{hfSalesData.ClientID}').value || '[]');
                const revenueData = JSON.parse(document.getElementById('{hfRevenueData.ClientID}').value || '[]');
                const revenueLabels = chartLabels; // Assuming same labels

                // --- Sales Quantity Chart (Bar) ---
                const salesCtx = document.getElementById('salesQuantityChart');
                if (salesCtx && chartLabels.length > 0 && salesData.length > 0) {{
                    const salesChart = new Chart(salesCtx.getContext('2d'), {{
                        type: 'bar',
                        data: {{
                            labels: chartLabels,
                            datasets: [{{ // Escaped braces for JS object literal
                                label: 'Số Lượng Bán',
                                data: salesData,
                                backgroundColor: generateColors(chartLabels.length),
                                borderColor: generateColors(chartLabels.length).map(c => c.replace('0.7', '1')),
                                borderWidth: 1
                            }}] // Escaped braces
                        }},
                        options: {{ // Escaped braces
                            responsive: true,
                            maintainAspectRatio: false,
                            scales: {{ // Escaped braces
                                y: {{ // Escaped braces
                                    beginAtZero: true,
                                    ticks: {{ precision: 0 }} // Escaped braces
                                }}
                            }},
                            plugins: {{ // Escaped braces
                                legend: {{ display: false }}, // Escaped braces
                                tooltip: {{ // Escaped braces
                                    callbacks: {{ // Escaped braces
                                        label: function(context) {{
                                            let label = context.dataset.label || '';
                                            if (label) {{ label += ': '; }}
                                            if (context.parsed.y !== null) {{ label += context.parsed.y; }}
                                            return label;
                                        }}
                                    }}
                                }}
                            }}
                        }}
                    }});
                }} else {{
                    console.error('Sales chart canvas or data not found/empty.');
                }}

                // --- Revenue Chart (Doughnut) ---
                const revenueCtx = document.getElementById('revenueChart');
                if (revenueCtx && revenueLabels.length > 0 && revenueData.length > 0) {{
                    const revenueChart = new Chart(revenueCtx.getContext('2d'), {{
                        type: 'doughnut',
                        data: {{ // Escaped braces
                            labels: revenueLabels,
                            datasets: [{{ // Escaped braces
                                label: 'Doanh Thu (VNĐ)',
                                data: revenueData,
                                backgroundColor: generateColors(revenueLabels.length),
                                hoverOffset: 4
                            }}] // Escaped braces
                        }},
                        options: {{ // Escaped braces
                            responsive: true,
                            maintainAspectRatio: false,
                            plugins: {{ // Escaped braces
                                legend: {{ position: 'top' }}, // Escaped braces
                                tooltip: {{ // Escaped braces
                                    callbacks: {{ // Escaped braces
                                        label: function(context) {{
                                            let label = context.label || '';
                                            if (label) {{ label += ': '; }}
                                            if (context.parsed !== null) {{
                                                label += new Intl.NumberFormat('vi-VN', {{ style: 'currency', currency: 'VND' }}).format(context.parsed);
                                            }}
                                            return label;
                                        }}
                                    }}
                                }}
                            }}
                        }}
                    }});
                }} else {{
                     console.error('Revenue chart canvas or data not found/empty.');
                }}
            }});
        </script>";

            ScriptManager.RegisterStartupScript(this, this.GetType(), "ChartScript", script, false);
        }


        protected void GridViewThongKe_PageIndexChanging(object sender, GridViewPageEventArgs e)
        {
            GridViewThongKe.PageIndex = e.NewPageIndex;
            BindData(); // Re-bind data for the new page
        }
    }
}