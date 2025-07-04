<%@ Page Title="Quản Lý Nội Dung Sách" Language="C#" MasterPageFile="~/WebForm/Admin/Admin.Master" AutoEventWireup="true" CodeBehind="SuaNoiDungSach.aspx.cs" Inherits="Webebook.WebForm.Admin.SuaNoiDungSach" %>
<%@ OutputCache Duration="1" VaryByParam="none" Location="None" NoStore="true" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/5.15.4/css/all.min.css" />
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    <style>
        .chapter-image-container { display: flex; flex-wrap: wrap; gap: 5px; align-items: center; }
        .chapter-image { height: 50px; width: auto; max-width: 70px; object-fit: cover; border: 1px solid #ccc; border-radius: 3px; }
        .btn-action { padding: 0.375rem 0.75rem; font-size: 0.875rem; border-radius: 0.375rem; cursor: pointer; border: none; /* Added border:none for consistency */ display: inline-block; text-align: center; vertical-align: middle; user-select: none; line-height: 1.5; }
        .btn-primary { background-color: #4f46e5; color: white; }
        .btn-primary:hover { background-color: #4338ca; }
        .btn-secondary { background-color: #6b7280; color: white; }
        .btn-secondary:hover { background-color: #4b5563; }
        .btn-link-edit { color: #3b82f6; margin-right: 0.75rem; text-decoration: none; background: none; border: none; padding: 0; cursor: pointer; }
        .btn-link-edit:hover { color: #1d4ed8; text-decoration: underline; }
        .btn-link-delete { color: #ef4444; text-decoration: none; background: none; border: none; padding: 0; cursor: pointer; }
        .btn-link-delete:hover { color: #b91c1c; text-decoration: underline; }
        .message-panel { padding: 0.75rem; margin-bottom: 1rem; border-radius: 0.375rem; }
        .message-success { color: #0f5132; background-color: #d1e7dd; border: 1px solid #badbcc; }
        .message-error { color: #842029; background-color: #f8d7da; border: 1px solid #f5c2c7; }
        /* Ensure Tailwind/other framework styles apply correctly */
        .min-w-full { min-width: 100%; }
        .divide-y > :not([hidden]) ~ :not([hidden]) { border-top-width: 1px; border-bottom-width: 0; }
        .divide-gray-200 { border-color: #e5e7eb; }
        .bg-gray-100 { background-color: #f3f4f6; }
        .px-4 { padding-left: 1rem; padding-right: 1rem; }
        .py-3 { padding-top: 0.75rem; padding-bottom: 0.75rem; }
        .text-left { text-align: left; }
        .text-xs { font-size: 0.75rem; line-height: 1rem; }
        .font-medium { font-weight: 500; }
        .text-gray-500 { color: #6b7280; }
        .uppercase { text-transform: uppercase; }
        .bg-white { background-color: #fff; }
        .hover\:bg-gray-50:hover { background-color: #f9fafb; }
        .text-sm { font-size: 0.875rem; line-height: 1.25rem; }
        .text-gray-900 { color: #111827; }
        .w-20 { width: 5rem; }
        .text-center { text-align: center; }
        .text-gray-700 { color: #374151; }
        .max-w-xs { max-width: 20rem; }
        .truncate { overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
        .text-red-500 { color: #ef4444; }
        .italic { font-style: italic; }
        .w-32 { width: 8rem; }
        .container { width: 100%; }
        .mx-auto { margin-left: auto; margin-right: auto; }
        .px-4 { padding-left: 1rem; padding-right: 1rem; }
        .py-6 { padding-top: 1.5rem; padding-bottom: 1.5rem; }
        .rounded-lg { border-radius: 0.5rem; }
        .shadow-sm { box-shadow: 0 1px 2px 0 rgb(0 0 0 / 0.05); }
        .mb-6 { margin-bottom: 1.5rem; }
        .border { border-width: 1px; }
        .border-gray-200 { border-color: #e5e7eb; }
        .flex { display: flex; }
        .flex-col { flex-direction: column; }
        .sm\:flex-row { flex-direction: row; }
        .sm\:justify-between { justify-content: space-between; }
        .sm\:items-center { align-items: center; }
        .mb-2 { margin-bottom: 0.5rem; }
        .text-xl { font-size: 1.25rem; line-height: 1.75rem; }
        .font-semibold { font-weight: 600; }
        .sm\:mb-0 { margin-bottom: 0; }
        .flex-wrap { flex-wrap: wrap; }
        .gap-2 { gap: 0.5rem; }
        .mt-2 { margin-top: 0.5rem; }
        .p-6 { padding: 1.5rem; }
        .shadow-md { box-shadow: 0 4px 6px -1px rgb(0 0 0 / 0.1), 0 2px 4px -2px rgb(0 0 0 / 0.1); }
        .text-lg { font-size: 1.125rem; line-height: 1.75rem; }
        .mb-4 { margin-bottom: 1rem; }
        .overflow-x-auto { overflow-x: auto; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="container mx-auto px-4 py-6">
        <div class="bg-gray-100 p-4 rounded-lg shadow-sm mb-6 border border-gray-200">
            <div class="flex flex-col sm:flex-row sm:justify-between sm:items-center mb-2">
                <h2 class="text-xl font-semibold text-gray-700 mb-2 sm:mb-0">Quản Lý Nội Dung Sách</h2>
                <div class="flex flex-wrap gap-2">
                    <asp:Button ID="btnAddNewChapter" runat="server" Text="Thêm Chương Mới" CssClass="btn-action btn-primary" OnClick="btnAddNewChapter_Click" />
                    <asp:Button ID="btnBackToEditInfo" runat="server" Text="Sửa Thông Tin Sách" CssClass="btn-action btn-secondary" OnClick="btnBackToEditInfo_Click" />
                </div>
            </div>
            <p class="mt-2 text-sm text-gray-600">
                <strong>Sách:</strong> <asp:Label ID="lblBookTitleContext" runat="server" Text="[Đang tải...]"></asp:Label>
                (ID: <asp:Label ID="lblSachIDContext" runat="server"></asp:Label>) -
                <strong>Loại Sách:</strong> <asp:Label ID="lblLoaiSachContext" runat="server" Text="[N/A]"></asp:Label>
            </p>
            <asp:HiddenField ID="hfSachID" runat="server" />
            <asp:HiddenField ID="hfLoaiSach" runat="server" />
        </div>

        <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="message-panel">
            <asp:Label ID="lblContentMessage" runat="server"></asp:Label>
        </asp:Panel>

        <div class="bg-white p-6 rounded-lg shadow-md">
            <h4 class="text-lg font-medium text-gray-700 mb-4">Danh Sách Chương</h4>
            <div class="overflow-x-auto">
                <asp:GridView ID="gvContent" runat="server" AutoGenerateColumns="False" DataKeyNames="IDNoiDung,SoChuong"
                    CssClass="min-w-full divide-y divide-gray-200" OnRowCommand="gvContent_RowCommand" OnRowDataBound="gvContent_RowDataBound" OnRowDeleting="gvContent_RowDeleting">
                    <HeaderStyle CssClass="bg-gray-100 px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase" />
                    <RowStyle CssClass="bg-white hover:bg-gray-50" />
                    <EmptyDataTemplate>
                        <div class="text-center py-4 text-gray-500">Chưa có chương nào cho sách này.</div>
                    </EmptyDataTemplate>
                    <Columns>
                        <asp:BoundField DataField="SoChuong" HeaderText="Chương" ItemStyle-CssClass="px-4 py-3 text-sm text-gray-900 w-20 text-center" />
                        <asp:BoundField DataField="TenChuong" HeaderText="Tên Chương" ItemStyle-CssClass="px-4 py-3 text-sm text-gray-700 max-w-xs truncate" />
                        <asp:TemplateField HeaderText="Nội Dung" ItemStyle-CssClass="px-4 py-3 text-sm">
                            <ItemTemplate>
                                <asp:Panel ID="pnlImages" runat="server" CssClass="chapter-image-container" Visible="false">
                                    <asp:Repeater ID="rptChapterImages" runat="server" OnItemDataBound="rptChapterImages_ItemDataBound">
                                        <ItemTemplate>
                                            <asp:Image ID="imgChapterPage" runat="server" CssClass="chapter-image" />
                                        </ItemTemplate>
                                    </asp:Repeater>
                                    <asp:Literal ID="litMoreImagesIndicator" runat="server" Visible="false"></asp:Literal>
                                </asp:Panel>
                                <asp:Label ID="lblContentText" runat="server" Visible="false" CssClass="text-gray-600 italic">[Nội dung dạng Text - Chưa hỗ trợ hiển thị]</asp:Label> <%-- Placeholder for text content --%>
                                <asp:Label ID="lblContentError" runat="server" Visible="false" CssClass="text-red-500 text-sm italic">[Chưa có nội dung hoặc lỗi hiển thị]</asp:Label>
                            </ItemTemplate>
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Hành Động" ItemStyle-CssClass="px-4 py-3 text-sm text-center w-32">
                            <ItemTemplate>
                                <asp:LinkButton ID="lnkEditChapter" runat="server" CommandName="EditChapter" CommandArgument='<%# Eval("IDNoiDung") %>' CssClass="btn-link-edit">
                                    <i class="fas fa-edit"></i>
                                </asp:LinkButton>
                                <asp:LinkButton ID="lnkDeleteContent" runat="server" CommandName="Delete" CommandArgument='<%# Eval("IDNoiDung") %>' CssClass="btn-link-delete">
                                    <%-- Thuộc tính OnClientClick đã được xóa bỏ --%>
                                    <i class="fas fa-trash"></i> 
                                </asp:LinkButton>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
        </div>
    </div>

        <%-- BẮT ĐẦU: THÊM SCRIPT CHO POPUP XÓA CHƯƠNG --%>
    <script type="text/javascript">
        function showChapterDeleteConfirmation(chapterNumber, contentId, sourceControlUniqueId) {
            Swal.fire({
                title: 'Bạn có chắc chắn muốn xóa?',
                html: `Bạn sắp xóa vĩnh viễn <strong>Chương ${chapterNumber}</strong>.<br/>Hành động này sẽ xóa cả các file ảnh/nội dung liên quan và <strong>không thể hoàn tác!</strong>`,
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#d33',
                cancelButtonColor: '#3085d6',
                confirmButtonText: '<i class="fas fa-trash"></i> Có, xóa ngay!',
                cancelButtonText: 'Hủy'
            }).then((result) => {
                if (result.isConfirmed) {
                    // Nếu xác nhận, trigger postback của ASP.NET để thực thi lệnh xóa
                    __doPostBack(sourceControlUniqueId, '');
                }
            });
        }
    </script>
    <%-- KẾT THÚC: THÊM SCRIPT CHO POPUP XÓA CHƯƠNG --%>
</asp:Content>