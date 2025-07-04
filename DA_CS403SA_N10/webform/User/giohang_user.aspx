﻿<%@ Page Title="Giỏ Hàng" Language="C#" MasterPageFile="~/WebForm/User/User.Master" AutoEventWireup="true" CodeBehind="giohang_user.aspx.cs" Inherits="Webebook.WebForm.User.giohang_user" %>
<%@ Import Namespace="System.Data" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <%-- Font Awesome và Styles không thay đổi --%>
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.0.0/css/all.min.css" />
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>

    <style>
        /* Styles không thay đổi */
        #<%= gvGioHang.ClientID %> {
            width: 100%;
            border-collapse: collapse;
        }
        #<%= gvGioHang.ClientID %> th,
        #<%= gvGioHang.ClientID %> td {
            padding: 0.75rem 1.25rem;
            vertical-align: middle;
        }
        #<%= gvGioHang.ClientID %> th {
            background-color: #F3F4F6; /* gray-100 */
            color: #6B7280; /* gray-500 */
            font-size: 0.75rem; /* text-xs */
            font-weight: 500; /* medium */
            text-transform: uppercase;
            letter-spacing: 0.05em; /* tracking-wider */
        }
        .gv-col-price, .gv-col-delete {
            text-align: right;
        }
        .gv-col-name {
            text-align: left;
        }
        .gridview-row:hover {
            background-color: #F9FAFB; /* gray-50 */
        }
        .item-checkbox, .header-checkbox {
            width: 1.5rem;
            height: 1.5rem;
            accent-color: #3B82F6; /* blue-500 */
        }
        .action-icons a {
            font-size: 1.25rem;
            color: #9CA3AF; /* gray-400 */
        }
        .action-icons a:hover {
            color: #EF4444; /* red-500 */
        }
        .product-image {
            width: 60px;
            height: 90px;
            object-fit: cover;
            border-radius: 0.375rem;
            border: 1px solid #E5E7EB; /* gray-200 */
            background-color: #F9FAFB; /* gray-50 */
            margin-left: auto;
            margin-right: auto;
            display: block;
        }
        .product-link:hover .product-image {
            opacity: 0.9;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
        }
        .book-title-link {
            color: #1F2937; /* gray-800 */
            font-weight: 500; /* medium */
            transition: color 0.15s ease-in-out;
        }
        .book-title-link:hover {
            color: #2563EB; /* blue-600 */
            text-decoration: underline;
        }

        @media (max-width: 640px) {
            #<%= gvGioHang.ClientID %> { display: block; }
            #<%= gvGioHang.ClientID %> thead { display: none; }
            #<%= gvGioHang.ClientID %> tr {
                display: flex;
                flex-direction: column;
                margin-bottom: 1.5rem;
                padding: 1rem;
                border: 1px solid #E5E7EB;
                border-radius: 0.5rem;
                background-color: #fff;
                box-shadow: 0 1px 3px rgba(0,0,0,0.1);
            }
            #<%= gvGioHang.ClientID %> td {
                display: flex;
                justify-content: space-between;
                align-items: center;
                padding: 0.5rem 0;
                border-bottom: 1px solid #F3F4F6;
            }
            #<%= gvGioHang.ClientID %> td:last-child { border-bottom: none; }
            /* Vô hiệu hóa nhãn ::before bằng cách không có thuộc tính data-label để đọc */
            #<%= gvGioHang.ClientID %> td::before {
                content: attr(data-label);
                font-weight: 600;
                color: #374151;
                margin-right: 0.5rem;
            }
            .item-checkbox, .header-checkbox { width: 1.5rem; height: 1.5rem; }
            .action-icons a { font-size: 1.25rem; padding: 0.5rem; }
            .gv-col-image { display: none; }
            .gv-col-name { text-align: right; }
            .book-title-link { max-width: 100%; white-space: normal; text-align: right; }
            .gv-col-price { justify-content: flex-end; }
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="container mx-auto px-4 lg:px-8 py-8">
        <h2 class="text-3xl font-semibold text-gray-800 mb-6 border-b pb-4">Giỏ Hàng Của Bạn</h2>

        <asp:Label ID="lblMessage" runat="server" CssClass="block mb-4 text-sm p-3 rounded-md" EnableViewState="false" Visible="false"></asp:Label>

        <asp:Panel ID="pnlCart" runat="server" Visible="false">
            <div class="bg-white shadow-md rounded-lg overflow-x-auto mb-8 border border-gray-200">
                <asp:GridView ID="gvGioHang" runat="server"
                    AutoGenerateColumns="False"
                    DataKeyNames="IDGioHang,IDSach"
                    CssClass="min-w-full"
                    GridLines="None"
                    OnRowCommand="gvGioHang_RowCommand"
                    OnRowDataBound="gvGioHang_RowDataBound"
                    EmptyDataText="<div class='text-center py-10 text-gray-500'>Giỏ hàng của bạn đang trống.</div>">
                    <HeaderStyle CssClass="bg-gray-100 border-b border-gray-200 text-gray-500" />
                    <RowStyle CssClass="border-b border-gray-200 gridview-row" />
                    <AlternatingRowStyle CssClass="border-b border-gray-200 gridview-row bg-gray-50" />
                    <EmptyDataRowStyle CssClass="border-t border-gray-200" />
<Columns>
                        <asp:TemplateField HeaderStyle-CssClass="px-4 py-3 text-center text-xs font-medium uppercase tracking-wider gv-col-checkbox">
                            <HeaderTemplate>
                                <div class="flex justify-center">
                                    <asp:CheckBox ID="chkHeader" runat="server" ToolTip="Chọn/Bỏ chọn tất cả" CssClass="header-checkbox form-checkbox"/>
                                </div>
                            </HeaderTemplate>
                            <ItemTemplate>
                                <%-- === MODIFIED: Đã xóa span "Chọn" và data-label. Div sẽ căn chỉnh checkbox sang phải. === --%>
                                <div class="flex justify-end items-center w-full">
                                    <asp:CheckBox ID="chkSelect" runat="server" CssClass="item-checkbox form-checkbox"/>
                                </div>
                            </ItemTemplate>
                            <ItemStyle CssClass="px-4 py-3 text-center"/>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Ảnh Bìa" HeaderStyle-CssClass="px-4 py-3 text-center text-xs font-medium uppercase tracking-wider gv-col-image">
                            <ItemTemplate>
                                <%-- === MODIFIED: Đã xóa data-label. === --%>
                                <div class="flex justify-center items-center py-2">
                                    <asp:HyperLink ID="hlProductImage" runat="server" CssClass="product-link"
                                        NavigateUrl='<%# string.Format("~/WebForm/User/chitietsach_user.aspx?IDSach={0}", Eval("IDSach")) %>'
                                        ToolTip='<%# "Xem chi tiết " + Eval("TenSach") %>'>
                                        <asp:Image ID="imgProduct" runat="server" CssClass="product-image"
                                            ImageUrl='<%# Eval("DuongDanBiaSach") != DBNull.Value && !string.IsNullOrEmpty(Eval("DuongDanBiaSach").ToString()) ? ResolveUrl(Eval("DuongDanBiaSach").ToString()) : ResolveUrl("~/Images/placeholder_cover.png") %>'
                                            AlternateText='<%# Eval("TenSach") %>' />
                                    </asp:HyperLink>
                                </div>
                            </ItemTemplate>
                            <ItemStyle CssClass="px-4 py-3"/>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Tên Sách" HeaderStyle-CssClass="px-6 py-3 text-left text-xs font-medium uppercase tracking-wider gv-col-name">
                            <ItemTemplate>
                                <%-- === MODIFIED: Đã xóa data-label. Div sẽ căn chỉnh link sang trái. === --%>
                                <div class="flex justify-start items-center w-full">
                                    <asp:HyperLink ID="hlTenSach" runat="server" CssClass="book-title-link text-sm"
                                        NavigateUrl='<%# string.Format("~/WebForm/User/chitietsach_user.aspx?IDSach={0}", Eval("IDSach")) %>'
                                        Text='<%# Eval("TenSach") %>'
                                        ToolTip='<%# "Xem chi tiết " + Eval("TenSach") %>' />
                                </div>
                            </ItemTemplate>
                            <ItemStyle CssClass="px-6 py-3"/>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Giá Sách (VNĐ)" HeaderStyle-CssClass="px-6 py-3 text-right text-xs font-medium uppercase tracking-wider gv-col-price">
                            <ItemTemplate>
                                <%-- === MODIFIED: Đã xóa span "Giá Sách" và data-label. Div sẽ căn chỉnh giá sang phải. === --%>
                                <div class="flex justify-end items-center w-full">
                                    <asp:Label ID="lblDonGia" runat="server"
                                        CssClass="item-price-display text-sm font-medium text-gray-700"
                                        Text='<%# string.Format("{0:N0} VNĐ", Convert.ToDecimal(Eval("GiaSach"))) %>'></asp:Label>
                                </div>
                            </ItemTemplate>
                            <ItemStyle CssClass="px-6 py-3 whitespace-nowrap text-sm font-medium text-gray-700 text-right"/>
                        </asp:TemplateField>

                        <asp:TemplateField HeaderText="Xóa" HeaderStyle-CssClass="px-6 py-3 text-center text-xs font-medium uppercase tracking-wider gv-col-delete">
                            <ItemTemplate>
                                <%-- === MODIFIED: Đã xóa span "Xóa" và data-label. Div sẽ căn chỉnh nút xóa sang phải. === --%>
                                <div class="flex justify-end items-center w-full">
                                <asp:LinkButton ID="lnkXoa" runat="server" CommandName="Xoa"
                                    CommandArgument='<%# Eval("IDGioHang") %>'
                                    CssClass="text-gray-400 hover:text-red-500 transition duration-150 ease-in-out" ToolTip="Xóa khỏi giỏ hàng">
                                    <%-- Thuộc tính OnClientClick đã được xóa bỏ --%>
                                    <i class="fas fa-trash-alt fa-fw text-base"></i>
                                </asp:LinkButton>
                                </div>
                            </ItemTemplate>
                            <ItemStyle CssClass="px-6 py-3 text-center"/>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>

            <%-- Phần Tổng cộng và Nút Thanh toán không thay đổi --%>
            <div class="mt-6 flex flex-col sm:flex-row justify-between items-center bg-white p-5 rounded-lg shadow-sm border border-gray-200">
                <div class="text-lg sm:text-xl text-gray-700 mb-3 sm:mb-0 total-section">
                    <span>Tổng cộng (<asp:Label ID="lblSelectedItemCount" runat="server" Text="0"></asp:Label> chọn): </span>
                    <asp:Label ID="lblSelectedTotal" runat="server" Text="0 VNĐ" CssClass="text-red-600 font-bold"></asp:Label>
                </div>
                <asp:Button ID="btnThanhToan" runat="server" Text="Tiến Hành Thanh Toán"
                    CssClass="bg-blue-600 hover:bg-blue-700 text-white font-bold py-2.5 px-6 rounded-md shadow hover:shadow-md focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500 transition duration-150 ease-in-out disabled:opacity-60 disabled:bg-gray-400 disabled:cursor-not-allowed disabled:shadow-none"
                    OnClick="btnThanhToan_Click" Enabled="false" />
            </div>
        </asp:Panel>

        <%-- Panel Giỏ hàng trống không thay đổi --%>
        <asp:Panel ID="pnlEmptyCart" runat="server" Visible="true" CssClass="text-center py-16 bg-white rounded-lg shadow-md border border-gray-200">
            <i class="fas fa-shopping-cart fa-3x text-gray-400 mb-4"></i>
            <p class="text-gray-500 text-lg mb-5">Giỏ hàng của bạn hiện đang trống.</p>
            <asp:HyperLink runat="server" NavigateUrl="~/WebForm/User/usertrangchu.aspx" Text="Tiếp tục mua sắm"
                CssClass="inline-block bg-blue-500 hover:bg-blue-600 text-white font-semibold py-2 px-5 rounded-md transition duration-150 ease-in-out shadow"/>
        </asp:Panel>
    </div>

    <%-- JavaScript không thay đổi --%>
    <script type="text/javascript">
        // Các hàm JavaScript hiện có: formatCurrency, updateTotalAndCheckoutButtonState, initializeCartEvents
        function formatCurrency(value) {
            const numberValue = Number(value);
            if (isNaN(numberValue)) { return "0 VNĐ"; }
            return numberValue.toLocaleString('vi-VN') + " VNĐ";
        }

        function updateTotalAndCheckoutButtonState() {
            let selectedTotal = 0;
            let selectedCount = 0;
            let itemSelected = false;
            const gridView = document.getElementById('<%= gvGioHang.ClientID %>');
            const checkoutButton = document.getElementById('<%= btnThanhToan.ClientID %>');
            const totalLabel = document.getElementById('<%= lblSelectedTotal.ClientID %>');
            const countLabel = document.getElementById('<%= lblSelectedItemCount.ClientID %>');
            let headerCheckboxInput = null;

            if (!gridView || !checkoutButton || !totalLabel || !countLabel) {
                console.error("Cart elements missing!");
                return;
            }

            const headerCheckboxElement = gridView.querySelector('th .header-checkbox');
            if (headerCheckboxElement) {
                headerCheckboxInput = headerCheckboxElement.querySelector('input[type=checkbox]');
                if (!headerCheckboxInput) headerCheckboxInput = headerCheckboxElement;
            }

            const itemCheckboxInputs = gridView.querySelectorAll('.item-checkbox input[type=checkbox], input.item-checkbox');
            let allItemsChecked = true;

            itemCheckboxInputs.forEach(checkboxInput => {
                const row = checkboxInput.closest('tr');
                let priceElement = row ? row.querySelector('[data-price]') : checkboxInput.closest('[data-price]');
                if (!priceElement) {
                    // Fallback to find the checkbox's parent div which now holds the data-price
                    const parentDiv = checkboxInput.closest('div[data-price]');
                    if (parentDiv) priceElement = parentDiv;
                    else priceElement = checkboxInput;
                }

                const priceString = priceElement.getAttribute('data-price') || '0';
                const price = parseFloat(priceString) || 0;

                if (checkboxInput.checked) {
                    selectedTotal += price;
                    selectedCount++;
                    itemSelected = true;
                } else {
                    allItemsChecked = false;
                }
            });

            totalLabel.textContent = formatCurrency(selectedTotal);
            countLabel.textContent = selectedCount;
            checkoutButton.disabled = !itemSelected;

            if (headerCheckboxInput) {
                headerCheckboxInput.checked = itemCheckboxInputs.length > 0 && allItemsChecked;
            }
        }

        function initializeCartEvents() {
            const gridView = document.getElementById('<%= gvGioHang.ClientID %>');
            if (!gridView) {
                console.warn('GridView not found on init.');
                return;
            }

            const headerCheckboxElement = gridView.querySelector('th .header-checkbox');
            if (headerCheckboxElement) {
                const headerCheckboxInput = headerCheckboxElement.querySelector('input[type=checkbox]') || headerCheckboxElement;
                if (headerCheckboxInput) {
                    headerCheckboxInput.addEventListener('change', function () {
                        const isChecked = headerCheckboxInput.checked;
                        const itemInputs = gridView.querySelectorAll('.item-checkbox input[type=checkbox], input.item-checkbox');
                        itemInputs.forEach(itemInput => {
                            if (!itemInput.disabled) {
                                itemInput.checked = isChecked;
                            }
                        });
                        updateTotalAndCheckoutButtonState();
                    });
                } else { console.warn('Header checkbox INPUT not found!'); }
            } else { console.warn('Header checkbox element not found!'); }

            const itemCheckboxInputs = gridView.querySelectorAll('.item-checkbox input[type=checkbox], input.item-checkbox');
            itemCheckboxInputs.forEach(checkboxInput => {
                checkboxInput.addEventListener('change', function () {
                    updateTotalAndCheckoutButtonState();
                });
            });

            updateTotalAndCheckoutButtonState();
        }

        if (document.readyState === 'loading') {
            document.addEventListener('DOMContentLoaded', initializeCartEvents);
        } else {
            initializeCartEvents();
        }
        if (typeof (Sys) !== 'undefined' && Sys.WebForms && Sys.WebForms.PageRequestManager) {
            Sys.WebForms.PageRequestManager.getInstance().add_endRequest(initializeCartEvents);
        }

        // ==================== BẮT ĐẦU: THÊM HÀM POPUP XÓA ====================
        function showCartItemDeleteConfirmation(cartItemId, bookTitle, sourceControlUniqueId) {
            Swal.fire({
                title: 'Xóa sách khỏi giỏ hàng?',
                html: `Bạn có chắc chắn muốn xóa sách<br><strong>${bookTitle}</strong>`,
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#d33',
                cancelButtonColor: '#6b7280',
                confirmButtonText: '<i class="fas fa-trash-alt"></i> Đồng ý, Xóa!',
                cancelButtonText: 'Hủy'
            }).then((result) => {
                if (result.isConfirmed) {
                    // Nếu xác nhận, trigger postback để server thực thi lệnh xóa
                    __doPostBack(sourceControlUniqueId, '');
                }
            });
        }
        // ==================== KẾT THÚC: THÊM HÀM POPUP XÓA ====================
    </script>
</asp:Content>