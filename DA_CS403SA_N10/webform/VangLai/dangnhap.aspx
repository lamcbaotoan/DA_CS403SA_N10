<%@ Page Title="Đăng Nhập" Language="C#" MasterPageFile="~/WebForm/VangLai/Site.Master" AutoEventWireup="true" CodeBehind="dangnhap.aspx.cs" Inherits="Webebook.WebForm.VangLai.dangnhap" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <%-- Font Awesome đã có trong MasterPage hoặc được link ở đây --%>
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.0.0/css/all.min.css" />
     <style>
        /* Có thể thêm CSS tùy chỉnh nếu cần */
        /* Ví dụ: style cho placeholder */
         input::placeholder {
            /* color: #9ca3af; */ /* gray-400 */
            /* font-size: 0.875rem; */
        }
         /* Smooth transition for inputs and buttons */
         input, button, a {
             transition: all 0.15s ease-in-out;
         }
     </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <%-- Container lớn để căn giữa --%>
    <div class="min-h-screen flex items-center justify-center bg-gradient-to-br from-purple-50 via-white to-blue-50 py-12 px-4 sm:px-6 lg:px-8">
         <%-- Box đăng nhập --%>
        <div class="bg-white p-8 md:p-10 rounded-2xl shadow-xl w-full max-w-md border border-gray-100">
            <div class="text-center mb-8">
                <%-- Logo (tùy chọn) --%>
                <%-- <img src="/path/to/your/logo.png" alt="Webebook Logo" class="mx-auto h-12 w-auto mb-4" /> --%>
                <h2 class="text-3xl font-extrabold text-center text-purple-700">Đăng Nhập Webebook</h2>
                <p class="text-sm text-gray-500 mt-2">Chào mừng bạn trở lại!</p>
            </div>

            <%-- Thông báo chung (Đăng ký thành công, Reset pass thành công) --%>
            <asp:Panel ID="pnlMessageContainer" runat="server" Visible="false" CssClass="mb-4 p-3 rounded-lg text-sm border flex items-center">
                <%-- CSS class và icon sẽ được đặt từ code-behind --%>
                 <i id="iconMessage" runat="server" class="fas mr-2 flex-shrink-0"></i>
                 <asp:Label ID="lblLoginMessage" runat="server"></asp:Label>
            </asp:Panel>

            <%-- Thông báo lỗi đăng nhập cụ thể --%>
             <%-- Label này sẽ được gán Text và CSS class từ code-behind --%>
            <asp:Label ID="lblLoginError" runat="server" Visible="false" CssClass="mb-4 p-3 rounded-lg text-sm border flex items-center"></asp:Label>

            <%-- Form đăng nhập --%>
            <div class="space-y-5">
                <div>
                    <label for="<%= txtLoginUsername.ClientID %>" class="block text-sm font-medium text-gray-700 mb-1">Tên đăng nhập / Email / SĐT</label>
                    <div class="relative rounded-md shadow-sm">
                        <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                            <i class="fas fa-user text-gray-400"></i>
                        </div>
                        <asp:TextBox ID="txtLoginUsername" runat="server" placeholder="Nhập thông tin đăng nhập..."
                            CssClass="block w-full pl-10 pr-3 py-2.5 border border-gray-300 rounded-md placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-purple-500 focus:border-purple-500 sm:text-sm"></asp:TextBox>
                    </div>
                     <asp:RequiredFieldValidator ID="rfvLoginUsername" runat="server" ControlToValidate="txtLoginUsername"
                         ErrorMessage="Vui lòng nhập thông tin đăng nhập." Display="Dynamic"
                         CssClass="mt-1 text-xs text-red-600 flex items-center">
                          <i class="fas fa-exclamation-circle mr-1 flex-shrink-0"></i> <span>Vui lòng nhập thông tin đăng nhập.</span>
                     </asp:RequiredFieldValidator>
                </div>

                <div>
                     <label for="<%= txtLoginPassword.ClientID %>" class="block text-sm font-medium text-gray-700 mb-1">Mật khẩu</label>
                     <div class="relative rounded-md shadow-sm">
                         <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                             <i class="fas fa-lock text-gray-400"></i>
                         </div>
                         <asp:TextBox ID="txtLoginPassword" runat="server" TextMode="Password" placeholder="Nhập mật khẩu..."
                            CssClass="block w-full pl-10 pr-10 py-2.5 border border-gray-300 rounded-md placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-purple-500 focus:border-purple-500 sm:text-sm"></asp:TextBox>
                         <div class="absolute inset-y-0 right-0 pr-3 flex items-center text-sm leading-5">
                             <span class="cursor-pointer text-gray-500 hover:text-purple-600" id="togglePassword">
                                 <i class="fas fa-eye"></i>
                             </span>
                         </div>
                    </div>
                    <asp:RequiredFieldValidator ID="rfvLoginPassword" runat="server" ControlToValidate="txtLoginPassword"
                        ErrorMessage="Vui lòng nhập mật khẩu." Display="Dynamic"
                        CssClass="mt-1 text-xs text-red-600 flex items-center">
                         <i class="fas fa-exclamation-circle mr-1 flex-shrink-0"></i> <span>Vui lòng nhập mật khẩu.</span>
                    </asp:RequiredFieldValidator>
                </div>
            </div>

            <%-- Nút Đăng nhập --%>
            <div class="mt-6">
                <asp:Button ID="btnLogin" runat="server" Text="Đăng Nhập"
                    CssClass="w-full flex justify-center py-3 px-4 border border-transparent rounded-lg shadow-sm text-sm font-medium text-white bg-purple-600 hover:bg-purple-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-purple-500"
                    OnClick="btnLogin_Click" />
            </div>

            <%-- Quên mật khẩu và Đăng ký link --%>
            <div class="mt-5 flex items-center justify-between text-sm">
                 <div class="text-left">
                     <a href='<%= ResolveUrl("~/WebForm/VangLai/quenmatkhau.aspx") %>' class="font-medium text-purple-600 hover:text-purple-500 hover:underline">
                         Quên mật khẩu?
                     </a>
                 </div>
                 <div class="text-right">
                     <span class="text-gray-600">Chưa có tài khoản?</span>
                     <a href='<%= ResolveUrl("~/WebForm/VangLai/dangky.aspx") %>' class="ml-1 font-medium text-purple-600 hover:text-purple-500 hover:underline">
                         Đăng ký ngay
                     </a>
                 </div>
            </div>
        </div> <%-- End login box --%>
    </div> <%-- End centering container --%>

    <%-- Script for password toggle (Giữ nguyên) --%>
    <script>
        document.addEventListener('DOMContentLoaded', () => {
            const togglePassword = document.querySelector('#togglePassword');
            const passwordInput = document.querySelector('#<%= txtLoginPassword.ClientID %>'); // Ensure correct ID
            const passwordIcon = togglePassword ? togglePassword.querySelector('i') : null;

            if (togglePassword && passwordInput && passwordIcon) {
                togglePassword.addEventListener('click', (e) => {
                    // Prevent Default if needed, though click on span might be fine
                    // e.preventDefault();
                    const type = passwordInput.getAttribute('type') === 'password' ? 'text' : 'password';
                    passwordInput.setAttribute('type', type);
                    // Toggle icon class
                    passwordIcon.classList.toggle('fa-eye');
                    passwordIcon.classList.toggle('fa-eye-slash');
                });
            } else {
                console.error("Password toggle elements not found.");
            }
        });
    </script>
</asp:Content>