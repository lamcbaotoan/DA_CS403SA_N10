<%@ Page Title="Đăng Ký" Language="C#" MasterPageFile="~/WebForm/VangLai/Site.Master" AutoEventWireup="true" CodeBehind="dangky.aspx.cs" Inherits="Webebook.WebForm.VangLai.dangky" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
     <%-- Font Awesome đã có trong MasterPage hoặc được link ở đây --%>
     <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.0.0/css/all.min.css" />
     <style>
        /* Smooth transition for inputs and buttons */
        input, button, a {
            transition: all 0.15s ease-in-out;
        }
        /* Styling cho validator và error label giống nhau */
        .validation-error {
             margin-top: 0.25rem; /* mt-1 */
             font-size: 0.75rem; /* text-xs */
             color: #dc2626; /* text-red-600 */
             display: flex;
             align-items: center;
        }
        .validation-error i {
             margin-right: 0.25rem; /* mr-1 */
             flex-shrink: 0; /* Ngăn icon bị co lại */
        }
        .validation-error span { /* Đảm bảo text không bị icon đẩy xuống */
            display: inline-block;
        }
     </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <%-- Container lớn để căn giữa - tương tự trang đăng nhập --%>
    <div class="min-h-screen flex items-center justify-center bg-gradient-to-br from-purple-50 via-white to-blue-50 py-12 px-4 sm:px-6 lg:px-8">
        <%-- Box đăng ký --%>
        <div class="bg-white p-8 md:p-10 rounded-2xl shadow-xl w-full max-w-md border border-gray-100">
             <div class="text-center mb-8">
                 <h2 class="text-3xl font-extrabold text-center text-purple-700">Tạo Tài Khoản Mới</h2>
                 <p class="text-sm text-gray-500 mt-2">Tham gia cộng đồng Webebook!</p>
             </div>

            <%-- Form đăng ký --%>
            <div class="space-y-5">
                <%-- Tên đăng nhập --%>
                <div>
                    <label for="<%= txtUsername.ClientID %>" class="block text-sm font-medium text-gray-700 mb-1">Tên đăng nhập</label>
                    <div class="relative rounded-md shadow-sm">
                         <asp:TextBox ID="txtUsername" runat="server" placeholder="Chọn tên đăng nhập"
                             CssClass="block w-full px-3 py-2.5 border border-gray-300 rounded-md placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-purple-500 focus:border-purple-500 sm:text-sm"></asp:TextBox>
                    </div>
                    <%-- Validator --%>
                    <asp:RequiredFieldValidator ID="rfvUsername" runat="server" ControlToValidate="txtUsername"
                        ErrorMessage="Vui lòng nhập tên đăng nhập." Display="Dynamic" SetFocusOnError="true"
                        CssClass="validation-error">
                         <i class="fas fa-exclamation-circle"></i> <span>Vui lòng nhập tên đăng nhập.</span>
                    </asp:RequiredFieldValidator>
                    <%-- Label cho lỗi backend (trùng user) - style giống validator --%>
                    <asp:Label ID="lblUsernameError" runat="server" EnableViewState="false" CssClass="validation-error"></asp:Label>
                </div>

                 <%-- Email --%>
                 <div>
                    <label for="<%= txtEmail.ClientID %>" class="block text-sm font-medium text-gray-700 mb-1">Email</label>
                    <div class="relative rounded-md shadow-sm">
                        <asp:TextBox ID="txtEmail" runat="server" TextMode="Email" placeholder="Nhập địa chỉ email"
                            CssClass="block w-full px-3 py-2.5 border border-gray-300 rounded-md placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-purple-500 focus:border-purple-500 sm:text-sm"></asp:TextBox>
                    </div>
                    <%-- Validators --%>
                    <asp:RequiredFieldValidator ID="rfvEmail" runat="server" ControlToValidate="txtEmail"
                        ErrorMessage="Vui lòng nhập email." Display="Dynamic" SetFocusOnError="true"
                        CssClass="validation-error">
                        <i class="fas fa-exclamation-circle"></i> <span>Vui lòng nhập email.</span>
                    </asp:RequiredFieldValidator>
                    <asp:RegularExpressionValidator ID="revEmail" runat="server" ControlToValidate="txtEmail"
                        ErrorMessage="Email không đúng định dạng." ValidationExpression="\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*" Display="Dynamic" SetFocusOnError="true"
                        CssClass="validation-error">
                        <i class="fas fa-exclamation-circle"></i> <span>Email không đúng định dạng.</span>
                    </asp:RegularExpressionValidator>
                    <%-- Label cho lỗi backend (trùng email) - style giống validator --%>
                    <asp:Label ID="lblEmailError" runat="server" EnableViewState="false" CssClass="validation-error"></asp:Label>
                 </div>

                 <%-- Mật khẩu --%>
                 <div>
                     <label for="<%= txtPassword.ClientID %>" class="block text-sm font-medium text-gray-700 mb-1">Mật khẩu</label>
                     <div class="relative rounded-md shadow-sm">
                         <asp:TextBox ID="txtPassword" runat="server" TextMode="Password" placeholder="Tạo mật khẩu (ít nhất 6 ký tự)"
                            CssClass="block w-full px-3 pr-10 py-2.5 border border-gray-300 rounded-md placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-purple-500 focus:border-purple-500 sm:text-sm"></asp:TextBox>
                         <div class="absolute inset-y-0 right-0 pr-3 flex items-center text-sm leading-5">
                              <span class="cursor-pointer text-gray-500 hover:text-purple-600" id="togglePassword">
                                  <i class="fas fa-eye"></i>
                              </span>
                         </div>
                    </div>
                    <%-- Validators --%>
                    <asp:RequiredFieldValidator ID="rfvPassword" runat="server" ControlToValidate="txtPassword"
                         ErrorMessage="Vui lòng nhập mật khẩu." Display="Dynamic" SetFocusOnError="true"
                         CssClass="validation-error">
                          <i class="fas fa-exclamation-circle"></i> <span>Vui lòng nhập mật khẩu.</span>
                     </asp:RequiredFieldValidator>
                    <asp:RegularExpressionValidator ID="revPasswordLength" runat="server" ControlToValidate="txtPassword"
                         ErrorMessage="Mật khẩu phải có ít nhất 6 ký tự." ValidationExpression=".{6,}" Display="Dynamic" SetFocusOnError="true"
                         CssClass="validation-error">
                          <i class="fas fa-exclamation-circle"></i> <span>Mật khẩu phải có ít nhất 6 ký tự.</span>
                    </asp:RegularExpressionValidator>
                    <%-- Label cho lỗi backend khác (nếu có) - style giống validator --%>
                    <asp:Label ID="lblPasswordError" runat="server" EnableViewState="false" CssClass="validation-error"></asp:Label>
                 </div>

                 <%-- Xác nhận Mật khẩu --%>
                 <div>
                    <label for="<%= txtConfirmPassword.ClientID %>" class="block text-sm font-medium text-gray-700 mb-1">Xác nhận mật khẩu</label>
                    <div class="relative rounded-md shadow-sm">
                        <asp:TextBox ID="txtConfirmPassword" runat="server" TextMode="Password" placeholder="Nhập lại mật khẩu"
                            CssClass="block w-full px-3 pr-10 py-2.5 border border-gray-300 rounded-md placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-purple-500 focus:border-purple-500 sm:text-sm"></asp:TextBox>
                         <div class="absolute inset-y-0 right-0 pr-3 flex items-center text-sm leading-5">
                              <span class="cursor-pointer text-gray-500 hover:text-purple-600" id="toggleConfirmPassword">
                                  <i class="fas fa-eye"></i>
                              </span>
                         </div>
                    </div>
                    <%-- Validators --%>
                     <asp:RequiredFieldValidator ID="rfvConfirmPassword" runat="server" ControlToValidate="txtConfirmPassword"
                         ErrorMessage="Vui lòng xác nhận mật khẩu." Display="Dynamic" SetFocusOnError="true"
                         CssClass="validation-error">
                          <i class="fas fa-exclamation-circle"></i> <span>Vui lòng xác nhận mật khẩu.</span>
                     </asp:RequiredFieldValidator>
                     <asp:CompareValidator ID="cvPassword" runat="server" ControlToValidate="txtConfirmPassword" ControlToCompare="txtPassword" Operator="Equal" Type="String"
                         ErrorMessage="Mật khẩu không trùng khớp." Display="Dynamic" SetFocusOnError="true"
                         CssClass="validation-error">
                          <i class="fas fa-exclamation-circle"></i> <span>Mật khẩu không trùng khớp.</span>
                    </asp:CompareValidator>
                 </div>
            </div>

            <%-- Nút Đăng ký --%>
            <div class="mt-8">
                 <asp:Button ID="btnDangKy" runat="server" Text="Đăng Ký Ngay"
                    CssClass="w-full flex justify-center py-3 px-4 border border-transparent rounded-lg shadow-sm text-sm font-medium text-white bg-purple-600 hover:bg-purple-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-purple-500"
                    OnClick="btnDangKy_Click" />
            </div>

            <%-- Link Đăng nhập --%>
             <div class="mt-5 text-center text-sm">
                  <span class="text-gray-600">Đã có tài khoản?</span>
                  <a href='<%= ResolveUrl("~/WebForm/VangLai/dangnhap.aspx") %>' class="ml-1 font-medium text-purple-600 hover:text-purple-500 hover:underline">
                      Đăng nhập
                  </a>
            </div>
        </div> <%-- End registration box --%>
    </div> <%-- End centering container --%>

    <%-- Script for password toggle (Giữ nguyên) --%>
    <script>
        document.addEventListener('DOMContentLoaded', () => {
            function setupToggle(toggleSpanId, inputId) {
                const toggleSpan = document.getElementById(toggleSpanId);
                const passwordInput = document.getElementById(inputId); // Get element by ID directly
                const icon = toggleSpan ? toggleSpan.querySelector('i') : null;

                if (toggleSpan && passwordInput && icon) {
                    toggleSpan.addEventListener('click', () => {
                        const type = passwordInput.getAttribute('type') === 'password' ? 'text' : 'password';
                        passwordInput.setAttribute('type', type);
                        icon.classList.toggle('fa-eye');
                        icon.classList.toggle('fa-eye-slash');
                    });
                } else {
                     // It's okay if toggle span not found, maybe log warning if needed
                     // console.warn(`Toggle elements not found for: ${toggleSpanId}, ${inputId}`);
                }
            }

             // Sử dụng ClientID để đảm bảo ID đúng sau khi render
             setupToggle('togglePassword', '<%= txtPassword.ClientID %>');
            setupToggle('toggleConfirmPassword', '<%= txtConfirmPassword.ClientID %>');
        });
    </script>
</asp:Content>