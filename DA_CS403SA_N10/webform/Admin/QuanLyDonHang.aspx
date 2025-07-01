<%@ Page Title="Quản Lý Đơn Hàng" Language="C#" MasterPageFile="~/WebForm/Admin/Admin.Master" AutoEventWireup="true" CodeBehind="QuanLyDonHang.aspx.cs" Inherits="Webebook.WebForm.Admin.QuanLyDonHang" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <%-- Các thư viện cần thiết --%>
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.0.0/css/all.min.css" />
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/flatpickr/dist/flatpickr.min.css">
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    
    <%-- ==================== BẮT ĐẦU CSS ĐÃ SỬA ==================== --%>
    <style>
        /* CSS tiêu chuẩn, không dùng @apply */
        .form-label {
            display: block;
            font-size: 0.875rem; /* 14px */
            line-height: 1.25rem;
            font-weight: 500;
            color: #374151; /* gray-700 */
            margin-bottom: 0.25rem;
        }

        .form-input, .form-select {
            margin-top: 0.25rem;
            display: block;
            width: 100%;
            padding: 0.5rem 0.75rem;
            background-color: #fff;
            border: 1px solid #d1d5db; /* gray-300 */
            border-radius: 0.375rem; /* rounded-md */
            box-shadow: 0 1px 2px 0 rgba(0, 0, 0, 0.05);
            font-size: 0.875rem;
        }
        .form-input:focus, .form-select:focus {
            outline: none;
            --tw-ring-color: #4f46e5; /* indigo-500 */
            --tw-ring-offset-shadow: var(--tw-ring-inset) 0 0 0 var(--tw-ring-offset-width) var(--tw-ring-offset-color);
            --tw-ring-shadow: var(--tw-ring-inset) 0 0 0 calc(1px + var(--tw-ring-offset-width)) var(--tw-ring-color);
            box-shadow: var(--tw-ring-offset-shadow), var(--tw-ring-shadow), var(--tw-shadow, 0 0 #0000);
            border-color: #4f46e5;
        }

        .btn {
            display: inline-flex;
            align-items: center;
            justify-content: center;
            padding: 0.5rem 1rem;
            border-width: 1px;
            box-shadow: 0 1px 2px 0 rgba(0, 0, 0, 0.05);
            font-size: 0.875rem;
            font-weight: 500;
            border-radius: 0.375rem;
            transition-property: color, background-color, border-color, text-decoration-color, fill, stroke, opacity, box-shadow, transform, filter, backdrop-filter;
            transition-timing-function: cubic-bezier(0.4, 0, 0.2, 1);
            transition-duration: 150ms;
            text-decoration: none;
        }
        .btn:focus {
            outline: 2px solid transparent;
            outline-offset: 2px;
            --tw-ring-offset-width: 2px;
            --tw-ring-color: #4f46e5; /* indigo-500 */
            --tw-ring-offset-shadow: var(--tw-ring-inset) 0 0 0 var(--tw-ring-offset-width) var(--tw-ring-offset-color);
            --tw-ring-shadow: var(--tw-ring-inset) 0 0 0 calc(2px + var(--tw-ring-offset-width)) var(--tw-ring-color);
            box-shadow: var(--tw-ring-offset-shadow), var(--tw-ring-shadow), var(--tw-shadow, 0 0 #0000);
        }

        .btn-primary {
            border-color: transparent;
            color: #fff;
            background-color: #4f46e5; /* indigo-600 */
        }
        .btn-primary:hover {
            background-color: #4338ca; /* indigo-700 */
        }
        .btn-secondary {
            border-color: #d1d5db; /* gray-300 */
            color: #374151; /* gray-700 */
            background-color: #fff;
        }
        .btn-secondary:hover {
            background-color: #f9fafb; /* gray-50 */
        }

        .status-badge { display: inline-block; padding: 2px 10px; font-size: 0.75rem; font-weight: 600; border-radius: 9999px; line-height: 1.5; }
        .status-pending { background-color: #fef3c7; color: #92400e; }
        .status-completed { background-color: #d1fae5; color: #065f46; }
        .status-cancelled { background-color: #fee2e2; color: #991b1b; }
        .status-failed { background-color: #e5e7eb; color: #374151; }
        .status-default { background-color: #f3f4f6; color: #4b5563; }
        .flatpickr-calendar { z-index: 1050 !important; }
    </style>
    <%-- ==================== KẾT THÚC CSS ĐÃ SỬA ==================== --%>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="container mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <h2 class="text-3xl font-bold leading-tight text-gray-900 mb-6">Quản Lý Đơn Hàng</h2>

        <%-- Khu vực thông báo --%>
        <asp:Panel ID="pnlMessage" runat="server" Visible="false" role="alert" class="mb-4 p-4 rounded-md">
            <div class="flex">
                <div class="py-1">
                    <i id="iconMessage" runat="server" class="fas fa-fw mr-2"></i>
                </div>
                <div>
                    <asp:Label ID="lblMessage" runat="server" EnableViewState="false"></asp:Label>
                </div>
            </div>
        </asp:Panel>

        <%-- ==================== BẮT ĐẦU KHU VỰC LỌC ĐÃ THIẾT KẾ LẠI ==================== --%>
        <div class="bg-white p-4 rounded-lg shadow-md border border-gray-200 mb-8">
            <div class="flex flex-wrap items-end gap-4">
                <div>
                    <label for="<%= ddlFilterStatus.ClientID %>" class="form-label">Trạng Thái</label>
                    <asp:DropDownList ID="ddlFilterStatus" runat="server" CssClass="form-select min-w-[150px]" AutoPostBack="true" OnSelectedIndexChanged="FilterChanged">
                        <asp:ListItem Text="Tất cả" Value=""></asp:ListItem>
                        <asp:ListItem Text="Chờ" Value="Pending"></asp:ListItem>
                        <asp:ListItem Text="Hoàn thành" Value="Completed"></asp:ListItem>
                        <asp:ListItem Text="Bị hủy bỏ" Value="Cancelled"></asp:ListItem>
                        <asp:ListItem Text="Thất bại" Value="Failed"></asp:ListItem>
                    </asp:DropDownList>
                </div>
                <div>
                    <label for="<%= ddlFilterPaymentMethod.ClientID %>" class="form-label">Phương thức</label>
                    <asp:DropDownList ID="ddlFilterPaymentMethod" runat="server" CssClass="form-select min-w-[180px]" AutoPostBack="true" OnSelectedIndexChanged="FilterChanged">
                        <asp:ListItem Text="Tất cả" Value=""></asp:ListItem>
                        <asp:ListItem Text="Chuyển khoản" Value="Bank"></asp:ListItem>
                        <asp:ListItem Text="Thẻ ngân hàng" Value="Card"></asp:ListItem>
                        <asp:ListItem Text="Ví điện tử" Value="Wallet"></asp:ListItem>
                    </asp:DropDownList>
                </div>
                <div>
                    <label for="txtFilterStartDate" class="form-label">Từ ngày</label>
                    <asp:TextBox ID="txtFilterStartDate" runat="server" CssClass="form-input min-w-[150px]" placeholder="dd/MM/yyyy" ClientIDMode="Static"></asp:TextBox>
                </div>
                <div>
                    <label for="txtFilterEndDate" class="form-label">Đến ngày</label>
                    <asp:TextBox ID="txtFilterEndDate" runat="server" CssClass="form-input min-w-[150px]" placeholder="dd/MM/yyyy" ClientIDMode="Static"></asp:TextBox>
                </div>
                <div class="flex items-center space-x-2">
                    <asp:Button ID="btnApplyFilter" runat="server" Text="Lọc" CssClass="btn btn-primary" OnClick="ApplyFilter_Click" />
                    <asp:Button ID="btnResetFilter" runat="server" Text="Reset" CssClass="btn btn-secondary" OnClick="ResetFilter_Click" CausesValidation="false" />
                </div>
            </div>
        </div>
        <%-- ==================== KẾT THÚC KHU VỰC LỌC ĐÃ THIẾT KẾ LẠI ==================== --%>


        <%-- Khu vực GridView (giữ nguyên không đổi) --%>
        <div class="bg-white shadow-md overflow-hidden sm:rounded-lg border border-gray-200">
            <div class="overflow-x-auto">
                <asp:GridView ID="GridViewDonHang" runat="server" AutoGenerateColumns="False" DataKeyNames="IDDonHang,IDNguoiDung" CssClass="min-w-full divide-y divide-gray-200"
                    OnRowCommand="GridViewDonHang_RowCommand" AllowPaging="True" PageSize="15" OnPageIndexChanging="GridViewDonHang_PageIndexChanging" OnRowDataBound="GridViewDonHang_RowDataBound">
                    <HeaderStyle CssClass="bg-gray-100" Font-Bold="true" />
                    <RowStyle CssClass="bg-white hover:bg-indigo-50 transition duration-150 ease-in-out" />
                    <AlternatingRowStyle CssClass="bg-gray-50 hover:bg-indigo-50 transition duration-150 ease-in-out" />
                    <PagerStyle CssClass="bg-gray-100 px-4 py-3 border-t border-gray-200 text-right" HorizontalAlign="Right" />
                    <EmptyDataTemplate>
                        <div class="text-center py-12 px-6">
                            <svg class="mx-auto h-12 w-12 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor" aria-hidden="true">
                                <path vector-effect="non-scaling-stroke" stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 13h6m-3-3v6m-9 1V7a2 2 0 012-2h10l4 4v10a2 2 0 01-2 2H4a2 2 0 01-2-2z" />
                            </svg>
                            <h3 class="mt-2 text-sm font-medium text-gray-900">Không có đơn hàng</h3>
                            <p class="mt-1 text-sm text-gray-500">Không tìm thấy đơn hàng nào phù hợp với bộ lọc hiện tại.</p>
                        </div>
                    </EmptyDataTemplate>
                    <Columns>
                        <asp:BoundField DataField="IDDonHang" HeaderText="ID ĐH" ReadOnly="True" SortExpression="IDDonHang" HeaderStyle-CssClass="px-4 py-3.5 text-left text-xs font-semibold text-gray-700 uppercase tracking-wider w-16" ItemStyle-CssClass="whitespace-nowrap px-4 py-4 text-sm text-gray-500 font-medium" />
                        <asp:TemplateField HeaderText="Người Đặt" SortExpression="TenNguoiDung" HeaderStyle-CssClass="px-4 py-3.5 text-left text-xs font-semibold text-gray-700 uppercase tracking-wider min-w-[180px]">
                            <ItemTemplate>
                                <div class="font-medium text-gray-900 truncate" title='<%# Eval("TenNguoiDung") %>'>
                                    <%# Eval("TenNguoiDung") %>
                                </div>
                                <div class="text-gray-500 text-xs">ID: <%# Eval("IDNguoiDung") %></div>
                            </ItemTemplate>
                            <ItemStyle CssClass="px-4 py-4 text-sm"/>
                        </asp:TemplateField>
                        <asp:BoundField DataField="NgayDat" HeaderText="Ngày Đặt" DataFormatString="{0:dd/MM/yyyy HH:mm}" SortExpression="NgayDat" HeaderStyle-CssClass="px-4 py-3.5 text-left text-xs font-semibold text-gray-700 uppercase tracking-wider" ItemStyle-CssClass="whitespace-nowrap px-4 py-4 text-sm text-gray-500" />
                        <asp:BoundField DataField="SoTien" HeaderText="Tổng Tiền" DataFormatString="{0:N0} VNĐ" SortExpression="SoTien" HeaderStyle-CssClass="px-4 py-3.5 text-right text-xs font-semibold text-gray-700 uppercase tracking-wider" ItemStyle-CssClass="whitespace-nowrap px-4 py-4 text-sm text-gray-600 text-right font-semibold" />
                        <asp:TemplateField HeaderText="PT Thanh Toán" SortExpression="PhuongThucThanhToan" HeaderStyle-CssClass="px-4 py-3.5 text-left text-xs font-semibold text-gray-700 uppercase tracking-wider">
                            <ItemTemplate>
                                <%# GetPaymentMethodText(Eval("PhuongThucThanhToan")?.ToString()) %>
                            </ItemTemplate>
                            <ItemStyle CssClass="whitespace-nowrap px-4 py-4 text-sm text-gray-500" />
                        </asp:TemplateField>
                    <%-- Cột Trạng Thái TT (Chỉ còn ItemTemplate) --%>
                    <asp:TemplateField HeaderText="Trạng Thái TT" SortExpression="TrangThaiThanhToan" HeaderStyle-CssClass="px-4 py-3.5 text-left text-xs font-semibold text-gray-700 uppercase tracking-wider" ItemStyle-CssClass="px-4 py-4 text-sm text-gray-500">
                        <ItemTemplate>
                            <span class='status-badge <%# GetStatusCssClass(Eval("TrangThaiThanhToan")?.ToString()) %>'>
                                <%# GetStatusText(Eval("TrangThaiThanhToan")?.ToString()) %>
                            </span>
                        </ItemTemplate>
                        <%-- Đã xóa EditItemTemplate --%>
                    </asp:TemplateField>

                    <%-- Cột Hành Động (Chỉ còn ItemTemplate) --%>
                    <asp:TemplateField HeaderText="Hành Động" HeaderStyle-CssClass="px-6 py-3.5 text-right text-xs font-semibold text-gray-700 uppercase tracking-wider" ItemStyle-CssClass="whitespace-nowrap px-6 py-4 text-right text-sm font-medium">
                        <ItemTemplate>
                            <div class="flex justify-end items-center space-x-4">
                                <asp:HyperLink ID="hlViewDetails" runat="server" NavigateUrl='<%# ResolveUrl("~/WebForm/Admin/ChiTietDonHang_Admin.aspx?IDDonHang=") + Eval("IDDonHang") %>' CssClass="text-gray-500 hover:text-indigo-700 transition" ToolTip="Xem chi tiết"><i class="fas fa-eye fa-fw text-base"></i></asp:HyperLink>
            
                                <%-- Nút Sửa giờ sẽ gọi popup --%>
                                <asp:LinkButton ID="lnkEditStatus" runat="server" CommandName="ShowEditPopup" CommandArgument='<%# Eval("IDDonHang") %>' CssClass="text-gray-500 hover:text-blue-700 transition" ToolTip="Sửa trạng thái"><i class="fas fa-pencil-alt fa-fw text-base"></i></asp:LinkButton>
            
                                <asp:LinkButton ID="lnkDeleteOrder" runat="server" CommandName="DeleteOrder" CommandArgument='<%# Eval("IDDonHang") %>' CssClass="text-red-500 hover:text-red-700 transition" ToolTip="Xóa đơn hàng"><i class="fas fa-trash-alt fa-fw text-base"></i></asp:LinkButton>
                            </div>
                        </ItemTemplate>
                        <%-- Đã xóa EditItemTemplate --%>
                    </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
        </div>
    </div>

    <%-- JavaScript Section --%>
    <script type="text/javascript">
        // Popup xác nhận xóa
        function showOrderDeleteConfirmation(orderId, sourceControlUniqueId) {
            Swal.fire({
                title: 'Bạn có chắc chắn muốn xóa?',
                html: `Bạn sắp xóa vĩnh viễn <strong>Đơn hàng #${orderId}</strong>.<br/>Tất cả chi tiết đơn hàng liên quan cũng sẽ bị xóa và hành động này <strong>không thể hoàn tác!</strong>`,
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#d33',
                cancelButtonColor: '#3085d6',
                confirmButtonText: '<i class="fas fa-trash-alt"></i> Có, xóa ngay!',
                cancelButtonText: 'Hủy bỏ'
            }).then((result) => {
                if (result.isConfirmed) {
                    __doPostBack(sourceControlUniqueId, '');
                }
            });
        }
    </script>
    <script src="https://cdn.jsdelivr.net/npm/flatpickr"></script>
    <script src="https://npmcdn.com/flatpickr/dist/l10n/vn.js"></script>
    <script>
        // Khởi tạo date pickers
        document.addEventListener('DOMContentLoaded', function () {
            flatpickr("#txtFilterStartDate", { dateFormat: "d/m/Y", locale: "vn", allowInput: true });
            flatpickr("#txtFilterEndDate", { dateFormat: "d/m/Y", locale: "vn", allowInput: true });
        });

        // ==================== BẮT ĐẦU: THÊM HÀM POPUP SỬA TRẠNG THÁI ====================
        function showEditStatusPopup(orderId, currentStatus, username) {
            Swal.fire({
                title: 'Cập nhật trạng thái',
                html: `Cho <strong>Đơn hàng #${orderId}</strong> của người dùng <strong>${username}</strong>`,
                input: 'select',
                inputOptions: {
                    'Pending': 'Chờ',
                    'Completed': 'Hoàn thành',
                    'Cancelled': 'Bị hủy bỏ',
                    'Failed': 'Thất bại'
                },
                inputValue: currentStatus, // Giá trị hiện tại được chọn sẵn
                showCancelButton: true,
                confirmButtonText: 'Cập nhật',
                cancelButtonText: 'Hủy',
                inputValidator: (value) => {
                    if (!value) {
                        return 'Bạn cần chọn một trạng thái!'
                    }
                }
            }).then((result) => {
                // Nếu người dùng nhấn "Cập nhật" và có chọn một giá trị
                if (result.isConfirmed && result.value) {
                    const newStatus = result.value;
                    // Trigger một postback tùy chỉnh với Target là 'UpdateOrderStatus'
                    // và Argument là chuỗi chứa ID và trạng thái mới
                    __doPostBack('UpdateOrderStatus', `${orderId}:${newStatus}`);
                }
            });
        }
        // ==================== KẾT THÚC: THÊM HÀM POPUP SỬA TRẠNG THÁI ====================
    </script>
</asp:Content>