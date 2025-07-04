<%@ Page Title="Trang Chủ Của Bạn" Language="C#" MasterPageFile="~/WebForm/User/User.Master" AutoEventWireup="true" CodeBehind="usertrangchu.aspx.cs" Inherits="Webebook.WebForm.User.usertrangchu" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <%-- Font Awesome cần có trong MasterPage hoặc link ở đây --%>
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.0.0/css/all.min.css" />
    <style>
        /* Giữ lại CSS cho cắt chữ 2 dòng */
        .line-clamp-2 {
            overflow: hidden; display: -webkit-box; -webkit-box-orient: vertical; -webkit-line-clamp: 2;
            min-height: 2.8em; /* line-height * 2 */ line-height: 1.4em;
        }

        /* CSS cho hiệu ứng fade-in card */
        .book-card-item {
            opacity: 0; transform: translateY(15px);
            transition: opacity 0.4s ease-out, transform 0.4s ease-out;
        }
        .book-card-item.visible { opacity: 1; transform: translateY(0); }

        /* Ảnh bìa chung */
         .book-cover-img {
             width: 100%; height: 260px; /* Chiều cao nhỏ hơn chút cho trang user */
             object-fit: cover; /* Hoặc contain */
             background-color: #f3f4f6; /* bg-gray-100 */
             border-bottom: 1px solid #e5e7eb; /* border-gray-200 */
         }
         /* Style cho nút Đọc tiếp */
          a.read-continue-button {
             /* Các lớp Tailwind tương đương:
                inline-block w-full mt-3 px-4 py-2
                bg-gradient-to-r from-blue-500 to-indigo-600 hover:from-blue-600 hover:to-indigo-700
                text-white text-sm font-semibold rounded-md shadow-md
                transition duration-150 ease-in-out text-center no-underline
             */
             display: inline-block; width: 100%; margin-top: 0.75rem; padding: 0.5rem 1rem;
             background-image: linear-gradient(to right, var(--tw-gradient-stops));
             --tw-gradient-from: #3b82f6; /* blue-500 */
             --tw-gradient-to: #4f46e5; /* indigo-600 */
             --tw-gradient-stops: var(--tw-gradient-from), var(--tw-gradient-to);
             color: white; font-size: 0.875rem; font-weight: 600; border-radius: 0.375rem;
             box-shadow: 0 4px 6px -1px rgb(0 0 0 / 0.1), 0 2px 4px -2px rgb(0 0 0 / 0.1);
             transition: all 0.15s ease-in-out; text-align: center; text-decoration: none;
         }
         a.read-continue-button:hover {
             --tw-gradient-from: #2563eb; /* blue-600 */
             --tw-gradient-to: #4338ca; /* indigo-700 */
         }
         a.read-continue-button i { margin-right: 0.5rem; }

         /* Style cho panel không có dữ liệu */
          .empty-data-panel {
              text-align: center; padding: 2.5rem 1rem; /* py-10 px-4 */
              color: #6b7280; /* text-gray-500 */
              background-color: #f9fafb; /* bg-gray-50 */
              border: 1px dashed #d1d5db; /* border-dashed border-gray-300 */
              border-radius: 0.75rem; /* rounded-xl */
              margin-top: 1.5rem; /* mt-6 */
         }
         .empty-data-panel i {
             font-size: 2.25rem; /* fa-3x */
             color: #d1d5db; /* text-gray-300 */
             margin-bottom: 0.75rem; /* mb-3 */
         }
         .empty-data-panel p { font-size: 0.875rem; /* text-sm */ }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="container mx-auto px-4 py-6 md:py-8">
        <%-- Chào mừng người dùng --%>
        <h1 class="text-2xl md:text-3xl font-bold text-gray-800 mb-8">
            Chào mừng trở lại, <asp:Label ID="lblUsername" runat="server" CssClass="text-purple-600"></asp:Label>!
        </h1>

        <%-- Tiếp Tục Đọc --%>
        <%-- Panel này sẽ luôn hiển thị, nội dung bên trong sẽ ẩn/hiện --%>
        <asp:Panel ID="pnlTiepTucDocSection" runat="server" class="mb-12">
            <section>
                <h2 class="text-xl md:text-2xl font-semibold text-gray-800 mb-5 border-l-4 border-green-500 pl-3">
                    <i class="fas fa-book-open text-green-600 mr-2"></i> Tiếp Tục Đọc
                </h2>
                <asp:Panel ID="pnlTiepTucDoc" runat="server">
                    <div class="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 xl:grid-cols-6 gap-5 md:gap-6">
                        <asp:Repeater ID="rptTiepTucDoc" runat="server">
                            <ItemTemplate>
                                <div class="book-card-item bg-white rounded-lg shadow-md overflow-hidden flex flex-col h-full border border-gray-100 hover:shadow-xl hover:-translate-y-1 transition duration-300 ease-in-out group">
                                    <%-- Link ảnh và tiêu đề trỏ đến trang chi tiết chương (nơi chọn chương) --%>
                                     <asp:HyperLink ID="hlBookLinkContImage" runat="server" NavigateUrl='<%# ResolveUrl("~/WebForm/User/chitietsach_chap.aspx?IDSach=") + Eval("IDSach") %>'>
                                         <asp:Image ID="imgBookCoverCont" runat="server" CssClass="book-cover-img transition duration-300 ease-in-out group-hover:scale-105"
                                             ImageUrl='<%# GetImageUrl(Eval("DuongDanBiaSach")) %>' AlternateText='<%# "Bìa sách " + Eval("TenSach") %>' />
                                     </asp:HyperLink>
                                    <div class="p-3 flex flex-col flex-grow">
                                        <div>
                                            <h3 class="text-sm font-semibold text-gray-900 mb-1 line-clamp-2 group-hover:text-green-700 transition duration-150" title='<%# Eval("TenSach") %>'>
                                                 <asp:HyperLink ID="hlTitleCont" runat="server" Text='<%# Eval("TenSach") %>'
                                                     NavigateUrl='<%# ResolveUrl("~/WebForm/User/chitietsach_chap.aspx?IDSach=") + Eval("IDSach") %>' CssClass="hover:underline"></asp:HyperLink>
                                            </h3>
                                            <p class="text-xs text-gray-500 mb-2 truncate" title='<%# Eval("TacGia") %>'><%# Eval("TacGia") %></p>
                                            <p class="text-xs text-blue-600 font-medium">Đọc đến: <%# FormatViTriDoc(Eval("ViTriDoc")) %></p>
                                        </div>
                                        <div class="mt-auto pt-2"> <%-- Đẩy nút xuống dưới --%>
                                             <%-- Link nút Đọc Tiếp trỏ đến trang đọc sách với chương cụ thể --%>
                                            <asp:HyperLink ID="hlReadButtonCont" runat="server"
                                                NavigateUrl='<%# ResolveUrl("~/WebForm/User/docsach.aspx?IDSach=") + Eval("IDSach") + (Eval("ViTriDoc") != DBNull.Value && Eval("ViTriDoc").ToString() != "0" ? "&SoChuong=" + Eval("ViTriDoc") : "") %>'
                                                CssClass="read-continue-button">
                                                <i class="fas fa-book-reader"></i> Đọc Tiếp
                                            </asp:HyperLink>
                                        </div>
                                    </div>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>
                    </div>
                </asp:Panel>
                <%-- Panel hiển thị khi không có sách đang đọc dở --%>
                <asp:Panel ID="pnlNoTiepTucDoc" runat="server" Visible="false" CssClass="empty-data-panel">
                    <i class="fas fa-hourglass-start"></i>
                    <p>Bạn chưa đọc dở cuốn sách nào gần đây. Hãy bắt đầu khám phá!</p>
                </asp:Panel>
            </section>
        </asp:Panel>

        <%-- Đề Xuất Cho Bạn --%>
        <section class="mb-12">
            <h2 class="text-xl md:text-2xl font-semibold text-gray-800 mb-5 border-l-4 border-purple-500 pl-3">
                <i class="fas fa-lightbulb text-purple-600 mr-2"></i> Có Thể Bạn Sẽ Thích
            </h2>
             <asp:Panel ID="pnlDeXuat" runat="server">
                <div class="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 xl:grid-cols-6 gap-5 md:gap-6">
                    <asp:Repeater ID="rptDeXuat" runat="server">
                        <ItemTemplate>
                             <%-- Sử dụng card sách tương tự trang chủ/danh sách --%>
                            <div class="book-card-item bg-white rounded-lg shadow-md overflow-hidden flex flex-col h-full border border-gray-100 hover:shadow-xl hover:-translate-y-1 transition duration-300 ease-in-out group">
                                 <a href='<%# ResolveUrl("~/WebForm/User/chitietsach_user.aspx?IDSach=") + Eval("IDSach") %>' class="block relative overflow-hidden">
                                    <asp:Image ID="imgBookCoverDeXuat" runat="server" CssClass="book-cover-img transition duration-300 ease-in-out group-hover:scale-105"
                                        ImageUrl='<%# GetImageUrl(Eval("DuongDanBiaSach")) %>' AlternateText='<%# "Bìa sách " + Eval("TenSach") %>' />
                                 </a>
                                <div class="p-3 flex flex-col flex-grow">
                                    <div>
                                        <h3 class="text-sm font-semibold text-gray-900 mb-1 line-clamp-2 group-hover:text-purple-700 transition duration-150" title='<%# Eval("TenSach") %>'>
                                            <a href='<%# ResolveUrl("~/WebForm/User/chitietsach_user.aspx?IDSach=") + Eval("IDSach") %>' class="hover:underline">
                                                <%# Eval("TenSach") %>
                                            </a>
                                        </h3>
                                        <p class="text-xs text-gray-500 mb-2 truncate" title='<%# Eval("TacGia") %>'><%# Eval("TacGia") %></p>
                                    </div>
                                    <p class="text-base font-bold text-red-600 mt-auto pt-1">
                                        <%# Eval("GiaSach", "{0:N0} VNĐ") %>
                                    </p>
                                </div>
                            </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
            </asp:Panel>
            <asp:Panel ID="pnlNoDeXuat" runat="server" Visible="false" CssClass="empty-data-panel">
                <i class="fas fa-magic"></i>
                <p>Chúng tôi đang tìm những cuốn sách phù hợp nhất dành cho bạn.</p>
            </asp:Panel>
        </section>

        <%-- Sách Mới Nhất --%>
        <section class="mb-12">
             <h2 class="text-xl md:text-2xl font-semibold text-gray-800 mb-5 border-l-4 border-blue-500 pl-3">
                 <i class="fas fa-history text-blue-600 mr-2"></i> Sách Mới Cập Nhật
            </h2>
             <asp:Panel ID="pnlSachMoiUser" runat="server">
                 <div class="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 xl:grid-cols-6 gap-5 md:gap-6">
                    <asp:Repeater ID="rptSachMoiUser" runat="server">
                        <ItemTemplate>
                             <%-- Sử dụng card sách tương tự trang chủ/danh sách --%>
                             <div class="book-card-item bg-white rounded-lg shadow-md overflow-hidden flex flex-col h-full border border-gray-100 hover:shadow-xl hover:-translate-y-1 transition duration-300 ease-in-out group">
                                 <a href='<%# ResolveUrl("~/WebForm/User/chitietsach_user.aspx?IDSach=") + Eval("IDSach") %>' class="block relative overflow-hidden">
                                     <asp:Image ID="imgBookCoverMoiUser" runat="server" CssClass="book-cover-img transition duration-300 ease-in-out group-hover:scale-105"
                                         ImageUrl='<%# GetImageUrl(Eval("DuongDanBiaSach")) %>' AlternateText='<%# "Bìa sách " + Eval("TenSach") %>' />
                                 </a>
                                 <div class="p-3 flex flex-col flex-grow">
                                     <div>
                                         <h3 class="text-sm font-semibold text-gray-900 mb-1 line-clamp-2 group-hover:text-blue-700 transition duration-150" title='<%# Eval("TenSach") %>'>
                                            <a href='<%# ResolveUrl("~/WebForm/User/chitietsach_user.aspx?IDSach=") + Eval("IDSach") %>' class="hover:underline">
                                                <%# Eval("TenSach") %>
                                            </a>
                                         </h3>
                                         <p class="text-xs text-gray-500 mb-2 truncate" title='<%# Eval("TacGia") %>'><%# Eval("TacGia") %></p>
                                     </div>
                                     <p class="text-base font-bold text-red-600 mt-auto pt-1">
                                        <%# Eval("GiaSach", "{0:N0} VNĐ") %>
                                    </p>
                                 </div>
                             </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
            </asp:Panel>
            <asp:Panel ID="pnlNoSachMoiUser" runat="server" Visible="false" CssClass="empty-data-panel">
                 <i class="fas fa-box-open"></i>
                <p>Chưa có sách mới nào được cập nhật gần đây.</p>
            </asp:Panel>
        </section>
    </div>

     <%-- Script JS cho hiệu ứng fade-in card (tương tự trang chủ) --%>
    <script type="text/javascript">
        function initializeCardFadeInUser() {
            const cards = document.querySelectorAll('.book-card-item');
            if (cards.length === 0) return;
            const observer = new IntersectionObserver((entries) => {
                entries.forEach((entry, index) => {
                    if (entry.isIntersecting) {
                        setTimeout(() => { entry.target.classList.add('visible'); }, index * 50);
                        observer.unobserve(entry.target);
                    }
                });
            }, { threshold: 0.1 });
            cards.forEach(card => { observer.observe(card); });
        }
        document.addEventListener('DOMContentLoaded', initializeCardFadeInUser);
        // Gọi lại nếu dùng UpdatePanel: Sys.WebForms.PageRequestManager.getInstance().add_endRequest(initializeCardFadeInUser);
    </script>
</asp:Content>