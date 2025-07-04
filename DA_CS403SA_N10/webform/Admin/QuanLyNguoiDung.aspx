<%@ Page Title="Quản Lý Người Dùng" Language="C#" MasterPageFile="~/WebForm/Admin/Admin.Master" AutoEventWireup="true" CodeBehind="QuanLyNguoiDung.aspx.cs" Inherits="Webebook.WebForm.Admin.QuanLyNguoiDung" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
<script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>

    <%-- Basic CSS for Modal Structure (can be refined further with pure Tailwind) --%>
    <style>
        /* Basic Modal Structure & Overlay */
        .modal {
            position: fixed; z-index: 1050; left: 0; top: 0; width: 100%; height: 100%;
            overflow: auto; background-color: rgba(0,0,0,0.5);
            opacity: 0; visibility: hidden;
            transition: opacity 0.3s ease, visibility 0s linear 0.3s; /* Fade out */
        }
        .modal.open {
            opacity: 1; visibility: visible;
            transition: opacity 0.3s ease; /* Fade in */
        }
        /* Modal Content Styling & Animation */
        .modal-content-wrapper { /* Added wrapper for centering */
            display: flex; align-items: center; justify-content: center;
            min-height: 100%; padding: 1rem; /* Add padding for spacing */
        }
        .modal-content {
             /* Tailwind classes applied directly below handle most styling */
            margin: auto; /* Center horizontally */
            position: relative;
            transform: translateY(-20px); opacity: 0;
            transition: transform 0.3s ease, opacity 0.3s ease, visibility 0s linear 0.3s;
        }
        .modal.open .modal-content {
            transform: translateY(0); opacity: 1;
        }
        /* Close Button */
        .close-button {
            position: absolute; right: 1rem; top: 1rem; /* Adjusted positioning */
            color: #9ca3af; /* gray-400 */
            font-size: 1.5rem; line-height: 1; font-weight: bold;
        }
        .close-button:hover, .close-button:focus {
            color: #4b5563; /* gray-600 */ text-decoration: none; cursor: pointer;
        }
        /* Modal Message Styles (Tailwind classes preferred now) */
        .modal-message { display: block; margin-bottom: 1rem; padding: .75rem 1.25rem; border: 1px solid transparent; border-radius: .25rem; }
        .modal-message-error { color: #721c24; background-color: #f8d7da; border-color: #f5c6cb; }
        .modal-message-warning { color: #856404; background-color: #fff3cd; border-color: #ffeeba; }
        .modal-message-success { color: #155724; background-color: #d4edda; border-color: #c3e6cb; }

        /* GridView Pager Styles */
        .grid-pager a, .grid-pager span {
            display: inline-block; padding: 0.5rem 0.75rem; margin-left: 0.25rem;
            border: 1px solid #e5e7eb; border-radius: 0.375rem;
            font-size: 0.875rem; line-height: 1.25rem;
            transition: background-color 0.15s ease-in-out;
        }
        .grid-pager a { color: #4f46e5; background-color: white; }
        .grid-pager a:hover { background-color: #f9fafb; }
        .grid-pager span {
            background-color: #4f46e5; color: white; border-color: #4f46e5;
            font-weight: 600;
        }

        /* Custom Style for Deleted Status Badge */
       .status-deleted {
            padding: 0.125rem 0.5rem; display: inline-flex; font-size: 0.75rem;
            line-height: 1.25rem; font-weight: 600; border-radius: 9999px;
            color: #6b7280; background-color: #e5e7eb; /* gray-500 text, gray-200 bg */
            border: 1px dashed #9ca3af; /* gray-400 border */
            opacity: 0.8; /* Slightly faded */
       }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <%-- Main container with background color and padding --%>
    <div class="container mx-auto px-4 py-6 bg-gray-50 min-h-screen">

        <%-- Page Header and Add Button --%>
            <div class="flex flex-wrap justify-between items-center mb-6 gap-4">
            <h2 class="text-2xl font-semibold text-gray-800">Danh Sách Người Dùng</h2>
            <%-- THAY ĐỔI TỪ asp:Button SANG asp:LinkButton --%>
            <asp:LinkButton ID="btnThemNguoiDungMoi" runat="server"
                CssClass="inline-flex items-center bg-indigo-600 hover:bg-indigo-700 text-white font-semibold py-2 px-4 rounded-md shadow hover:shadow-lg transition duration-150 ease-in-out focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500"
                OnClick="btnThemNguoiDungMoi_Click">
                <i class="fas fa-plus mr-2"></i> <%-- Icon bây giờ nằm trong LinkButton hợp lệ --%>
                <span>Thêm Người Dùng Mới</span> <%-- Thêm span cho text nếu cần tách biệt rõ ràng --%>
            </asp:LinkButton>
            <%-- KẾT THÚC THAY ĐỔI --%>
        </div>

        <%-- Filter Section --%>
        <div class="bg-white p-4 rounded-lg shadow border border-gray-200 mb-6 flex flex-wrap items-end gap-4">
            <div>
                <label for="<%= ddlFilterRole.ClientID %>" class="block text-sm font-medium text-gray-700 mb-1">Lọc theo Vai trò:</label>
                <asp:DropDownList ID="ddlFilterRole" runat="server" CssClass="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm min-w-[150px]" AutoPostBack="false">
                    <asp:ListItem Text="-- Tất Cả --" Value=""></asp:ListItem>
                    <asp:ListItem Text="Admin" Value="0"></asp:ListItem>
                    <asp:ListItem Text="User" Value="1"></asp:ListItem>
                </asp:DropDownList>
            </div>
            <div>
                <label for="<%= ddlFilterStatus.ClientID %>" class="block text-sm font-medium text-gray-700 mb-1">Lọc theo Trạng thái:</label>
                <asp:DropDownList ID="ddlFilterStatus" runat="server" CssClass="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm min-w-[180px]" AutoPostBack="false">
                    <asp:ListItem Text="-- Hoạt động/Khóa --" Value="ActiveOrLocked" Selected="True"></asp:ListItem>
                    <asp:ListItem Text="Tất Cả Trạng Thái" Value="All"></asp:ListItem>
                    <asp:ListItem Text="Hoạt động (Active)" Value="Active"></asp:ListItem>
                    <asp:ListItem Text="Bị khóa (Locked)" Value="Locked"></asp:ListItem>
                    <asp:ListItem Text="Đã xóa (Deleted)" Value="Deleted"></asp:ListItem>
                </asp:DropDownList>
            </div>
                <div>
                <%-- THAY ĐỔI TỪ asp:Button SANG asp:LinkButton --%>
                <asp:LinkButton ID="btnFilter" runat="server"
                    CssClass="inline-flex items-center bg-indigo-600 hover:bg-indigo-700 text-white font-bold py-2 px-4 rounded-md shadow hover:shadow-lg transition duration-150 ease-in-out focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500"
                    OnClick="btnFilter_Click">
                     <i class="fas fa-filter mr-1"></i>
                     <span>Lọc</span> <%-- Chuyển Text vào đây --%>
                 </asp:LinkButton>
                 <%-- KẾT THÚC THAY ĐỔI --%>
            </div>
        </div>
        <%-- End Filter Section --%>

        <%-- Message Area --%>
        <asp:Panel ID="pnlMessageArea" runat="server" Visible="false" class="mb-4">
             <div id="successAlert" runat="server" visible="false" class="bg-green-100 border-l-4 border-green-500 text-green-700 p-4 rounded-md" role="alert">
                 <p class="font-bold">Thành công!</p>
                 <p><asp:Literal ID="litSuccessMessage" runat="server" /></p>
             </div>
              <div id="errorAlert" runat="server" visible="false" class="bg-red-100 border-l-4 border-red-500 text-red-700 p-4 rounded-md" role="alert">
                 <p class="font-bold">Lỗi!</p>
                 <p><asp:Literal ID="litErrorMessage" runat="server" /></p>
             </div>
              <div id="warningAlert" runat="server" visible="false" class="bg-yellow-100 border-l-4 border-yellow-500 text-yellow-700 p-4 rounded-md" role="alert">
                 <p class="font-bold">Cảnh báo!</p>
                 <p><asp:Literal ID="litWarningMessage" runat="server" /></p>
             </div>
             <div id="infoAlert" runat="server" visible="false" class="bg-blue-100 border-l-4 border-blue-500 text-blue-700 p-4 rounded-md" role="alert">
                 <p class="font-bold">Thông tin</p>
                 <p><asp:Literal ID="litInfoMessage" runat="server" /></p>
             </div>
        </asp:Panel>
        <%-- End Message Area --%>

        <asp:HiddenField ID="hfUserID" runat="server" />

        <%-- GridView Container --%>
        <div class="bg-white shadow-lg rounded-lg overflow-hidden border border-gray-200">
            <asp:GridView ID="GridViewNguoiDung" runat="server" AutoGenerateColumns="False" DataKeyNames="IDNguoiDung"
                CssClass="min-w-full divide-y divide-gray-200"
                OnRowCommand="GridViewNguoiDung_RowCommand" AllowPaging="True" PageSize="15" OnPageIndexChanging="GridViewNguoiDung_PageIndexChanging" EmptyDataText="Không có người dùng nào khớp với bộ lọc."
                OnRowDataBound="GridViewNguoiDung_RowDataBound">
                <HeaderStyle CssClass="bg-gray-100 px-4 py-3 text-left text-xs font-semibold text-gray-600 uppercase tracking-wider" />
                <RowStyle CssClass="bg-white hover:bg-indigo-50 transition duration-150 ease-in-out" />
                <AlternatingRowStyle CssClass="bg-gray-50 hover:bg-indigo-50 transition duration-150 ease-in-out" />
                <PagerStyle CssClass="bg-gray-100 px-4 py-3 border-t border-gray-200 text-right grid-pager" HorizontalAlign="Right" />
                <Columns>
                    <asp:BoundField DataField="IDNguoiDung" HeaderText="ID" ReadOnly="True" ItemStyle-CssClass="px-4 py-3 whitespace-nowrap text-sm text-gray-500 w-16" />
                    <asp:BoundField DataField="Username" HeaderText="Username" ItemStyle-CssClass="px-4 py-3 whitespace-nowrap text-sm text-gray-900 font-medium" />
                    <asp:BoundField DataField="Email" HeaderText="Email" ItemStyle-CssClass="px-4 py-3 whitespace-nowrap text-sm text-gray-500" />
                    <asp:BoundField DataField="Ten" HeaderText="Tên" ItemStyle-CssClass="px-4 py-3 whitespace-nowrap text-sm text-gray-500" />
                    <asp:TemplateField HeaderText="Vai Trò" ItemStyle-CssClass="px-4 py-3 whitespace-nowrap text-sm text-gray-500">
                        <ItemTemplate>
                            <%# Convert.ToInt32(Eval("VaiTro")) == 0 ? "Admin" : "User" %>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Trạng Thái" ItemStyle-CssClass="px-4 py-3 whitespace-nowrap text-sm">
                        <ItemTemplate>
                             <asp:Label ID="lblTrangThai" runat="server" Text='<%# Eval("TrangThai") %>'
                                CssClass='<%# GetStatusCssClass(Eval("TrangThai")) %>'>
                            </asp:Label>
                        </ItemTemplate>
                    </asp:TemplateField>
                    <asp:TemplateField HeaderText="Hành Động" ItemStyle-CssClass="px-4 py-3 whitespace-nowrap text-sm font-medium text-center">
                         <ItemTemplate>
                            <div class="flex items-center justify-center space-x-4">
                                <%-- Edit Button --%>
                                <asp:LinkButton ID="lnkEditUser" runat="server"
                                    CommandName="ShowEditModal" CommandArgument='<%# Eval("IDNguoiDung") %>'
                                    CssClass="text-gray-400 hover:text-indigo-600 transition duration-150 ease-in-out"
                                    ToolTip="Sửa thông tin" Visible='<%# CanModifyUser(Convert.ToInt32(Eval("IDNguoiDung"))) %>'
                                    OnClientClick='<%# "openEditModal(" + Eval("IDNguoiDung") + "); return false;" %>'>
                                    <i class="fas fa-edit fa-fw"></i>
                                </asp:LinkButton>
                                <%-- Lock/Unlock Button --%>
                                <asp:LinkButton ID="lnkToggleLock" runat="server"
                                    CommandName='<%# Eval("TrangThai").ToString().Equals("Active", StringComparison.OrdinalIgnoreCase) ? "LockUser" : "UnlockUser" %>' CommandArgument='<%# Eval("IDNguoiDung") %>'
                                    CssClass='<%# Eval("TrangThai").ToString().Equals("Active", StringComparison.OrdinalIgnoreCase) ? "text-gray-400 hover:text-yellow-600" : "text-gray-400 hover:text-green-600" %> transition duration-150 ease-in-out'
                                    ToolTip='<%# Eval("TrangThai").ToString().Equals("Active", StringComparison.OrdinalIgnoreCase) ? "Khóa tài khoản" : "Mở khóa tài khoản" %>'
                                    Visible='<%# CanModifyUser(Convert.ToInt32(Eval("IDNguoiDung"))) %>'>
                                    <%-- OnClientClick đã được xóa --%>
                                    <i class='<%# Eval("TrangThai").ToString().Equals("Active", StringComparison.OrdinalIgnoreCase) ? "fas fa-lock fa-fw" : "fas fa-lock-open fa-fw" %>'></i>
                                </asp:LinkButton>

                                <%-- Soft Delete Button --%>
                                <asp:LinkButton ID="lnkDeleteUser" runat="server"
                                    CommandName="DeleteUser" CommandArgument='<%# Eval("IDNguoiDung") %>'
                                    CssClass="text-gray-400 hover:text-red-600 transition duration-150 ease-in-out"
                                    ToolTip="Vô hiệu hóa tài khoản" Visible='<%# CanModifyUser(Convert.ToInt32(Eval("IDNguoiDung"))) %>'>
                                    <%-- OnClientClick đã được xóa --%>
                                    <i class="fas fa-user-slash fa-fw"></i>
                                </asp:LinkButton>
                            </div>
                        </ItemTemplate>
                    </asp:TemplateField>
                </Columns>
            </asp:GridView>
        </div>
        <%-- End GridView Container --%>
    </div> <%-- End Main container --%>

    <%-- Edit User Modal --%>
    <div id="editUserModal" class="modal">
        <div class="modal-content-wrapper"> <%-- Wrapper for centering --%>
            <div class="modal-content bg-white rounded-lg shadow-xl w-full max-w-2xl">
                <div class="modal-header px-6 py-4 border-b border-gray-200 flex justify-between items-center">
                    <h2 class="text-lg font-semibold text-gray-800">Chỉnh sửa Người dùng</h2>
                    <button type="button" class="close-button" onclick="closeEditModal()" aria-label="Close">&times;</button>
                </div>
                <div class="modal-body px-6 py-5">
                    <%-- Modal Message Area --%>
                     <asp:Panel ID="pnlModalMessageArea" runat="server" Visible="false" class="mb-4">
                         <div id="modalSuccessAlert" runat="server" visible="false" class="modal-message modal-message-success" role="alert">
                             <p><asp:Literal ID="litModalSuccessMessage" runat="server" /></p>
                         </div>
                          <div id="modalErrorAlert" runat="server" visible="false" class="modal-message modal-message-error" role="alert">
                             <p><asp:Literal ID="litModalErrorMessage" runat="server" /></p>
                         </div>
                          <div id="modalWarningAlert" runat="server" visible="false" class="modal-message modal-message-warning" role="alert">
                             <p><asp:Literal ID="litModalWarningMessage" runat="server" /></p>
                         </div>
                         <%-- Add Info similarly if needed --%>
                    </asp:Panel>
                    <%-- End Modal Message Area --%>

                    <asp:UpdatePanel ID="UpdatePanelModal" runat="server" UpdateMode="Conditional">
                        <ContentTemplate>
                            <asp:HiddenField ID="hfEditUserID" runat="server" />
                             <div class="space-y-4"> <%-- Use space-y for consistent spacing --%>
                                <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                                    <div class="form-group">
                                        <label for="<%= txtEditUsername.ClientID %>" class="block text-sm font-medium text-gray-700">Username:</label>
                                        <asp:TextBox ID="txtEditUsername" runat="server" CssClass="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm bg-gray-100 cursor-not-allowed" ReadOnly="true"></asp:TextBox>
                                    </div>
                                    <div class="form-group">
                                        <label for="<%= txtEditEmail.ClientID %>" class="block text-sm font-medium text-gray-700">Email:</label>
                                        <asp:TextBox ID="txtEditEmail" runat="server" CssClass="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm" TextMode="Email" oninput="clearModalMessage()"></asp:TextBox>
                                    </div>
                                    <div class="form-group">
                                        <label for="<%= txtEditTen.ClientID %>" class="block text-sm font-medium text-gray-700">Tên:</label>
                                        <asp:TextBox ID="txtEditTen" runat="server" CssClass="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm" oninput="clearModalMessage()"></asp:TextBox>
                                    </div>
                                    <div class="form-group">
                                        <label for="<%= txtEditDienThoai.ClientID %>" class="block text-sm font-medium text-gray-700">Điện thoại:</label>
                                        <asp:TextBox ID="txtEditDienThoai" runat="server" CssClass="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm" oninput="clearModalMessage()"></asp:TextBox>
                                    </div>
                                    <div class="form-group">
                                        <label for="<%= ddlEditVaiTro.ClientID %>" class="block text-sm font-medium text-gray-700">Vai trò:</label>
                                        <asp:DropDownList ID="ddlEditVaiTro" runat="server" CssClass="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm" onchange="clearModalMessage()">
                                            <asp:ListItem Text="Admin" Value="0"></asp:ListItem>
                                            <asp:ListItem Text="User" Value="1"></asp:ListItem>
                                        </asp:DropDownList>
                                    </div>
                                    <div class="form-group">
                                        <label for="<%= ddlEditTrangThai.ClientID %>" class="block text-sm font-medium text-gray-700">Trạng thái:</label>
                                        <asp:DropDownList ID="ddlEditTrangThai" runat="server" CssClass="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm" onchange="clearModalMessage()">
                                            <asp:ListItem Text="Active" Value="Active"></asp:ListItem>
                                            <asp:ListItem Text="Locked" Value="Locked"></asp:ListItem>
                                        </asp:DropDownList>
                                    </div>
                                </div>

                                <div class="pt-4 border-t border-gray-200 space-y-4"> <%-- Separator and spacing --%>
                                     <div class="form-group">
                                        <label for="<%= txtEditMatKhauMoi.ClientID %>" class="block text-sm font-medium text-gray-700">Mật khẩu mới <span class="text-gray-500 font-normal">(để trống nếu không đổi):</span></label>
                                        <asp:TextBox ID="txtEditMatKhauMoi" runat="server" CssClass="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm" TextMode="Password" oninput="clearModalMessage()"></asp:TextBox>
                                    </div>
                                    <div class="form-group">
                                        <label for="<%= txtEditXacNhanMatKhau.ClientID %>" class="block text-sm font-medium text-gray-700">Xác nhận mật khẩu mới:</label>
                                        <asp:TextBox ID="txtEditXacNhanMatKhau" runat="server" CssClass="mt-1 block w-full rounded-md border-gray-300 shadow-sm focus:border-indigo-500 focus:ring-indigo-500 sm:text-sm" TextMode="Password" oninput="clearModalMessage()"></asp:TextBox>
                                    </div>
                                </div>
                             </div>
                        </ContentTemplate>
                    </asp:UpdatePanel>
                </div>
                <div class="modal-footer px-6 py-4 bg-gray-50 border-t border-gray-200 flex justify-end space-x-3">
                    <asp:Button ID="btnCancelEdit" runat="server" Text="Hủy" CssClass="py-2 px-4 border border-gray-300 rounded-md shadow-sm text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500" OnClientClick="closeEditModal(); return false;" />
                    <%-- Add loading state handling to this button via JS if desired --%>
                    <asp:Button ID="btnSaveChanges" runat="server" Text="Lưu thay đổi" CssClass="py-2 px-4 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500" OnClick="btnSaveChanges_Click" />
                </div>
            </div>
        </div>
    </div>

    <%-- JavaScript for Modal Interaction & AJAX --%>
    <script type="text/javascript">
        var modal = document.getElementById('editUserModal');
        var hfUserID = document.getElementById('<%= hfUserID.ClientID %>');
        var hfEditUserID = document.getElementById('<%= hfEditUserID.ClientID %>');

        // References to modal message panels (adjust IDs if changed)
        var pnlModalMessageArea = document.getElementById('<%= pnlModalMessageArea.ClientID %>');
        var modalSuccessAlert = document.getElementById('<%= modalSuccessAlert.ClientID %>');
        var modalErrorAlert = document.getElementById('<%= modalErrorAlert.ClientID %>');
        var modalWarningAlert = document.getElementById('<%= modalWarningAlert.ClientID %>');
        var litModalSuccessMessage = document.getElementById('<%= litModalSuccessMessage.ClientID %>');
        var litModalErrorMessage = document.getElementById('<%= litModalErrorMessage.ClientID %>');
        var litModalWarningMessage = document.getElementById('<%= litModalWarningMessage.ClientID %>');


        function openEditModal(userId) {
            console.log('Opening modal for UserID:', userId);
            hfUserID.value = userId;
            hfEditUserID.value = userId; // Set hidden field inside UpdatePanel too
            clearModalMessage();
            // Optional: Show loading indicator here
            PageMethods.GetUserData(userId, onGetUserDataSuccess, onGetUserDataFailure);
        }

        function onGetUserDataSuccess(response) {
            // Optional: Hide loading indicator here
            console.log('User data response:', response);
            if (response && response.error) {
                onGetUserDataFailure(response.error); // Show error from server
            } else if (response) {
                document.getElementById('<%= txtEditUsername.ClientID %>').value = response.Username || '';
                document.getElementById('<%= txtEditEmail.ClientID %>').value = response.Email || '';
                document.getElementById('<%= txtEditTen.ClientID %>').value = response.Ten || '';
                document.getElementById('<%= txtEditDienThoai.ClientID %>').value = response.DienThoai || '';
                document.getElementById('<%= ddlEditVaiTro.ClientID %>').value = response.VaiTro;
                document.getElementById('<%= ddlEditTrangThai.ClientID %>').value = response.TrangThai;

                var passField = document.getElementById('<%= txtEditMatKhauMoi.ClientID %>');
                if (passField) passField.value = '';
                var confirmPassField = document.getElementById('<%= txtEditXacNhanMatKhau.ClientID %>');
                if (confirmPassField) confirmPassField.value = '';

                clearModalMessage();
                modal.classList.add('open'); // Add 'open' class to show modal with transition
            } else {
                onGetUserDataFailure("Không nhận được dữ liệu người dùng hợp lệ.");
            }
        }

        function onGetUserDataFailure(error) {
             // Optional: Hide loading indicator here
            var errorMessage = "Lỗi khi tải dữ liệu người dùng.";
            if (typeof error === 'string') { errorMessage = error; }
            else if (error && error.get_message) { errorMessage = error.get_message(); }
            else if (error && error.message) { errorMessage = error.message; }

            console.error('Failed to get user data:', errorMessage);

            // Display error in the modal's alert structure
            if (litModalErrorMessage && modalErrorAlert && pnlModalMessageArea) {
                 litModalErrorMessage.innerText = errorMessage;
                 modalSuccessAlert.style.display = 'none';
                 modalWarningAlert.style.display = 'none';
                 modalErrorAlert.style.display = 'block';
                 pnlModalMessageArea.style.display = 'block';
             }

            modal.classList.add('open'); // Still show modal to display the error
        }

        function closeEditModal() {
            modal.classList.remove('open'); // Remove 'open' class to hide modal
            // Optional: Reset form fields more explicitly if needed after UpdatePanel updates
             hfUserID.value = '';
             hfEditUserID.value = ''; // Clear hidden field in UpdatePanel
             clearModalMessage();
             // Consider resetting input fields explicitly if UpdatePanel doesn't clear them reliably
             // document.getElementById('<%= txtEditEmail.ClientID %>').value = ''; ... etc.
             document.getElementById('<%= txtEditMatKhauMoi.ClientID %>').value = '';
             document.getElementById('<%= txtEditXacNhanMatKhau.ClientID %>').value = '';
        }

        function clearModalMessage() {
            // Hide all modal message panels
            if (pnlModalMessageArea) {
                pnlModalMessageArea.style.display = 'none';
            }
            if (modalSuccessAlert) modalSuccessAlert.style.display = 'none';
            if (modalErrorAlert) modalErrorAlert.style.display = 'none';
            if (modalWarningAlert) modalWarningAlert.style.display = 'none';

            // Clear text potentially
            if (litModalSuccessMessage) litModalSuccessMessage.innerText = '';
            if (litModalErrorMessage) litModalErrorMessage.innerText = '';
            if (litModalWarningMessage) litModalWarningMessage.innerText = '';
        }

        // Close modal if background overlay is clicked
        window.addEventListener('click', function (event) {
            // Check if the click is directly on the modal overlay wrapper
            if (event.target.classList.contains('modal-content-wrapper')) {
                closeEditModal();
            }
        });

        // Add this if you want to clear modal messages when user starts typing in fields
        // Call this function in oninput event of textboxes/onchange of dropdowns inside modal
        // function clearModalMessageOnChange() { clearModalMessage(); }
        // Example usage in ASPX: oninput="clearModalMessageOnChange()"

    </script>
        <%-- BẮT ĐẦU: THÊM SCRIPT CHO CÁC POPUP XÁC NHẬN --%>
    <script type="text/javascript">
        // Hàm chung để kích hoạt postback
        function triggerPostBack(sourceControlUniqueId) {
            __doPostBack(sourceControlUniqueId, '');
        }

        // Popup xác nhận KHÓA tài khoản
        function showLockConfirmation(username, sourceControlUniqueId) {
            Swal.fire({
                title: 'Bạn có chắc chắn muốn KHÓA?',
                html: `Tài khoản <strong>${username}</strong> sẽ tạm thời không thể đăng nhập.`,
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#f59e0b', // Màu vàng
                cancelButtonColor: '#6b7280',
                confirmButtonText: 'Đồng ý, Khóa!',
                cancelButtonText: 'Hủy'
            }).then((result) => {
                if (result.isConfirmed) {
                    triggerPostBack(sourceControlUniqueId);
                }
            });
        }

        // Popup xác nhận MỞ KHÓA tài khoản
        function showUnlockConfirmation(username, sourceControlUniqueId) {
            Swal.fire({
                title: 'Bạn có chắc chắn muốn MỞ KHÓA?',
                html: `Tài khoản <strong>${username}</strong> sẽ có thể đăng nhập trở lại.`,
                icon: 'info',
                showCancelButton: true,
                confirmButtonColor: '#10b981', // Màu xanh lá
                cancelButtonColor: '#6b7280',
                confirmButtonText: 'Đồng ý, Mở khóa!',
                cancelButtonText: 'Hủy'
            }).then((result) => {
                if (result.isConfirmed) {
                    triggerPostBack(sourceControlUniqueId);
                }
            });
        }

        // Popup xác nhận VÔ HIỆU HÓA (Xóa mềm) tài khoản
        function showDisableConfirmation(username, sourceControlUniqueId) {
            Swal.fire({
                title: 'Bạn có chắc chắn muốn VÔ HIỆU HÓA?',
                html: `Tài khoản <strong>${username}</strong> sẽ bị ẩn và không thể đăng nhập.<br/>Dữ liệu liên quan (đơn hàng,...) <strong>vẫn được giữ lại</strong>.`,
                icon: 'error',
                showCancelButton: true,
                confirmButtonColor: '#ef4444', // Màu đỏ
                cancelButtonColor: '#6b7280',
                confirmButtonText: 'Đồng ý, Vô hiệu hóa!',
                cancelButtonText: 'Hủy'
            }).then((result) => {
                if (result.isConfirmed) {
                    triggerPostBack(sourceControlUniqueId);
                }
            });
        }
    </script>
    <%-- KẾT THÚC: THÊM SCRIPT --%>
</asp:Content>