<%@ Page Title="Quản Lý Sách" Language="C#" MasterPageFile="~/WebForm/Admin/Admin.Master" AutoEventWireup="true" CodeBehind="QuanLySach.aspx.cs" Inherits="Webebook.WebForm.Admin.QuanLySach" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <%-- Đảm bảo Font Awesome đã được liên kết (nên đặt trong MasterPage) --%>
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/5.15.4/css/all.min.css" integrity="sha512-1ycn6IcaQQ40/MKBW2W4Rhis/DbILU74C1vSrLJxCq57o941Ym01SwNsOMqvEBFlcgUa6xLiPY/NS5R+E6ztJQ==" crossorigin="anonymous" referrerpolicy="no-referrer" />
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    <style>
        /* Các lớp utility của Tailwind sẽ xử lý hầu hết việc tạo kiểu. */
        .gridview-footer td { /* Ví dụ tạo kiểu cho phần phân trang */
             border-top: 1px solid #e5e7eb; /* tương đương border-gray-200 */
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="container mx-auto px-4 sm:px-6 lg:px-8 py-8">

        <%-- Tiêu đề trang --%>
        <div class="flex flex-col sm:flex-row justify-between sm:items-center mb-8 gap-4">
            <h1 class="text-3xl font-bold text-gray-900">
                Quản Lý Sách
            </h1>
            <asp:Button ID="btnThemSachMoi" runat="server" Text="Thêm Sách Mới"
                CssClass="inline-flex items-center justify-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 transition duration-150 ease-in-out w-full sm:w-auto"
                OnClick="btnThemSachMoi_Click" />
        </div>

        <%-- Khu vực Lọc/Tìm kiếm --%>
        <div class="mb-6 p-4 bg-gray-50 border border-gray-200 rounded-lg shadow-sm">
            <div class="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-4 gap-4 items-end">
                <%-- Ô tìm kiếm --%>
                <div>
                    <label for="<%= txtSearchTerm.ClientID %>" class="block text-sm font-medium text-gray-700">Tìm kiếm (Tên sách, Tác giả)</label>
                    <asp:TextBox ID="txtSearchTerm" runat="server" CssClass="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm"></asp:TextBox>
                </div>

                <%-- Lọc theo Trạng thái (Đã cập nhật) --%>
                <div>
                    <label for="<%= ddlStatusFilter.ClientID %>" class="block text-sm font-medium text-gray-700">Trạng thái Nội dung</label>
                    <asp:DropDownList ID="ddlStatusFilter" runat="server" CssClass="mt-1 block w-full pl-3 pr-10 py-2 text-base border-gray-300 focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm rounded-md"
                        AppendDataBoundItems="true">
                        <asp:ListItem Text="-- Tất cả Trạng thái --" Value=""></asp:ListItem>
                        <%-- Cập nhật các tùy chọn trạng thái theo hình ảnh --%>
                         <asp:ListItem Text="Hoàn thành" Value="Hoàn thành"></asp:ListItem>
                         <asp:ListItem Text="Đang cập nhật" Value="Đang cập nhật"></asp:ListItem>
                         <asp:ListItem Text="Tạm dừng" Value="Tạm dừng"></asp:ListItem>
                    </asp:DropDownList>
                </div>

                <%-- Các nút hành động lọc --%>
                <div class="flex space-x-2 md:col-start-4 justify-end">
                     <asp:Button ID="btnFilter" runat="server" Text="Lọc / Tìm" OnClick="btnFilter_Click"
                        CssClass="inline-flex items-center justify-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-blue-600 hover:bg-blue-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 transition duration-150 ease-in-out" />
                     <asp:Button ID="btnClearFilter" runat="server" Text="Bỏ lọc" OnClick="btnClearFilter_Click" CausesValidation="false"
                        CssClass="inline-flex items-center justify-center px-4 py-2 border border-gray-300 text-sm font-medium rounded-md shadow-sm text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 transition duration-150 ease-in-out" />
                </div>
            </div>
        </div>
        <%-- Hết Khu vực Lọc/Tìm kiếm --%>

        <%-- Khu vực hiển thị thông báo --%>
        <asp:Label ID="lblMessage" runat="server" EnableViewState="false" Visible="false" CssClass="mb-4"></asp:Label>

        <%-- Container cho GridView --%>
        <div class="bg-white shadow-md rounded-lg overflow-hidden border border-gray-200">
             <div class="overflow-x-auto">
                <asp:GridView ID="GridViewSach" runat="server" AutoGenerateColumns="False"
                    DataKeyNames="IDSach" CssClass="min-w-full divide-y divide-gray-200"
                    OnRowCommand="GridViewSach_RowCommand" OnRowDeleting="GridViewSach_RowDeleting"
                    OnRowDataBound="GridViewSach_RowDataBound"
                    AllowPaging="True" PageSize="10" OnPageIndexChanging="GridViewSach_PageIndexChanging"
                    EmptyDataText=" "> <%-- Sử dụng EmptyDataTemplate thay thế --%>

                    <%-- Kiểu Header --%>
                    <HeaderStyle CssClass="bg-gray-50" />

                    <%-- Kiểu Dòng (Đã thêm hiệu ứng hover) --%>
                    <RowStyle CssClass="bg-white hover:bg-gray-100 transition duration-150 ease-in-out" />
                    <AlternatingRowStyle CssClass="bg-gray-50 hover:bg-gray-100 transition duration-150 ease-in-out" />

                    <%-- Kiểu Phân trang --%>
                    <PagerStyle CssClass="bg-white px-4 py-3 sm:px-6 border-t border-gray-200 text-sm gridview-footer" HorizontalAlign="Right" />

                    <Columns>
                        <%-- Cột ID --%>
                        <asp:BoundField DataField="IDSach" HeaderText="ID" ReadOnly="True">
                            <HeaderStyle CssClass="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider w-16" />
                            <ItemStyle CssClass="px-6 py-4 whitespace-nowrap text-sm font-mono text-gray-500 align-middle" />
                        </asp:BoundField>

                        <%-- Cột Ảnh bìa --%>
                        <asp:TemplateField HeaderText="Ảnh Bìa">
                             <HeaderStyle CssClass="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider w-20" />
                             <ItemStyle CssClass="px-6 py-4 whitespace-nowrap text-sm text-gray-500 align-middle" />
                            <ItemTemplate>
                                <asp:Image ID="imgBiaSach" runat="server"
                                    ImageUrl='<%# Eval("DuongDanBiaSach") != DBNull.Value && !string.IsNullOrWhiteSpace(Eval("DuongDanBiaSach").ToString()) ? ResolveUrl(Eval("DuongDanBiaSach").ToString()) : "" %>'
                                    AlternateText='<%# "Bìa sách " + Eval("TenSach") %>'
                                    CssClass="h-16 w-12 object-contain rounded border border-gray-200 bg-gray-50"
                                    Visible='<%# Eval("DuongDanBiaSach") != DBNull.Value && !string.IsNullOrWhiteSpace(Eval("DuongDanBiaSach").ToString()) %>' />
                                <asp:Panel ID="pnlNoImage" runat="server"
                                    Visible='<%# Eval("DuongDanBiaSach") == DBNull.Value || string.IsNullOrWhiteSpace(Eval("DuongDanBiaSach").ToString()) %>'
                                    CssClass="h-16 w-12 flex items-center justify-center bg-gray-100 text-gray-400 rounded border border-dashed border-gray-300">
                                    <i class="fas fa-image fa-lg"></i>
                                </asp:Panel>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <%-- Cột Tên Sách (Chữ đậm hơn) --%>
                        <asp:BoundField DataField="TenSach" HeaderText="Tên Sách">
                              <HeaderStyle CssClass="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider" />
                              <ItemStyle CssClass="px-6 py-4 whitespace-normal text-sm font-semibold text-gray-900 align-middle" />
                        </asp:BoundField>

                        <%-- Cột Tác Giả --%>
                        <asp:BoundField DataField="TacGia" HeaderText="Tác Giả">
                              <HeaderStyle CssClass="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider" />
                              <ItemStyle CssClass="px-6 py-4 whitespace-nowrap text-sm text-gray-700 align-middle" />
                        </asp:BoundField>

                        <%-- Cột Giá Sách (Đổi màu, chữ đậm vừa) --%>
                        <asp:BoundField DataField="GiaSach" HeaderText="Giá" DataFormatString="{0:N0} VNĐ">
                              <HeaderStyle CssClass="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider" />
                              <ItemStyle CssClass="px-6 py-4 whitespace-nowrap text-sm text-indigo-700 font-medium text-right align-middle" HorizontalAlign="Right" />
                        </asp:BoundField>

                        <%-- Cột NXB --%>
                        <asp:BoundField DataField="NhaXuatBan" HeaderText="NXB">
                              <HeaderStyle CssClass="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider" />
                              <ItemStyle CssClass="px-6 py-4 whitespace-nowrap text-sm text-gray-600 align-middle" />
                        </asp:BoundField>

                        <%-- Cột Trạng Thái Nội Dung (Sử dụng Badge) --%>
                        <asp:TemplateField HeaderText="Trạng Thái ND">
                             <HeaderStyle CssClass="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider" />
                             <ItemStyle CssClass="px-6 py-4 whitespace-nowrap align-middle" />
                            <ItemTemplate>
                                <%-- Gọi hàm helper từ code-behind để tạo badge --%>
                                <%# GetStatusBadge(Eval("TrangThaiNoiDung")) %>
                            </ItemTemplate>
                        </asp:TemplateField>

                        <%-- Cột Hành Động --%>
                        <asp:TemplateField HeaderText="Hành Động">
                             <HeaderStyle CssClass="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider w-auto" />
                             <ItemStyle CssClass="px-6 py-4 whitespace-nowrap text-right text-sm font-medium space-x-3 align-middle" HorizontalAlign="Right" />
                            <ItemTemplate>
                                <asp:LinkButton ID="lnkSua" runat="server" CommandName="EditBook" CommandArgument='<%# Eval("IDSach") %>'
                                    CssClass="text-indigo-600 hover:text-indigo-900 transition duration-150 ease-in-out inline-flex items-center" ToolTip="Sửa">
                                    <i class="fas fa-edit fa-fw"></i>
                                </asp:LinkButton>
                                <asp:LinkButton ID="lnkXoa" runat="server" CommandName="Delete" CommandArgument='<%# Eval("IDSach") %>'
                                    CssClass="text-red-600 hover:text-red-900 transition duration-150 ease-in-out inline-flex items-center" ToolTip="Xóa">
                                    <%-- Thuộc tính OnClientClick đã được xóa bỏ khỏi đây --%>
                                    <i class="fas fa-trash fa-fw"></i>
                                </asp:LinkButton>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>

                    <%-- Template khi không có dữ liệu --%>
                    <EmptyDataTemplate>
                        <div class="text-center py-16 px-6">
                            <i class="fas fa-book-open fa-5x mb-4 text-gray-300"></i>
                            <p class="text-xl font-semibold text-gray-700 mb-2">Không tìm thấy sách nào</p>
                             <%-- Hiển thị thông báo phù hợp khi có bộ lọc được áp dụng --%>
                             <asp:PlaceHolder ID="phEmptyDataMessage" runat="server">
                                 <p class="text-sm text-gray-500 mb-6">Hãy thử thay đổi bộ lọc hoặc xóa bộ lọc để tìm kiếm rộng hơn.</p>
                             </asp:PlaceHolder>
                             <asp:PlaceHolder ID="phNoDataMessage" runat="server" Visible="false">
                                 <p class="text-sm text-gray-500 mb-6">Hiện tại chưa có dữ liệu sách nào trong hệ thống. Hãy thêm sách đầu tiên!</p>
                                 <asp:Button ID="btnThemSachEmpty" runat="server" Text="Thêm Sách Mới"
                                     CssClass="inline-flex items-center justify-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 transition duration-150 ease-in-out"
                                     OnClick="btnThemSachMoi_Click" />
                            </asp:PlaceHolder>
                        </div>
                    </EmptyDataTemplate>

                </asp:GridView>
            </div> <%-- Hết overflow-x-auto --%>
        </div> <%-- Hết Grid Container --%>

    </div> <%-- Hết Container --%>

        <%-- BẮT ĐẦU: THÊM SCRIPT CHO POPUP XÓA --%>
    <script type="text/javascript">
        function showDeleteConfirmation(bookId, bookName, sourceControlUniqueId) {
            const confirmationText = 'XÓA';
            
            Swal.fire({
                title: '<span style="color: #d33;">BẠN CHẮC CHẮN MUỐN XÓA?</span>',
                html: `
                    <div class="text-left">
                        <p class="mb-2">Bạn đang chuẩn bị xóa vĩnh viễn sách:</p>
                        <p class="font-bold text-lg text-red-600 mb-4">${bookName} (ID: ${bookId})</p>
                        <p class="font-semibold mb-2">Hành động này <strong>không thể hoàn tác</strong> và sẽ xóa tất cả dữ liệu liên quan:</p>
                        <ul class="list-disc list-inside text-sm text-gray-600 space-y-1 bg-gray-50 p-3 rounded-md border">
                            <li>Trong Tủ sách của tất cả người dùng</li>
                            <li>Trong Giỏ hàng của tất cả người dùng</li>
                            <li>Trong các Chi tiết đơn hàng đã có</li>
                            <li>Tất cả bình luận và tương tác</li>
                            <li>Nội dung sách, chương sách (nếu có)</li>
                        </ul>
                        <p class="mt-4">Để xác nhận, vui lòng gõ <strong class="text-red-700">${confirmationText}</strong> vào ô bên dưới:</p>
                    </div>`,
                icon: 'warning',
                input: 'text',
                inputPlaceholder: `Gõ '${confirmationText}' để xác nhận`,
                inputAttributes: {
                    autocapitalize: 'off',
                    autocorrect: 'off'
                },
                showCancelButton: true,
                confirmButtonText: 'Tôi hiểu và Xác Nhận Xóa',
                cancelButtonText: 'Hủy',
                confirmButtonColor: '#d33', // Màu đỏ cho nút xóa
                cancelButtonColor: '#3085d6',
                // Kiểm tra đầu vào trước khi xác nhận
                preConfirm: () => {
                    if (Swal.getInput().value.trim().toUpperCase() !== confirmationText) {
                        Swal.showValidationMessage(`Vui lòng gõ chính xác "${confirmationText}" để xác nhận.`);
                        return false; // Ngăn không cho đóng popup
                    }
                    return true;
                },
                allowOutsideClick: () => !Swal.isLoading()
            }).then((result) => {
                if (result.isConfirmed) {
                    // Nếu xác nhận thành công, trigger postback của ASP.NET để thực thi lệnh xóa
                    __doPostBack(sourceControlUniqueId, '');
                }
            });
        }
    </script>
    <%-- KẾT THÚC: THÊM SCRIPT CHO POPUP XÓA --%>
</asp:Content>