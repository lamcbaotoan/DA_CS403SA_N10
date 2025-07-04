<%@ Page Title="Đọc sách" Language="C#" MasterPageFile="~/WebForm/User/User.Master" AutoEventWireup="true" CodeBehind="docsach.aspx.cs" Inherits="Webebook.WebForm.User.docsach" %>
<%@ Import Namespace="System.Linq" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <%-- jQuery --%>
    <script src="https://code.jquery.com/jquery-3.6.0.min.js"></script>
    <%-- Font Awesome --%>
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css" />
    <%-- Google Fonts --%>
    <link rel="preconnect" href="https://fonts.googleapis.com">
    <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin>
    <link href="https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700&family=Lora:ital,wght@0,400..700;1,400..700&family=Merriweather:ital,wght@0,400;0,700;1,400;1,700&family=Source+Sans+Pro:wght@400;600;700&display=swap" rel="stylesheet">

    <style>
        html, body {
            overflow-x: hidden; /* Ngăn cuộn ngang body */
        }
        /* Keyframes cho spinner */
        @keyframes spin { 0% { transform: translateY(-50%) rotate(0deg); } 100% { transform: translateY(-50%) rotate(360deg); } }
        .comment-loading-spinner { display: none; position: absolute; right: 115px; top: 50%; transform: translateY(-50%); border: 3px solid #f3f3f3; border-top: 3px solid #4f46e5; border-radius: 50%; width: 20px; height: 20px; animation: spin 1s linear infinite; }

        /* Ẩn/hiện thanh điều hướng */
        .reader-nav { transition: transform 0.3s ease-in-out; }
        .reader-nav.hidden-by-scroll { transform: translateY(calc(100% + 1.5rem)); }

        /* --- CSS CHO CÀI ĐẶT HIỂN THỊ --- */
        .reader-settings-popup .setting-btn[aria-pressed="true"], .reader-settings-popup .setting-select option:checked { background-color: #e0e7ff; border-color: #4f46e5; color: #3730a3; font-weight: 500; }
        .reader-settings-popup .setting-btn[data-setting="bgColor"][aria-pressed="true"] { box-shadow: 0 0 0 2px #fff, 0 0 0 4px #4f46e5; }

        /* Kiểu Nền Đọc Truyện Chữ & Ảnh (Áp dụng cho container) */
        .reader-container { transition: background-color 0.3s, color 0.3s; padding: 0; /* Mặc định tràn cạnh */}
        .reader-container.bg-sepia { background-color: #fbf0d9; color: #5c4d36; }
        .reader-container.bg-gray-200 { background-color: #e5e7eb; color: #1f2937; }
        .reader-container.bg-gray-800 { background-color: #1f2937; color: #d1d5db; }
        .reader-container.bg-black { background-color: #000000; color: #9ca3af; }

        /* Màu link nền tối */
        .reader-container.bg-gray-800 a, .reader-container.bg-black a { color: #93c5fd; }
        .reader-container.bg-gray-800 a:hover, .reader-container.bg-black a:hover { color: #dbeafe; }

        /* Reset prose nền tối */
        .reader-container.bg-gray-800 .prose, .reader-container.bg-black .prose { color: inherit; }
        .reader-container.bg-gray-800 .prose :where(a):not(:where([class~="not-prose"] *)), .reader-container.bg-black .prose :where(a):not(:where([class~="not-prose"] *)) { color: #93c5fd; }
        .reader-container.bg-gray-800 .prose :where(a):not(:where([class~="not-prose"] *)):hover, .reader-container.bg-black .prose :where(a):not(:where([class~="not-prose"] *)):hover { color: #dbeafe; }
        .reader-container.bg-gray-800 .prose :where(strong):not(:where([class~="not-prose"] *)), .reader-container.bg-black .prose :where(strong):not(:where([class~="not-prose"] *)) { color: inherit; }
        .prose-invert { --tw-prose-body: var(--tw-prose-invert-body,#d1d5db); --tw-prose-headings: var(--tw-prose-invert-headings,#fff); --tw-prose-lead: var(--tw-prose-invert-lead,#9ca3af); --tw-prose-links: var(--tw-prose-invert-links,#fff); --tw-prose-bold: var(--tw-prose-invert-bold,#fff); --tw-prose-counters: var(--tw-prose-invert-counters,#9ca3af); --tw-prose-bullets: var(--tw-prose-invert-bullets,#4b5563); --tw-prose-hr: var(--tw-prose-invert-hr,#374151); --tw-prose-quotes: var(--tw-prose-invert-quotes,#f3f4f6); --tw-prose-quote-borders: var(--tw-prose-invert-quote-borders,#374151); --tw-prose-captions: var(--tw-prose-invert-captions,#9ca3af); --tw-prose-code: var(--tw-prose-invert-code,#fff); --tw-prose-pre-code: var(--tw-prose-invert-pre-code,#d1d5db); --tw-prose-pre-bg: var(--tw-prose-invert-pre-bg,rgb(0 0 0 / 50%)); --tw-prose-th-borders: var(--tw-prose-invert-th-borders,#4b5563); --tw-prose-td-borders: var(--tw-prose-invert-td-borders,#374151); }

        /* --- CSS IMAGE READER - Final --- */
        .image-reader { display: flex; overflow: hidden; width: 100%; box-sizing: border-box; position: relative; background-color: #fff; }
        .image-reader .image-item { box-sizing: border-box; flex-shrink: 0; display: flex; justify-content: center; align-items: center; overflow: hidden; }
        .image-reader .reader-image { display: block; margin: 0 auto; max-width: 100%; max-height: 100%; object-fit: contain; } /* contain là an toàn nhất */
        .image-reader-vertical { flex-direction: column; overflow-y: auto; overflow-x: hidden; height: auto; background-color: transparent; }
        .image-reader-vertical .image-item { width: 100%; margin-bottom: 1px; height: auto; aspect-ratio: unset; padding: 0; /* Sát nhau */ }
        .image-reader-vertical.fit-width .reader-image { width: 100%; height: auto; max-width: 100%; /* Đảm bảo không vượt quá */ }
        .image-reader-vertical.fit-original .image-item { width: 100%; /* Item vẫn chiếm 100% để căn giữa ảnh */ text-align: center; }
        .image-reader-vertical.fit-original .reader-image { max-width: 665px; /* Giới hạn chiều rộng */ width: auto; height: auto; display: inline-block; /* Để căn giữa hoạt động */ }
        .image-reader-horizontal { flex-direction: row; overflow-x: auto; overflow-y: hidden; scroll-snap-type: x proximity; -webkit-overflow-scrolling: touch; height: calc(100vh - 60px); }
        .image-reader-horizontal .image-item { scroll-snap-align: center; height: 100%; padding: 0 2px; /* Thêm lại khoảng cách nhỏ ngang */ display: flex; justify-content: center; align-items: center; overflow: hidden;}
        .image-reader-horizontal.fit-width .reader-image { max-width: 100%; max-height: 100%; width: auto; height: auto; object-fit: contain; } /* Vừa cả rộng và cao của item */
        .image-reader-horizontal.fit-original .reader-image { max-width: none; max-height: 100%; /* Giới hạn chiều cao */ height: auto; width: auto; object-fit: contain; }
        .image-reader-horizontal.page-single .image-item { width: 100%; }
        .image-reader-horizontal.page-double .image-item { width: 50%; }
        .image-reader-horizontal.page-triple .image-item { width: 33.333333%; }
        .image-reader-horizontal.read-rtl { direction: rtl; }
        .image-reader-horizontal.read-rtl .image-item { direction: ltr; }
        #imageTapOverlay { position: absolute; top: 0; left: 0; width: 100%; height: 100%; z-index: 10; display: none; cursor: pointer; }
        #imageTapOverlay.active { display: block; }

        /* --- START: ANTI-COPY CSS --- */
        /* Chặn người dùng bôi đen/chọn nội dung trong khu vực đọc */
        #readerContent {
            -webkit-user-select: none; /* Safari */
            -moz-user-select: none;    /* Firefox */
            -ms-user-select: none;     /* IE10+/Edge */
            user-select: none;         /* Standard */
        }

        /* Chặn người dùng click/chuột phải trực tiếp vào ảnh để lưu */
        .reader-image {
            pointer-events: none;
        }

        /* Ẩn toàn bộ nội dung khi người dùng cố gắng in trang */
        @media print {
            body * {
                display: none !important;
            }
            body::before {
                content: "Việc in ấn nội dung từ trang này không được phép.";
                display: block;
                padding: 2rem;
                font-size: 1.5rem;
                font-weight: bold;
                text-align: center;
                color: #000;
            }
        }
        /* --- END: ANTI-COPY CSS --- */

    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <%-- ScriptManager --%>
    <%-- <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager> --%>

    <%-- Thông báo --%>
    <asp:Label ID="lblMessage" runat="server" EnableViewState="false" Visible="false"></asp:Label>

    <%-- Container chính (Tràn cạnh, padding 0) --%>
    <div id="readerContentContainer" class="reader-container mt-0 mb-6 bg-white rounded-lg shadow-lg min-h-[80vh] relative text-gray-800" style="padding: 0;">
        <%-- Tiêu đề (Giới hạn chiều rộng riêng) --%>
        <div class="text-center mb-8 border-b border-gray-200 pb-5 max-w-4xl mx-auto px-4 md:px-0">
            <h1 class="text-2xl md:text-3xl font-bold leading-tight"> <asp:Label ID="lblBookTitleRead" runat="server" Text="Đang tải..."></asp:Label> </h1>
            <p class="text-base text-gray-600 mt-2"> <asp:Label ID="lblChapterInfoRead" runat="server" Text="Đang tải..."></asp:Label> </p>
        </div>

        <%-- Khu vực Nội dung đọc --%>
        <div id="readerContent" class="reader-content">
            <%-- Panel TEXT --%>
            <asp:Panel ID="pnlTextContent" runat="server" Visible="false">
                <div id="textContentWrapper" class="prose prose-lg prose-indigo max-w-none px-4 md:px-6 py-2"> <asp:Literal ID="litTextContent" runat="server" Mode="PassThrough"></asp:Literal> </div>
            </asp:Panel>
            <%-- Panel IMAGE --%>
            <asp:Panel ID="pnlImageContent" runat="server" Visible="false">
                <div id="imageContentWrapper" class="not-prose image-reader image-reader-vertical fit-width"> <%-- Mặc định dọc, vừa khung --%>
                    <asp:Repeater ID="rptImageContent" runat="server">
                        <ItemTemplate>
                            <div class="image-item" data-index="<%# Container.ItemIndex %>"> <img src="<%# Container.DataItem %>" alt="Trang <%# Container.ItemIndex + 1 %>" loading="lazy" class="reader-image block mx-auto" /> </div>
                        </ItemTemplate>
                    </asp:Repeater>
                </div>
            </asp:Panel>
            <%-- Panel FILE VIEWER --%>
            <asp:Panel ID="pnlFileViewer" runat="server" Visible="false"> <div class="max-w-4xl mx-auto px-4 md:px-0"> <asp:Literal ID="litFileViewerContent" runat="server" Mode="PassThrough"></asp:Literal> </div> </asp:Panel>
        </div>

        <%-- Khu vực Bình luận (Giới hạn chiều rộng) --%>
        <div class="max-w-3xl mx-auto px-4 md:px-0 mt-8">
            <asp:UpdatePanel ID="UpdatePanelComments" runat="server" UpdateMode="Conditional">
                <ContentTemplate>
                    <div class="comment-section pt-8 border-t border-gray-200">
                        <h2 class="text-xl font-semibold text-gray-800 mb-6">Bình luận (<asp:Label ID="lblCommentCount" runat="server" Text="0"></asp:Label>)</h2>
                        <div class="comment-form mb-8">
                            <asp:TextBox ID="txtCommentInput" runat="server" TextMode="MultiLine" placeholder="Viết bình luận của bạn..." Rows="3" CssClass="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:ring-indigo-500 focus:border-indigo-500 text-sm"></asp:TextBox>
                            <asp:RequiredFieldValidator ID="rfvComment" runat="server" ControlToValidate="txtCommentInput" ValidationGroup="CommentValidation" ErrorMessage="Vui lòng nhập nội dung." Display="Dynamic" CssClass="text-red-600 text-xs mt-1"></asp:RequiredFieldValidator>
                            <div class="text-right mt-3 relative">
                                <asp:Button ID="btnSubmitComment" runat="server" Text="Gửi bình luận" OnClick="btnSubmitComment_Click" ValidationGroup="CommentValidation" CssClass="inline-flex items-center px-4 py-2 bg-indigo-600 border border-transparent rounded-md font-semibold text-sm text-white shadow-sm hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 disabled:opacity-50" />
                                <div id="commentSpinner" class="comment-loading-spinner"></div>
                            </div>
                        </div>
                        <asp:Label ID="lblNoComments" runat="server" Text="Chưa có bình luận nào." CssClass="text-gray-500 text-sm italic block text-center" Visible="false"></asp:Label>
                        <div class="comment-list space-y-6">
                            <asp:Repeater ID="rptComments" runat="server">
                                <ItemTemplate>
                                    <div class="comment-item flex gap-3">
                                        <div class="comment-avatar flex-shrink-0"> <asp:Image ID="imgCommentAvatar" runat="server" ImageUrl='<%# GetAvatarUrl(Eval("AnhNen")) %>' AlternateText='<%# Eval("TenHienThi") %>' CssClass="w-10 h-10 rounded-full object-cover border border-gray-200" /> </div>
                                        <div class="comment-body flex-grow bg-gray-50 p-3 rounded-md border border-gray-100">
                                            <div class="comment-header flex justify-between items-baseline mb-1"> <span class="comment-author font-semibold text-gray-800 text-sm"><%# Eval("TenHienThi") %></span> <span class="comment-date text-xs text-gray-500"><%# Eval("NgayBinhLuan", "{0:dd/MM/yyyy HH:mm}") %></span> </div>
                                            <div class="comment-text text-sm text-gray-700 leading-relaxed prose prose-sm max-w-none"> <asp:Literal ID="litCommentText" runat="server" Text='<%# FormatCommentText(Eval("BinhLuan")) %>' Mode="PassThrough"></asp:Literal> </div>
                                        </div>
                                    </div>
                                </ItemTemplate>
                            </asp:Repeater>
                        </div>
                    </div>
                </ContentTemplate>
                <Triggers> <asp:AsyncPostBackTrigger ControlID="btnSubmitComment" EventName="Click" /> </Triggers>
            </asp:UpdatePanel>
        </div>

    </div> <%-- Kết thúc reader-container --%>

    <%-- Panel Điều hướng --%>
    <asp:Panel ID="pnlNavigation" runat="server" CssClass="reader-nav fixed bottom-2 right-2 md:bottom-4 md:right-4 flex flex-col items-center gap-2 z-40" Visible="false">
        <div id="nav-wrapper" class="p-1 rounded-full bg-gray-900/60 backdrop-blur-sm">
            <asp:HyperLink ID="hlPrevChap" runat="server" ToolTip="Chương trước" CssClass="flex items-center justify-center w-11 h-11 hover:bg-gray-700/80 text-white rounded-full transition duration-200 ease-in-out"><i class="fas fa-chevron-left text-sm"></i></asp:HyperLink>
            <asp:HyperLink ID="hlNextChap" runat="server" ToolTip="Chương sau" CssClass="flex items-center justify-center w-11 h-11 hover:bg-gray-700/80 text-white rounded-full transition duration-200 ease-in-out"><i class="fas fa-chevron-right text-sm"></i></asp:HyperLink>
            <asp:LinkButton ID="btnToggleChapterList" runat="server" OnClientClick="toggleChapterPopup(); return false;" ToolTip="Danh sách chương" Visible="false" CssClass="flex items-center justify-center w-11 h-11 hover:bg-gray-700/80 text-white rounded-full transition duration-200 ease-in-out cursor-pointer"><i class="fas fa-list-ul text-sm"></i></asp:LinkButton>
            <asp:HyperLink ID="hlInfo" runat="server" ToolTip="Thông tin sách" CssClass="flex items-center justify-center w-11 h-11 hover:bg-gray-700/80 text-white rounded-full transition duration-200 ease-in-out"><i class="fas fa-info-circle text-sm"></i></asp:HyperLink>
            <asp:HyperLink ID="hlBookshelf" runat="server" ToolTip="Tủ sách" CssClass="flex items-center justify-center w-11 h-11 hover:bg-gray-700/80 text-white rounded-full transition duration-200 ease-in-out"><i class="fas fa-book-bookmark text-sm"></i></asp:HyperLink>
            <asp:LinkButton ID="btnToggleReaderSettings" runat="server" OnClientClick="toggleReaderSettingsPopup(); return false;" ToolTip="Cài đặt hiển thị" CssClass="flex items-center justify-center w-11 h-11 hover:bg-gray-700/80 text-white rounded-full transition duration-200 ease-in-out cursor-pointer"><i class="fas fa-cog text-sm"></i></asp:LinkButton>
        </div>
    </asp:Panel>

    <%-- Popup Danh sách chương --%>
    <asp:Panel ID="pnlChapterListPopup" runat="server" CssClass="chapter-popup fixed inset-0 bg-black/60 z-50 hidden items-center justify-center p-4" Style="display: none;">
        <div class="chapter-popup-content bg-white p-5 md:p-6 rounded-lg shadow-xl max-h-[80vh] overflow-y-auto w-full max-w-sm relative">
            <button type="button" class="chapter-popup-close absolute top-2 right-2 text-gray-500 hover:text-gray-800 text-2xl leading-none" title="Đóng" onclick="toggleChapterPopup();">×</button>
            <h3 class="text-lg font-semibold mb-4 text-center text-gray-800">Danh sách chương</h3>
            <ul class="popup-chapter-list -mx-1">
                <asp:Repeater ID="rptChapterListPopup" runat="server"> <ItemTemplate> <li> <a href='<%# ResolveUrl("~/WebForm/User/docsach.aspx?IDSach=") + Eval("IDSach") + "&SoChuong=" + Eval("SoChuong") %>' onclick="toggleChapterPopup();" class="block py-2 px-3 text-sm text-gray-700 hover:bg-indigo-50 rounded-md transition duration-150 ease-in-out truncate <%# Convert.ToInt32(Eval("SoChuong")) == currentChuong ? "bg-indigo-100 font-semibold text-indigo-700" : "" %>"> Chương <%# Eval("SoChuong") %> <%# !string.IsNullOrEmpty(Eval("TenChuong")?.ToString()) ? ": " + Server.HtmlEncode(Eval("TenChuong").ToString()) : "" %> </a> </li> </ItemTemplate> </asp:Repeater>
            </ul>
        </div>
    </asp:Panel>

    <%-- Popup Cài đặt Đọc --%>
    <asp:Panel ID="pnlReaderSettingsPopup" runat="server" CssClass="reader-settings-popup fixed inset-0 bg-black/60 z-50 hidden items-center justify-center p-4" Style="display: none;">
        <div class="reader-settings-content bg-white p-5 md:p-6 rounded-lg shadow-xl max-h-[85vh] overflow-y-auto w-full max-w-xs relative text-sm">
            <button type="button" class="reader-settings-close absolute top-2 right-2 text-gray-500 hover:text-gray-800 text-2xl leading-none" title="Đóng" onclick="toggleReaderSettingsPopup();">×</button>
            <h3 class="text-base font-semibold mb-5 text-center text-gray-800 border-b pb-2">Cài đặt hiển thị</h3>
            <%-- Cài đặt cho Truyện Chữ (Chỉ còn Màu nền, Font, Căn chỉnh) --%>
            <div id="textReaderSettings" class="space-y-4" style="display: none;">
                <div><label class="block font-medium text-gray-700 mb-1.5">Màu nền</label><div class="flex flex-wrap gap-2" data-setting="bgColor"><button type="button" data-value="bg-white" class="setting-btn w-8 h-8 rounded border border-gray-300 bg-white focus:outline-none focus:ring-2 focus:ring-offset-1 focus:ring-indigo-500" title="Trắng"></button><button type="button" data-value="bg-sepia" class="setting-btn w-8 h-8 rounded border border-gray-300 bg-[#fbf0d9] focus:outline-none focus:ring-2 focus:ring-offset-1 focus:ring-indigo-500" title="Sepia"></button><button type="button" data-value="bg-gray-200" class="setting-btn w-8 h-8 rounded border border-gray-300 bg-gray-200 focus:outline-none focus:ring-2 focus:ring-offset-1 focus:ring-indigo-500" title="Xám"></button><button type="button" data-value="bg-gray-800" class="setting-btn w-8 h-8 rounded border border-gray-600 bg-gray-800 focus:outline-none focus:ring-2 focus:ring-offset-1 focus:ring-indigo-500" title="Tối"></button><button type="button" data-value="bg-black" class="setting-btn w-8 h-8 rounded border border-gray-600 bg-black focus:outline-none focus:ring-2 focus:ring-offset-1 focus:ring-indigo-500" title="Đen"></button></div></div>
                <div><label for="selectFontFamily" class="block font-medium text-gray-700 mb-1">Font chữ</label><select id="selectFontFamily" data-setting="fontFamily" class="setting-select mt-1 block w-full pl-3 pr-10 py-1.5 text-base border border-gray-300 focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm rounded-md"><option value="'Times New Roman', Times, serif">Times New Roman</option><option value="'Inter', sans-serif">Inter</option><option value="Arial, Helvetica, sans-serif">Arial</option><option value="'Lora', serif">Lora</option><option value="'Source Sans Pro', sans-serif">Source Sans Pro</option><option value="'Merriweather', serif">Merriweather</option><option value="'Verdana', Geneva, sans-serif">Verdana</option></select></div>
                <div><label class="block font-medium text-gray-700 mb-1.5">Căn chỉnh</label><div class="flex justify-around" data-setting="textAlign"><button type="button" data-value="text-left" class="setting-btn p-1.5 rounded border border-transparent hover:bg-gray-100 hover:border-gray-300 focus:outline-none focus:ring-1 focus:ring-indigo-500" title="Trái"><i class="fas fa-align-left w-5 h-5 inline-block"></i></button><button type="button" data-value="text-center" class="setting-btn p-1.5 rounded border border-transparent hover:bg-gray-100 hover:border-gray-300 focus:outline-none focus:ring-1 focus:ring-indigo-500" title="Giữa"><i class="fas fa-align-center w-5 h-5 inline-block"></i></button><button type="button" data-value="text-right" class="setting-btn p-1.5 rounded border border-transparent hover:bg-gray-100 hover:border-gray-300 focus:outline-none focus:ring-1 focus:ring-indigo-500" title="Phải"><i class="fas fa-align-right w-5 h-5 inline-block"></i></button><button type="button" data-value="text-justify" class="setting-btn p-1.5 rounded border border-transparent hover:bg-gray-100 hover:border-gray-300 focus:outline-none focus:ring-1 focus:ring-indigo-500" title="Đều"><i class="fas fa-align-justify w-5 h-5 inline-block"></i></button></div></div>
            </div>
            <%-- Cài đặt cho Truyện Tranh --%>
            <div id="imageReaderSettings" class="space-y-4" style="display: none;">
                <div><label class="block font-medium text-gray-700 mb-1.5">Hướng xem</label><div class="flex gap-2" data-setting="orientation"><button type="button" data-value="vertical" class="setting-btn flex-1 px-2 py-1 border border-gray-300 rounded text-xs hover:bg-gray-100 focus:outline-none focus:ring-1 focus:ring-indigo-500 flex items-center justify-center gap-1"><i class="fas fa-arrows-alt-v"></i> Dọc</button><button type="button" data-value="horizontal" class="setting-btn flex-1 px-2 py-1 border border-gray-300 rounded text-xs hover:bg-gray-100 focus:outline-none focus:ring-1 focus:ring-indigo-500 flex items-center justify-center gap-1"><i class="fas fa-arrows-alt-h"></i> Ngang</button></div></div>
                <div id="horizontalPageModeSettings" class="space-y-1" style="display: none;"><label class="block font-medium text-gray-700 mb-1.5">Kiểu xem ngang</label><div class="grid grid-cols-3 gap-2" data-setting="horizontalMode"><button type="button" data-value="single" class="setting-btn px-2 py-1 border border-gray-300 rounded text-xs hover:bg-gray-100 focus:outline-none focus:ring-1 focus:ring-indigo-500">1 trang</button><button type="button" data-value="double" class="setting-btn px-2 py-1 border border-gray-300 rounded text-xs hover:bg-gray-100 focus:outline-none focus:ring-1 focus:ring-indigo-500">2 trang</button><button type="button" data-value="triple" class="setting-btn px-2 py-1 border border-gray-300 rounded text-xs hover:bg-gray-100 focus:outline-none focus:ring-1 focus:ring-indigo-500">3 trang</button></div></div>
                <div id="readingDirectionSetting" style="display: none;"><label class="block font-medium text-gray-700 mb-1.5">Hướng đọc (Ngang)</label><div class="flex gap-2" data-setting="readDirection"><button type="button" data-value="ltr" class="setting-btn flex-1 px-2 py-1 border border-gray-300 rounded text-xs hover:bg-gray-100 focus:outline-none focus:ring-1 focus:ring-indigo-500">Trái->Phải</button><button type="button" data-value="rtl" class="setting-btn flex-1 px-2 py-1 border border-gray-300 rounded text-xs hover:bg-gray-100 focus:outline-none focus:ring-1 focus:ring-indigo-500">Phải->Trái</button></div></div>
                <div><label class="block font-medium text-gray-700 mb-1.5">Hiển thị ảnh</label><div class="flex gap-2" data-setting="imageFit"><button type="button" data-value="fit-width" class="setting-btn flex-1 px-2 py-1 border border-gray-300 rounded text-xs hover:bg-gray-100 focus:outline-none focus:ring-1 focus:ring-indigo-500">Vừa khung</button><button type="button" data-value="fit-original" class="setting-btn flex-1 px-2 py-1 border border-gray-300 rounded text-xs hover:bg-gray-100 focus:outline-none focus:ring-1 focus:ring-indigo-500">Gốc</button></div></div>
                <div><label class="block font-medium text-gray-700 mb-1.5">Chuyển trang</label><div class="flex items-center gap-2" data-setting="tapToNavigate"><label class="flex items-center cursor-pointer"><input type="checkbox" id="chkTapToNavigate" class="setting-checkbox form-checkbox h-4 w-4 text-indigo-600 border-gray-300 rounded focus:ring-indigo-500"><span class="ml-2 text-gray-700">Chạm để chuyển</span></label></div></div>
            </div>
        </div>
    </asp:Panel>

    <%-- JavaScript xử lý --%>
    <script type="text/javascript">
        // Khai báo biến toàn cục
        var body = document.body;
        var popupPanel = document.getElementById('<%= pnlChapterListPopup.ClientID %>');
        var settingsPopupPanel = document.getElementById('<%= pnlReaderSettingsPopup.ClientID %>');
        var readerContainer = document.getElementById('readerContentContainer');
        var readerContentDiv = document.getElementById('readerContent');
        var textContentWrapper = document.getElementById('textContentWrapper');
        var imageContentWrapper = document.getElementById('imageContentWrapper');
        var textSettingsDiv = document.getElementById('textReaderSettings');
        var imageSettingsDiv = document.getElementById('imageReaderSettings');
        var imageTapOverlay = null;

        // 1. Popup Danh sách chương
        function toggleChapterPopup() { if (popupPanel) { var i=popupPanel.style.display==='none'||popupPanel.style.display===''; popupPanel.style.display=i?'flex':'none'; if(body)body.style.overflow=i?'hidden':''; } else { console.error("Popup chương lỗi!"); } }
        if (popupPanel) { popupPanel.addEventListener('click', function (e) { if (e.target === popupPanel) toggleChapterPopup(); }); }
        document.addEventListener('keydown', function (e) { if (e.key === "Escape" && popupPanel && popupPanel.style.display !== 'none') toggleChapterPopup(); });

        // 2. Loading Spinner UpdatePanel
        if (typeof Sys !== 'undefined' && Sys.WebForms && Sys.WebForms.PageRequestManager) { var prm=Sys.WebForms.PageRequestManager.getInstance(); var btnSub=document.getElementById('<%= btnSubmitComment.ClientID %>'); var spin=document.getElementById('commentSpinner'); var cmtIn=document.getElementById('<%= txtCommentInput.ClientID %>'); function BeginReq(s,a){if(a.get_postBackElement()===btnSub&&spin&&btnSub){spin.style.display='inline-block';btnSub.disabled=true;btnSub.classList.add('opacity-50','cursor-not-allowed');if(cmtIn)cmtIn.disabled=true;}} function EndReq(s,a){if(spin)spin.style.display='none';if(btnSub){btnSub.disabled=false;btnSub.classList.remove('opacity-50','cursor-not-allowed');} if(cmtIn)cmtIn.disabled=false; if(a.get_error()){console.error("Async Error:"+a.get_error().message);a.set_errorHandled(true);}} prm.add_beginRequest(BeginReq); prm.add_endRequest(EndReq); } else { console.error("Sys.WebForms.PageRequestManager lỗi."); }

        // 4. Popup Cài đặt Đọc và Logic
        function toggleReaderSettingsPopup() { if(settingsPopupPanel){var i=settingsPopupPanel.style.display==='none'||settingsPopupPanel.style.display==='';if(i){var isTxt=$('#<%=pnlTextContent.ClientID%>').is(':visible');var isImg=$('#<%=pnlImageContent.ClientID%>').is(':visible');if(isTxt){$(textSettingsDiv).show();$(imageSettingsDiv).hide();loadAndApplyTextSettings();}else if(isImg){$(textSettingsDiv).hide();$(imageSettingsDiv).show();loadAndApplyImageSettings();}else{$(textSettingsDiv).hide();$(imageSettingsDiv).hide();}} settingsPopupPanel.style.display=i?'flex':'none';if(body)body.style.overflow=i?'hidden':'';}else{console.error("Popup cài đặt lỗi!");} }
        if (settingsPopupPanel) { settingsPopupPanel.addEventListener('click', function (e) { if (e.target === settingsPopupPanel) toggleReaderSettingsPopup(); }); }
        document.addEventListener('keydown', function (e) { if (e.key === "Escape" && settingsPopupPanel && settingsPopupPanel.style.display !== 'none') toggleReaderSettingsPopup(); });

        // --- Logic Cài đặt Truyện Chữ (Đã xóa Font Size, Padding; Default Times New Roman) ---
        const textDefaults = { bgColor: 'bg-white', fontFamily: "'Times New Roman', Times, serif", textAlign: 'text-justify' };
        function applyTextSetting(setting, value) { if (!readerContainer) return; const $rc=$(readerContainer); const $tcw=$(textContentWrapper); console.log(`>>> Apply Text: ${setting}=${value}`); switch(setting){case 'bgColor': $rc.removeClass('bg-white bg-sepia bg-gray-200 bg-gray-800 bg-black'); if(value&&value!=='bg-white')$rc.addClass(value); if($tcw.length>0){if(value==='bg-gray-800'||value==='bg-black'){$tcw.removeClass('prose-indigo').addClass('prose-invert');$rc.removeClass('text-gray-800').addClass('text-gray-200');}else{$tcw.removeClass('prose-invert').addClass('prose-indigo');$rc.removeClass('text-gray-200').addClass('text-gray-800');}} break; case 'fontFamily': if($tcw.length>0){$tcw.css('font-family',value); console.log("Applied font:", value);} break; case 'textAlign': if($tcw.length>0){$tcw.removeClass('text-left text-center text-right text-justify'); if(value)$tcw.addClass(value);} break; } localStorage.setItem(`reader${setting.charAt(0).toUpperCase()+setting.slice(1)}`, value); }
        function loadAndApplyTextSettings() { const s={bgColor:localStorage.getItem('readerBgColor')||textDefaults.bgColor, fontFamily:localStorage.getItem('readerFontFamily')||textDefaults.fontFamily, textAlign:localStorage.getItem('readerTextAlign')||textDefaults.textAlign}; console.log("Load text:",s); applyTextSetting('bgColor',s.bgColor); applyTextSetting('fontFamily',s.fontFamily); applyTextSetting('textAlign',s.textAlign); $('#textReaderSettings [data-setting="bgColor"] button').attr('aria-pressed','false'); $(`#textReaderSettings [data-setting="bgColor"] button[data-value="${s.bgColor}"]`).attr('aria-pressed','true'); $('#selectFontFamily').val(s.fontFamily); $('#textReaderSettings [data-setting="textAlign"] button').attr('aria-pressed','false'); $(`#textReaderSettings [data-setting="textAlign"] button[data-value="${s.textAlign}"]`).attr('aria-pressed','true'); }

        // --- Logic Cài đặt Truyện Tranh ---
        const imageDefaults = { orientation: 'vertical', horizontalMode: 'double', readDirection: 'ltr', imageFit: 'fit-width', tapToNavigate: true }; // Mặc định dọc
        function applyImageSetting(setting, value) { if(!imageContentWrapper)return; console.log(`Apply Image: ${setting}=${value}`); const $iw=$(imageContentWrapper); switch(setting){case 'orientation': $iw.removeClass('image-reader-vertical image-reader-horizontal'); $iw.addClass(`image-reader-${value}`); $('#horizontalPageModeSettings').toggle(value === 'horizontal'); $('#readingDirectionSetting').toggle(value === 'horizontal'); imageContentWrapper.scrollTop=0;imageContentWrapper.scrollLeft=0; if(value==='horizontal'){let cHMode=localStorage.getItem('imageHorizontalMode')||imageDefaults.horizontalMode; applyImageSetting('horizontalMode',cHMode); let cDir=localStorage.getItem('imageReadDirection')||imageDefaults.readDirection; if(cDir==='rtl'){setTimeout(()=>{try{imageContentWrapper.scrollLeft=imageContentWrapper.scrollWidth-imageContentWrapper.clientWidth;}catch(e){console.error("RTL scroll error",e)}},50);}}else{$iw.removeClass('page-single page-double page-triple');} break; case 'horizontalMode': if($iw.hasClass('image-reader-horizontal')){$iw.removeClass('page-single page-double page-triple'); $iw.addClass(`page-${value}`);} break; case 'readDirection': $iw.removeClass('read-ltr read-rtl').addClass(`read-${value}`); if(value==='rtl'&&$iw.hasClass('image-reader-horizontal')){setTimeout(()=>{try{imageContentWrapper.scrollLeft=imageContentWrapper.scrollWidth-imageContentWrapper.clientWidth;}catch(e){console.error("RTL scroll error",e)}},50);} break; case 'imageFit': $iw.removeClass('fit-width fit-original').addClass(value); break; case 'tapToNavigate': var isC=(value===true||value==='true'); if(isC){enableTapNavigation();}else{disableTapNavigation();} localStorage.setItem(`image${setting.charAt(0).toUpperCase()+setting.slice(1)}`,isC.toString());return;} localStorage.setItem(`image${setting.charAt(0).toUpperCase()+setting.slice(1)}`,value); }
        function loadAndApplyImageSettings() { const s={orientation:localStorage.getItem('imageOrientation')||imageDefaults.orientation, horizontalMode:localStorage.getItem('imageHorizontalMode')||imageDefaults.horizontalMode, readDirection:localStorage.getItem('imageReadDirection')||imageDefaults.readDirection, imageFit:localStorage.getItem('imageImageFit')||imageDefaults.imageFit, tapToNavigate:localStorage.getItem('imageTapToNavigate')===null?imageDefaults.tapToNavigate:(localStorage.getItem('imageTapToNavigate')==='true')}; console.log("Load image:",s); applyImageSetting('orientation', s.orientation); applyImageSetting('horizontalMode', s.horizontalMode); applyImageSetting('readDirection', s.readDirection); applyImageSetting('imageFit', s.imageFit); applyImageSetting('tapToNavigate', s.tapToNavigate); $('#imageReaderSettings [data-setting="orientation"] button').attr('aria-pressed','false'); $(`#imageReaderSettings [data-setting="orientation"] button[data-value="${s.orientation}"]`).attr('aria-pressed','true'); $('#imageReaderSettings [data-setting="horizontalMode"] button').attr('aria-pressed','false'); $(`#imageReaderSettings [data-setting="horizontalMode"] button[data-value="${s.horizontalMode}"]`).attr('aria-pressed','true'); $('#imageReaderSettings [data-setting="readDirection"] button').attr('aria-pressed','false'); $(`#imageReaderSettings [data-setting="readDirection"] button[data-value="${s.readDirection}"]`).attr('aria-pressed','true'); $('#imageReaderSettings [data-setting="imageFit"] button').attr('aria-pressed','false'); $(`#imageReaderSettings [data-setting="imageFit"] button[data-value="${s.imageFit}"]`).attr('aria-pressed','true'); $('#chkTapToNavigate').prop('checked', s.tapToNavigate); }
        function enableTapNavigation() { if(!imageContentWrapper)return; if(!imageTapOverlay){imageTapOverlay=$('<div id="imageTapOverlay"></div>')[0]; $(imageContentWrapper).prepend(imageTapOverlay); $(imageTapOverlay).off('click.tapNav').on('click.tapNav',handleImageTap);} $(imageTapOverlay).addClass('active'); console.log("Tap enabled."); }
        function disableTapNavigation() { if(imageTapOverlay){$(imageTapOverlay).removeClass('active').off('click.tapNav');} console.log("Tap disabled."); }

        // --- Logic Chạm để Chuyển Trang (Phân biệt Dọc/Ngang, Fix lỗi) ---
        function handleImageTap(event) {
            if (!imageContentWrapper) { console.error("imageContentWrapper not found"); return; }
            var prevL=document.getElementById('<%= hlPrevChap.ClientID %>'); var nextL=document.getElementById('<%= hlNextChap.ClientID %>');
            var prevE=prevL&&prevL.getAttribute('disabled')!=='disabled'&&!$(prevL).hasClass('disabled'); var nextE=nextL&&nextL.getAttribute('disabled')!=='disabled'&&!$(nextL).hasClass('disabled');
            const cont=imageContentWrapper; const cW=$(cont).width(); const cH=$(cont).height(); const cX=event.offsetX; const cY=event.offsetY;
            const mode=localStorage.getItem('imageOrientation')||imageDefaults.orientation; console.log(`Tap detected. Mode: ${mode}`); // Sử dụng orientation

            if (mode === 'vertical') {
                const thrH = cW / 2; let action = (cX < thrH) ? 'down' : 'up'; console.log(`Vertical Tap Action: ${action}`);
                let cS = cont.scrollTop; let mS = cont.scrollHeight - cH;
                if (action === 'up' && cS <= 1) { console.log("Đầu chương (dọc)."); if (prevE) { console.log("-> Prev Chap"); prevL.click(); return; } else { console.log("Hết."); return; } }
                if (action === 'down' && cS >= mS - 1) { console.log("Cuối chương (dọc)."); if (nextE) { console.log("-> Next Chap"); nextL.click(); return; } else { console.log("Hết."); return; } }
                let a = cH * 0.85; let t = (action === 'down') ? cS + a : cS - a; t = Math.max(0, Math.min(t, mS));
                $(cont).stop().animate({ scrollTop: t }, 150); console.log(`Vertical scroll ${action} to ${t}`);
            }
            else {
                const isRTL=(localStorage.getItem('imageReadDirection')||imageDefaults.readDirection)==='rtl'; const thrH=cW/2; let action=(cX<thrH)?(isRTL?'next':'prev'):(isRTL?'prev':'next'); console.log(`Horizontal Tap Action: ${action}, RTL: ${isRTL}`);
                let currentItem=null; let minCenterDist = Infinity; const items = Array.from(cont.querySelectorAll('.image-item')); const cScrollLeft = cont.scrollLeft; const cCenterX = cScrollLeft + cW / 2;
                items.forEach((item, index) => { const iRect = item.getBoundingClientRect(); const contRect = cont.getBoundingClientRect(); const iLeftInCont = iRect.left - contRect.left + cScrollLeft; const iCenterX = iLeftInCont + iRect.width / 2; const centerDist = Math.abs(iCenterX - cCenterX); if (centerDist < minCenterDist) { minCenterDist = centerDist; currentItem = item; } });
                if (!currentItem && items.length > 0) { currentItem = items[0]; console.warn("Dùng item đầu."); } if (!currentItem) { console.error("No image-item."); return; }
                console.log("Current item (H):", currentItem);
                let targetItem = null;
                if (action === 'prev') { targetItem = currentItem.previousElementSibling; if (!targetItem) { console.log("Đầu chương (ngang)."); if (prevE) { prevL.click(); } return; } }
                else { targetItem = currentItem.nextElementSibling; if (!targetItem) { console.log("Cuối chương (ngang)."); if (nextE) { nextL.click(); } return; } }
                if (targetItem) { console.log("Scroll to (H):",targetItem); let inlineOpt = 'center'; if($(cont).is('.page-double, .page-triple')){inlineOpt = isRTL ? 'end' : 'start';} targetItem.scrollIntoView({ behavior: 'smooth', block: 'nearest', inline: inlineOpt }); }
            }
        }

        // --- $(document).ready ---
        $(document).ready(function () {
            console.log("Document Ready: Initializing...");

            // --- START: ANTI-COPY JAVASCRIPT ---
            // Chặn sự kiện chuột phải trên toàn bộ trang
            $(document).on('contextmenu', function (e) {
                e.preventDefault();
            });

            // Chặn các phím tắt phổ biến
            $(document).on('keydown', function (e) {
                // Chặn Ctrl+C (Copy), Ctrl+S (Save), Ctrl+P (Print), Ctrl+U (View Source)
                if (e.ctrlKey || e.metaKey) { // e.metaKey là phím Command trên Mac
                    switch (e.key.toLowerCase()) {
                        case 'c': // Copy
                        case 's': // Save
                        case 'p': // Print
                        case 'u': // View Source
                            e.preventDefault();
                            break;
                    }
                }
            });
            // --- END: ANTI-COPY JAVASCRIPT ---

            // 3. Logic ẩn/hiện thanh điều hướng
            var $navPanel=$('#<%= pnlNavigation.ClientID %>'); if($navPanel.length>0){var lST=0;var dt=5;var nH=$navPanel.outerHeight();var isHid=false;var tT=null;var tD=300; $(window).scroll(function(e){var st=$(this).scrollTop();if(Math.abs(lST-st)<=dt)return;if(isHid){$navPanel.addClass('hidden-by-scroll');lST=st;return;} if(st>lST&&st>nH){$navPanel.addClass('hidden-by-scroll');}else{if(st+$(window).height()<$(document).height()||st===0){$navPanel.removeClass('hidden-by-scroll');}} lST=st;}); $('#nav-wrapper').on('click',function(e){if(tT===null){tT=setTimeout(function(){tT=null;},tD);}else{clearTimeout(tT);tT=null;isHid=!isHid;if(isHid){$navPanel.addClass('hidden-by-scroll');}else{$navPanel.removeClass('hidden-by-scroll');$(window).trigger('scroll');}}}); $('#nav-wrapper').find('a, button').on('dblclick',function(e){e.stopPropagation();});}

            // 4. Khởi tạo cài đặt đọc
            var isTextVisible = $('#<%= pnlTextContent.ClientID %>').is(':visible'); var isImageVisible = $('#<%= pnlImageContent.ClientID %>').is(':visible');
            if (isTextVisible) { console.log("Init text settings..."); loadAndApplyTextSettings(); } else if (isImageVisible) { console.log("Init image settings..."); loadAndApplyImageSettings(); }

            // 5. Gắn sự kiện cho các nút cài đặt
            var $settingsPopup = $('#<%= pnlReaderSettingsPopup.ClientID %>');
            console.log("Binding setting events to:", $settingsPopup[0]);
            // Text Settings
            $settingsPopup.off('click.settingTxt').on('click.settingTxt', '#textReaderSettings [data-setting="bgColor"] button', function () { applyTextSetting('bgColor', $(this).data('value')); $(this).attr('aria-pressed', 'true').siblings().attr('aria-pressed', 'false'); });
            $settingsPopup.off('change.settingTxt').on('change.settingTxt', '#selectFontFamily', function () { applyTextSetting('fontFamily', $(this).val()); });
            $settingsPopup.on('click.settingTxt', '#textReaderSettings [data-setting="textAlign"] button', function () { applyTextSetting('textAlign', $(this).data('value')); $(this).attr('aria-pressed', 'true').siblings().attr('aria-pressed', 'false'); });
            // Image Settings
            $settingsPopup.on('click.settingImg', '#imageReaderSettings [data-setting="orientation"] button', function () { applyImageSetting('orientation', $(this).data('value')); $(this).attr('aria-pressed', 'true').siblings().attr('aria-pressed', 'false'); });
            $settingsPopup.on('click.settingImg', '#imageReaderSettings [data-setting="horizontalMode"] button', function () { applyImageSetting('horizontalMode', $(this).data('value')); $(this).attr('aria-pressed', 'true').siblings().attr('aria-pressed', 'false'); });
            $settingsPopup.on('click.settingImg', '#imageReaderSettings [data-setting="readDirection"] button', function () { applyImageSetting('readDirection', $(this).data('value')); $(this).attr('aria-pressed', 'true').siblings().attr('aria-pressed', 'false'); });
            $settingsPopup.on('click.settingImg', '#imageReaderSettings [data-setting="imageFit"] button', function () { applyImageSetting('imageFit', $(this).data('value')); $(this).attr('aria-pressed', 'true').siblings().attr('aria-pressed', 'false'); });
            $settingsPopup.off('change.settingImg').on('change.settingImg', '#chkTapToNavigate', function () { applyImageSetting('tapToNavigate', $(this).is(':checked')); });
            console.log("Event bindings complete.");
        });
    </script>

</asp:Content>