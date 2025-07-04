<%@ Page Title="Thông Tin Cá Nhân" Language="C#" MasterPageFile="~/WebForm/User/User.Master" AutoEventWireup="true" CodeBehind="thongtincanhan.aspx.cs" Inherits="Webebook.WebForm.User.thongtincanhan" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <%-- Font Awesome for icons --%>
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.0.0/css/all.min.css" integrity="sha512-9usAa10IRO0HhonpyAIVpjrylPvoDwiPUiKdWk5t3PyolY1cOd4DSE0Ga+ri4AuTroPR5aQvXU9xC6qOPnzFeg==" crossorigin="anonymous" referrerpolicy="no-referrer" />
    <style>
        /* Custom styles for smoother transitions and specific overrides if needed */
        .modal-base {
            transition: visibility 0s linear 0.3s, opacity 0.3s ease;
        }
        .modal-base.visible {
            transition-delay: 0s;
        }
        .modal-container {
            transition: transform 0.3s ease-out, opacity 0.3s ease-out;
            border-radius: 0.75rem; 
            border: 1px solid #e5e7eb;
        }
        /* Password toggle section animation */
        #<%= pnlChangePasswordSection.ClientID %>.hidden-section { max-height: 0; opacity: 0; margin-top: 0 !important; padding-top: 0 !important; padding-bottom: 0 !important; border-width: 0 !important; overflow: hidden; }
        #<%= pnlChangePasswordSection.ClientID %> { transition: all 0.4s ease-in-out; max-height: 500px; opacity: 1; overflow: hidden; }
        #pwToggleIcon { transition: transform 0.3s ease; }
        #pwToggleIcon.rotate-180 { transform: rotate(180deg); }

        /* Make FileUpload visually hidden but accessible */
        .visually-hidden-input {
            position: absolute;
            width: 1px;
            height: 1px;
            margin: -1px;
            padding: 0;
            overflow: hidden;
            clip: rect(0, 0, 0, 0);
            border: 0;
        }

        /* Style the label to look like a button */
        .avatar-edit-label {
            position: absolute;
            bottom: 0;
            right: 0;
            background-color: rgba(0, 0, 0, 0.65);
            color: white;
            border-radius: 50%;
            width: 2.25rem;
            height: 2.25rem;
            display: flex;
            align-items: center;
            justify-content: center;
            cursor: pointer;
            transition: background-color 0.2s ease;
            border: 2px solid white;
            box-shadow: 0 1px 3px rgba(0,0,0,0.2);
        }
        .avatar-edit-label:hover {
            background-color: rgba(0, 0, 0, 0.85);
        }
            .avatar-edit-label i {
            font-size: 0.9rem;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <%-- ScriptManager is needed for RegisterStartupScript, ensure it's on the MasterPage or uncomment below --%>
    <%-- <asp:ScriptManager ID="ScriptManager1" runat="server"></asp:ScriptManager> --%>

    <div class="container mx-auto px-4 sm:px-6 lg:px-8 py-8 min-h-[calc(100vh-200px)]">
        <%-- Optional: Basic Footer Height Adjustment --%>
        <%-- Ví dụ bắt đầu từ Heading --%>
        <h1 class="text-3xl font-bold text-gray-800 mb-6 pb-3 border-b border-gray-200">
            <i class="fas fa-user-circle mr-2 text-indigo-600"></i>Thông Tin Cá Nhân
        </h1>

        <%-- General Message Area --%>
        <asp:Label ID="lblMessage" runat="server" EnableViewState="false" Visible="false"></asp:Label>

        <%-- Profile Summary Card --%>
        <div class="bg-white p-6 sm:p-8 rounded-xl shadow-lg border border-gray-100 max-w-3xl mx-auto mb-10">
            <div class="flex flex-col sm:flex-row items-center sm:items-start gap-6">
                <div class="flex-shrink-0 relative">
                    <asp:Image ID="imgProfileAvatar" runat="server" CssClass="w-28 h-28 md:w-32 md:h-32 rounded-full object-cover border-4 border-indigo-100 shadow-md" ImageUrl="~/Images/default-avatar.png" AlternateText="Ảnh đại diện" />
                </div>
                <div class="flex-grow text-center sm:text-left mt-4 sm:mt-0">
                    <h2 class="text-2xl font-semibold text-gray-800">
                        <asp:Label ID="lblProfileDisplayName" runat="server" Text="Tên Người Dùng"></asp:Label>
                    </h2>
                    <p class="text-sm text-gray-500 mb-4">
                        <asp:Label ID="lblProfileUserID" runat="server" Text="@username"></asp:Label>
                    </p>
                    <div class="flex flex-wrap gap-3 justify-center sm:justify-start">
                        <asp:LinkButton ID="btnShowEditPopup" runat="server"
                            CssClass="inline-flex items-center px-4 py-2 border border-gray-300 rounded-md shadow-sm text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-1 focus:ring-indigo-500 transition ease-in-out duration-150"
                            OnClientClick="showEditModal(); return false;" >
                            <i class='fas fa-pencil-alt mr-1'></i> Sửa Hồ Sơ
                        </asp:LinkButton>
                        <asp:LinkButton ID="btnShowContact" runat="server"
                            CssClass="inline-flex items-center px-4 py-2 border border-gray-300 rounded-md shadow-sm text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-1 focus:ring-indigo-500 transition ease-in-out duration-150"
                            OnClientClick="showContactModal(); return false;" CausesValidation="false">
                            <i class='fas fa-address-book mr-1'></i> Xem Liên Hệ
                        </asp:LinkButton>
                    </div>
                </div>
            </div>
        </div>

        <%-- Dashboard Stats Panel --%>
        <asp:Panel ID="pnlDashboardStats" runat="server" Visible="true" CssClass="max-w-3xl mx-auto mb-10">
            <h3 class="text-xl font-semibold text-gray-700 mb-5">Tổng Quan Hoạt Động</h3>
            <div class="grid grid-cols-1 sm:grid-cols-2 gap-6">
                <%-- Stat Box 1: Orders --%>
                <div class="bg-gradient-to-br from-blue-50 to-indigo-100 p-6 rounded-lg shadow-md border border-blue-100 flex items-center gap-4 hover:shadow-lg transition-shadow duration-200">
                    <div class="text-blue-500 bg-white p-3 rounded-full shadow">
                        <i class="fas fa-shopping-bag fa-lg w-6 h-6"></i>
                    </div>
                    <div>
                        <div class="text-3xl font-bold text-blue-700">
                            <asp:Literal ID="litTotalOrders" runat="server">0</asp:Literal>
                        </div>
                        <div class="text-sm font-medium text-blue-800 mt-1">Đơn Hàng Đã Đặt</div>
                    </div>
                </div>
                <%-- Stat Box 2: Reviews --%>
                <div class="bg-gradient-to-br from-green-50 to-teal-100 p-6 rounded-lg shadow-md border border-green-100 flex items-center gap-4 hover:shadow-lg transition-shadow duration-200">
                    <div class="text-green-500 bg-white p-3 rounded-full shadow">
                        <i class="fas fa-star fa-lg w-6 h-6"></i>
                    </div>
                    <div>
                        <div class="text-3xl font-bold text-green-700">
                            <asp:Literal ID="litTotalReviews" runat="server">0</asp:Literal>
                        </div>
                        <div class="text-sm font-medium text-green-800 mt-1">Đánh Giá Đã Gửi</div>
                    </div>
                </div>
            </div>
        </asp:Panel>

        <%-- === EDIT PROFILE MODAL === --%>
        <div id="editProfileModal" class="modal-base fixed inset-0 z-50 flex items-center justify-center p-4 hidden opacity-0" onclick="closeModalOnClickOutside(event, 'editProfileModal')">
            <%-- Overlay --%>
            <div class="modal-overlay fixed inset-0 bg-black bg-opacity-60 transition-opacity duration-300 ease-out" aria-hidden="true"></div>

            <%-- Modal Container --%>
            <div class="modal-container relative bg-white rounded-lg shadow-xl w-full sm:max-w-lg transition-all duration-300 ease-out transform scale-95 opacity-0" role="dialog" aria-modal="true" aria-labelledby="edit-modal-headline">
                <button type="button" class="absolute top-3 right-3 text-gray-400 hover:text-gray-600 transition-colors" onclick="hideEditModal()">
                    <span class="sr-only">Đóng</span><i class="fas fa-times fa-lg"></i>
                </button>

                <div class="pt-6 px-6 space-y-5"> <h3 class="text-xl leading-6 font-semibold text-gray-900" id="edit-modal-headline">
                        <i class="fas fa-user-edit mr-2 text-indigo-600"></i>Chỉnh sửa hồ sơ
                    </h3>

                    <%-- Message label inside modal --%>
                    <asp:Label ID="lblEditMessage" runat="server" EnableViewState="false" Visible="false"></asp:Label>

                    <%-- Avatar Edit Section --%>
                    <div class="text-center">
                        <div class="relative inline-block avatar-edit-container">
                            <asp:Image ID="imgEditAvatar" runat="server" CssClass="w-24 h-24 rounded-full object-cover border-2 border-gray-300 shadow-md mx-auto" ImageUrl="~/Images/default-avatar.png" AlternateText="Ảnh đại diện hiện tại"/>
                            <label for="<%=fuEditAvatar.ClientID %>" class="avatar-edit-label">
                                <i class="fas fa-camera"></i>
                            </label>
                            <asp:FileUpload ID="fuEditAvatar" runat="server" CssClass="visually-hidden-input" accept="image/png, image/jpeg, image/gif" onchange="previewAvatar(this)" />
                        </div>
                        <p class="text-xs text-gray-500 mt-2">Nhấn biểu tượng máy ảnh để đổi ảnh (Tối đa 5MB)</p>
                    </div>

                    <%-- Webebook ID (Readonly) --%>
                    <div class="text-sm bg-gray-100 p-3 rounded-md border border-gray-200 text-center">
                        <span class="font-medium text-gray-600">Webebook ID:</span>
                        <asp:Literal ID="litEditUserID" runat="server" />
                    </div>

                    <%-- Form Fields --%>
                    <div>
                        <label for="<%=txtEditTen.ClientID %>" class="block text-sm font-medium text-gray-700 mb-1">Tên hiển thị</label>
                        <asp:TextBox ID="txtEditTen" runat="server" CssClass="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-offset-1 focus:ring-indigo-500 focus:border-indigo-500 transition" placeholder="Nhập tên bạn muốn hiển thị"></asp:TextBox>
                        <p class="mt-1 text-xs text-gray-500">Tên này sẽ hiển thị công khai trên trang web.</p>
                    </div>

                    <h4 class="text-md font-semibold text-gray-700 pt-3 border-t border-gray-200">Thông tin liên hệ (Riêng tư)</h4>
                    <div>
                        <label for="<%=txtEditEmail.ClientID %>" class="block text-sm font-medium text-gray-700 mb-1">Email <span class="text-red-500">*</span></label>
                        <asp:TextBox ID="txtEditEmail" runat="server" type="email" CssClass="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-offset-1 focus:ring-indigo-500 focus:border-indigo-500 transition" placeholder="vidu@email.com"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="rfvEmail" runat="server" ControlToValidate="txtEditEmail" ErrorMessage="Email không được để trống." CssClass="text-red-600 text-xs mt-1 block" Display="Dynamic" ValidationGroup="EditProfileGroup" />
                        <asp:RegularExpressionValidator ID="revEmail" runat="server" ControlToValidate="txtEditEmail" ErrorMessage="Địa chỉ email không hợp lệ." CssClass="text-red-600 text-xs mt-1 block" Display="Dynamic" ValidationGroup="EditProfileGroup" ValidationExpression="\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*" />
                    </div>
                    <div>
                        <label for="<%=txtEditDienThoai.ClientID %>" class="block text-sm font-medium text-gray-700 mb-1">Điện thoại</label>
                        <asp:TextBox ID="txtEditDienThoai" runat="server" type="tel" CssClass="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-offset-1 focus:ring-indigo-500 focus:border-indigo-500 transition" placeholder="Số điện thoại (không bắt buộc)"></asp:TextBox>
                    </div>

                    <%-- Change Password Section (Collapsible) --%>
                    <div class="pt-4 border-t border-gray-200 mt-5">
                        <button type="button" onclick="togglePasswordSection(this); return false;" class="text-sm font-medium text-indigo-600 hover:text-indigo-800 focus:outline-none flex items-center w-full justify-between group">
                            <span>Đổi mật khẩu</span>
                            <i class="fas fa-chevron-down fa-xs ml-2 transition-transform duration-300" id="pwToggleIcon"></i>
                        </button>
                        <asp:Panel ID="pnlChangePasswordSection" runat="server" CssClass="hidden-section mt-4 space-y-4 border bg-gray-50 p-4 rounded-md">
                            <div>
                                <label for="<%= txtMatKhauCu.ClientID %>" class="block text-sm font-medium text-gray-700 mb-1">Mật khẩu hiện tại <span class="text-red-500">*</span></label>
                                <asp:TextBox ID="txtMatKhauCu" runat="server" TextMode="Password" CssClass="mt-1 px-3 py-2 border border-gray-300 rounded-md w-full shadow-sm focus:outline-none focus:ring-1 focus:ring-indigo-500 focus:border-indigo-500 transition"></asp:TextBox>
                                <asp:RequiredFieldValidator ID="rfvOldPass" runat="server" ControlToValidate="txtMatKhauCu" ErrorMessage="Vui lòng nhập mật khẩu hiện tại." CssClass="text-red-600 text-xs mt-1 block" Display="Dynamic" ValidationGroup="ChangePassGroupInModal" />
                            </div>
                            <div>
                                <label for="<%= txtMatKhauMoi.ClientID %>" class="block text-sm font-medium text-gray-700 mb-1">Mật khẩu mới <span class="text-red-500">*</span></label>
                                <asp:TextBox ID="txtMatKhauMoi" runat="server" TextMode="Password" CssClass="mt-1 px-3 py-2 border border-gray-300 rounded-md w-full shadow-sm focus:outline-none focus:ring-1 focus:ring-indigo-500 focus:border-indigo-500 transition"></asp:TextBox>
                                <asp:RequiredFieldValidator ID="rfvNewPass" runat="server" ControlToValidate="txtMatKhauMoi" ErrorMessage="Mật khẩu mới không được để trống." CssClass="text-red-600 text-xs mt-1 block" Display="Dynamic" ValidationGroup="ChangePassGroupInModal" />
                                <p class="text-xs text-gray-500 mt-1">Ít nhất 6 ký tự.</p>
                            </div>
                            <div>
                                <label for="<%= txtXacNhanMatKhau.ClientID %>" class="block text-sm font-medium text-gray-700 mb-1">Xác nhận mật khẩu mới <span class="text-red-500">*</span></label>
                                <asp:TextBox ID="txtXacNhanMatKhau" runat="server" TextMode="Password" CssClass="mt-1 px-3 py-2 border border-gray-300 rounded-md w-full shadow-sm focus:outline-none focus:ring-1 focus:ring-indigo-500 focus:border-indigo-500 transition"></asp:TextBox>
                                <asp:CompareValidator ID="cvConfirmPass" runat="server" ControlToValidate="txtXacNhanMatKhau" ControlToCompare="txtMatKhauMoi" Operator="Equal" Type="String" ErrorMessage="Xác nhận mật khẩu không khớp." CssClass="text-red-600 text-xs mt-1 block" Display="Dynamic" ValidationGroup="ChangePassGroupInModal" />
                            </div>
                            <div class="text-right mt-4">
                                <asp:LinkButton ID="btnLuuMatKhau" runat="server" CssClass="inline-flex items-center justify-center py-2 px-4 border border-transparent shadow-sm text-sm font-medium rounded-md text-white bg-green-600 hover:bg-green-700 focus:outline-none focus:ring-2 focus:ring-offset-1 focus:ring-green-500 transition duration-150" OnClick="btnLuuMatKhau_Click" ValidationGroup="ChangePassGroupInModal">
                                    <i class='fas fa-key mr-1'></i> Lưu Mật Khẩu
                                </asp:LinkButton>
                            </div>
                        </asp:Panel>
                    </div>
                </div>

                <%-- Modal Footer Buttons --%>
                <div class="bg-gray-50 px-4 py-3 sm:px-6 sm:flex sm:flex-row-reverse rounded-b-lg mt-6">
                    <asp:LinkButton ID="btnSaveChanges" runat="server" OnClick="btnSaveChanges_Click" CssClass="w-full inline-flex items-center justify-center rounded-md border border-transparent shadow-sm px-4 py-2 bg-indigo-600 text-base font-medium text-white hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 sm:ml-3 sm:w-auto sm:text-sm transition duration-150" ValidationGroup="EditProfileGroup">
                        <i class='fas fa-save mr-1'></i> Lưu Thay Đổi Hồ Sơ
                    </asp:LinkButton>
                    <asp:Button ID="btnCancelModal" runat="server" Text="Hủy Bỏ" CausesValidation="false" OnClientClick="hideEditModal(); return false;" CssClass="mt-3 w-full inline-flex justify-center rounded-md border border-gray-300 shadow-sm px-4 py-2 bg-white text-base font-medium text-gray-700 hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-gray-400 sm:mt-0 sm:ml-3 sm:w-auto sm:text-sm transition duration-150" />
                </div>
            </div> <%-- End Modal Container --%>
        </div> <%-- End Edit Modal Base --%>


        <%-- === CONTACT INFO MODAL === --%>
        <div id="contactInfoModal" class="modal-base fixed inset-0 z-50 flex items-center justify-center p-4 hidden opacity-0" onclick="closeModalOnClickOutside(event, 'contactInfoModal')">
            <div class="modal-overlay fixed inset-0 bg-black bg-opacity-60 transition-opacity duration-300 ease-out" aria-hidden="true"></div>
            <div class="modal-container relative bg-white rounded-lg shadow-xl w-full sm:max-w-md transition-all duration-300 ease-out transform scale-95 opacity-0" role="dialog" aria-modal="true" aria-labelledby="contact-modal-headline">
                <button type="button" class="absolute top-3 right-3 text-gray-400 hover:text-gray-600 transition-colors" onclick="hideContactModal()">
                    <span class="sr-only">Đóng</span><i class="fas fa-times fa-lg"></i>
                </button>
                <div class="p-6 space-y-4">
                    <h3 class="text-xl leading-6 font-semibold text-gray-900" id="contact-modal-headline">
                        <i class="fas fa-address-card mr-2 text-teal-600"></i>Thông Tin Liên Hệ
                    </h3>
                    <div class="text-sm text-gray-700 space-y-3 mt-4 border-t pt-4">
                        <p class="flex items-start">
                            <i class="fas fa-phone fa-fw mt-1 mr-3 text-gray-500 w-4 text-center flex-shrink-0"></i>
                            <span>Điện thoại: <span id="lblModalPhone" class="ml-2 font-medium text-gray-800"></span></span>
                        </p>
                        <p class="flex items-start">
                            <i class="fas fa-envelope fa-fw mt-1 mr-3 text-gray-500 w-4 text-center flex-shrink-0"></i>
                            <span>Email: <span id="lblModalEmail" class="ml-2 font-medium text-gray-800 break-all"></span></span>
                        </p>
                    </div>
                </div>
                <div class="bg-gray-50 px-4 py-3 sm:px-6 text-right rounded-b-lg mt-6">
                    <button type="button" onclick="hideContactModal()" class="inline-flex justify-center rounded-md border border-gray-300 shadow-sm px-4 py-2 bg-white text-sm font-medium text-gray-700 hover:bg-gray-100 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 transition duration-150">
                        Đóng
                    </button>
                </div>
            </div>
        </div>

        <%-- Hidden fields to store current data for JS --%>
        <asp:HiddenField ID="hfUsername" runat="server" />
        <asp:HiddenField ID="hfCurrentTen" runat="server" />
        <asp:HiddenField ID="hfCurrentDienThoai" runat="server" />
        <asp:HiddenField ID="hfCurrentEmail" runat="server" />
        <asp:HiddenField ID="hfCurrentAvatarUrl" runat="server" />

    </div> <%-- End Container --%>

    <%-- JavaScript cho Modals & Interactions --%>
    <script type="text/javascript">
        // Cache DOM elements
        const editModal = document.getElementById('editProfileModal');
        const contactModal = document.getElementById('contactInfoModal');
        const editModalContainer = editModal ? editModal.querySelector('.modal-container') : null;
        const contactModalContainer = contactModal ? contactModal.querySelector('.modal-container') : null;
        const editModalOverlay = editModal ? editModal.querySelector('.modal-overlay') : null;
        const contactModalOverlay = contactModal ? contactModal.querySelector('.modal-overlay') : null;

        const pwSection = document.getElementById('<%= pnlChangePasswordSection.ClientID %>');
        const pwToggleIcon = document.getElementById('pwToggleIcon');
        const editMsgLabel = document.getElementById('<%= lblEditMessage.ClientID %>');

        const txtEditTen = document.getElementById('<%= txtEditTen.ClientID %>');
        const txtEditDienThoai = document.getElementById('<%= txtEditDienThoai.ClientID %>');
        const txtEditEmail = document.getElementById('<%= txtEditEmail.ClientID %>');
        const imgEditAvatar = document.getElementById('<%= imgEditAvatar.ClientID %>');
        const fuEditAvatar = document.getElementById('<%= fuEditAvatar.ClientID %>'); // Cache FileUpload input
        const txtMatKhauCu = document.getElementById('<%= txtMatKhauCu.ClientID %>');
        const txtMatKhauMoi = document.getElementById('<%= txtMatKhauMoi.ClientID %>');
        const txtXacNhanMatKhau = document.getElementById('<%= txtXacNhanMatKhau.ClientID %>');

        const hfCurrentTen = document.getElementById('<%= hfCurrentTen.ClientID %>');
        const hfCurrentDienThoai = document.getElementById('<%= hfCurrentDienThoai.ClientID %>');
        const hfCurrentEmail = document.getElementById('<%= hfCurrentEmail.ClientID %>');
        const hfCurrentAvatarUrl = document.getElementById('<%= hfCurrentAvatarUrl.ClientID %>');
        const defaultAvatarUrl = '<%= ResolveUrl("~/Images/default-avatar.png") %>'; // Cache default URL

        // --- Generic Modal Functions ---
        function showModal(modalElement, modalContainer, modalOverlay) {
            if (!modalElement || !modalContainer || !modalOverlay) return;

            modalElement.classList.remove('hidden');
            document.body.classList.add('overflow-hidden'); // Prevent body scrolling

            requestAnimationFrame(() => {
                modalOverlay.classList.remove('opacity-0');
                modalContainer.classList.remove('opacity-0', 'scale-95');
                modalElement.classList.add('visible', 'opacity-100');
            });
        }

        function hideModal(modalElement, modalContainer, modalOverlay) {
            if (!modalElement || !modalContainer || !modalOverlay) return;

            modalOverlay.classList.add('opacity-0');
            modalContainer.classList.add('opacity-0', 'scale-95');
            modalElement.classList.remove('visible', 'opacity-100');

            setTimeout(() => {
                if (!modalElement.classList.contains('visible')) {
                    modalElement.classList.add('hidden');
                    if (!document.querySelector('.modal-base.visible')) {
                        document.body.classList.remove('overflow-hidden');
                    }
                }
            }, 300); // Match transition duration
        }

        function closeModalOnClickOutside(event, modalId) {
            const specificModal = document.getElementById(modalId);
            if (event.target === specificModal && specificModal && specificModal.classList.contains('visible')) {
                if (modalId === 'editProfileModal') hideEditModal();
                else if (modalId === 'contactInfoModal') hideContactModal();
            }
        }

        // --- Edit Profile Modal Specific ---
        function showEditModal() {
            if (editModal) {
                // Reset password section
                if (pwSection && !pwSection.classList.contains('hidden-section')) {
                    pwSection.classList.add('hidden-section');
                    if (pwToggleIcon) pwToggleIcon.classList.remove('rotate-180');
                    if (txtMatKhauCu) txtMatKhauCu.value = '';
                    if (txtMatKhauMoi) txtMatKhauMoi.value = '';
                    if (txtXacNhanMatKhau) txtXacNhanMatKhau.value = '';
                }
                // Clear previous file selection from FileUpload
                if (fuEditAvatar) fuEditAvatar.value = '';

                // Populate form from hidden fields
                try {
                    if (txtEditTen && hfCurrentTen) txtEditTen.value = hfCurrentTen.value;
                    if (txtEditDienThoai && hfCurrentDienThoai) txtEditDienThoai.value = hfCurrentDienThoai.value;
                    if (txtEditEmail && hfCurrentEmail) txtEditEmail.value = hfCurrentEmail.value;
                    if (imgEditAvatar && hfCurrentAvatarUrl) imgEditAvatar.src = hfCurrentAvatarUrl.value || defaultAvatarUrl; // Use cached default URL
                } catch (e) {
                    console.error("Lỗi khi điền dữ liệu vào modal sửa hồ sơ:", e);
                    if (imgEditAvatar) imgEditAvatar.src = defaultAvatarUrl; // Fallback on error
                }

                if (editMsgLabel) editMsgLabel.style.display = 'none'; // Hide previous messages
                showModal(editModal, editModalContainer, editModalOverlay);
            }
        }
        function showEditModalOnError() { if (editModal) showModal(editModal, editModalContainer, editModalOverlay); }
        function hideEditModal() { if (editModal) hideModal(editModal, editModalContainer, editModalOverlay); }

        // --- Toggle Password Section ---
        function togglePasswordSection(button) {
            if (pwSection) {
                const isHidden = pwSection.classList.toggle('hidden-section');
                if (pwToggleIcon) { pwToggleIcon.classList.toggle('rotate-180', !isHidden); }
                if (editMsgLabel) editMsgLabel.style.display = 'none';
            }
            return false;
        }

        // --- Contact Info Modal Specific ---
        function showContactModal() {
            if (contactModal && hfCurrentDienThoai && hfCurrentEmail) {
                const phone = hfCurrentDienThoai.value.trim() || 'Chưa cập nhật';
                const email = hfCurrentEmail.value.trim() || 'Chưa cập nhật';
                const phoneSpan = contactModal.querySelector('#lblModalPhone');
                const emailSpan = contactModal.querySelector('#lblModalEmail');
                if (phoneSpan) phoneSpan.textContent = phone;
                if (emailSpan) emailSpan.textContent = email;
                showModal(contactModal, contactModalContainer, contactModalOverlay);
            }
        }
        function hideContactModal() { if (contactModal) hideModal(contactModal, contactModalContainer, contactModalOverlay); }

        // --- Avatar Preview ---
        function previewAvatar(fileInput) {
            const imgPreview = imgEditAvatar;
            const currentAvatarUrl = hfCurrentAvatarUrl ? hfCurrentAvatarUrl.value : defaultAvatarUrl;

            if (fileInput.files && fileInput.files[0] && imgPreview) {
                const file = fileInput.files[0];
                const reader = new FileReader();
                const maxSize = 5 * 1024 * 1024; // 5MB
                const allowedTypes = ['image/png', 'image/jpeg', 'image/gif'];

                if (!allowedTypes.includes(file.type)) {
                    alert('Lỗi: Chỉ chấp nhận ảnh định dạng PNG, JPG, GIF.');
                    fileInput.value = '';
                    imgPreview.src = currentAvatarUrl || defaultAvatarUrl;
                    return;
                }
                if (file.size > maxSize) {
                    alert('Lỗi: Kích thước ảnh phải nhỏ hơn hoặc bằng 5MB.');
                    fileInput.value = '';
                    imgPreview.src = currentAvatarUrl || defaultAvatarUrl;
                    return;
                }
                reader.onload = function (e) { imgPreview.src = e.target.result; }
                reader.readAsDataURL(file);
            } else {
                imgPreview.src = currentAvatarUrl || defaultAvatarUrl;
            }
        }

        // Optional: Keyboard support (Escape key to close modals)
        document.addEventListener('keydown', function (event) {
            if (event.key === 'Escape') {
                if (editModal && editModal.classList.contains('visible')) { hideEditModal(); }
                else if (contactModal && contactModal.classList.contains('visible')) { hideContactModal(); }
            }
        });
    </script>
</asp:Content>