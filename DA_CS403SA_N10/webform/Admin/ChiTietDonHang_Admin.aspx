<%@ Page Title="Chi Tiết Đơn Hàng" Language="C#" MasterPageFile="~/WebForm/Admin/Admin.Master" AutoEventWireup="true" CodeBehind="ChiTietDonHang_Admin.aspx.cs" Inherits="Webebook.WebForm.Admin.ChiTietDonHang_Admin" %>
<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <%-- Add Font Awesome if not already in MasterPage --%>
    <%-- <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.0.0/css/all.min.css" /> --%>
    <style>
        .icon-prefix {
            margin-right: 0.5em;
        }
    </style>
</asp:Content>
<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="container mx-auto px-4 sm:px-6 lg:px-8 py-8">

        <%-- Container for Heading and Back Button --%>
        <div class="flex justify-between items-center mb-8">
            <h2 class="text-3xl font-bold text-gray-800 flex items-center">
                 <i class="fas fa-receipt text-indigo-600 icon-prefix"></i> Chi Tiết Đơn Hàng
            </h2>
            <%-- Back Button --%>
            <a href='<%= ResolveUrl("~/WebForm/Admin/QuanLyDonHang.aspx") %>'
               class="bg-gray-500 hover:bg-gray-600 text-white font-medium py-2 px-4 rounded inline-flex items-center transition duration-150 ease-in-out text-sm no-underline">
                <i class="fas fa-arrow-left mr-2"></i> Quay Lại Danh Sách
            </a>
        </div>

        <%-- Thông báo lỗi/thành công --%>
        <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="mb-6 p-4 border rounded-md flex items-center" EnableViewState="false">
             <asp:Label ID="lblMessageIcon" runat="server" CssClass="icon-prefix"></asp:Label>
             <asp:Label ID="lblMessageText" runat="server"></asp:Label>
        </asp:Panel>


        <%-- Phần thông tin đơn hàng --%>
        <div class="bg-white p-6 rounded-lg shadow-md mb-8 border border-gray-200">
            <h3 class="text-xl font-semibold text-gray-700 mb-5 border-b border-gray-200 pb-3 flex items-center">
                 <i class="fas fa-info-circle text-gray-500 icon-prefix"></i> Thông tin chung
            </h3>
            <%-- ... Rest of the general info grid ... --%>
             <div class="grid grid-cols-1 md:grid-cols-2 gap-x-6 gap-y-4">
                <div>
                    <p class="text-sm font-medium text-gray-500 mb-1">ID Đơn Hàng:</p>
                    <p class="text-lg font-semibold text-gray-900"><asp:Label ID="lblIDDonHang" runat="server">N/A</asp:Label></p>
                </div>
                <div>
                    <p class="text-sm font-medium text-gray-500 mb-1">Người Đặt:</p>
                    <p class="text-lg text-gray-900"><asp:Label ID="lblNguoiDat" runat="server">N/A</asp:Label></p>
                </div>
                <div>
                    <p class="text-sm font-medium text-gray-500 mb-1">Ngày Đặt:</p>
                    <p class="text-lg text-gray-900"><asp:Label ID="lblNgayDat" runat="server">N/A</asp:Label></p>
                </div>
                 <div>
                    <p class="text-sm font-medium text-gray-500 mb-1">Phương Thức Thanh Toán:</p>
                    <p class="text-lg text-gray-900"><asp:Label ID="lblPhuongThuc" runat="server">N/A</asp:Label></p>
                </div>
                 <div>
                    <p class="text-sm font-medium text-gray-500 mb-1">Trạng Thái Thanh Toán:</p>
                    <%-- CSS Class will be set in code-behind --%>
                    <asp:Label ID="lblTrangThai" runat="server" CssClass="text-base px-3 py-1 rounded-full font-semibold inline-block align-middle"></asp:Label>
                </div>
                 <div>
                    <p class="text-sm font-medium text-gray-500 mb-1">Tổng Tiền:</p>
                    <p class="text-xl font-bold text-indigo-600"><asp:Label ID="lblTongTien" runat="server">0 VNĐ</asp:Label></p>
                </div>
            </div>
        </div>

        <%-- Phần chi tiết sản phẩm trong đơn hàng --%>
        <%-- ... Rest of the page ... --%>
         <div class="bg-white shadow-md rounded-lg overflow-hidden border border-gray-200">
             <h3 class="text-xl font-semibold text-gray-700 p-4 border-b border-gray-200 flex items-center">
                 <i class="fas fa-book text-gray-500 icon-prefix"></i> Sản phẩm trong đơn hàng
             </h3>
             <div class="overflow-x-auto">
                 <asp:GridView ID="gvChiTiet" runat="server" AutoGenerateColumns="False"
                     CssClass="min-w-full divide-y divide-gray-200"
                     EmptyDataText="Không có sản phẩm nào trong đơn hàng này."
                     GridLines="None">
                     <HeaderStyle CssClass="bg-gray-100 text-left text-xs font-semibold text-gray-600 uppercase tracking-wider" />
                     <RowStyle CssClass="bg-white hover:bg-gray-50 transition duration-150 ease-in-out" />
                     <AlternatingRowStyle CssClass="bg-gray-50 hover:bg-gray-100 transition duration-150 ease-in-out" /> <%-- Optional: Add different background for alternating rows --%>
                     <Columns>
                         <asp:BoundField DataField="IDSach" HeaderText="ID Sách">
                              <HeaderStyle CssClass="px-6 py-3" />
                              <ItemStyle CssClass="px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900" />
                         </asp:BoundField>
                         <asp:BoundField DataField="TenSach" HeaderText="Tên Sách">
                             <HeaderStyle CssClass="px-6 py-3" />
                             <ItemStyle CssClass="px-6 py-4 whitespace-nowrap text-sm text-gray-800" />
                         </asp:BoundField>
                         <asp:BoundField DataField="SoLuong" HeaderText="Số Lượng">
                              <HeaderStyle CssClass="px-6 py-3 text-center" />
                              <ItemStyle CssClass="px-6 py-4 whitespace-nowrap text-sm text-gray-600 text-center" />
                         </asp:BoundField>
                         <asp:BoundField DataField="Gia" HeaderText="Đơn Giá" DataFormatString="{0:N0} VNĐ">
                              <HeaderStyle CssClass="px-6 py-3 text-right" />
                              <ItemStyle CssClass="px-6 py-4 whitespace-nowrap text-sm text-gray-700 font-semibold text-right" />
                         </asp:BoundField>
                     </Columns>
                     <EmptyDataRowStyle CssClass="px-6 py-10 text-center text-gray-500 italic" />
                 </asp:GridView>
             </div>
        </div>
    </div>
</asp:Content>