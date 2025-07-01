<%@ Page Title="Lịch Sử Mua Hàng" Language="C#" MasterPageFile="~/WebForm/User/User.Master" AutoEventWireup="true" CodeBehind="lichsumuahang.aspx.cs" Inherits="Webebook.WebForm.User.lichsumuahang" %>
<%@ Import Namespace="System.Web.UI.HtmlControls" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css" integrity="sha512-iecdLmaskl7CVkqkXNQ/ZH/XLlvWZOJyj7Yy7tcenmpD1ypASozpmT/E0iPtmFIB46ZmdtAc9eNBvH0H/ZpiBw==" crossorigin="anonymous" referrerpolicy="no-referrer" />
    <style>
        /* Có thể bỏ các style không cần thiết nếu đã dùng Tailwind */
         .book-item-img {
             width: 48px; /* w-12 */
             height: 72px; /* ~tỷ lệ 2:3 */
             object-fit: cover;
             flex-shrink: 0;
             background-color: #f3f4f6; /* bg-gray-100 */
         }
         /* Base styles for status badge - Chỉ chứa các style cố định */
          .status-badge-base {
             display: inline-block;
             padding: 0.25rem 0.6rem; /* py-1 px-2.5 */
             font-size: 0.75rem; /* text-xs */
             font-weight: 600; /* font-semibold */
             border-radius: 9999px; /* rounded-full */
             text-transform: uppercase;
             letter-spacing: 0.05em; /* tracking-wide */
             border-width: 1px;
             border-style: solid;
             border-color: transparent; /* Màu border sẽ được set cùng màu nền/chữ */
         }
         /* Transition chung */
         a, button, input, select, asp:LinkButton, .order-card-hover { transition: all 0.15s ease-in-out; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="container mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <%-- Tiêu đề trang --%>
        <h2 class="text-3xl font-bold text-gray-800 mb-6 border-b border-gray-300 pb-4">Lịch Sử Mua Hàng</h2>

        <%-- Khu vực thông báo --%>
        <asp:Label ID="lblMessage" runat="server" EnableViewState="false" Visible="false"></asp:Label>

        <%-- Repeater cho các đơn hàng --%>
        <asp:Repeater ID="rptOrders" runat="server" OnItemDataBound="rptOrders_ItemDataBound">
            <ItemTemplate>
                <%-- Card cho mỗi đơn hàng - Style bằng Tailwind --%>
                <div class="order-card-hover bg-white p-5 sm:p-6 mb-6 rounded-lg shadow-md border border-gray-200 hover:border-gray-300 hover:shadow-lg">
                    <%-- Header đơn hàng --%>
                    <div class="flex flex-col sm:flex-row sm:justify-between sm:items-center border-b border-gray-200 pb-4 mb-4 gap-2">
                        <div class="flex-grow mb-2 sm:mb-0">
                            <span class="text-base font-semibold text-gray-800 mr-2">Đơn hàng #<%# Eval("IDDonHang") %></span>
                            <span class="block sm:inline text-sm text-gray-500 mt-1 sm:mt-0">
                                <i class="far fa-calendar-alt mr-1.5 text-gray-400"></i>Ngày đặt: <%# Eval("NgayDat", "{0:dd/MM/yyyy HH:mm}") %>
                            </span>
                        </div>
                        <div class="text-left sm:text-right flex-shrink-0">
                            <%-- Literal để render span với class động từ code-behind --%>
                            <asp:Literal ID="ltrStatus" runat="server"></asp:Literal>
                        </div>
                    </div>

                    <%-- Danh sách sách trong đơn hàng --%>
                    <div class="space-y-3 mb-5">
                        <asp:Repeater ID="rptOrderItems" runat="server" OnItemDataBound="rptOrderItems_ItemDataBound">
                            <ItemTemplate>
                                <div class="flex items-center justify-between p-2 rounded hover:bg-gray-50 transition-colors duration-150">
                                    <%-- Ảnh và Tên sách --%>
                                    <div class="flex items-center space-x-3 flex-grow min-w-0 mr-4"> <%-- min-w-0 quan trọng cho truncate --%>
                                        <asp:HyperLink ID="hlBookImageLink" runat="server" CssClass="block flex-shrink-0">
                                            <asp:Image ID="imgBookCover" runat="server" CssClass="book-item-img rounded border border-gray-200" AlternateText='<%# Eval("TenSach") %>' />
                                        </asp:HyperLink>
                                        <asp:HyperLink ID="hlBookTitleLink" runat="server"
                                            CssClass="text-sm font-medium text-gray-800 truncate hover:text-blue-600 hover:underline"
                                            title='<%# Eval("TenSach") %>'> <%-- title attribute cho tooltip mặc định --%>
                                            <%-- Text gán từ code-behind --%>
                                        </asp:HyperLink>
                                    </div>
                                    <%-- Nút/Link Đánh giá --%>
                                    <div class="flex-shrink-0 ml-auto text-right" style="min-width: 90px;">
                                        <asp:HyperLink ID="hlReview" runat="server"
                                             NavigateUrl="#" CssClass="text-xs text-center text-blue-600 hover:text-blue-800 hover:underline px-2.5 py-1.5 border border-blue-200 rounded bg-blue-50 hover:bg-blue-100 transition whitespace-nowrap"
                                             ToolTip="Viết đánh giá cho sách này" Visible="false">
                                             <i class="far fa-star text-xs mr-1"></i>Đánh giá
                                        </asp:HyperLink>
                                        <span ID="spnCannotReview" runat="server" class="text-xs text-gray-400 italic whitespace-nowrap" Visible="false">(Chưa thể đánh giá)</span>
                                    </div>
                                </div>
                            </ItemTemplate>
                            <%-- Không cần Separator nếu chỉ có khoảng cách --%>
                        </asp:Repeater>
                    </div>

                    <%-- Footer đơn hàng --%>
                    <div class="flex flex-col sm:flex-row sm:justify-between sm:items-center border-t border-gray-200 pt-4 gap-3">
                        <div class="text-sm font-semibold text-gray-700 text-left">
                            Tổng tiền: <span class="text-lg text-red-600 font-bold ml-1"><%# FormatCurrency(Eval("SoTien")) %></span>
                        </div>
                        <div class="text-left sm:text-right w-full sm:w-auto">
                            <asp:HyperLink ID="hlViewDetails" runat="server"
                                NavigateUrl='<%# Eval("IDDonHang", "~/WebForm/User/chitietdonhang.aspx?IDDonHang={0}") %>'
                                CssClass="inline-block text-sm text-white bg-gradient-to-r from-blue-500 to-blue-600 hover:from-blue-600 hover:to-blue-700 focus:ring-4 focus:ring-blue-300 font-medium rounded-lg px-5 py-2.5 text-center shadow-sm transition ease-in-out duration-150 w-full sm:w-auto">
                                <i class="fas fa-receipt mr-1.5"></i> Xem Chi Tiết Đơn Hàng
                            </asp:HyperLink>
                        </div>
                    </div>
                </div> <%-- End order-card --%>
            </ItemTemplate>
        </asp:Repeater>

        <%-- Panel hiển thị khi không có đơn hàng --%>
<%-- Panel displayed when there are no orders --%>
        <asp:Panel ID="pnlNoOrders" runat="server" Visible="false" CssClass="text-center py-16 bg-white rounded-lg shadow-sm border border-gray-200 mt-6">
            <div class="flex flex-col items-center">
                 <i class="fas fa-shopping-bag fa-3x text-gray-400 mb-5"></i>
                 <p class="text-gray-500 text-lg mb-5">Lịch sử mua hàng của bạn hiện đang trống.</p>
                 <%-- Link to browse books --%>
                 <%-- Đã xóa comment gây lỗi khỏi dòng dưới --%>
                 <asp:HyperLink runat="server" NavigateUrl="~/WebForm/User/danhsachsach_user.aspx"
                     CssClass="inline-block bg-gradient-to-r from-blue-500 to-blue-600 hover:from-blue-600 hover:to-blue-700 text-white font-semibold py-2.5 px-6 rounded-lg transition duration-150 ease-in-out shadow hover:shadow-md transform hover:-translate-y-0.5">
                     <i class="fas fa-search mr-2"></i> Khám phá Sách Ngay
                 </asp:HyperLink>
            </div>
        </asp:Panel>

    </div> <%-- End container --%>
</asp:Content>