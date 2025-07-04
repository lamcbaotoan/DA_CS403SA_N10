<%@ Page Title="Danh Sách Sách" Language="C#" MasterPageFile="~/WebForm/User/User.Master" AutoEventWireup="true" CodeBehind="danhsachsach_user.aspx.cs" Inherits="Webebook.WebForm.User.danhsachsach_user" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <%-- Font Awesome cần có trong MasterPage hoặc link ở đây --%>
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.0.0/css/all.min.css" />
    <style>
        /* Giữ lại CSS cho cắt chữ 2 dòng */
        .line-clamp-2 {
            overflow: hidden; display: -webkit-box; -webkit-box-orient: vertical; -webkit-line-clamp: 2;
            min-height: 2.8em; line-height: 1.4em;
        }

        /* CSS cho hiệu ứng fade-in card */
        .book-card-item {
            opacity: 0; transform: translateY(15px);
            transition: opacity 0.4s ease-out, transform 0.4s ease-out;
        }
        .book-card-item.visible { opacity: 1; transform: translateY(0); }

        /* Ảnh bìa */
         .book-cover-img {
             width: 100%; height: 260px; /* Điều chỉnh nếu cần */
             object-fit: cover; background-color: #f3f4f6;
             border-bottom: 1px solid #e5e7eb;
         }
         /* Transition chung */
         a, button, input, select, asp:LinkButton { transition: all 0.15s ease-in-out; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="container mx-auto px-4 py-6 md:py-8">
        <h2 class="text-3xl font-bold text-gray-800 mb-6">Khám Phá Sách</h2>

        <%-- Thông báo (Thêm giỏ thành công, lỗi...) --%>
        <asp:Label ID="lblMessage" runat="server" EnableViewState="false" Visible="false"></asp:Label>

        <%-- Khu vực Lọc - Style giống trang VangLai --%>
        <div class="bg-white p-5 md:p-6 rounded-xl shadow-lg border border-gray-100 mb-8">
             <div class="grid grid-cols-1 md:grid-cols-3 lg:grid-cols-4 gap-4 items-end">
                 <div class="lg:col-span-2">
                    <label for="<%= txtSearchFilter.ClientID %>" class="block text-sm font-medium text-gray-700 mb-1">Tìm kiếm</label>
                     <div class="relative">
                         <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none"><i class="fas fa-search text-gray-400"></i></div>
                        <asp:TextBox ID="txtSearchFilter" runat="server" placeholder="Tên sách, tác giả..." CssClass="block w-full pl-10 pr-3 py-2 border border-gray-300 rounded-lg placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-purple-500 focus:border-purple-500 sm:text-sm transition duration-150"></asp:TextBox>
                     </div>
                 </div>
                 <div>
                    <label for="<%= ddlGenreFilter.ClientID %>" class="block text-sm font-medium text-gray-700 mb-1">Thể loại</label>
                    <asp:DropDownList ID="ddlGenreFilter" runat="server" CssClass="block w-full py-2 pl-3 pr-10 border border-gray-300 bg-white rounded-lg focus:outline-none focus:ring-2 focus:ring-purple-500 focus:border-purple-500 sm:text-sm transition duration-150" AutoPostBack="false"></asp:DropDownList> <%-- Nên để AutoPostBack="false" và dùng nút Lọc --%>
                 </div>
                 <div class="flex space-x-2 mt-4 md:mt-0 md:justify-self-end">
                      <asp:Button ID="btnApplyFilter" runat="server" Text="Lọc" CssClass="flex-grow md:flex-grow-0 px-5 py-2 bg-purple-600 text-white font-semibold rounded-lg shadow-sm hover:bg-purple-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-purple-500 transition duration-150 ease-in-out" OnClick="btnApplyFilter_Click" />
                     <asp:Button ID="btnClearFilter" runat="server" Text="Xóa lọc" CausesValidation="false" CssClass="flex-grow md:flex-grow-0 px-5 py-2 bg-gray-200 text-gray-700 font-semibold rounded-lg shadow-sm hover:bg-gray-300 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-gray-400 transition duration-150 ease-in-out" OnClick="btnClearFilter_Click" />
                 </div>
             </div>
        </div>

        <%-- Danh sách sách - Container --%>
        <asp:Panel ID="bookGridContainer" runat="server">
             <div class="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 xl:grid-cols-6 gap-5 md:gap-6">
                <asp:Repeater ID="rptSachUser" runat="server" OnItemCommand="rptSachUser_ItemCommand">
                    <ItemTemplate>
                        <div class="book-card-item bg-white rounded-lg shadow-md overflow-hidden flex flex-col h-full border border-gray-100 hover:shadow-xl hover:-translate-y-1 transition duration-300 ease-in-out group">
                             <%-- Link ảnh trỏ đến chi tiết user --%>
                            <a href='<%# ResolveUrl("~/WebForm/User/chitietsach_user.aspx?IDSach=") + Eval("IDSach") %>' class="block relative overflow-hidden">
                                <asp:Image ID="imgCover" runat="server" CssClass="book-cover-img transition duration-300 ease-in-out group-hover:scale-105"
                                    ImageUrl='<%# GetImageUrl(Eval("DuongDanBiaSach")) %>'
                                    AlternateText='<%# "Bìa " + Eval("TenSach") %>' />
                            </a>
                             <div class="p-3 flex flex-col flex-grow">
                                <%-- Info --%>
                                <div>
                                    <h3 class="text-sm font-semibold text-gray-900 mb-1 line-clamp-2 group-hover:text-purple-700 transition duration-150" title='<%# Eval("TenSach") %>'>
                                         <%-- Link tiêu đề trỏ đến chi tiết user --%>
                                        <a href='<%# ResolveUrl("~/WebForm/User/chitietsach_user.aspx?IDSach=") + Eval("IDSach") %>' class="hover:underline">
                                            <%# Eval("TenSach") %>
                                        </a>
                                    </h3>
                                    <p class="text-xs text-gray-500 mb-2 truncate" title='<%# Eval("TacGia") %>'>
                                        <%# Eval("TacGia") %>
                                    </p>
                                </div>
                                 <%-- Giá và Buttons --%>
                                <div class="mt-auto pt-2">
                                    <p class="text-base font-bold text-red-600 mb-3">
                                        <%# Eval("GiaSach", "{0:N0} VNĐ") %>
                                    </p>
                                     <%-- Các nút Action --%>
                                    <div class="flex items-center space-x-2">
                                         <%-- Nút Xem Chi Tiết --%>
                                         <asp:HyperLink ID="hlViewDetail" runat="server"
                                             NavigateUrl='<%# ResolveUrl("~/WebForm/User/chitietsach_user.aspx?IDSach=") + Eval("IDSach") %>'
                                             CssClass="flex-1 inline-flex items-center justify-center px-3 py-1.5 border border-purple-600 text-purple-600 rounded-md hover:bg-purple-50 hover:text-purple-700 transition duration-150 text-xs font-medium" ToolTip="Xem Chi Tiết">
                                             <i class="fas fa-eye mr-1"></i> Chi tiết
                                         </asp:HyperLink>
                                         <%-- Nút Thêm Giỏ Hàng --%>
                                          <asp:LinkButton ID="btnAddToCart" runat="server" CausesValidation="false"
                                              CommandName="AddToCart" CommandArgument='<%# Eval("IDSach") %>'
                                              CssClass="flex-1 inline-flex items-center justify-center px-3 py-1.5 border border-transparent bg-emerald-500 text-white rounded-md hover:bg-emerald-600 transition duration-150 text-xs font-medium" ToolTip="Thêm vào giỏ hàng">
                                              <i class="fas fa-cart-plus mr-1"></i> Thêm giỏ
                                         </asp:LinkButton>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </ItemTemplate>
                </asp:Repeater>
            </div>
        </asp:Panel>

        <%-- Panel không có dữ liệu --%>
        <asp:Panel ID="pnlEmptyData" runat="server" Visible="false" CssClass="mt-8 text-center py-12 px-6 bg-white shadow-lg rounded-xl border border-gray-100">
             <div class="flex flex-col items-center">
                 <i class="fas fa-book-open fa-4x mb-5 text-gray-300"></i>
                 <p class="text-xl font-medium text-gray-700 mb-2">Không tìm thấy sách phù hợp</p>
                 <p class="text-gray-500">Vui lòng thử lại với bộ lọc khác hoặc <asp:LinkButton ID="lnkClearFilterEmpty" runat="server" Text="xóa bộ lọc hiện tại" OnClick="btnClearFilter_Click" CssClass="text-purple-600 hover:underline font-medium" CausesValidation="false" />.</p> <%-- Added CausesValidation=false --%>
            </div>
        </asp:Panel>

        <%-- Phân trang --%>
        <div class="mt-10 flex justify-center items-center space-x-3">
             <asp:Button ID="btnPrevPage" runat="server" Text="< Trang trước" OnClick="Pager_Click" CommandArgument="Prev" Enabled="false" CausesValidation="false"
                CssClass="px-4 py-2 border border-gray-300 bg-white text-sm font-medium rounded-md text-gray-700 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed transition duration-150" />
            <asp:Label ID="lblPagerInfo" runat="server" CssClass="text-sm text-gray-700 font-medium px-3 py-2 bg-gray-100 rounded-md shadow-inner"></asp:Label>
            <asp:Button ID="btnNextPage" runat="server" Text="Trang sau >" OnClick="Pager_Click" CommandArgument="Next" Enabled="false" CausesValidation="false"
                 CssClass="px-4 py-2 border border-gray-300 bg-white text-sm font-medium rounded-md text-gray-700 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed transition duration-150" />
        </div>
    </div>

    <%-- Script JS (Tùy chọn: Thêm fade-in) --%>
     <script type="text/javascript">
         function initializeCardFadeInUserList() {
             const cards = document.querySelectorAll('.book-card-item');
             if (cards.length === 0) return;
             cards.forEach(card => card.classList.remove('visible'));
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
         document.addEventListener('DOMContentLoaded', initializeCardFadeInUserList);
         // Gọi lại sau PostBack nếu cần:
         // Sys.WebForms.PageRequestManager.getInstance().add_endRequest(initializeCardFadeInUserList);
         // Hoặc dùng ScriptManager.RegisterStartupScript từ code-behind trong LoadBookList.
     </script>
</asp:Content>