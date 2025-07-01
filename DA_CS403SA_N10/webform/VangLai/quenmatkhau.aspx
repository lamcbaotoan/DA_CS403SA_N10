<%@ Page Title="Quên Mật Khẩu" Language="C#" MasterPageFile="~/WebForm/VangLai/Site.Master" AutoEventWireup="true" CodeBehind="quenmatkhau.aspx.cs" Inherits="Webebook.WebForm.VangLai.quenmatkhau" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <%-- Font Awesome nếu cần cho icon --%>
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.0.0/css/all.min.css" />
    <style>
        /* CSS cho hiệu ứng modal - Đã sửa lỗi chặn click */
        #popupMaXacNhan {
            transition: opacity 0.3s ease-out;
        }
        #popupMaXacNhan .modal-content {
            transition: transform 0.3s ease-out, opacity 0.3s ease-out;
        }

        /* Trạng thái ẩn: Quan trọng là display: none */
        #popupMaXacNhan.modal-hidden {
            display: none;
            opacity: 0;
        }

        /* Trạng thái hiện */
        #popupMaXacNhan.modal-visible {
            display: flex; /* Hoặc block */
            opacity: 1;
        }
        /* Áp dụng transform/opacity cho nội dung khi hiện */
        #popupMaXacNhan.modal-visible .modal-content {
           opacity: 1;
           transform: translateY(0) scale(1);
        }
        /* Đặt transform/opacity ban đầu cho nội dung trước khi hiện */
        #popupMaXacNhan .modal-content {
           opacity: 0;
           transform: translateY(-20px) scale(0.95);
        }

        /* Đảm bảo nội dung trang không bị scroll khi modal mở */
        body.modal-open {
            overflow: hidden;
        }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="min-h-screen flex items-center justify-center bg-gray-100 py-12 px-4 sm:px-6 lg:px-8">
        <div class="max-w-md w-full space-y-8">
            <%-- Phần Form Nhập Email --%>
            <div id="divRequestEmail" runat="server" class="bg-white p-8 md:p-10 rounded-2xl shadow-xl space-y-6">
                <div>
                    <h2 class="text-center text-3xl font-extrabold text-gray-900">
                        Quên Mật Khẩu
                    </h2>
                    <p class="mt-2 text-center text-sm text-gray-600">
                        Nhập email của bạn để nhận mã xác nhận.
                    </p>
                </div>

                 <%-- Label hiển thị thông báo chung (lỗi, thành công) --%>
                 <asp:Label ID="lblMessage" runat="server" EnableViewState="false" Visible="false"></asp:Label>

                <%-- Input Email --%>
                <div class="rounded-md shadow-sm -space-y-px">
                     <div>
                         <label for="<%=txtEmail.ClientID%>" class="sr-only">Email</label>
                         <asp:TextBox ID="txtEmail" runat="server" TextMode="Email" CssClass="appearance-none rounded-md relative block w-full px-3 py-3 border border-gray-300 placeholder-gray-500 text-gray-900 focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 focus:z-10 sm:text-sm" placeholder="Địa chỉ Email"></asp:TextBox>
                         <asp:RequiredFieldValidator ID="rfvEmail" runat="server" ControlToValidate="txtEmail"
                             ErrorMessage="Vui lòng nhập địa chỉ email." CssClass="text-red-600 text-xs mt-1 block" Display="Dynamic" ValidationGroup="RequestGroup"></asp:RequiredFieldValidator>
                     </div>
                </div>

                <%-- Nút Gửi Yêu Cầu --%>
                <div>
                     <asp:Button ID="btnGui" runat="server" Text="Gửi Yêu Cầu"
                         CssClass="group relative w-full flex justify-center py-3 px-4 border border-transparent text-sm font-medium rounded-md text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 transition duration-150 ease-in-out"
                         OnClick="btnGui_Click" ValidationGroup="RequestGroup" />
                </div>

                 <p class="mt-4 text-center text-sm text-gray-600">
                    Nhớ mật khẩu?
                    <a href="dangnhap.aspx" class="font-medium text-indigo-600 hover:text-indigo-500 hover:underline">
                        Đăng nhập ngay
                    </a>
                </p>
            </div>

            <%-- Phần Form Đặt Lại Mật Khẩu (ẩn ban đầu) --%>
            <div id="divResetPassword" runat="server" class="bg-white p-8 md:p-10 rounded-2xl shadow-xl space-y-6" visible="false">
                 <div>
                    <h2 class="text-center text-3xl font-extrabold text-gray-900">
                        Đặt Lại Mật Khẩu
                    </h2>
                     <p class="mt-2 text-center text-sm text-gray-600">
                        Nhập mã xác nhận và mật khẩu mới của bạn.
                    </p>
                </div>

                <%-- Label hiển thị thông báo cho phần reset --%>
                <asp:Label ID="lblResetMessage" runat="server" EnableViewState="false" Visible="false"></asp:Label>

                <%-- Input Mã Xác Nhận --%>
                 <div>
                     <label for="<%=txtMaXacNhan.ClientID%>" class="block text-sm font-medium text-gray-700 mb-1">Mã Xác Nhận</label>
                     <asp:TextBox ID="txtMaXacNhan" runat="server" CssClass="appearance-none rounded-md relative block w-full px-3 py-3 border border-gray-300 placeholder-gray-500 text-gray-900 focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 focus:z-10 sm:text-sm" placeholder="Nhập mã gồm 4 chữ số"></asp:TextBox>
                      <asp:RequiredFieldValidator ID="rfvCode" runat="server" ControlToValidate="txtMaXacNhan"
                             ErrorMessage="Vui lòng nhập mã xác nhận." CssClass="text-red-600 text-xs mt-1 block" Display="Dynamic" ValidationGroup="ResetGroup"></asp:RequiredFieldValidator>
                 </div>

                <%-- Input Mật khẩu mới --%>
                <div>
                    <label for="<%=txtMatKhauMoi.ClientID%>" class="block text-sm font-medium text-gray-700 mb-1">Mật khẩu mới</label>
                    <asp:TextBox ID="txtMatKhauMoi" runat="server" TextMode="Password" CssClass="appearance-none rounded-md relative block w-full px-3 py-3 border border-gray-300 placeholder-gray-500 text-gray-900 focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 focus:z-10 sm:text-sm" placeholder="Nhập mật khẩu mới"></asp:TextBox>
                    <asp:RequiredFieldValidator ID="rfvNewPassword" runat="server" ControlToValidate="txtMatKhauMoi"
                             ErrorMessage="Vui lòng nhập mật khẩu mới." CssClass="text-red-600 text-xs mt-1 block" Display="Dynamic" ValidationGroup="ResetGroup"></asp:RequiredFieldValidator>
                </div>

                 <%-- Input Xác nhận mật khẩu --%>
                 <div>
                    <label for="<%=txtXacNhanMatKhau.ClientID%>" class="block text-sm font-medium text-gray-700 mb-1">Xác nhận mật khẩu</label>
                    <asp:TextBox ID="txtXacNhanMatKhau" runat="server" TextMode="Password" CssClass="appearance-none rounded-md relative block w-full px-3 py-3 border border-gray-300 placeholder-gray-500 text-gray-900 focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 focus:z-10 sm:text-sm" placeholder="Nhập lại mật khẩu mới"></asp:TextBox>
                     <asp:RequiredFieldValidator ID="rfvConfirmPassword" runat="server" ControlToValidate="txtXacNhanMatKhau"
                             ErrorMessage="Vui lòng xác nhận mật khẩu." CssClass="text-red-600 text-xs mt-1 block" Display="Dynamic" ValidationGroup="ResetGroup"></asp:RequiredFieldValidator>
                     <asp:CompareValidator ID="cvPasswords" runat="server" ControlToValidate="txtXacNhanMatKhau" ControlToCompare="txtMatKhauMoi" Operator="Equal" Type="String"
                         ErrorMessage="Mật khẩu xác nhận không khớp." CssClass="text-red-600 text-xs mt-1 block" Display="Dynamic" ValidationGroup="ResetGroup"></asp:CompareValidator>
                </div>

                <%-- Nút Xác Nhận và Hủy --%>
                <div class="flex flex-col space-y-3">
                    <asp:Button ID="btnXacNhan" runat="server" Text="Xác Nhận Đổi Mật Khẩu"
                        CssClass="group relative w-full flex justify-center py-3 px-4 border border-transparent text-sm font-medium rounded-md text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 transition duration-150 ease-in-out"
                        OnClick="btnXacNhan_Click" ValidationGroup="ResetGroup" />
                    <asp:Button ID="btnHuy" runat="server" Text="Hủy" CausesValidation="false"
                        CssClass="group relative w-full flex justify-center py-3 px-4 border border-gray-300 text-sm font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 transition duration-150 ease-in-out"
                        OnClick="btnHuy_Click" />
                </div>
            </div>


             <%-- Script điều khiển popup --%>
            <script type="text/javascript">
                const modal = document.getElementById('popupMaXacNhan');

                function showPopup() {
                    if (modal) {
                        modal.classList.remove('modal-hidden');
                        modal.classList.add('modal-visible');
                        document.body.classList.add('modal-open');
                    }
                }

                function hidePopup() {
                    if (modal) {
                        modal.classList.add('modal-hidden');
                        modal.classList.remove('modal-visible');
                        document.body.classList.remove('modal-open');
                    }
                }
            </script>
        </div>
    </div>
</asp:Content>