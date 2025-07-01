<%@ Page Title="Chi Tiết Đơn Hàng" Language="C#" MasterPageFile="~/WebForm/User/User.Master" AutoEventWireup="true" CodeBehind="chitietdonhang.aspx.cs" Inherits="Webebook.WebForm.User.chitietdonhang" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.0.0/css/all.min.css" />
<style>
    /* 1. Tinh chỉnh style ảnh bìa: Thêm đổ bóng và nhất quán hơn */
    .book-item-img-detail {
        width: 60px;
        height: 90px;
        object-fit: cover;
        border-radius: 0.375rem; /* ~ rounded-md */
        border: 1px solid #e5e7eb; /* border-gray-200 */
        flex-shrink: 0;
        background-color: #f9fafb; /* bg-gray-50 */
        box-shadow: 0 4px 6px -1px rgb(0 0 0 / 0.1), 0 2px 4px -2px rgb(0 0 0 / 0.1); /* shadow-md */
    }

    /* 2. CSS cho giao diện bảng trên di động (Responsive Table) */
    @media (max-width: 640px) { /* Áp dụng cho màn hình nhỏ hơn 640px (sm) */
        .responsive-table thead {
            display: none; /* Ẩn header của bảng */
        }
        .responsive-table tbody,
        .responsive-table tr,
        .responsive-table td {
            display: block; /* Biến các thành phần của bảng thành block */
            width: 100%;
        }
        .responsive-table tr {
            margin-bottom: 1rem; /* Khoảng cách giữa các thẻ sản phẩm */
            border: 1px solid #e5e7eb;
            border-radius: 0.5rem; /* Bo góc cho thẻ */
            padding: 1rem;
            box-shadow: 0 1px 3px 0 rgb(0 0 0 / 0.1), 0 1px 2px -1px rgb(0 0 0 / 0.1);
        }
        .responsive-table td {
            padding: 0.5rem 0;
            text-align: right; /* Căn phải cho dữ liệu */
            position: relative;
            border-bottom: 1px dashed #e5e7eb; /* Ngăn cách các dòng thông tin */
        }
        .responsive-table tr:last-child td:last-child {
            border-bottom: none; /* Bỏ đường gạch ở dòng cuối cùng của thẻ */
        }
        .responsive-table td:before {
            content: attr(data-label); /* Lấy nội dung từ attribute data-label */
            position: absolute;
            left: 0;
            font-weight: 600; /* In đậm nhãn */
            color: #4b5563; /* text-gray-600 */
            text-align: left;
        }
        /* Style riêng cho ô chứa ảnh và tên sản phẩm */
        .responsive-table .product-cell {
            display: flex; /* Dùng flexbox để ảnh và tên nằm cạnh nhau */
            align-items: center;
            text-align: left;
            padding: 0 0 0.75rem 0; /* Căn lại padding */
            border-bottom: 1px solid #d1d5db;
        }
        .responsive-table .product-cell:before {
            display: none; /* Không cần label "Sản phẩm" */
        }
        .responsive-table .product-cell .book-info {
            margin-left: 1rem;
        }
        .responsive-table .product-cell .truncate {
            white-space: normal; /* Cho phép tên sách xuống dòng */
        }
        /* Căn lại cho các ô số liệu */
        .responsive-table .price-cell,
        .responsive-table .quantity-cell,
        .responsive-table .total-cell {
             padding-right: 0; /* Căn phải tuyệt đối */
        }
    }

    /* Tooltip style (giữ nguyên) */
    .truncate[title]:hover::after {
        content: attr(title); position: absolute; left: 0; top: 100%; z-index: 10;
        background: rgba(17, 24, 39, 0.9); color: white; padding: 6px 10px;
        border-radius: 4px; font-size: 0.75rem; line-height: 1.2; white-space: nowrap;
        margin-top: 6px; width: max-content; max-width: 300px; pointer-events: none;
        box-shadow: 0 2px 5px rgba(0,0,0,0.2);
    }
    .truncate[title] { position: relative; cursor: help; }
    .truncate { display: inline-block; vertical-align: middle; max-width: 100%; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
</style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="container mx-auto max-w-5xl px-4 sm:px-6 lg:px-8 py-10">
        <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center mb-8 pb-4 border-b border-gray-200 gap-4">
            <h2 class="text-2xl lg:text-3xl font-semibold text-gray-800">Chi Tiết Đơn Hàng</h2>
            <asp:HyperLink ID="hlBackToHistory" runat="server" NavigateUrl="~/WebForm/User/lichsumuahang.aspx"
                CssClass="inline-flex items-center px-4 py-2 border border-gray-300 rounded-md shadow-sm text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 transition duration-150 ease-in-out whitespace-nowrap">
                <i class="fas fa-arrow-left mr-2 text-gray-500"></i> Quay lại Lịch sử mua hàng
            </asp:HyperLink>
        </div>

        <asp:Label ID="lblMessage" runat="server" EnableViewState="false" Visible="false"></asp:Label>

        <%-- Panel Thông tin đơn hàng --%>
        <asp:Panel ID="pnlOrderInfo" runat="server" Visible="true" CssClass="bg-white p-6 rounded-lg shadow-md border border-gray-200 mb-8">
            <h3 class="text-lg font-semibold text-gray-800 mb-5 pb-3 border-b border-gray-200">Thông tin đơn hàng</h3>
            <dl class="grid grid-cols-[auto,1fr] gap-x-6 gap-y-3 text-sm">
                <dt class="font-semibold text-gray-600">Mã đơn hàng:</dt>
                <dd class="text-gray-800 font-medium"><asp:Label ID="lblIDDonHang" runat="server" CssClass="font-mono"></asp:Label></dd>
                <dt class="font-semibold text-gray-600">Ngày đặt:</dt>
                <dd class="text-gray-800"><asp:Label ID="lblNgayDat" runat="server"></asp:Label></dd>
                <dt class="font-semibold text-gray-600">Trạng thái:</dt>
                <dd><asp:Literal ID="ltrTrangThai" runat="server"></asp:Literal></dd>
                <dt class="font-semibold text-gray-600">Thanh toán:</dt>
                <dd class="text-gray-800"><asp:Label ID="lblPhuongThuc" runat="server"></asp:Label></dd>
            </dl>
        </asp:Panel>

        <%-- Panel Danh sách sản phẩm (ĐÃ CẬP NHẬT) --%>
        <asp:Panel ID="pnlOrderItems" runat="server" Visible="true" CssClass="bg-white rounded-lg shadow-md border border-gray-200 overflow-hidden">
            <h3 class="text-lg font-semibold text-gray-800 p-6 border-b border-gray-200">Sản phẩm trong đơn hàng</h3>
            <div class="overflow-x-auto">
                <asp:Repeater ID="rptOrderItems" runat="server">
                    <HeaderTemplate>
                        <table class="min-w-full divide-y divide-gray-200 responsive-table">
                            <thead class="bg-gray-50">
                                <tr>
                                    <th scope="col" class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider w-[45%]" colspan="2">Sản phẩm</th>
                                    <th scope="col" class="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider w-[20%]">Đơn giá</th>
                                    <th scope="col" class="px-6 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider w-[10%]">Số lượng</th>
                                    <th scope="col" class="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider w-[25%]">Thành tiền</th>
                                </tr>
                            </thead>
                            <tbody class="bg-white divide-y divide-gray-200 sm:divide-y-0">
                    </HeaderTemplate>
                    <ItemTemplate>
                        <tr class="hover:bg-gray-50 transition duration-150 ease-in-out">
                            <%-- Gộp ảnh và tên vào một cell để xử lý trên mobile dễ hơn --%>
                            <td class="px-6 py-4 align-middle product-cell" colspan="2">
                                <asp:Image ID="imgBookCover" runat="server" CssClass="book-item-img-detail" ImageUrl='<%# GetBookImageUrl(Eval("DuongDanBiaSach")) %>' AlternateText='<%# Eval("TenSach") %>' />
                                <div class="book-info">
                                    <span class="font-medium text-gray-900 text-sm truncate" title='<%# Eval("TenSach") %>'>
                                        <%# Eval("TenSach") %>
                                    </span>
                                </div>
                            </td>
                            <%-- Thêm data-label cho các cell còn lại --%>
                            <td class="px-6 py-4 whitespace-nowrap text-right text-sm text-gray-600 align-middle price-cell" data-label="Đơn giá:"><%# FormatCurrency(Eval("Gia")) %></td>
                            <td class="px-6 py-4 whitespace-nowrap text-center text-sm text-gray-600 align-middle quantity-cell" data-label="Số lượng:">x <%# Eval("SoLuong") %></td>
                            <td class="px-6 py-4 whitespace-nowrap text-right text-sm font-semibold text-gray-800 align-middle total-cell" data-label="Thành tiền:"><%# CalculateLineTotal(Eval("SoLuong"), Eval("Gia")) %></td>
                        </tr>
                    </ItemTemplate>
                    <FooterTemplate>
                            </tbody>
                        </table>
                    </FooterTemplate>
                </asp:Repeater>
                
                <asp:Panel ID="pnlNoOrderItemsMessage" runat="server" Visible="false">
                    <div class="text-center py-16 px-6">
                        <i class="fas fa-shopping-cart fa-3x text-gray-400 mb-4"></i>
                        <p class="text-gray-500">Không có sản phẩm nào trong đơn hàng này.</p>
                    </div>
                </asp:Panel>
            </div>

            <div class="bg-gray-50 px-6 py-4 border-t border-gray-300 text-right">
                <span class="text-sm font-medium text-gray-600 uppercase mr-2">Tổng cộng:</span>
                <asp:Label ID="lblTongTienValue" runat="server" CssClass="text-lg font-bold text-gray-900"></asp:Label>
            </div>
        </asp:Panel>

    </div>
</asp:Content>