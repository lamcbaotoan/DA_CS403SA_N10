<%@ Page Title="Trang Chủ " Language="C#" MasterPageFile="~/WebForm/VangLai/Site.Master" AutoEventWireup="true" CodeBehind="trangchu.aspx.cs" Inherits="Webebook.WebForm.VangLai.trangchu" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.0.0/css/all.min.css" />
    <style>
        .line-clamp-2 {
            overflow: hidden;
            display: -webkit-box;
            -webkit-box-orient: vertical;
            -webkit-line-clamp: 2;
            min-height: 2.8em; 
            line-height: 1.4em;
        }

        .book-card-item {
            opacity: 0;
            transform: translateY(15px);
            transition: opacity 0.4s ease-out, transform 0.4s ease-out;
        }
        .book-card-item.visible {
            opacity: 1;
            transform: translateY(0);
        }

         .book-cover-img {
             width: 100%;
             height: 280px; 
             object-fit: cover; 
             background-color: #f3f4f6; 
         }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <%-- Banner / Hero Section --%>
    <div class="bg-gradient-to-r from-purple-100 via-blue-50 to-indigo-100 p-8 md:p-12 rounded-xl shadow-lg mb-10 text-center relative overflow-hidden">
        <div class="absolute inset-0 opacity-10 pattern-texture"></div>
        <div class="relative z-10">
            <h1 class="text-3xl md:text-4xl lg:text-5xl font-extrabold text-purple-800 mb-4">
                Khám Phá Thế Giới Sách Điện Tử
            </h1>
            <p class="text-gray-700 text-lg md:text-xl mb-8 max-w-3xl mx-auto">
                Hàng ngàn đầu sách thuộc mọi thể loại đang chờ bạn khám phá. Đọc mọi lúc, mọi nơi.
            </p>
            <asp:HyperLink ID="hlExploreBooks" runat="server" NavigateUrl="~/WebForm/VangLai/danhsachsach.aspx"
                CssClass="inline-block px-8 py-3 bg-purple-600 hover:bg-purple-700 text-white font-semibold rounded-lg shadow-md transition duration-200 ease-in-out transform hover:scale-105">
                <i class="fas fa-book-open mr-2"></i> Xem Tất Cả Sách
            </asp:HyperLink>
        </div>
    </div>

    <%-- Sách Nổi Bật --%>
    <section class="mb-12">
        <h2 class="text-2xl md:text-3xl font-bold text-gray-800 mb-6 pb-2 border-b-2 border-purple-300 inline-block">
             <i class="fas fa-star text-yellow-500 mr-2"></i> Sách Nổi Bật
        </h2>
        <asp:Panel ID="pnlSachNoiBat" runat="server">
            <div class="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 gap-5 md:gap-7">
                <asp:Repeater ID="rptSachNoiBat" runat="server">
                    <ItemTemplate>
                        <div class="book-card-item bg-white rounded-lg shadow-md overflow-hidden flex flex-col h-full border border-gray-100 hover:shadow-xl hover:-translate-y-1 transition duration-300 ease-in-out group">
                             <asp:HyperLink ID="hlBookLinkNoiBat" runat="server" NavigateUrl='<%# ResolveUrl("~/WebForm/VangLai/chitietsach.aspx?IDSach=") + Eval("IDSach") %>'>
                                 <%-- Ảnh bìa - Đã xóa comment lỗi --%>
                                 <asp:Image ID="imgBookCoverNoiBat" runat="server" CssClass="book-cover-img"
                                     ImageUrl='<%# GetImageUrl(Eval("DuongDanBiaSach")) %>'
                                     AlternateText='<%# "Bìa sách " + Eval("TenSach") %>' />
                             </asp:HyperLink>
                             <div class="p-4 flex flex-col flex-grow">
                                 <div>
                                     <h3 class="text-base font-semibold text-gray-900 mb-1 line-clamp-2 group-hover:text-purple-700 transition duration-150" title='<%# Eval("TenSach") %>'>
                                         <asp:HyperLink ID="hlTitleNoiBat" runat="server" Text='<%# Eval("TenSach") %>'
                                             NavigateUrl='<%# ResolveUrl("~/WebForm/VangLai/chitietsach.aspx?IDSach=") + Eval("IDSach") %>'
                                             CssClass="no-underline hover:underline"></asp:HyperLink>
                                     </h3>
                                     <p class="text-sm text-gray-500 mb-2 truncate" title='<%# Eval("TacGia") %>'>
                                         <%# Eval("TacGia") %>
                                     </p>
                                 </div>
                                 <p class="text-lg font-bold text-red-600 mt-auto">
                                     <%# Eval("GiaSach", "{0:N0} VNĐ") %>
                                 </p>
                            </div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
            </div>
        </asp:Panel>
        <asp:Panel ID="pnlNoSachNoiBat" runat="server" Visible="false" CssClass="mt-6 text-center py-10 px-4 text-gray-500 bg-gray-50 border border-dashed border-gray-300 rounded-lg">
             <i class="fas fa-book-reader fa-3x text-gray-400 mb-3"></i>
            <p>Hiện chưa có sách nổi bật nào.</p>
        </asp:Panel>
    </section>

    <%-- Sách Mới Nhất --%>
    <section class="mb-12">
         <h2 class="text-2xl md:text-3xl font-bold text-gray-800 mb-6 pb-2 border-b-2 border-blue-300 inline-block">
             <i class="fas fa-history text-blue-500 mr-2"></i> Sách Mới Nhất
        </h2>
        <asp:Panel ID="pnlSachMoi" runat="server">
            <div class="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 gap-5 md:gap-7">
                <asp:Repeater ID="rptSachMoi" runat="server">
                    <ItemTemplate>
                        <div class="book-card-item bg-white rounded-lg shadow-md overflow-hidden flex flex-col h-full border border-gray-100 hover:shadow-xl hover:-translate-y-1 transition duration-300 ease-in-out group">
                             <asp:HyperLink ID="hlBookLinkMoi" runat="server" NavigateUrl='<%# ResolveUrl("~/WebForm/VangLai/chitietsach.aspx?IDSach=") + Eval("IDSach") %>'>
                                <%-- Ảnh bìa - Đã xóa comment lỗi --%>
                                 <asp:Image ID="imgBookCoverMoi" runat="server" CssClass="book-cover-img"
                                     ImageUrl='<%# GetImageUrl(Eval("DuongDanBiaSach")) %>'
                                     AlternateText='<%# "Bìa sách " + Eval("TenSach") %>' />
                             </asp:HyperLink>
                             <div class="p-4 flex flex-col flex-grow">
                                 <div>
                                     <h3 class="text-base font-semibold text-gray-900 mb-1 line-clamp-2 group-hover:text-blue-700 transition duration-150" title='<%# Eval("TenSach") %>'>
                                         <asp:HyperLink ID="hlTitleMoi" runat="server" Text='<%# Eval("TenSach") %>'
                                             NavigateUrl='<%# ResolveUrl("~/WebForm/VangLai/chitietsach.aspx?IDSach=") + Eval("IDSach") %>'
                                             CssClass="no-underline hover:underline"></asp:HyperLink>
                                     </h3>
                                     <p class="text-sm text-gray-500 mb-2 truncate" title='<%# Eval("TacGia") %>'>
                                         <%# Eval("TacGia") %>
                                     </p>
                                 </div>
                                 <p class="text-lg font-bold text-red-600 mt-auto">
                                      <%# Eval("GiaSach", "{0:N0} VNĐ") %>
                                 </p>
                             </div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
            </div>
        </asp:Panel>
         <asp:Panel ID="pnlNoSachMoi" runat="server" Visible="false" CssClass="mt-6 text-center py-10 px-4 text-gray-500 bg-gray-50 border border-dashed border-gray-300 rounded-lg">
             <i class="fas fa-box-open fa-3x text-gray-400 mb-3"></i>
            <p>Chưa có sách mới nào được cập nhật.</p>
        </asp:Panel>
    </section>

    <%-- Script cho hiệu ứng fade-in card --%>
    <script>
        document.addEventListener('DOMContentLoaded', function () {
            const cards = document.querySelectorAll('.book-card-item');
            const observer = new IntersectionObserver((entries) => {
                entries.forEach((entry, index) => {
                    if (entry.isIntersecting) {
                        setTimeout(() => {
                            entry.target.classList.add('visible');
                        }, index * 50);
                        observer.unobserve(entry.target);
                    }
                });
            }, { threshold: 0.1 });

            cards.forEach(card => {
                observer.observe(card);
            });
        });
    </script>

</asp:Content>