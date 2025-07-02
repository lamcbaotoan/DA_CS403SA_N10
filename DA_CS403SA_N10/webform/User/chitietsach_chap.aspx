<%@ Page Title="Chi Tiết Sách" Language="C#" MasterPageFile="~/WebForm/User/User.Master" AutoEventWireup="true" CodeBehind="chitietsach_chap.aspx.cs" Inherits="Webebook.WebForm.User.chitietsach_chap" %>

<%-- *** chitietsach_chap.aspx (Đã cập nhật phần Mô tả theo chitietsach_user.aspx) *** --%>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <%-- jQuery is required --%>
    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
    <%-- Font Awesome should be linked in User.Master or here --%>
    <%-- Example: <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.0.0/css/all.min.css" /> --%>
    <style>
        /* --- CSS Cần thiết cho Toggle Mô tả (từ chitietsach_user) --- */
        /* Đảm bảo bạn có plugin @tailwindcss/line-clamp hoặc định nghĩa thủ công */
        /* Ví dụ định nghĩa thủ công (nếu không dùng plugin): */
        /*
        .line-clamp-3 {
            overflow: hidden;
            display: -webkit-box;
            -webkit-box-orient: vertical;
            -webkit-line-clamp: 3;
        }
        */

        /* Class để bỏ giới hạn dòng */
        .line-clamp-full {
            -webkit-line-clamp: unset !important;
             /* overflow: visible !important; /* Có thể cần thiết tùy thuộc vào layout */
             max-height: none !important; /* Bỏ giới hạn chiều cao nếu có */
        }

        /* Style nút toggle từ chitietsach_user.aspx */
         .toggle-button {
             color: #4f46e5; /* Tím indigo-600 */
             cursor: pointer;
             font-size: 0.875rem; /* text-sm */
             font-weight: 500; /* font-medium */
             margin-top: 0.25rem; /* mt-1 */
             display: inline-block;
             transition: color 0.15s ease-in-out;
         }
         .toggle-button:hover {
             color: #4338ca; /* Tím indigo-700 */
             text-decoration: underline;
         }


        /* --- CSS gốc khác của chitietsach_chap.aspx --- */
        .read-continue-button.disabled {
            background-color: #d1d5db; /* gray-300 */
            border-color: #d1d5db; /* gray-300 */
            color: #6b7280; /* gray-500 */
            cursor: not-allowed;
            pointer-events: none; /* Prevent clicks */
            opacity: 0.7;
        }
        .read-continue-button.disabled:hover {
            background-color: #d1d5db; /* Keep same color on hover when disabled */
        }
        .chapter-icon {
            transition: transform 0.2s ease-in-out, color 0.15s ease-in-out;
        }
        .chapter-list-item:hover .chapter-icon {
             transform: translateX(4px);
             color: #4f46e5; /* indigo-600 */
        }
        /* Fix for comment vertical line on last item */
        .comment-list > li:last-child .comment-timeline-line {
             display: none;
        }
        /* Ensure prose styles apply correctly within comment */
        .comment-text-prose {
            line-height: 1.625; /* Consistent line height */
        }
        .comment-text-prose p:first-of-type { margin-top: 0; }
        .comment-text-prose p:last-of-type { margin-bottom: 0; }

        /* Các style khác nếu cần */

    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="container mx-auto px-4 sm:px-6 lg:px-8 py-8 md:py-12">

        <%-- Back Button --%>
        <div class="mb-6">
            <asp:HyperLink ID="hlBackToBookshelf" runat="server" NavigateUrl="~/WebForm/User/tusach.aspx"
                CssClass="inline-flex items-center px-4 py-2 border border-gray-300 shadow-sm text-sm font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 transition ease-in-out duration-150 group">
                <i class="fas fa-arrow-left mr-2 text-gray-400 group-hover:text-gray-500 transition-colors"></i>
                Quay lại Tủ sách
            </asp:HyperLink>
        </div>

        <%-- Message Label (Success/Error) --%>
        <asp:Label ID="lblMessage" runat="server" EnableViewState="false" Visible="false"></asp:Label> <%-- CSS class set dynamically from code-behind --%>

        <%-- Main Content Panel --%>
        <asp:Panel ID="pnlBookDetails" runat="server" Visible="false"> <%-- Start hidden, shown if data loads --%>
            <%-- Book Details Section --%>
            <div class="bg-white shadow-lg rounded-lg overflow-hidden md:flex md:gap-8 lg:gap-12 mb-10">
                <%-- Cover Image Column --%>
                <div class="md:w-1/3 p-6 flex justify-center items-start md:items-center">
                     <div class="aspect-[2/3] w-full max-w-[250px] md:max-w-none">
                         <asp:Image ID="imgBiaSach" runat="server"
                                  CssClass="rounded-md shadow-md w-full h-full object-cover border border-gray-100 bg-gray-50" /> <%-- Added bg-gray-50 as subtle placeholder bg --%>
                     </div>
                </div>

                <%-- Book Info Column --%>
                <div class="md:w-2/3 p-6 lg:p-8 flex flex-col space-y-5">
                    <%-- Top section: Title, Author, Genres, Read Button --%>
                    <div>
                        <h1 class="text-3xl lg:text-4xl font-bold text-gray-900 mb-2 leading-tight">
                            <asp:Label ID="lblTenSach" runat="server"></asp:Label>
                        </h1>
                        <p class="text-lg text-gray-500 mb-5">
                            Tác giả: <asp:Label ID="lblTacGia" runat="server" CssClass="font-medium text-gray-700"></asp:Label>
                        </p>
                        <%-- Genres (Badges) --%>
                        <div class="mb-5">
                            <span class="text-sm font-semibold text-gray-500 mr-2 align-middle">Thể loại:</span>
                            <div class="inline-flex flex-wrap gap-2 align-middle">
                                <asp:Repeater ID="rptGenres" runat="server">
                                    <ItemTemplate>
                                        <span class="inline-block bg-indigo-100 text-indigo-800 text-xs font-semibold px-3 py-1 rounded-full">
                                            <%# Container.DataItem %>
                                        </span>
                                    </ItemTemplate>
                                </asp:Repeater>
                                <asp:Label ID="lblNoGenres" runat="server" Visible="false" Text="Chưa phân loại" CssClass="text-xs italic text-gray-400 align-middle"></asp:Label>
                            </div>
                        </div>
                        <%-- Read/Continue Button --%>
                        <div class="mb-6">
                             <asp:HyperLink ID="hlReadContinue" runat="server" Visible="false"
                                  CssClass="read-continue-button inline-flex items-center justify-center px-6 py-3 border border-transparent text-base font-medium rounded-md shadow-sm text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 transition duration-150 ease-in-out">
                                 <%-- Text/Icon set dynamically from code-behind --%>
                             </asp:HyperLink>
                        </div>
                    </div>

                    <%-- Middle Section: Other Metadata (Pushed down by flex-col) --%>
                    <div class="space-y-3 text-sm text-gray-700 border-t border-gray-200 pt-5 mt-auto">
                        <div class="flex"> <span class="font-medium text-gray-500 w-28 flex-shrink-0">Loại sách:</span> <asp:Label ID="lblLoaiSach" runat="server" CssClass="min-w-0 flex-1"></asp:Label> </div>
                        <div class="flex"> <span class="font-medium text-gray-500 w-28 flex-shrink-0">NXB:</span> <asp:Label ID="lblNhaXuatBan" runat="server" CssClass="min-w-0 flex-1"></asp:Label> </div>
                        <div class="flex"> <span class="font-medium text-gray-500 w-28 flex-shrink-0">Nhóm dịch:</span> <asp:Label ID="lblNhomDich" runat="server" CssClass="min-w-0 flex-1"></asp:Label> </div>
                        <div class="flex"> <span class="font-medium text-gray-500 w-28 flex-shrink-0">Trạng thái:</span> <asp:Label ID="lblTrangThai" runat="server" CssClass="font-semibold text-green-600 min-w-0 flex-1"></asp:Label> </div>
                    </div>

                    <%-- **** FIXED DESCRIPTION SECTION **** --%>
                    <div class="border-t border-gray-200 pt-5">
                        <h3 class="text-lg font-semibold text-gray-800 mb-2">Mô tả</h3>
                        <div id="descriptionContainer">
                             <%-- Div chứa nội dung, dùng prose styling gốc và line-clamp --%>
                             <div id="descriptionContent" class="prose prose-sm max-w-none text-gray-700 leading-relaxed line-clamp-3">
                                <asp:Literal ID="lblMoTa" runat="server" Mode="PassThrough"></asp:Literal>
                             </div>
                             <%-- Nút toggle giống chitietsach_user --%>
                             <button type="button" id="toggleDescriptionBtn" onclick="toggleContent('descriptionContent', this)" class="toggle-button" style="display: none;">
                                 Xem thêm <i class="fas fa-chevron-down text-xs ml-1"></i>
                             </button>
                        </div>
                    </div>
                     <%-- **** END FIXED DESCRIPTION SECTION **** --%>
                </div>
            </div>

            <%-- Chapter List Section --%>
            <div class="bg-white shadow-lg rounded-lg p-6 lg:p-8 mb-10">
                <h2 class="text-xl font-semibold text-gray-900 mb-5 border-b border-gray-200 pb-4">Danh sách chương</h2>
                <asp:Label ID="lblNoChapters" runat="server" Text="Truyện này hiện chưa có chương nào." CssClass="text-gray-500 text-sm italic block" Visible="false"></asp:Label>
                <asp:Panel ID="pnlChapterList" runat="server" Visible="false" CssClass="border border-gray-200 rounded-md overflow-hidden max-h-96 overflow-y-auto">
                    <ul class="divide-y divide-gray-200 list-none p-0 m-0">
                        <asp:Repeater ID="rptChapters" runat="server">
                            <ItemTemplate>
                                <li class="chapter-list-item group">
                                    <a href='<%# ResolveUrl("~/WebForm/User/docsach.aspx?IDSach=") + Eval("IDSach") + "&SoChuong=" + Eval("SoChuong") %>'
                                       class="flex justify-between items-center px-4 py-3.5 hover:bg-indigo-50 transition duration-150 ease-in-out">
                                        <span class="text-sm text-gray-800 group-hover:text-indigo-700 truncate pr-4">
                                            Chương <%# Eval("SoChuong") %>
                                            <%# !string.IsNullOrEmpty(Eval("TenChuong")?.ToString()) ? ": " + Server.HtmlEncode(Eval("TenChuong").ToString()) : "" %>
                                        </span>
                                        <i class="fas fa-chevron-right text-xs text-gray-400 chapter-icon flex-shrink-0"></i>
                                    </a>
                                </li>
                            </ItemTemplate>
                        </asp:Repeater>
                    </ul>
                </asp:Panel>
            </div>

             <%-- ========= REDESIGNED COMMENT SECTION V2 ========= --%>
             <div class="bg-white shadow-md rounded-xl overflow-hidden">
                <div class="p-6 lg:p-8">
                    <h2 class="text-2xl font-bold text-gray-800 mb-6">Bình luận gần đây</h2>

                    <asp:Label ID="lblNoBookComments" runat="server" Text="Chưa có bình luận nào cho sách này." CssClass="text-gray-500 italic block my-6 text-center" Visible="false"></asp:Label>

                    <div class="flow-root">
                        <%-- Added comment-list class for CSS targeting --%>
                        <ul role="list" class="comment-list -mb-8">
                            <asp:Repeater ID="rptBookComments" runat="server">
                                <ItemTemplate>
                                    <li>
                                        <div class="relative pb-8 group">
                                            <%-- Added comment-timeline-line class for CSS targeting --%>
                                            <span class="comment-timeline-line absolute top-5 left-5 -ml-px h-full w-0.5 bg-gray-200" aria-hidden="true"></span>

                                            <div class="relative flex items-start space-x-4">
                                                <%-- Avatar --%>
                                                <div class="flex-shrink-0">
                                                     <asp:Image ID="imgCommentAvatar" runat="server"
                                                              ImageUrl='<%# GetAvatarUrl(Eval("AnhNen")) %>'
                                                              AlternateText='<%# "Avatar của " + Eval("TenHienThi") %>'
                                                              CssClass="h-10 w-10 rounded-full object-cover bg-gray-100 ring-2 ring-white group-hover:ring-gray-100 transition duration-150 ease-in-out" />
                                                </div>

                                                <%-- Main Content --%>
                                                <div class="min-w-0 flex-1 bg-white p-0.5 rounded-md transition duration-150 ease-in-out group-hover:bg-gray-50">
                                                    <div>
                                                        <%-- Author Name & Metadata Row --%>
                                                        <div class="flex justify-between items-center text-sm mb-1 flex-wrap gap-x-2"> <%-- Added flex-wrap and gap --%>
                                                             <asp:HyperLink ID="hlCommentAuthor" runat="server"
                                                                    CssClass="font-medium text-gray-900 hover:text-indigo-600 whitespace-nowrap"
                                                                    NavigateUrl='<%# Eval("IDNguoiDung", "~/WebForm/User/thanhvien.aspx?id={0}") %>'
                                                                    Text='<%# Eval("TenHienThi") %>'>
                                                             </asp:HyperLink>

                                                             <%-- Metadata (Chapter + Date) --%>
                                                             <div class="flex-shrink-0 flex items-center space-x-2 text-xs text-gray-500 whitespace-nowrap">
                                                                 <asp:Panel runat="server" Visible='<%# Eval("SoChap") != DBNull.Value && Eval("SoChap") != null %>'>
                                                                     <span class="inline-flex items-center px-2 py-0.5 rounded-full font-medium bg-indigo-100 text-indigo-700">
                                                                         <%-- Optional Icon: <i class="fas fa-bookmark mr-1 text-indigo-500"></i> --%>
                                                                         Chương <%# Eval("SoChap") %>
                                                                     </span>
                                                                 </asp:Panel>
                                                                 <span title='<%# Eval("NgayBinhLuan", "{0:dd/MM/yyyy HH:mm:ss}") %>'>
                                                                      <%-- Optional Icon: <i class="far fa-clock mr-1"></i> --%>
                                                                      <%# FormatRelativeTime(Eval("NgayBinhLuan")) %>
                                                                 </span>
                                                             </div>
                                                        </div>
                                                    </div>

                                                    <%-- Comment Text - Using prose --%>
                                                    <div class="mt-1 prose prose-sm max-w-none comment-text-prose text-gray-700 prose-a:text-indigo-600 hover:prose-a:text-indigo-800 break-words">
                                                         <asp:Literal ID="litCommentText" runat="server" Text='<%# FormatCommentText(Eval("BinhLuan")) %>' Mode="PassThrough"></asp:Literal>
                                                    </div>
                                                </div>
                                            </div>
                                        </div>
                                    </li>
                                </ItemTemplate>
                            </asp:Repeater>
                        </ul>
                    </div> <%-- End flow-root --%>
                </div>
            </div>
             <%-- ========= END REDESIGNED COMMENT SECTION V2 ========= --%>

        </asp:Panel> <%-- End pnlBookDetails --%>
    </div> <%-- End Container --%>

    <%-- **** FIXED JAVASCRIPT **** --%>
    <script type="text/javascript">
        $(document).ready(function () {
            // Thêm độ trễ để đảm bảo DOM render xong trước khi kiểm tra chiều cao
            setTimeout(function () {
                // Chỉ kiểm tra cho phần Mô tả của trang này
                checkAndShowToggle('#descriptionContent', '#toggleDescriptionBtn');

                // Thêm các lời gọi checkAndShowToggle khác nếu cần cho các phần tử khác
            }, 150);
        });

        // Hàm kiểm tra overflow và hiển thị nút Xem thêm/Ẩn bớt
        function checkAndShowToggle(contentSelector, buttonSelector) {
            var contentElement = $(contentSelector);
            var buttonElement = $(buttonSelector);

            if (contentElement.length > 0 && buttonElement.length > 0) {
                // So sánh scrollHeight (chiều cao thực tế) với clientHeight (chiều cao nhìn thấy)
                // +2 để xử lý sai số nhỏ
                if (contentElement[0].scrollHeight > contentElement[0].clientHeight + 2) {
                    buttonElement.show(); // Hiện nút
                    // Đặt text ban đầu là Xem thêm
                    buttonElement.html('Xem thêm <i class="fas fa-chevron-down text-xs ml-1"></i>');
                } else {
                    buttonElement.hide(); // Ẩn nút
                }
            } else {
                // Ghi log lỗi nếu không tìm thấy element
                if (contentElement.length === 0) console.error("CheckToggle Error: Content element not found:", contentSelector);
                if (buttonElement.length === 0) console.error("CheckToggle Error: Button element not found:", buttonSelector);
            }
        }

        // Hàm xử lý click nút Xem thêm/Ẩn bớt
        function toggleContent(contentId, buttonElement) {
            var contentDiv = $('#' + contentId);
            var $button = $(buttonElement); // Lưu jQuery object của nút

            // Toggle class line-clamp-3 (giới hạn dòng) và line-clamp-full (bỏ giới hạn)
            contentDiv.toggleClass('line-clamp-3 line-clamp-full');

            // Đổi text và icon trên nút dựa vào class hiện tại
            if (contentDiv.hasClass('line-clamp-full')) {
                // Đang mở rộng -> Đổi thành "Ẩn bớt"
                $button.html('Ẩn bớt <i class="fas fa-chevron-up text-xs ml-1"></i>');
            } else {
                // Đang thu gọn -> Đổi thành "Xem thêm"
                $button.html('Xem thêm <i class="fas fa-chevron-down text-xs ml-1"></i>');
            }
        }
    </script>
     <%-- **** END FIXED JAVASCRIPT **** --%>
</asp:Content>