<%@ Page Title="Tủ Sách" Language="C#" MasterPageFile="~/WebForm/User/User.Master" AutoEventWireup="true" CodeBehind="tusach.aspx.cs" Inherits="Webebook.WebForm.User.tusach" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        /* Style cho card sách trong tủ */
        .bookshelf-grid {
            display: grid;
            grid-template-columns: repeat(auto-fill, minmax(180px, 1fr)); /* Responsive grid */
            gap: 1.5rem; /* Khoảng cách giữa các card */
        }
        .book-card-shelf {
            background-color: white;
            border-radius: 0.5rem; /* rounded-lg */
            box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -1px rgba(0, 0, 0, 0.06); /* shadow-md */
            overflow: hidden;
            transition: transform 0.2s ease-in-out, box-shadow 0.2s ease-in-out;
            display: flex;
             flex-direction: column;
        }
        .book-card-shelf:hover {
            transform: translateY(-4px);
            box-shadow: 0 10px 15px -3px rgba(0, 0, 0, 0.1), 0 4px 6px -2px rgba(0, 0, 0, 0.05); /* shadow-lg */
        }
        .book-cover-link {
             display: block;
             height: 250px; /* Fixed height for cover area */
             background-color: #f3f4f6; /* gray-100 for placeholder bg */
        }
        .book-card-shelf img.book-cover-shelf {
            width: 100%;
            height: 100%; /* Fill the link area */
            object-fit: cover; /* Đảm bảo ảnh che phủ khu vực */
        }
        .book-card-shelf .book-info-shelf {
            padding: 1rem;
            display: flex;
             flex-direction: column;
             flex-grow: 1; /* Allow info section to grow */
             justify-content: space-between; /* Push button to bottom */
        }
         /* Make title area take up available space before progress/button */
         .book-card-shelf .book-title-author-wrapper {
            flex-grow: 1;
            margin-bottom: 0.5rem; /* Add some space before progress */
        }

        .book-card-shelf .book-title-shelf {
            font-size: 1rem; /* text-base */
            font-weight: 600; /* font-semibold */
            color: #1f2937; /* gray-800 */
            margin-bottom: 0.15rem; /* Reduced margin */
            /* Giới hạn 2 dòng */
             overflow: hidden;
             display: -webkit-box;
             -webkit-line-clamp: 2;
             -webkit-box-orient: vertical;
             text-overflow: ellipsis;
             min-height: 2.8em; /* Ensure space for 2 lines */
        }
        .book-card-shelf .book-author-shelf {
            font-size: 0.875rem; /* text-sm */
            color: #6b7280; /* gray-500 */
            white-space: nowrap;
             overflow: hidden;
             text-overflow: ellipsis;
        }
         /* Progress styles */
         .book-card-shelf .book-progress-shelf {
             font-size: 0.75rem; /* text-xs */
             color: #6b7280; /* gray-500 */
             margin-bottom: 0.5rem; /* space before button */
             min-height: 1.2em; /* Ensure space */
        }

        .book-card-shelf .read-button {
            display: inline-block;
            width: 100%; /* Full width button */
            text-align: center;
            background-color: #3b82f6; /* blue-500 */
            color: white;
            padding: 0.5rem 1rem;
            border-radius: 0.375rem; /* rounded-md */
            font-weight: 500; /* font-medium */
            text-decoration: none;
            transition: background-color 0.2s, opacity 0.2s;
        }
        .book-card-shelf .read-button:hover:not(.disabled) { /* Prevent hover effect on disabled */
            background-color: #2563eb; /* blue-600 */
        }
         .book-card-shelf .read-button[disabled], /* For potentially disabled elements */
         .book-card-shelf .read-button.disabled { /* Class added via code-behind */
             background-color: #9ca3af; /* gray-400 */
             opacity: 0.7;
             cursor: not-allowed;
         }
        .empty-bookshelf {
            text-align: center;
            padding: 3rem 1rem;
            background-color: #f9fafb; /* gray-50 */
            border: 1px dashed #d1d5db; /* gray-300 */
            border-radius: 0.5rem;
            color: #6b7280; /* gray-500 */
        }
        .empty-bookshelf i {
             color: #9ca3af; /* gray-400 */
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="container mx-auto px-4 py-6">
        <h2 class="text-2xl font-semibold text-gray-800 mb-6">Tủ Sách Của Bạn</h2>

        <asp:Label ID="lblMessage" runat="server" CssClass="block mb-4 p-3 rounded-md border text-sm" EnableViewState="false" Visible="false"></asp:Label>

        <asp:Panel ID="pnlBookshelfGrid" runat="server" CssClass="bookshelf-grid">
            <%-- Add OnItemDataBound event handler --%>
<%-- File: WebForm/User/tusach.aspx --%>

<asp:Repeater ID="rptTuSach" runat="server" OnItemDataBound="rptTuSach_ItemDataBound">
    <ItemTemplate>
        <div class="book-card-shelf">
            <%-- Link to Book Details Page --%>
            <%-- SỬA ĐƯỜNG DẪN Ở ĐÂY --%>
            <asp:HyperLink ID="hlBookImageLink" runat="server" NavigateUrl='<%# ResolveUrl("~/WebForm/User/chitietsach_chap.aspx?IDSach=") + Eval("IDSach") %>' CssClass="book-cover-link">
                <asp:Image ID="imgBookCover" runat="server"
                    ImageUrl='<%# Eval("DuongDanBiaSach") != DBNull.Value && !string.IsNullOrEmpty(Eval("DuongDanBiaSach").ToString()) ? ResolveUrl(Eval("DuongDanBiaSach").ToString()) : ResolveUrl("~/Images/placeholder_cover.png") %>'
                    AlternateText='<%# "Bìa sách " + Eval("TenSach") %>'
                    CssClass="book-cover-shelf" />
            </asp:HyperLink>
            <div class="book-info-shelf">
                 <%-- Các phần tử khác giữ nguyên --%>
                 <div class="book-title-author-wrapper">
                    <h3 class="book-title-shelf" title='<%# Eval("TenSach") %>'><%# Eval("TenSach") %></h3>
                    <p class="book-author-shelf" title='<%# Eval("TacGia") %>'><%# Eval("TacGia") %></p>
                 </div>
                 <p class="book-progress-shelf">
                    <asp:Literal ID="litProgress" runat="server"></asp:Literal>
                 </p>
                 <asp:HyperLink ID="hlReadButton" runat="server" NavigateUrl="#" CssClass="read-button">
                    <i class="fas fa-book-open mr-1"></i>
                    <asp:Literal ID="litReadButtonText" runat="server">Đọc</asp:Literal>
                 </asp:HyperLink>
            </div>
        </div>
    </ItemTemplate>
</asp:Repeater>
        </asp:Panel>

        <%-- Message when bookshelf is empty --%>
        <asp:Panel ID="pnlEmptyBookshelf" runat="server" Visible="false" CssClass="empty-bookshelf">
            <i class="fas fa-book-open fa-3x mb-4"></i>
            <p class="text-lg font-medium mb-2">Tủ sách của bạn đang trống</p>
            <p class="text-sm mb-4">Hãy khám phá và thêm những cuốn sách yêu thích vào bộ sưu tập của bạn!</p>
            <asp:HyperLink runat="server" NavigateUrl="~/WebForm/User/danhsachsach_user.aspx" CssClass="inline-block bg-blue-500 hover:bg-blue-600 text-white font-bold py-2 px-4 rounded transition duration-150">
                 Khám Phá Sách Ngay
            </asp:HyperLink>
        </asp:Panel>

    </div>
</asp:Content>