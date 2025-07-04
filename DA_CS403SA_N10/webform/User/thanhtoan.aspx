<%@ Page Title="Thanh Toán" Language="C#" MasterPageFile="~/WebForm/User/User.Master" AutoEventWireup="true" CodeBehind="thanhtoan.aspx.cs" Inherits="Webebook.WebForm.User.thanhtoan" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css" integrity="sha512-iecdLmaskl7CVkqkXNQ/ZH/XLlvWZOJyj7Yy7tcenmpD1ypASozpmT/E0iPtmFIB46ZmdtAc9eNBvH0H/ZpiBw==" crossorigin="anonymous" referrerpolicy="no-referrer" />
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>
    <%-- <script src="https://cdn.tailwindcss.com"></script> --%>
    <style>
        /* CSS gốc (giữ nguyên) */
        #loadingOverlay { position: fixed; top: 0; left: 0; width: 100%; height: 100%; background-color: rgba(0, 0, 0, 0.6); z-index: 9999; display: flex; justify-content: center; align-items: center; flex-direction: column; visibility: hidden; opacity: 0; transition: opacity 0.3s ease-in-out, visibility 0.3s ease-in-out; }
        #loadingOverlay.visible { visibility: visible; opacity: 1; }
        .spinner { border: 4px solid rgba(255, 255, 255, 0.3); width: 48px; height: 48px; border-radius: 50%; border-left-color: #ffffff; animation: spin 1s ease infinite; margin-bottom: 16px; }
        @keyframes spin { 0% { transform: rotate(0deg); } 100% { transform: rotate(360deg); } }
        #loadingOverlay p { color: #e5e7eb; font-weight: 600; font-size: 1.1rem; }
        .payment-option-item label { display: flex; align-items: center; padding: 1rem 1.25rem; border: 2px solid #e5e7eb; border-radius: 0.5rem; cursor: pointer; transition: all 0.25s ease-out; width: 100%; background-color: #fff; box-shadow: 0 1px 3px 0 rgba(0, 0, 0, 0.1), 0 1px 2px 0 rgba(0, 0, 0, 0.06); }
        .payment-option-item label:hover { background-color: #f9fafb; border-color: #9ca3af; box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1), 0 2px 4px -1px rgba(0, 0, 0, 0.06); }
        .payment-option-item.selected label { border-color: #3b82f6; background-color: #eff6ff; font-weight: 600; color: #1d4ed8; box-shadow: 0 0 0 2px rgba(59, 130, 246, 0.4); }
        .payment-option-item input[type="radio"] { position: absolute; opacity: 0; width: 0; height: 0; }
        .payment-option-item label i { width: 20px; text-align: center; margin-right: 0.75rem; }
        .payment-details-panel { transition: opacity 0.3s ease-out, max-height 0.4s ease-out; overflow: hidden; max-height: 0; opacity: 0; }
        .payment-details-panel.visible { max-height: 1000px; opacity: 1; margin-top: 1rem; }
        .form-input { margin-top: 0.25rem; display: block; width: 100%; padding: 0.75rem 1rem; border: 1px solid #d1d5db; border-radius: 0.375rem; box-shadow: 0 1px 2px 0 rgba(0, 0, 0, 0.05); transition: border-color 0.2s ease, box-shadow 0.2s ease; }
        .form-input:focus { outline: none; border-color: #3b82f6; box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.3); }
        
        /* Bổ sung/Chỉnh sửa CSS */
        .order-summary-table th { background-color: #f9fafb; font-weight: 600; color: #4b5563;}
        .order-summary-table td { color: #374151; }
        
        /* ============== TỐI ƯU GIAO DIỆN BẢNG CHO MOBILE (BẮT ĐẦU) ============== */
        @media (max-width: 640px) { /* Áp dụng cho màn hình nhỏ hơn breakpoint 'sm' của Tailwind */
            .order-summary-table thead {
                display: none; /* Ẩn hoàn toàn header của bảng */
            }
            .order-summary-table, .order-summary-table tbody, .order-summary-table tr, .order-summary-table td {
                display: block; /* Chuyển tất cả thành block-level element */
                width: 100%;
            }
            .order-summary-table tr {
                padding: 1rem 0;
                border-bottom: 1px solid #e5e7eb; /* Đường kẻ phân cách giữa các sản phẩm */
            }
            .order-summary-table tbody tr:last-child {
                border-bottom: none; /* Bỏ đường kẻ ở sản phẩm cuối cùng */
            }
            .order-summary-table td {
                display: flex; /* Dùng flexbox để căn chỉnh nhãn và giá trị */
                justify-content: space-between; /* Đẩy nhãn và giá trị về 2 phía */
                align-items: center;
                padding: 0.5rem 0.25rem; /* Căn chỉnh lại padding */
                border: none; /* Bỏ border của từng ô */
                text-align: right; /* Căn phải cho giá trị */
            }
            .order-summary-table td::before {
                content: attr(data-label); /* Lấy nội dung từ thuộc tính data-label */
                font-weight: 600;
                color: #4b5563;
                text-align: left; /* Căn trái cho nhãn */
                margin-right: 1rem; /* Khoảng cách giữa nhãn và giá trị */
            }
            /* Định dạng đặc biệt cho ô tên sản phẩm */
            .order-summary-table td[data-label="Sản phẩm"] {
                flex-direction: column; /* Hiển thị tên sản phẩm trên 1 dòng riêng */
                align-items: flex-start;
                font-size: 1rem;
                font-weight: bold;
                margin-bottom: 0.5rem;
            }
            .order-summary-table td[data-label="Sản phẩm"]::before {
                display: none; /* Không cần nhãn "Sản phẩm:" */
            }
            
            /* Tùy chỉnh cho phần footer (Tổng cộng) trên mobile */
            .order-summary-table tfoot tr, .order-summary-table tfoot td {
                display: flex;
                justify-content: space-between;
                align-items: center;
                width: 100%;
                padding: 1rem 0.25rem;
            }
            .order-summary-table tfoot .total-label {
                 font-size: 1.125rem; /* text-lg */
                 font-weight: bold;
                 text-transform: uppercase;
                 color: #1f2937;
            }
             .order-summary-table tfoot .total-value {
                font-size: 1.25rem; /* text-xl */
                font-weight: bold;
                color: #dc2626; /* text-red-600 */
            }
        }
        /* ============== TỐI ƯU GIAO DIỆN BẢNG CHO MOBILE (KẾT THÚC) ============== */

    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div id="loadingOverlay">
        <div class="spinner"></div>
        <p>Đang xử lý đơn hàng...</p>
    </div>

    <div class="bg-gray-50 min-h-screen py-12">
        <div class="container mx-auto px-4 lg:px-8">
            <h1 class="text-3xl lg:text-4xl font-bold text-gray-800 mb-8 text-center">Hoàn Tất Thanh Toán</h1>
            <asp:Label ID="lblMessage" runat="server" CssClass="block mb-6 text-sm p-4 rounded-lg border" EnableViewState="false" Visible="false"></asp:Label>

            <div class="flex flex-col lg:flex-row gap-8 lg:gap-12 mt-6">

                <asp:Panel ID="pnlOrderSummary" runat="server" CssClass="w-full lg:w-3/5 bg-white p-6 md:p-8 rounded-xl shadow-lg border border-gray-200">
                    <h2 class="text-2xl font-semibold text-gray-800 mb-6 border-b border-gray-200 pb-4">Thông Tin Đơn Hàng</h2>
                    <div class="overflow-x-auto">
                        <asp:Repeater ID="rptSelectedItems" runat="server">
                            <HeaderTemplate>
                                <table class="min-w-full text-sm order-summary-table">
                                    <thead class="bg-gray-50">
                                        <tr>
                                            <th scope="col" class="px-4 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Sản Phẩm</th>
                                            <th scope="col" class="px-4 py-3 text-center text-xs font-medium text-gray-500 uppercase tracking-wider">Số Lượng</th>
                                            <th scope="col" class="px-4 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">Đơn giá</th>
                                            <th scope="col" class="px-4 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">Thành tiền</th>
                                        </tr>
                                    </thead>
                                    <tbody class="bg-white divide-y divide-gray-200 sm:divide-y-0">
                            </HeaderTemplate>
                            <ItemTemplate>
                                <tr>
                                    <%-- Thay đổi: Thêm data-label cho CSS mobile, bỏ truncate --%>
                                    <td class="px-4 py-3 font-bold text-gray-800" data-label="Sản phẩm">
                                        <%# Eval("TenSach") %>
                                    </td>
                                    <td class="px-4 py-3 text-gray-600 text-center" data-label="Số lượng">
                                        <%# Eval("SoLuong") %>
                                    </td>
                                    <td class="px-4 py-3 text-gray-600 text-right" data-label="Đơn giá">
                                        <%# FormatCurrency(Eval("DonGia")) %>
                                    </td>
                                    <td class="px-4 py-3 text-gray-700 text-right font-semibold" data-label="Thành tiền">
                                        <%# FormatCurrency(Eval("ThanhTien")) %>
                                    </td>
                                </tr>
                            </ItemTemplate>
                            <FooterTemplate>
                                    </tbody>
                                    <tfoot class="border-t-2 border-gray-300 bg-gray-50">
                                        <tr>
                                             <%-- Thay đổi: Đơn giản hóa footer --%>
                                            <td colspan="3" class="px-4 py-4 text-right text-base font-bold text-gray-800 uppercase total-label">Tổng Cộng:</td>
                                            <td class="px-4 py-4 text-right text-xl font-bold text-red-600 total-value"><%# FormatCurrency(this.GrandTotal) %></td>
                                        </tr>
                                    </tfoot>
                                </table>
                            </FooterTemplate>
                        </asp:Repeater>
                    </div>
                    <div class="mt-8">
                        <asp:HyperLink ID="hlBackToCart" runat="server" NavigateUrl="~/WebForm/User/giohang_user.aspx"
                            CssClass="inline-flex items-center px-5 py-2.5 border border-gray-300 rounded-md shadow-sm text-sm font-medium text-gray-700 bg-white hover:bg-gray-100 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 transition ease-in-out duration-150"
                            Visible='<%# !this.IsBuyNowMode %>'>
                            <i class="fas fa-arrow-left mr-2"></i> Quay lại giỏ hàng
                        </asp:HyperLink>
                    </div>
                </asp:Panel>

                <%-- Phần Panel Thanh Toán (pnlPaymentMethods) giữ nguyên không thay đổi --%>
                <asp:Panel ID="pnlPaymentMethods" runat="server" CssClass="w-full lg:w-2/5 bg-white p-6 md:p-8 rounded-xl shadow-lg border border-gray-200">
                    <h2 class="text-2xl font-semibold text-gray-800 mb-6 border-b border-gray-200 pb-4">Chọn Phương Thức Thanh Toán</h2>
                    <div id="paymentOptionsContainer" class="space-y-4">
                         <asp:RadioButtonList ID="rblPaymentMethod" runat="server" RepeatLayout="Flow" RepeatDirection="Vertical"
                               CssClass="" CssItemClass="payment-option-item"
                               AutoPostBack="false"> 
                                <asp:ListItem Value="Bank" Selected="True">
                                    <i class='fas fa-university'></i> Chuyển khoản ngân hàng
                                </asp:ListItem>
                                <asp:ListItem Value="Card">
                                    <i class='fas fa-credit-card'></i> Thẻ ngân hàng (ATM/Visa/Mastercard)
                                </asp:ListItem>
                                <asp:ListItem Value="Wallet">
                                    <i class='fas fa-wallet'></i> Ví điện tử (Momo, ViettelPay, VNPay)
                                </asp:ListItem>
                         </asp:RadioButtonList>
                    </div>

                    <asp:Panel ID="pnlBankInfo" runat="server" CssClass="payment-details-panel p-5 border rounded-lg bg-blue-50 border-blue-200 space-y-3 text-sm">
                        <h4 class="font-semibold text-gray-800 mb-2 text-base">Thông tin chuyển khoản:</h4>
                        <p><strong>Ngân hàng:</strong> <span class="font-semibold text-blue-800">[MB Bank]</span></p>
                        <p><strong>Số tài khoản:</strong> <span class="font-semibold text-blue-800">[0376512695]</span> <button type="button" class="ml-2 text-blue-600 hover:text-blue-800 text-xs" onclick="copyToClipboard('0376512695')"><i class="far fa-copy"></i> Copy</button></p>
                        <p><strong>Chủ tài khoản:</strong> <span class="font-semibold text-blue-800">[Lam Chu Bao Toan]</span></p>
                        <p><strong>Nội dung CK:</strong> <span class="font-semibold text-red-600">TTDH [Mã đơn hàng]</span> <span class="text-xs text-gray-500">(Sẽ được cấp sau khi đặt)</span></p>
                        <div class="mt-4 text-center">
                            <img src="/Images/QR/placeholder-qr.png" alt="QR Code Chuyển khoản" class="mx-auto w-36 h-36 md:w-40 md:h-40 border-2 border-gray-300 shadow-md rounded-lg" />
                            <p class="text-xs text-gray-600 mt-2">Quét mã QR để thanh toán nhanh chóng</p>
                        </div>
                    </asp:Panel>

                    <asp:Panel ID="pnlCardForm" runat="server" CssClass="payment-details-panel p-5 border rounded-lg bg-gray-50 border-gray-200 space-y-4 text-sm">
                        <h4 class="font-semibold text-gray-800 mb-2 text-base">Nhập thông tin thẻ:</h4>
                        <div>
                            <label for="<%=txtCardNumber.ClientID%>" class="block text-sm font-medium text-gray-700 mb-1">Số thẻ</label>
                            <asp:TextBox ID="txtCardNumber" runat="server" CssClass="form-input" placeholder="**** **** **** ****"></asp:TextBox>
                        </div>
                        <div>
                            <label for="<%=txtCardName.ClientID%>" class="block text-sm font-medium text-gray-700 mb-1">Tên chủ thẻ</label>
                            <asp:TextBox ID="txtCardName" runat="server" CssClass="form-input" placeholder="NGUYEN VAN A"></asp:TextBox>
                        </div>
                        <div class="flex flex-col sm:flex-row gap-4">
                            <div class="flex-1">
                                <label for="<%=txtCardExpiry.ClientID%>" class="block text-sm font-medium text-gray-700 mb-1">Ngày hết hạn</label>
                                <asp:TextBox ID="txtCardExpiry" runat="server" CssClass="form-input" placeholder="MM/YY"></asp:TextBox>
                            </div>
                            <div class="sm:w-1/3">
                                <label for="<%=txtCardCVV.ClientID%>" class="block text-sm font-medium text-gray-700 mb-1">CVV</label>
                                <asp:TextBox ID="txtCardCVV" runat="server" CssClass="form-input" placeholder="123" type="password" MaxLength="4"></asp:TextBox>
                            </div>
                        </div>
                         <p class="text-xs text-gray-500 mt-3"><i class="fas fa-lock mr-1 text-green-600"></i> Thông tin thẻ của bạn được mã hóa và bảo mật.</p>
                    </asp:Panel>

                    <asp:Panel ID="pnlWalletInfo" runat="server" CssClass="payment-details-panel p-5 border rounded-lg bg-yellow-50 border-yellow-200 space-y-4">
                        <h4 class="font-semibold text-gray-800 mb-3 text-base">Quét mã QR bằng ví điện tử:</h4>
                        <div class="flex flex-wrap justify-around items-center gap-4">
                            <div class="text-center flex-shrink-0 p-2">
                                <img src="/Images/Icons/momo-logo.png" alt="Momo" class="h-8 mx-auto mb-2" />
                                <img src="/Images/QR/placeholder-qr-momo.png" alt="QR Momo" class="w-32 h-32 border border-gray-300 shadow-sm rounded-md" />
                                <p class="text-xs text-gray-600 mt-1 font-medium">Momo</p>
                            </div>
                             <div class="text-center flex-shrink-0 p-2">
                                <img src="/Images/Icons/viettelmoney-logo.png" alt="Viettel Money" class="h-8 mx-auto mb-2" />
                                <img src="/Images/QR/placeholder-qr-viettelmoney.png" alt="QR Viettel Money" class="w-32 h-32 border border-gray-300 shadow-sm rounded-md" />
                                <p class="text-xs text-gray-600 mt-1 font-medium">Viettel Money</p>
                            </div>
                            <div class="text-center flex-shrink-0 p-2">
                                <img src="/Images/Icons/vnpay-logo.png" alt="VNPay" class="h-8 mx-auto mb-2" />
                                <img src="/Images/QR/placeholder-qr-vnpay.png" alt="QR VNPay" class="w-32 h-32 border border-gray-300 shadow-sm rounded-md" />
                                <p class="text-xs text-gray-600 mt-1 font-medium">VNPay</p>
                            </div>
                        </div>
                    </asp:Panel>

                    <div class="mt-10">
                        <asp:Button ID="btnXacNhan" runat="server" Text="Xác Nhận & Đặt Hàng"
                            CssClass="w-full bg-indigo-600 hover:bg-indigo-700 text-white font-bold py-3 px-6 rounded-lg shadow-md hover:shadow-lg focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 transition duration-200 ease-in-out disabled:opacity-50 disabled:cursor-not-allowed"
                            OnClick="btnXacNhan_Click" OnClientClick="showProcessing(); return true;" />
                           <p class="text-xs text-gray-500 mt-3 text-center"><i class="fas fa-info-circle mr-1"></i> Bằng việc nhấn nút, bạn đồng ý với <a href="gioithieu_user.aspx#terms" class="text-indigo-600 hover:underline">Điều khoản dịch vụ</a>.</p>
                    </div>
                </asp:Panel>
            </div>
        </div>
    </div>

<script type="text/javascript">
    // Toàn bộ phần script giữ nguyên không thay đổi
    function showProcessing() {
        const overlay = document.getElementById('loadingOverlay');
        if (overlay) overlay.classList.add('visible');
        const button = document.getElementById('<%= btnXacNhan.ClientID %>');
            if (button && !button.disabled) {
                setTimeout(() => {
                    const currentButton = document.getElementById('<%= btnXacNhan.ClientID %>');
                     if (currentButton && !currentButton.disabled) {
                          currentButton.disabled = true;
                     }
                }, 50);
        }
    }
    function handlePaymentMethodChange() {
        const paymentOptionsContainer = document.getElementById('paymentOptionsContainer');
        if (!paymentOptionsContainer) { return; }
        const bankPanelId = '<%= pnlBankInfo.ClientID %>';
        const cardPanelId = '<%= pnlCardForm.ClientID %>';
        const walletPanelId = '<%= pnlWalletInfo.ClientID %>';
        const radios = paymentOptionsContainer.querySelectorAll('input[type="radio"]');
        const detailPanels = document.querySelectorAll('.payment-details-panel');
        const listItems = paymentOptionsContainer.querySelectorAll('.payment-option-item');

        const updatePanelVisibility = (selectedValue) => {
            detailPanels.forEach(panel => {
                panel.classList.remove('visible');
                panel.style.maxHeight = '0';
                panel.style.opacity = '0';
                panel.style.marginTop = '0';
            });
            let panelIdToShow = null;
            if (selectedValue === 'Bank') panelIdToShow = bankPanelId;
            else if (selectedValue === 'Card') panelIdToShow = cardPanelId;
            else if (selectedValue === 'Wallet') panelIdToShow = walletPanelId;

            if (panelIdToShow) {
                const panelToShow = document.getElementById(panelIdToShow);
                if (panelToShow) {
                    setTimeout(() => {
                        panelToShow.style.maxHeight = '1000px';
                        panelToShow.style.opacity = '1';
                        panelToShow.style.marginTop = '1rem';
                        panelToShow.classList.add('visible');
                    }, 10);
                }
            }
            listItems.forEach(item => item.classList.remove('selected'));
            radios.forEach(r => {
                if (r.value === selectedValue && r.checked) {
                    let parentItem = r.closest('.payment-option-item');
                    if (parentItem) {
                        parentItem.classList.add('selected');
                    }
                }
            });
        };
        radios.forEach(radio => {
            radio.addEventListener('change', function () {
                if (this.checked) {
                    updatePanelVisibility(this.value);
                }
            });
            if (radio.checked) {
                updatePanelVisibility(radio.value);
                let initialPanelId = null;
                if (radio.value === 'Bank') initialPanelId = bankPanelId;
                else if (radio.value === 'Card') initialPanelId = cardPanelId;
                else if (radio.value === 'Wallet') initialPanelId = walletPanelId;

                if (initialPanelId) {
                    const initialPanel = document.getElementById(initialPanelId);
                    if (initialPanel) {
                        initialPanel.style.transition = 'none';
                        initialPanel.style.maxHeight = '1000px';
                        initialPanel.style.opacity = '1';
                        initialPanel.style.marginTop = '1rem';
                        initialPanel.classList.add('visible');
                        setTimeout(() => { initialPanel.style.transition = ''; }, 50);
                    }
                }
            }
        });
    }
    function copyToClipboard(text) {
        navigator.clipboard.writeText(text).then(() => {
            alert('Đã sao chép: ' + text);
        }).catch(err => {
            console.error('Không thể sao chép: ', err);
            alert('Lỗi khi sao chép!');
        });
    }
    document.addEventListener('DOMContentLoaded', function () {
        handlePaymentMethodChange();
        const overlay = document.getElementById('loadingOverlay');
        const messageLabel = document.getElementById('<%= lblMessage.ClientID %>');
        const button = document.getElementById('<%= btnXacNhan.ClientID %>');
        if (overlay && overlay.classList.contains('visible')) {
            overlay.classList.remove('visible');
        }
        if (button && button.disabled) {
            const isMessageVisible = messageLabel && messageLabel.offsetHeight > 0 && messageLabel.innerText.trim() !== '';
            if (isMessageVisible) {
                button.disabled = false;
            }
        }
    });
    window.addEventListener('load', function () {
        const overlay = document.getElementById('loadingOverlay');
        if (overlay && overlay.classList.contains('visible')) {
            overlay.classList.remove('visible');
        }
    });
</script>
</asp:Content>