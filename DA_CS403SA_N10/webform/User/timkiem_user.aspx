<%@ Page Title="Kết Quả Tìm Kiếm" Language="C#" MasterPageFile="~/WebForm/User/User.Master" AutoEventWireup="true" CodeBehind="timkiem_user.aspx.cs" Inherits="Webebook.WebForm.User.timkiem_user" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.0.0/css/all.min.css" />
    <style>
        /* CSS cần thiết */
        .line-clamp-2 { overflow: hidden; display: -webkit-box; -webkit-box-orient: vertical; -webkit-line-clamp: 2; min-height: 2.8em; line-height: 1.4em; }
        .book-cover-img { width: 100%; height: 260px; object-fit: cover; background-color: #f3f4f6; border-bottom: 1px solid #e5e7eb; }
        .book-card-item { opacity: 0; transform: translateY(15px); transition: opacity 0.4s ease-out, transform 0.4s ease-out; }
        .book-card-item.visible { opacity: 1; transform: translateY(0); }
        a, button, input, select, asp:LinkButton { transition: all 0.15s ease-in-out; }
        /* Popup Style */
         .popup-overlay { position: fixed; inset: 0; background-color: rgba(0,0,0,0.6); z-index: 40; display: none; opacity: 0; transition: opacity 0.2s ease-out; }
         .popup-content { position: fixed; top: 50%; left: 50%; transform: translate(-50%, -50%) scale(0.95); background-color: white; padding: 1.5rem 2rem; border-radius: 0.75rem; box-shadow: 0 20px 25px -5px rgb(0 0 0 / 0.1), 0 8px 10px -6px rgb(0 0 0 / 0.1); z-index: 50; display: none; text-align: center; max-width: 90%; width: 400px; opacity: 0; transition: opacity 0.2s ease-out, transform 0.2s ease-out; }
         .popup-overlay.visible, .popup-content.visible { display: block; opacity: 1; transform: translate(-50%, -50%) scale(1); }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="container mx-auto px-4 py-6 md:py-8">
        <h2 class="text-2xl md:text-3xl font-semibold text-gray-800 mb-6">
            Kết Quả Tìm Kiếm cho: "<asp:Literal ID="litKeyword" runat="server" />"
        </h2>
        <asp:Label ID="lblMessage" runat="server" EnableViewState="false" Visible="false"></asp:Label>

        <%-- Repeater hiển thị kết quả --%>
        <asp:Repeater ID="rptKetQuaUser" runat="server" OnItemCommand="rptKetQuaUser_ItemCommand">
            <HeaderTemplate>
                <div class="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 xl:grid-cols-6 gap-5 md:gap-6"> <%-- Grid container --%>
            </HeaderTemplate>
            <ItemTemplate>
           <div class="book-card-item bg-white rounded-lg shadow-md overflow-hidden flex flex-col h-full border border-gray-100 hover:shadow-xl hover:-translate-y-1 transition duration-300 ease-in-out group">
                    <a href='<%# ResolveUrl("~/WebForm/User/chitietsach_user.aspx?IDSach=") + Eval("IDSach") %>' class="block relative overflow-hidden">
                        <%-- Đã xóa comment lỗi khỏi dòng ImageUrl --%>
                        <asp:Image ID="imgCover" runat="server" CssClass="book-cover-img transition duration-300 ease-in-out group-hover:scale-105"
                            ImageUrl='<%# GetImageUrl(Eval("DuongDanBiaSach")) %>'
                            AlternateText='<%# "Bìa " + Eval("TenSach") %>' />
                    </a>
                    <div class="p-3 flex flex-col flex-grow">
                        <div>
                            <h3 class="text-sm font-semibold text-gray-900 mb-1 line-clamp-2 group-hover:text-purple-700 transition duration-150" title='<%# Eval("TenSach") %>'>
                                <a href='<%# ResolveUrl("~/WebForm/User/chitietsach_user.aspx?IDSach=") + Eval("IDSach") %>' class="hover:underline">
                                    <%# Eval("TenSach") %>
                                </a>
                            </h3>
                            <p class="text-xs text-gray-500 mb-2 truncate" title='<%# Eval("TacGia") %>'>
                                <%# Eval("TacGia") %>
                            </p>
                        </div>
                         <div class="mt-auto pt-2"> <%-- Đẩy giá và nút xuống dưới --%>
                            <p class="text-base font-bold text-red-600 mb-3">
                                <%# Eval("GiaSach", "{0:N0} VNĐ") %>
                            </p>
                             <asp:LinkButton ID="btnAddToCart" runat="server" CausesValidation="false"
                                 CommandName="AddToCart" CommandArgument='<%# Eval("IDSach") %>'
                                 CssClass="w-full inline-flex items-center justify-center px-3 py-2 border border-transparent bg-emerald-500 text-white rounded-md hover:bg-emerald-600 transition duration-150 text-xs font-medium" ToolTip="Thêm vào giỏ hàng">
                                 <i class="fas fa-cart-plus mr-1.5"></i> Thêm vào giỏ
                             </asp:LinkButton>
                             <asp:HiddenField ID="hfBookName" runat="server" Value='<%# Eval("TenSach") %>' />
                         </div>
                    </div>
                 </div>
            </ItemTemplate>
            <FooterTemplate>
                </div> <%-- Đóng grid container --%>
            </FooterTemplate>
        </asp:Repeater>

        <%-- Thông báo khi không có kết quả --%>
        <asp:Panel ID="pnlNoResults" runat="server" Visible="false" CssClass="mt-8 text-center py-12 px-6 bg-white shadow-lg rounded-xl border border-gray-100">
             <div class="flex flex-col items-center">
                 <i class="fas fa-search fa-4x mb-5 text-gray-300"></i>
                 <p class="text-xl font-medium text-gray-700 mb-2">Không tìm thấy kết quả</p>
                 <p class="text-gray-500">Không có sách nào khớp với từ khóa "<asp:Literal ID="litNoResultKeyword" runat="server" />". Vui lòng thử lại.</p>
            </div>
        </asp:Panel>
    </div> <%-- End container --%>

     <%-- Popup Sách đã có trong giỏ --%>
     <div id="alreadyInCartPopupOverlay" class="popup-overlay" onclick="hideAlreadyInCartPopup()"></div> <div id="alreadyInCartPopup" class="popup-content"> <i class="fas fa-check-circle fa-2x text-blue-500 mb-4"></i><h3 class="text-xl font-semibold mb-3">Đã Có Trong Giỏ Hàng</h3><p class="text-sm text-gray-600 mb-6">Sách '<span id="popupCartBookName" class="font-semibold text-gray-800"></span>' đã có trong giỏ hàng của bạn.</p><div class="flex justify-center space-x-4"><button type="button" onclick="goToCart();" class="px-6 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg font-medium transition duration-150"><i class="fas fa-shopping-cart mr-1.5"></i> Xem giỏ hàng</button><button type="button" onclick="hideAlreadyInCartPopup();" class="px-6 py-2 bg-gray-200 hover:bg-gray-300 text-gray-800 rounded-lg font-medium transition duration-150">Đóng</button></div></div>

    <%-- JavaScript cho hiệu ứng card và popup --%>
     <script type="text/javascript">
         function initializeCardFadeInSearch() { const cards = document.querySelectorAll('.book-card-item'); if (cards.length === 0) return; const observer = new IntersectionObserver((entries) => { entries.forEach((entry, index) => { if (entry.isIntersecting) { setTimeout(() => { entry.target.classList.add('visible'); }, index * 50); observer.unobserve(entry.target); } }); }, { threshold: 0.1 }); cards.forEach(card => { card.classList.remove('visible'); observer.observe(card); }); } document.addEventListener('DOMContentLoaded', initializeCardFadeInSearch);
         // JS Popup "Đã có trong giỏ"
         const alreadyInCartOverlay = document.getElementById('alreadyInCartPopupOverlay'); const alreadyInCartPopup = document.getElementById('alreadyInCartPopup'); const popupCartBookNameSpan = document.getElementById('popupCartBookName'); const cartPageUrl = '<%= ResolveUrl("~/WebForm/User/giohang_user.aspx") %>'; function showAlreadyInCartPopup(bookName) { if (popupCartBookNameSpan) { popupCartBookNameSpan.textContent = bookName; } fadeIn(alreadyInCartOverlay); fadeIn(alreadyInCartPopup); } function hideAlreadyInCartPopup() { fadeOut(alreadyInCartOverlay); fadeOut(alreadyInCartPopup); } function goToCart() { window.location.href = cartPageUrl; } function fadeIn(element) { if (!element) return; element.style.opacity = 0; element.style.display = 'block'; requestAnimationFrame(() => { element.style.transition = 'opacity 0.2s ease-out'; element.style.opacity = 1; element.classList.add('visible'); }); } function fadeOut(element) { if (!element) return; element.style.opacity = 1; element.classList.remove('visible'); element.style.transition = 'opacity 0.2s ease-in'; element.style.opacity = 0; setTimeout(() => { element.style.display = 'none'; }, 200); }
     </script>
</asp:Content>