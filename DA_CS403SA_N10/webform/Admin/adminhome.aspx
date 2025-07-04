<%@ Page Title="Bảng Điều Khiển" Language="C#" MasterPageFile="~/WebForm/Admin/Admin.Master" AutoEventWireup="true" CodeBehind="adminhome.aspx.cs" Inherits="Webebook.WebForm.Admin.adminhome" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <%-- Liên kết Chart.js (sử dụng CDN) --%>
    <script src="https://cdn.jsdelivr.net/npm/chart.js"></script>
    <%-- Đảm bảo Font Awesome đã được liên kết trong Admin.Master --%>
    <style>
        .transition-all-ease {
            transition: all 0.3s ease-in-out;
        }
        .chart-container {
            position: relative;
            /* height: 300px; */ /* Hoặc chiều cao mong muốn */
            width: 100%;
            background-color: white;
            padding: 1rem;
            border-radius: 0.5rem;
            box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -1px rgba(0, 0, 0, 0.06);
        }
        /* Tùy chỉnh thêm nếu cần */
        .stat-card-icon {
            width: 3.5rem; /* Điều chỉnh kích thước khu vực icon nếu cần */
            height: 3.5rem;
            display: flex;
            align-items: center;
            justify-content: center;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="container mx-auto px-4 py-8">

        <%-- Khu vực Chào mừng, Đồng hồ và Ngày --%>
        <div class="mb-8 p-6 bg-white rounded-lg shadow-md flex flex-col sm:flex-row justify-between items-center">
            <div>
                <h2 class="text-2xl font-semibold text-gray-800">
                    <asp:Literal ID="litWelcomeMessage" runat="server"></asp:Literal>
                </h2>
                <p id="currentDate" class="text-gray-600"></p> <%-- Ngày sẽ được cập nhật bằng JS --%>
            </div>
            <div id="liveClock" class="text-3xl font-bold text-blue-600 mt-4 sm:mt-0">
                <%-- Đồng hồ sẽ được cập nhật bằng JS --%>
                00:00:00
            </div>
        </div>

        <h2 class="text-3xl font-semibold text-gray-800 mb-6">Tổng Quan Nhanh</h2>

        <%-- Các thẻ thống kê (ĐÃ CẬP NHẬT GIAO DIỆN) --%>
        <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6 mb-12"> <%-- Giảm gap một chút --%>
            <%-- Thẻ Tổng Số Sách --%>
            <div class="bg-gradient-to-br from-blue-100 to-blue-200 p-4 rounded-lg shadow-md transition-all-ease hover:shadow-lg hover:scale-[1.02] flex items-center"> <%-- Giảm padding, đổi shadow, thêm flex --%>
                <div class="stat-card-icon flex-shrink-0 text-blue-600 mr-4"> <%-- Bỏ background tròn, điều chỉnh màu text icon --%>
                    <i class="fas fa-book fa-2x"></i>
                </div>
                <div> <%-- Div chứa text --%>
                    <p class="text-sm text-blue-700 font-medium uppercase tracking-wider">Tổng Số Sách</p> <%-- Điều chỉnh màu, font weight --%>
                    <asp:Label ID="lblTongSach" runat="server" Text="0" CssClass="text-2xl font-bold text-gray-800 block"></asp:Label> <%-- Giảm cỡ chữ, đổi màu, thêm block --%>
                </div>
            </div>

            <%-- Thẻ Đơn Hàng Mới --%>
            <div class="bg-gradient-to-br from-green-100 to-green-200 p-4 rounded-lg shadow-md transition-all-ease hover:shadow-lg hover:scale-[1.02] flex items-center">
                <div class="stat-card-icon flex-shrink-0 text-green-600 mr-4">
                     <i class="fas fa-shopping-cart fa-2x"></i>
                </div>
                <div>
                    <p class="text-sm text-green-700 font-medium uppercase tracking-wider">Đơn Hàng Mới</p>
                    <asp:Label ID="lblDonHangMoi" runat="server" Text="0" CssClass="text-2xl font-bold text-gray-800 block"></asp:Label>
                </div>
            </div>

            <%-- Thẻ Người Dùng --%>
             <div class="bg-gradient-to-br from-yellow-100 to-yellow-200 p-4 rounded-lg shadow-md transition-all-ease hover:shadow-lg hover:scale-[1.02] flex items-center">
                <div class="stat-card-icon flex-shrink-0 text-yellow-600 mr-4">
                    <i class="fas fa-users fa-2x"></i>
                </div>
                <div>
                    <p class="text-sm text-yellow-700 font-medium uppercase tracking-wider">Người Dùng</p>
                    <asp:Label ID="lblNguoiDung" runat="server" Text="0" CssClass="text-2xl font-bold text-gray-800 block"></asp:Label>
                </div>
            </div>

             <%-- Thẻ Doanh Thu Tháng --%>
            <div class="bg-gradient-to-br from-red-100 to-red-200 p-4 rounded-lg shadow-md transition-all-ease hover:shadow-lg hover:scale-[1.02] flex items-center">
                <div class="stat-card-icon flex-shrink-0 text-red-600 mr-4">
                     <i class="fas fa-dollar-sign fa-2x"></i>
                </div>
                <div>
                    <p class="text-sm text-red-700 font-medium uppercase tracking-wider">Doanh Thu (Tháng)</p>
                    <%-- Label Doanh thu giữ nguyên hoặc điều chỉnh class nếu cần --%>
                    <asp:Label ID="lblDoanhThuThang" runat="server" Text="0 VNĐ" CssClass="text-xl font-bold text-gray-800 block"></asp:Label> <%-- Điều chỉnh cỡ chữ cho phù hợp --%>
                </div>
            </div>
        </div> <%-- End Stat Cards Grid --%>

        <%-- Khu vực Biểu đồ --%>
        <h2 class="text-3xl font-semibold text-gray-800 mb-6">Thống Kê Chi Tiết</h2>
        <div class="grid grid-cols-1 lg:grid-cols-2 gap-8">
            <%-- Biểu đồ Doanh thu --%>
            <div class="chart-container">
                <canvas id="revenueChart"></canvas>
            </div>
           <%-- Biểu đồ Trạng thái Đơn hàng --%>
            <div class="chart-container">
                <canvas id="userChart"></canvas> <%-- Giữ nguyên ID userChart nếu JS đã dùng --%>
            </div>
        </div>

    </div> <%-- End Container --%>

    <%-- Script cho Đồng hồ, Ngày và Biểu đồ (Giữ nguyên) --%>
    <script>
        // Hàm cập nhật Đồng hồ và Ngày
        function updateClockAndDate() {
            const now = new Date();
            const timeOptions = { hour: '2-digit', minute: '2-digit', second: '2-digit', hour12: false }; // Định dạng HH:MM:SS
            const dateOptions = { weekday: 'long', year: 'numeric', month: 'long', day: 'numeric' }; // Định dạng "Thứ Sáu, ngày 11 tháng 4 năm 2025"

            document.getElementById('liveClock').innerText = now.toLocaleTimeString('vi-VN', timeOptions);
            document.getElementById('currentDate').innerText = now.toLocaleDateString('vi-VN', dateOptions);
        }
        updateClockAndDate();
        setInterval(updateClockAndDate, 1000);

        // --- Script Khởi tạo Biểu đồ ---
        const revenueChartDataJson = <%= RevenueChartJson %>;
        const userChartDataJson = <%= UserChartJson %>; // Đổi tên biến này nếu cần rõ nghĩa hơn

        // Hàm vẽ biểu đồ Doanh thu (line chart)
        const ctxRevenue = document.getElementById('revenueChart').getContext('2d');
        const revenueChart = new Chart(ctxRevenue, {
            type: 'line',
            data: {
                labels: revenueChartDataJson.Labels,
                datasets: [{
                    label: 'Doanh thu (VNĐ)',
                    data: revenueChartDataJson.Data,
                    borderColor: 'rgb(54, 162, 235)',
                    backgroundColor: 'rgba(54, 162, 235, 0.2)',
                    tension: 0.1,
                    fill: true
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    title: { display: true, text: 'Doanh Thu 6 Tháng Gần Nhất' },
                    tooltip: {
                        callbacks: {
                            label: function (context) {
                                let label = context.dataset.label || '';
                                if (label) { label += ': '; }
                                if (context.parsed.y !== null) {
                                    label += new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(context.parsed.y);
                                }
                                return label;
                            }
                        }
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        ticks: {
                            callback: function (value) {
                                return new Intl.NumberFormat('vi-VN', { notation: 'compact', maximumFractionDigits: 1 }).format(value);
                            }
                        }
                    }
                }
            }
        });

        // Hàm vẽ biểu đồ Trạng thái Đơn hàng (doughnut chart)
        const ctxOrderStatus = document.getElementById('userChart').getContext('2d');
        const orderStatusChart = new Chart(ctxOrderStatus, {
            type: 'doughnut', // Đổi thành 'doughnut'
            data: {
                labels: userChartDataJson.Labels, // Nhãn từ C# (Completed, Pending, Failed,...)
                datasets: [{
                    label: 'Số lượng đơn',
                    data: userChartDataJson.Data, // Số lượng từ C#
                    backgroundColor: [ // Cập nhật màu sắc nếu cần
                        'rgb(75, 192, 192)',  // Teal/Greenish Blue
                        'rgb(255, 205, 86)',  // Yellow
                        'rgb(255, 99, 132)',   // Red/Pink
                        'rgb(54, 162, 235)',   // Blue
                        'rgb(153, 102, 255)', // Purple
                        'rgb(255, 159, 64)'  // Orange
                    ],
                    hoverOffset: 4
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    title: {
                        display: true,
                        text: 'Phân Bổ Trạng Thái Đơn Hàng' // Tiêu đề mới
                    },
                    tooltip: {
                        callbacks: {
                            label: function (context) {
                                let label = context.label || '';
                                let value = context.raw || 0;
                                let total = context.chart.getDatasetMeta(0).total;
                                let percentage = total > 0 ? ((value / total) * 100).toFixed(1) + '%' : '0%';
                                if (label) { label += ': '; }
                                label += value + ' (' + percentage + ')';
                                return label;
                            }
                        }
                    },
                    legend: {
                        position: 'top',
                    },
                }
                // Không cần scales cho doughnut chart
            }
        });
    </script>
</asp:Content>