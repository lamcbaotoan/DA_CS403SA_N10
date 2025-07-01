<%@ Page Title="Báo Cáo Doanh Thu" Language="C#" MasterPageFile="~/WebForm/Admin/Admin.Master" AutoEventWireup="true" CodeBehind="BaoCaoDoanhThu.aspx.cs" Inherits="Webebook.WebForm.Admin.BaoCaoDoanhThu" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <%-- Font Awesome (if not global) --%>
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.0.0/css/all.min.css" />
    <%-- Chart.js CDN --%>
    <script src="https://cdn.jsdelivr.net/npm/chart.js@3.9.1/dist/chart.min.js"></script> <%-- Using v3 --%>
    <style>
        input[type="date"]::-webkit-calendar-picker-indicator { cursor: pointer; opacity: 0.6; }
        input[type="date"]::-webkit-calendar-picker-indicator:hover { opacity: 1; }
        /* Ensure canvas container has relative position if needed for tooltips */
        .chart-container { position: relative; /* height: 40vh; width: 80vw; */ /* Adjust size as needed */ }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="container mx-auto px-4 py-8">
        <h1 class="text-3xl font-bold text-gray-800 mb-6 border-l-4 border-blue-500 pl-4">Báo Cáo Doanh Thu</h1>

        <div class="bg-white p-6 rounded-xl shadow-lg mb-8">
             <h2 class="text-xl font-semibold text-gray-700 mb-5">Chọn Khoảng Thời Gian</h2>
            <div class="flex flex-wrap items-end gap-4 md:gap-6">
                <%-- From Date --%>
                <div class="flex-grow">
                    <label for="<%= txtTuNgay.ClientID %>" class="block text-sm font-medium text-gray-600 mb-1">Từ Ngày:</label>
                    <asp:TextBox ID="txtTuNgay" runat="server" TextMode="Date" CssClass="w-full px-4 py-2 border border-gray-300 rounded-lg shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition duration-150 ease-in-out"></asp:TextBox>
                </div>
                <%-- To Date --%>
                <div class="flex-grow">
                    <label for="<%= txtDenNgay.ClientID %>" class="block text-sm font-medium text-gray-600 mb-1">Đến Ngày:</label>
                    <asp:TextBox ID="txtDenNgay" runat="server" TextMode="Date" CssClass="w-full px-4 py-2 border border-gray-300 rounded-lg shadow-sm focus:outline-none focus:ring-2 focus:ring-blue-500 focus:border-transparent transition duration-150 ease-in-out"></asp:TextBox>
                </div>
                 <%-- Submit Button --%>
                <div class="pt-1">
                    <asp:Button ID="btnXemBaoCao" runat="server" Text="Xem Báo Cáo"
                        CssClass="inline-flex items-center bg-blue-600 hover:bg-blue-700 text-white font-semibold py-2 px-5 rounded-lg shadow-md hover:shadow-lg focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 transition duration-150 ease-in-out"
                        OnClick="btnXemBaoCao_Click" />
                </div>
            </div>
             <asp:Label ID="lblMessage" runat="server" CssClass="block mt-4 px-4 py-2 rounded-md text-sm" EnableViewState="false"></asp:Label>
        </div>


        <div class="bg-gradient-to-r from-green-50 to-emerald-50 p-6 rounded-xl shadow-lg border border-green-200 mb-8">
             <div class="flex items-center justify-between mb-4">
                <h2 class="text-xl font-semibold text-gray-800">
                    <i class="fas fa-coins mr-2 text-green-600"></i>Tổng Doanh Thu
                </h2>
                 <span class="text-xs font-medium bg-green-100 text-green-700 px-2 py-1 rounded-full">Đơn Hàng Hoàn Thành</span>
            </div>
            <div class="text-center md:text-left">
                <p class="text-4xl lg:text-5xl font-bold text-green-700 mb-2">
                    <asp:Label ID="lblTongDoanhThu" runat="server" Text="0 VNĐ"></asp:Label>
                </p>
                <p class="text-sm text-gray-600">
                    <asp:Label ID="lblKhoangThoiGian" runat="server" Text="Vui lòng chọn khoảng thời gian"></asp:Label>
                </p>
            </div>
        </div>

        <% if (HasChartData) { %> <%-- Only render this section if there's chart data --%>
         <div class="bg-white p-6 rounded-xl shadow-lg mb-8">
             <h2 class="text-xl font-semibold text-gray-700 mb-5">
                 <i class="fas fa-chart-line mr-2 text-blue-600"></i>Biểu Đồ Doanh Thu Hàng Ngày
             </h2>
             <div class="chart-container" style="height:300px;"> <%-- Set height for canvas container --%>
                 <canvas id="revenueChartCanvas"></canvas>
             </div>
         </div>
         <% } %> <%-- End conditional rendering --%>

    </div> <%-- End Container --%>

    <%-- JavaScript for Chart Initialization --%>
    <%-- IMPORTANT: Place this script *after* the canvas element --%>
    <% if (HasChartData) { %> <%-- Only run script if there's data --%>
    <script>
        // Ensure script runs after DOM is ready (though placement often suffices in simple pages)
        document.addEventListener('DOMContentLoaded', function () {
            const ctx = document.getElementById('revenueChartCanvas')?.getContext('2d'); // Use optional chaining for safety

            if (!ctx) {
                console.error("Chart canvas element not found!");
                return;
            }

            // --- Get Data from Code-Behind (Rendered Inline) ---
            // Use <\%= ... %> for direct output. Use html decoding if needed, but JSON should be safe.
            const chartLabels = JSON.parse('<%= ChartLabelsJson %>');
            const chartData = JSON.parse('<%= ChartDataJson %>');

            // --- Destroy existing chart instance if it exists (for updates on postback) ---
            // Chart.js v3+ stores instances differently. A simple way is to check a global var or canvas attribute.
             let existingChart = Chart.getChart('revenueChartCanvas'); // Check if a chart instance exists for this canvas
             if (existingChart) {
                 existingChart.destroy(); // Destroy it before creating a new one
             }


            // --- Create the Chart ---
            const revenueChart = new Chart(ctx, {
                type: 'line', // Type of chart
                data: {
                    labels: chartLabels, // Labels for X-axis (dates)
                    datasets: [{
                        label: 'Doanh Thu Hàng Ngày (VNĐ)', // Legend label
                        data: chartData, // Data points for Y-axis
                        borderColor: 'rgb(22, 163, 74)', // Green line color (Tailwind green-600)
                        backgroundColor: 'rgba(22, 163, 74, 0.1)', // Light green fill under the line
                        borderWidth: 2, // Line thickness
                        fill: true, // Fill area under line
                        tension: 0.1 // Slight curve to the line
                    }]
                },
                options: {
                    responsive: true, // Make chart responsive
                    maintainAspectRatio: false, // Allow chart to fill container height
                    scales: {
                        y: {
                            beginAtZero: true, // Start Y-axis at 0
                            ticks: {
                                // Format Y-axis labels as currency
                                callback: function(value, index, values) {
                                    return value.toLocaleString('vi-VN') + ' VNĐ';
                                }
                            }
                        },
                        x: {
                             // Optional: Add title to X-axis if needed
                             // title: { display: true, text: 'Ngày' }
                        }
                    },
                    plugins: {
                        tooltip: {
                            callbacks: {
                                // Format tooltip value as currency
                                label: function(context) {
                                    let label = context.dataset.label || '';
                                    if (label) {
                                        label += ': ';
                                    }
                                    if (context.parsed.y !== null) {
                                        label += context.parsed.y.toLocaleString('vi-VN') + ' VNĐ';
                                    }
                                    return label;
                                }
                            }
                        },
                         legend: {
                            display: true // Show legend
                        }
                    }
                }
            });
        });
    </script>
    <% } %> <%-- End conditional script --%>

</asp:Content>