<%@ Page Title="Kết Quả Tìm Kiếm" Language="C#" MasterPageFile="~/WebForm/VangLai/Site.Master" AutoEventWireup="true" CodeBehind="timkiem.aspx.cs" Inherits="Webebook.WebForm.VangLai.timkiem" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <%-- Font Awesome nếu chưa có trong MasterPage --%>
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.0.0/css/all.min.css" integrity="sha512-9usAa10IRO0HhonpyAIVpjrylPvoDwiPUiKdWk5t3PyolY1cOd4DSE0Ga+ri4AuTroPR5aQvXU9xC6qOPnzFeg==" crossorigin="anonymous" referrerpolicy="no-referrer" />
    <style>
        /* CSS tùy chỉnh nếu cần, ví dụ: hiệu ứng fade-in */
        .book-card-item {
            opacity: 0;
            transform: translateY(10px);
            transition: opacity 0.5s ease-out, transform 0.5s ease-out;
        }
        .book-card-item.visible {
            opacity: 1;
            transform: translateY(0);
        }

        /* Tùy chỉnh chiều cao cố định cho ảnh bìa nếu muốn đồng nhất */
        .book-cover-fixed-height {
             height: 14rem; /* Hoặc giá trị khác bạn muốn */
             width: 100%;
             object-fit: contain; /* Hoặc object-cover nếu chấp nhận cắt ảnh */
             background-color: #f8fafc; /* Màu nền nhẹ cho vùng ảnh */
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="bg-gray-50 min-h-screen"> <%-- Nền xám nhạt cho toàn bộ vùng nội dung --%>
        <div class="container mx-auto px-4 py-8 md:py-12">
            <h2 class="text-3xl lg:text-4xl font-bold text-gray-800 mb-8">
                Kết Quả Tìm Kiếm cho: "<asp:Literal ID="litKeyword" runat="server" />"
            </h2>

            <%-- Thông báo (Giữ nguyên logic hiển thị từ code-behind) --%>
            <asp:Label ID="lblMessage" runat="server" EnableViewState="false" Visible="false"></asp:Label>

            <asp:Repeater ID="rptKetQua" runat="server">
                <HeaderTemplate>
                    <div class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 xl:grid-cols-5 gap-6 md:gap-8">
                </HeaderTemplate>
                <ItemTemplate>
                     <%-- Thêm class 'book-card-item' cho hiệu ứng fade-in --%>
                    <div class="book-card-item group bg-white border border-gray-200 rounded-xl shadow-lg overflow-hidden flex flex-col p-4 transition duration-300 ease-in-out hover:shadow-xl hover:-translate-y-1 hover:border-indigo-300">
                        <asp:HyperLink runat="server" NavigateUrl='<%# ResolveUrl("~/WebForm/VangLai/chitietsach.aspx?IDSach=") + Eval("IDSach") %>' CssClass="block mb-4 overflow-hidden rounded-md">
                            <asp:Image ID="imgCover" runat="server"
                                CssClass="book-cover-fixed-height transition duration-300 ease-in-out group-hover:opacity-90 group-hover:scale-105"
                                ImageUrl='<%# Eval("DuongDanBiaSach") != DBNull.Value && !string.IsNullOrEmpty(Eval("DuongDanBiaSach").ToString()) ? ResolveUrl(Eval("DuongDanBiaSach").ToString()) : ResolveUrl("~/Images/placeholder_cover.png") %>'
                                AlternateText='<%# "Bìa " + Eval("TenSach") %>' />
                        </asp:HyperLink>

                        <div class="flex flex-col flex-grow mt-2"> <%-- flex-grow để đẩy nút xuống dưới --%>
                            <asp:HyperLink runat="server" NavigateUrl='<%# ResolveUrl("~/WebForm/VangLai/chitietsach.aspx?IDSach=") + Eval("IDSach") %>' ToolTip='<%# Eval("TenSach") %>'>
                                <h3 class="text-lg font-semibold text-gray-900 mb-1 truncate transition duration-150 ease-in-out group-hover:text-indigo-600">
                                    <%# Eval("TenSach") %>
                                </h3>
                            </asp:HyperLink>
                            <p class="text-sm text-gray-500 mb-2 truncate">Tác giả: <%# Eval("TacGia") %></p>
                            <p class="text-xl font-bold text-blue-600 mb-4"><%# Eval("GiaSach", "{0:N0} VNĐ") %></p>

                            <%-- Nút Xem Chi Tiết - Đẩy xuống dưới cùng --%>
                            <div class="mt-auto">
                                 <asp:HyperLink ID="hlViewDetail" runat="server"
                                    NavigateUrl='<%# ResolveUrl("~/WebForm/VangLai/chitietsach.aspx?IDSach=") + Eval("IDSach") %>'
                                    CssClass="inline-flex items-center justify-center w-full px-4 py-2 bg-indigo-600 border border-transparent rounded-lg font-medium text-white hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 transition duration-150 ease-in-out text-sm"
                                    ToolTip="Xem Chi Tiết">
                                     <i class="fas fa-eye mr-2"></i> Xem chi tiết
                                </asp:HyperLink>
                            </div>
                        </div>
                    </div>
                </ItemTemplate>
                <FooterTemplate>
                    </div> <%-- Close grid --%>
                </FooterTemplate>
                <SeparatorTemplate></SeparatorTemplate> <%-- Có thể bỏ trống nếu không cần separator --%>
            </asp:Repeater>

            <%-- Panel Không Tìm Thấy Kết Quả --%>
            <asp:Panel ID="pnlNoResults" runat="server" Visible="false" CssClass="text-center py-12 px-6 bg-gray-100 shadow-md rounded-lg mt-10 border border-gray-200">
                <div class="flex flex-col items-center">
                     <i class="fas fa-book-open fa-3x md:fa-4x mb-4 text-gray-400"></i>
                     <p class="text-xl font-medium text-gray-700 mb-2">Không tìm thấy sách nào phù hợp.</p>
                     <p class="text-gray-500">Vui lòng thử tìm kiếm với từ khóa khác hoặc kiểm tra lại chính tả.</p>
                </div>
            </asp:Panel>
        </div>
    </div>

    <%-- Script cho hiệu ứng fade-in (offline - client side) --%>
    <script>
        document.addEventListener('DOMContentLoaded', function() {
            const cards = document.querySelectorAll('.book-card-item');
            cards.forEach((card, index) => {
                // Thêm một độ trễ nhỏ cho mỗi card để tạo hiệu ứng lần lượt
                setTimeout(() => {
                    card.classList.add('visible');
                }, index * 100); // 100ms delay between each card
            });
        });
    </script>
</asp:Content>