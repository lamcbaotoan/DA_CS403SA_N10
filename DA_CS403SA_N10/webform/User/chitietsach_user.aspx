<%@ Page Title="Chi Tiết Sách" Language="C#" MasterPageFile="~/WebForm/User/User.Master" AutoEventWireup="true" CodeBehind="chitietsach_user.aspx.cs" Inherits="Webebook.WebForm.User.chitietsach_user" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.0.0/css/all.min.css" /> <%-- Đảm bảo có Font Awesome --%>
    <style>
        /* Giữ lại CSS gốc cho các thành phần không phải layout/button CHÍNH */
        .genre-badge { display: inline-block; padding: 0.25rem 0.6rem; margin-right: 0.3rem; margin-bottom: 0.3rem; font-size: 0.75rem; font-weight: 500; border-radius: 0.25rem; background-color: #e0e7ff; color: #4338ca; border: 1px solid #c7d2fe; }
        .book-type-badge { display: inline-block; padding: 0.25rem 0.6rem; margin-left: 0.5rem; font-size: 0.75rem; font-weight: 600; border-radius: 0.25rem; background-color: #d1fae5; color: #047857; border: 1px solid #a7f3d0; }
        .description-summary, .comment-summary { overflow: hidden; display: -webkit-box; -webkit-line-clamp: 3; -webkit-box-orient: vertical; transition: max-height 0.3s ease-out; max-height: 4.5em; line-height: 1.5em; }
        .description-full, .comment-full { max-height: none; -webkit-line-clamp: unset; }
        .toggle-button { color: #4f46e5; cursor: pointer; font-size: 0.875rem; font-weight: 500; margin-top: 0.25rem; display: inline-block; }
        .toggle-button:hover { text-decoration: underline; }
        .comment-item { border-bottom: 1px solid #e5e7eb; padding-bottom: 1rem; margin-bottom: 1rem; }
        .comment-item:last-child { border-bottom: none; margin-bottom: 0; }
        .comment-avatar { width: 2.5rem; height: 2.5rem; border-radius: 50%; object-fit: cover; background-color: #f3f4f6; margin-right: 0.75rem; flex-shrink:0; border: 1px solid #eee;}
        .comment-meta { font-size: 0.8rem; color: #6b7280; }
        .rating-stars i { margin-right: 1px;}
        .prose br { margin-bottom: 0.5em !important; display: block; content: "";}

        /* Ảnh bìa */
         .book-cover-main-img {
             display: block; /* Để margin auto hoạt động */
             width: auto; /* Để giữ tỷ lệ */
             max-width: 100%; /* Không vượt quá container */
             height: auto;
             max-height: 550px; /* Giới hạn chiều cao tối đa */
             object-fit: contain;
             background-color: #f9fafb; /* Nền nhẹ */
             margin-left: auto; /* Căn giữa nếu ảnh nhỏ hơn container */
             margin-right: auto;
         }
         /* Transition chung */
         a, button, input, select, asp:LinkButton { transition: all 0.15s ease-in-out; }

         /* ---- THÊM CSS CHO POPUP ---- */
         .popup-overlay { position: fixed; inset: 0; background-color: rgba(0,0,0,0.6); z-index: 40; display: none; opacity: 0; transition: opacity 0.2s ease-out; }
         .popup-content {
             position: fixed; top: 50%; left: 50%; transform: translate(-50%, -50%) scale(0.95);
             background-color: white; padding: 1.5rem 2rem; border-radius: 0.75rem; /* rounded-xl */
             box-shadow: 0 20px 25px -5px rgb(0 0 0 / 0.1), 0 8px 10px -6px rgb(0 0 0 / 0.1); /* shadow-2xl */
             z-index: 50; display: none; text-align: center; max-width: 90%; width: 400px; /* Điều chỉnh nếu cần */
             opacity: 0; transition: opacity 0.2s ease-out, transform 0.2s ease-out;
         }
         .popup-overlay.visible, .popup-content.visible { display: block; opacity: 1; transform: translate(-50%, -50%) scale(1); }
         /* ---- KẾT THÚC CSS POPUP ---- */
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="container mx-auto px-4 py-6 md:py-8">
        <asp:Label ID="lblMessage" runat="server" EnableViewState="false" Visible="false"></asp:Label>

        <%-- Container chính --%>
        <div id="bookDetailContainer" runat="server">
            <%-- ** SỬA LẠI BỐ CỤC VÀ STYLE NÚT ** --%>
            <div class="bg-white p-6 md:p-8 rounded-2xl shadow-xl mb-10 border border-gray-100 flex flex-col lg:flex-row gap-6 md:gap-8">
                <%-- Cột Ảnh Bìa --%>
                <div class="w-full lg:w-1/3 flex justify-center items-start">
                    <asp:Image ID="imgBiaSach" runat="server" CssClass="book-cover-img rounded-lg shadow-md border border-gray-200" AlternateText="Ảnh bìa sách" />
                </div>
                <%-- Cột Thông Tin --%>
                <div class="w-full lg:w-2/3 flex flex-col">
                    <h1 class="text-2xl lg:text-3xl font-bold text-gray-900 mb-3 leading-tight"><asp:Literal ID="litTenSach" runat="server"></asp:Literal></h1>
                    <div class="mb-4 flex flex-wrap items-center gap-2"> <asp:Repeater ID="rptGenres" runat="server"><ItemTemplate><span class="genre-badge"><%# Container.DataItem %></span></ItemTemplate></asp:Repeater> <asp:Label ID="lblLoaiSach" runat="server" Visible="false" CssClass="book-type-badge"></asp:Label> </div>
                    <div class="text-sm text-gray-600 space-y-1.5 mb-5"> <p><strong class="font-medium text-gray-700">Tác giả:</strong> <asp:Label ID="lblTacGia" runat="server">[Chưa cập nhật]</asp:Label></p> <p><strong class="font-medium text-gray-700">Nhóm dịch:</strong> <asp:Label ID="lblNhomDich" runat="server">[Chưa cập nhật]</asp:Label></p> <p><strong class="font-medium text-gray-700">Trạng thái:</strong> <asp:Label ID="lblTrangThai" runat="server">[Chưa cập nhật]</asp:Label></p> </div>
                    <%-- Phần Giá và Nút Hành động - Dùng Tailwind classes --%>
                    <div class="bg-gradient-to-r from-gray-50 to-gray-100 p-4 rounded-lg mb-6 border flex flex-col sm:flex-row items-center justify-between gap-4">
                        <p class="text-3xl font-bold text-red-600"><asp:Label ID="lblGiaSach" runat="server">[Giá liên hệ]</asp:Label></p>
                        <div class="flex items-center space-x-3 w-full sm:w-auto">
                             <%-- Nút Thêm Giỏ Hàng - Tailwind style --%>
                            <asp:LinkButton ID="btnThemGioHangUser" runat="server"
                                ToolTip="Thêm vào giỏ hàng"
                                CausesValidation="false"
                                CssClass="flex-1 sm:flex-none inline-flex items-center justify-center px-5 py-2.5 bg-blue-600 text-white font-semibold rounded-lg shadow-sm hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 transition duration-150 ease-in-out disabled:opacity-50 disabled:cursor-not-allowed"
                                OnClick="btnThemGioHangUser_Click"
                                Enabled="false"> <%-- Enabled sẽ được set từ code-behind --%>
                                <%-- Icon và Text đặt bên trong thẻ LinkButton --%>
                                <i class="fas fa-cart-plus mr-1"></i> Thêm vào giỏ
                            </asp:LinkButton>
                            <%-- Nút Mua Ngay - Tailwind style --%>
                            <asp:HyperLink ID="hlMuaNgay" runat="server" Visible="false" ToolTip="Mua ngay sản phẩm này"
                                CssClass="flex-1 sm:flex-none inline-flex items-center justify-center px-5 py-2.5 bg-emerald-500 text-white font-semibold rounded-lg shadow-sm hover:bg-emerald-600 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-emerald-400 transition duration-150 ease-in-out">
                                <i class="fas fa-shopping-bag mr-1.5"></i> Mua Ngay
                            </asp:HyperLink>
                        </div>
                    </div>
                     <%-- Phần Mô tả --%>
                      <div class="mt-auto border-t border-gray-200 pt-5">
                         <h3 class="text-lg font-semibold text-gray-800 mb-2">Mô tả chi tiết</h3>
                         <div id="descriptionContainer"> <div id="descriptionContent" class="text-gray-700 text-sm leading-relaxed prose-custom max-w-none collapsible-content line-clamp-3"><asp:Literal ID="litMoTa" runat="server" Mode="PassThrough"></asp:Literal></div> <button type="button" id="toggleDescriptionBtn" onclick="toggleContent('descriptionContent', this)" class="text-sm font-medium text-purple-600 hover:text-purple-800 mt-1 cursor-pointer hover:underline" style="display: none;">Xem thêm</button> </div>
                      </div>
                </div>
            </div>

            <%-- Có thể bạn quan tâm (Giữ nguyên cấu trúc và logic gốc) --%>
            <section id="recommendationSection" class="mb-10">
                <h2 class="text-2xl font-bold text-gray-800 mb-6 pb-2 border-b-2 border-purple-300 inline-block"><i class="fas fa-thumbs-up text-purple-500 mr-2"></i> Có Thể Bạn Quan Tâm</h2>
                 <asp:Panel ID="pnlRecommendations" runat="server">
                     <div class="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-6 gap-5 md:gap-7">
                         <asp:Repeater ID="rptRecommendations" runat="server">
                             <ItemTemplate>
                                  <%-- Sử dụng style card đã định nghĩa trong <style> --%>
                                 <div class="book-card-item bg-white rounded-lg shadow-md overflow-hidden flex flex-col h-full border border-gray-100 hover:shadow-xl hover:-translate-y-1 transition duration-300 ease-in-out group">
                                     <a href='<%# ResolveUrl("~/WebForm/User/chitietsach_user.aspx?IDSach=") + Eval("IDSach") %>' class="block relative overflow-hidden">
                                         <asp:Image ID="imgRecCover" runat="server" CssClass="rec-book-cover-img transition duration-300 ease-in-out group-hover:scale-105" ImageUrl='<%# GetImageUrl(Eval("DuongDanBiaSach")) %>' AlternateText='<%# "Bìa " + Eval("TenSach") %>' />
                                     </a>
                                     <div class="p-3 flex flex-col flex-grow"> <div> <h3 class="text-sm font-semibold text-gray-900 mb-1 line-clamp-2 group-hover:text-purple-700 transition duration-150" title='<%# Eval("TenSach") %>'> <a href='<%# ResolveUrl("~/WebForm/User/chitietsach_user.aspx?IDSach=") + Eval("IDSach") %>' class="hover:underline"><%# Eval("TenSach") %></a> </h3> <p class="text-xs text-gray-500 mb-2 truncate" title='<%# Eval("TacGia") %>'><%# Eval("TacGia") %></p> </div> </div>
                                 </div>
                             </ItemTemplate>
                         </asp:Repeater>
                     </div>
                 </asp:Panel>
                 <asp:Panel ID="pnlNoRecommendations" runat="server" Visible="false" CssClass="mt-6 text-center py-6 px-4 text-gray-500 bg-gray-50 border border-dashed border-gray-300 rounded-lg">
                     <asp:Label ID="lblNoRecText" runat="server" Text="Chưa có gợi ý nào."></asp:Label>
                 </asp:Panel>
            </section>

             <%-- Phần Đánh Giá Sách (Giữ nguyên cấu trúc và style gốc) --%>
             <section id="reviewSection" runat="server" class="bg-white p-6 md:p-8 rounded-lg shadow-md"> <%-- Dùng style gốc cho phần này --%>
                 <h3 class="text-xl font-semibold text-gray-800 mb-4 pb-3 border-b">Đánh Giá Sách</h3>
                 <div class="flex items-center space-x-4 mb-6 p-4 bg-gray-50 rounded-md border"> <div class="text-center"><p class="text-3xl font-bold text-yellow-500"> <asp:Literal ID="litAverageRating" runat="server" Text="N/A"></asp:Literal> <i class="fas fa-star text-2xl align-middle"></i> </p><p class="text-xs text-gray-500">(<asp:Literal ID="litTotalReviews" runat="server" Text="0"></asp:Literal> đánh giá)</p></div> </div>
                 <h4 class="text-lg font-semibold text-gray-700 mb-4">Tất cả đánh giá (<asp:Literal ID="litCommentReviewCount" runat="server" Text="0"></asp:Literal>)</h4>
                 <div class="mt-4 space-y-5"> <asp:Repeater ID="rptComments" runat="server" OnItemDataBound="rptComments_ItemDataBound"> <ItemTemplate> <div class="comment-item flex"> <asp:Image ID="imgCommentAvatar" runat="server" CssClass="comment-avatar" ImageUrl='<%# Eval("AnhNenUrl") %>' AlternateText='<%# "Avatar của " + Eval("TenHienThi") %>' /> <div class="flex-1"> <div class="flex items-center justify-between mb-1"><asp:HyperLink ID="hlUserProfileLink" runat="server" NavigateUrl='<%# Eval("IDNguoiDung", "~/WebForm/User/thanhvien.aspx?id={0}") %>' Text='<%# Eval("TenHienThi") %>' CssClass="font-semibold text-sm text-blue-600 hover:underline"></asp:HyperLink><span class="comment-meta"><%# Eval("EntryDate", "{0:dd/MM/yyyy HH:mm}") %></span></div> <asp:Panel ID="pnlRatingStars" runat="server" CssClass="rating-stars mb-1" Visible='<%# Eval("Rating") != DBNull.Value && Convert.ToInt32(Eval("Rating")) > 0 %>'><div class="text-yellow-400 text-sm"><asp:Literal ID="litStars" runat="server"></asp:Literal></div></asp:Panel> <div class="mt-1"><div id='commentContent_<%# Eval("EntryType") %>_<%# Eval("EntryID") %>' class="text-sm text-gray-700 comment-summary prose prose-sm max-w-none"><asp:Literal ID="litCommentText" runat="server" Text='<%# Eval("ContentText") %>' Mode="PassThrough"></asp:Literal></div><button type="button" onclick='toggleComment("<%# Eval("EntryType") %>", <%# Eval("EntryID") %>)' id='toggleCommentBtn_<%# Eval("EntryType") %>_<%# Eval("EntryID") %>' class="toggle-button" style="display: none;">Xem thêm</button></div> </div> </div> </ItemTemplate> </asp:Repeater> <asp:Panel ID="pnlNoComments" runat="server" Visible="false" CssClass="text-center text-gray-500 py-6"><p><asp:Label ID="lblNoCommentsText" runat="server" Text="Chưa có đánh giá nào cho sách này."></asp:Label></p></asp:Panel> </div>
             </section>
        </div> <%-- End Book Detail Container --%>

    </div> <%-- End Page Container --%>

    <%-- **** THÊM HTML CHO POPUP "ĐÃ CÓ TRONG GIỎ HÀNG" **** --%>
    <div id="alreadyInCartPopupOverlay" class="popup-overlay" onclick="hideAlreadyInCartPopup()"></div>
    <div id="alreadyInCartPopup" class="popup-content">
        <i class="fas fa-check-circle fa-2x text-blue-500 mb-4"></i>
        <h3 class="text-xl font-semibold mb-3">Đã Có Trong Giỏ Hàng</h3>
        <p class="text-sm text-gray-600 mb-6">
            Sách '<span id="popupCartBookName" class="font-semibold text-gray-800"></span>' đã có trong giỏ hàng của bạn.
        </p>
        <div class="flex justify-center space-x-4">
            <button type="button" onclick="goToCart();"
                class="px-6 py-2 bg-blue-600 hover:bg-blue-700 text-white rounded-lg font-medium transition duration-150">
                <i class="fas fa-shopping-cart mr-1.5"></i> Xem giỏ hàng
            </button>
            <button type="button" onclick="hideAlreadyInCartPopup();"
                class="px-6 py-2 bg-gray-200 hover:bg-gray-300 text-gray-800 rounded-lg font-medium transition duration-150">
                Đóng
            </button>
        </div>
    </div>
    <%-- **** KẾT THÚC HTML POPUP **** --%>

    <%-- JavaScript (Giữ lại toggle và popup giỏ hàng) --%>
     <script type="text/javascript">
         $(document).ready(function () {
             // Thêm độ trễ nhỏ (ví dụ: 150ms) trước khi kiểm tra chiều cao
             setTimeout(function () {
                 // Kiểm tra cho Mô tả
                 checkAndShowToggle('#descriptionContent', '#toggleDescriptionBtn');

                 // Kiểm tra cho các Bình luận (giữ nguyên)
                 $('.collapsible-content[id^="commentContent_"]').each(function () {
                     var contentId = '#' + $(this).attr('id');
                     var btnId = '#toggleCommentBtn_' + contentId.substring('#commentContent_'.length);
                     checkAndShowToggle(contentId, btnId);
                 });
             }, 150); // Tăng nhẹ độ trễ nếu cần
         });

         // Hàm kiểm tra và hiển thị nút Xem thêm
         function checkAndShowToggle(contentSelector, buttonSelector) {
             var contentElement = $(contentSelector);
             if (contentElement.length > 0) {
                 // Dùng scrollHeight và clientHeight là cách phổ biến nhất
                 // Thêm dung sai nhỏ (+1) để tránh lỗi làm tròn pixel
                 if (contentElement[0].scrollHeight > contentElement[0].clientHeight + 1) {
                     $(buttonSelector).show(); // Hiện nút nếu nội dung dài hơn vùng chứa
                 } else {
                     $(buttonSelector).hide(); // Ẩn nút nếu nội dung vừa đủ hoặc ngắn hơn
                 }
             } else {
                 // Log lỗi nếu không tìm thấy phần tử
                 console.error("Không tìm thấy phần tử:", contentSelector);
             }
         }

         // Hàm xử lý khi click nút Xem thêm/Ẩn bớt
         function toggleContent(contentId, buttonElement) {
             var contentDiv = $('#' + contentId);
             // Toggle class để mở rộng/thu gọn
             contentDiv.toggleClass('line-clamp-3 line-clamp-full');
             // Đổi chữ trên nút
             $(buttonElement).text(contentDiv.hasClass('line-clamp-full') ? 'Ẩn bớt' : 'Xem thêm');
         }

         // Hàm toggleComment gốc
         function toggleComment(entryType, entryId) { var contentId = '#commentContent_' + entryType + '_' + entryId; var btnId = '#toggleCommentBtn_' + entryType + '_' + entryId; $(contentId).toggleClass('comment-summary comment-full'); $(btnId).text($(contentId).hasClass('comment-full') ? 'Ẩn bớt' : 'Xem thêm'); }

          // JS Popup "Đã có trong giỏ"
          const alreadyInCartOverlay = document.getElementById('alreadyInCartPopupOverlay'); const alreadyInCartPopup = document.getElementById('alreadyInCartPopup'); const popupCartBookNameSpan = document.getElementById('popupCartBookName'); const cartPageUrl = '<%= ResolveUrl("~/WebForm/User/giohang_user.aspx") %>'; function showAlreadyInCartPopup(bookName) { if (popupCartBookNameSpan) { popupCartBookNameSpan.textContent = bookName; } fadeIn(alreadyInCartOverlay); fadeIn(alreadyInCartPopup); } function hideAlreadyInCartPopup() { fadeOut(alreadyInCartOverlay); fadeOut(alreadyInCartPopup); } function goToCart() { window.location.href = cartPageUrl; } function fadeIn(element) { if (!element) return; element.style.opacity = 0; element.style.display = 'block'; requestAnimationFrame(() => { element.style.transition = 'opacity 0.2s ease-out'; element.style.opacity = 1; }); } function fadeOut(element) { if (!element) return; element.style.opacity = 1; element.style.transition = 'opacity 0.2s ease-in'; element.style.opacity = 0; setTimeout(() => { element.style.display = 'none'; }, 200); }
     </script>
</asp:Content>