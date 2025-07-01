<%@ Page Title="Thống Kê Sách Bán Chạy" Language="C#" MasterPageFile="~/WebForm/Admin/Admin.Master" AutoEventWireup="true" CodeBehind="ThongKeSach.aspx.cs" Inherits="Webebook.WebForm.Admin.ThongKeSach" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <%-- Chart.js Library --%>
    <script src="https://cdn.jsdelivr.net/npm/chart.js"></script>
    <style>
        /* Custom styles for chart container aspect ratio and GridView image */
        .chart-container {
            position: relative;
            margin: auto;
            height: 45vh; /* Slightly taller */
            width: 100%;  /* Let container control width */
            max-width: 550px; /* Max width */
            margin-bottom: 1rem;
        }

        .grid-image {
            height: 3.5rem; /* h-14 */
            width: 3rem;   /* w-12 */
            object-fit: cover;
            border-radius: 0.375rem; /* rounded-md */
            border: 1px solid #e5e7eb; /* border border-gray-200 */
            box-shadow: 0 1px 3px 0 rgba(0, 0, 0, 0.1), 0 1px 2px 0 rgba(0, 0, 0, 0.06); /* subtle shadow */
        }

         /* Ensure pager buttons have some spacing */
        .grid-pager table { margin: 0 auto; } /* Center align if needed */
        .grid-pager td { padding: 0 4px; }

        /* Style for links within the pager */
        .grid-pager a, .grid-pager span {
            display: inline-block;
            padding: 6px 12px;
            border-radius: 0.375rem; /* rounded-md */
            text-decoration: none;
            transition: background-color 0.2s ease-in-out, color 0.2s ease-in-out;
            font-size: 0.875rem; /* text-sm */
            border: 1px solid #d1d5db; /* border-gray-300 */
            color: #374151; /* text-gray-700 */
            background-color: #fff; /* bg-white */
        }

        .grid-pager a:hover {
            background-color: #f3f4f6; /* hover:bg-gray-100 */
            color: #1f2937; /* hover:text-gray-800 */
        }

        /* Style for the current page number */
        .grid-pager span {
            background-color: #60a5fa; /* bg-blue-400 */
            color: white;
            border-color: #60a5fa; /* border-blue-400 */
            font-weight: 600; /* font-semibold */
        }

    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="container mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <h2 class="text-4xl font-extrabold text-transparent bg-clip-text bg-gradient-to-r from-blue-500 to-purple-600 mb-10 text-center tracking-tight">
            Thống Kê Sách Bán Chạy
        </h2>

        <asp:Label ID="lblMessage" runat="server" CssClass="block mb-6 text-center font-semibold p-3 rounded-md" EnableViewState="false"></asp:Label>

        <%-- Charts Section --%>
        <div class="grid grid-cols-1 lg:grid-cols-2 gap-8 mb-12">
            <%-- Sales Quantity Chart Card --%>
            <div class="bg-white p-6 rounded-xl shadow-lg hover:shadow-xl transition-shadow duration-300 ease-in-out border border-gray-100">
                <h3 class="text-xl font-semibold text-gray-800 mb-5 text-center">Top Sách Theo Số Lượng Bán</h3>
                <div class="chart-container">
                    <canvas id="salesQuantityChart"></canvas>
                </div>
            </div>

            <%-- Revenue Chart Card --%>
            <div class="bg-white p-6 rounded-xl shadow-lg hover:shadow-xl transition-shadow duration-300 ease-in-out border border-gray-100">
                <h3 class="text-xl font-semibold text-gray-800 mb-5 text-center">Top Sách Theo Doanh Thu</h3>
                <div class="chart-container">
                    <canvas id="revenueChart"></canvas>
                </div>
            </div>
        </div>

        <%-- GridView Section --%>
        <h3 class="text-2xl font-semibold text-gray-800 mb-6">Chi Tiết Thống Kê</h3>
        <div class="bg-white shadow-xl rounded-xl overflow-hidden border border-gray-200">
            <div class="overflow-x-auto">
                <asp:GridView ID="GridViewThongKe" runat="server" AutoGenerateColumns="False"
                    CssClass="min-w-full divide-y divide-gray-200"
                    AllowPaging="True" PageSize="10" OnPageIndexChanging="GridViewThongKe_PageIndexChanging"
                    EmptyDataText="<div class='text-center py-10 text-gray-500'>Hiện chưa có dữ liệu thống kê nào.</div>"
                    DataKeyNames="IDSach"
                    GridLines="None"> <%-- Remove default gridlines, use borders/dividers instead --%>

                    <HeaderStyle CssClass="bg-gradient-to-r from-blue-500 to-purple-600 text-white px-6 py-3 text-left text-xs font-semibold uppercase tracking-wider" />
                    <%-- Apply hover effect directly to RowStyle and AlternatingRowStyle --%>
                    <RowStyle CssClass="bg-white hover:bg-blue-50 transition duration-150 ease-in-out" />
                    <AlternatingRowStyle CssClass="bg-gray-50 hover:bg-blue-50 transition duration-150 ease-in-out" />
                    <PagerStyle CssClass="bg-gray-50 px-4 py-4 sm:px-6 border-t border-gray-200 text-right grid-pager" HorizontalAlign="Center" /> <%-- Center pager, apply custom CSS class --%>

                    <Columns>
                        <asp:BoundField DataField="IDSach" HeaderText="ID" ReadOnly="True">
                            <ItemStyle CssClass="px-6 py-4 whitespace-nowrap text-sm text-gray-500 font-medium w-16" />
                            <HeaderStyle CssClass="w-16"/>
                        </asp:BoundField>

                        <asp:TemplateField HeaderText="Ảnh Bìa">
                            <ItemTemplate>
                                <asp:Image ID="imgBiaSach" runat="server"
                                    ImageUrl='<%# Eval("DuongDanBiaSach") != DBNull.Value && !string.IsNullOrEmpty(Eval("DuongDanBiaSach").ToString()) ? ResolveUrl(Eval("DuongDanBiaSach").ToString()) : ResolveUrl("~/Images/placeholder_book_cover.svg") %>'
                                    AlternateText='<%# "Bìa " + Eval("TenSach") %>'
                                    CssClass="grid-image" />
                            </ItemTemplate>
                            <ItemStyle CssClass="px-6 py-2 whitespace-nowrap" Width="80px" />
                           <HeaderStyle Width="80px" />
                        </asp:TemplateField>

                        <asp:BoundField DataField="TenSach" HeaderText="Tên Sách">
                            <ItemStyle CssClass="px-6 py-4 whitespace-nowrap text-sm text-gray-900 font-semibold max-w-xs truncate" />
                        </asp:BoundField>

                        <asp:BoundField DataField="TacGia" HeaderText="Tác Giả">
                             <ItemStyle CssClass="px-6 py-4 whitespace-nowrap text-sm text-gray-600" />
                        </asp:BoundField>

                        <asp:BoundField DataField="TongSoLuongBan" HeaderText="SL Bán">
                            <ItemStyle CssClass="px-6 py-4 whitespace-nowrap text-sm text-blue-600 font-bold text-center" />
                            <HeaderStyle CssClass="text-center"/>
                        </asp:BoundField>

                        <asp:BoundField DataField="TongDoanhThu" HeaderText="Tổng Doanh Thu" DataFormatString="{0:N0} VNĐ">
                            <ItemStyle CssClass="px-6 py-4 whitespace-nowrap text-sm text-green-600 font-semibold text-right" />
                            <HeaderStyle CssClass="text-right"/>
                        </asp:BoundField>
                    </Columns>
                    <%-- EmptyDataRowStyle is handled by EmptyDataText styling now --%>
                </asp:GridView>
            </div>
        </div>
    </div>

    <%-- Hidden Fields to store chart data --%>
    <asp:HiddenField ID="hfChartLabels" runat="server" />
    <asp:HiddenField ID="hfSalesData" runat="server" />
    <asp:HiddenField ID="hfRevenueLabels" runat="server" /> <%-- Separate labels for revenue chart --%>
    <asp:HiddenField ID="hfRevenueData" runat="server" />
    <asp:HiddenField ID="hfSalesColors" runat="server" /> <%-- Store generated colors --%>
    <asp:HiddenField ID="hfRevenueColors" runat="server" /> <%-- Store generated colors --%>

    <%-- JavaScript for Charts will be registered from code-behind --%>
</asp:Content>