<%@ Page Title="Thêm Người Dùng Mới" Language="C#" MasterPageFile="~/WebForm/Admin/Admin.Master" AutoEventWireup="true" CodeBehind="ThemNguoiDung.aspx.cs" Inherits="Webebook.WebForm.Admin.ThemNguoiDung" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="container mx-auto px-4 py-8 max-w-2xl"> <%-- Tăng max-w một chút nếu muốn rộng hơn --%>
        <h2 class="text-3xl font-bold text-gray-800 mb-6 text-center">Thêm Người Dùng Mới</h2> <%-- Tăng cỡ chữ, in đậm, căn giữa --%>

        <%-- Thông báo chung (nếu có) --%>
        <asp:Label ID="lblMessage" runat="server" CssClass="block mb-4 text-center" EnableViewState="false"></asp:Label>

        <div class="bg-white p-8 rounded-xl shadow-lg space-y-6"> <%-- Tăng padding, bo góc mạnh hơn, shadow lớn hơn, tăng khoảng cách giữa các mục --%>

            <%-- Tên đăng nhập --%>
            <div>
                <label for="<%=txtUsername.ClientID %>" class="block text-sm font-medium text-gray-700 mb-1">Tên đăng nhập <span class="text-red-500">*</span></label>
                <asp:TextBox ID="txtUsername" runat="server" CssClass="w-full px-4 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 transition duration-150 ease-in-out"></asp:TextBox> <%-- Thêm padding, hiệu ứng focus rõ hơn, transition --%>
                <asp:RequiredFieldValidator ID="rfvUsername" runat="server" ControlToValidate="txtUsername" ErrorMessage="Tên đăng nhập là bắt buộc." Display="Dynamic" CssClass="text-red-600 text-xs mt-1"></asp:RequiredFieldValidator> <%-- Màu đỏ đậm hơn --%>
                <asp:Label ID="lblUsernameError" runat="server" CssClass="text-red-600 text-xs mt-1" EnableViewState="false"></asp:Label>
            </div>

            <%-- Email --%>
            <div>
                <label for="<%=txtEmail.ClientID %>" class="block text-sm font-medium text-gray-700 mb-1">Email <span class="text-red-500">*</span></label>
                <asp:TextBox ID="txtEmail" runat="server" TextMode="Email" CssClass="w-full px-4 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 transition duration-150 ease-in-out"></asp:TextBox>
                <asp:RequiredFieldValidator ID="rfvEmail" runat="server" ControlToValidate="txtEmail" ErrorMessage="Email là bắt buộc." Display="Dynamic" CssClass="text-red-600 text-xs mt-1"></asp:RequiredFieldValidator>
                <asp:RegularExpressionValidator ID="revEmail" runat="server" ControlToValidate="txtEmail" ErrorMessage="Email không đúng định dạng." ValidationExpression="\w+([-+.']\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*" Display="Dynamic" CssClass="text-red-600 text-xs mt-1"></asp:RegularExpressionValidator>
                <asp:Label ID="lblEmailError" runat="server" CssClass="text-red-600 text-xs mt-1" EnableViewState="false"></asp:Label>
            </div>

            <%-- Mật khẩu --%>
            <div>
                <label for="<%=txtPassword.ClientID %>" class="block text-sm font-medium text-gray-700 mb-1">Mật khẩu <span class="text-red-500">*</span></label>
                <asp:TextBox ID="txtPassword" runat="server" TextMode="Password" CssClass="w-full px-4 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 transition duration-150 ease-in-out"></asp:TextBox>
                <asp:RequiredFieldValidator ID="rfvPassword" runat="server" ControlToValidate="txtPassword" ErrorMessage="Mật khẩu là bắt buộc." Display="Dynamic" CssClass="text-red-600 text-xs mt-1"></asp:RequiredFieldValidator>
                 <asp:RegularExpressionValidator ID="revPasswordLength" runat="server" ControlToValidate="txtPassword" ErrorMessage="Mật khẩu phải có ít nhất 6 ký tự." ValidationExpression=".{6,}" Display="Dynamic" CssClass="text-red-600 text-xs mt-1"></asp:RegularExpressionValidator>
            </div>

            <%-- Xác nhận mật khẩu --%>
            <div>
                <label for="<%=txtConfirmPassword.ClientID %>" class="block text-sm font-medium text-gray-700 mb-1">Xác nhận mật khẩu <span class="text-red-500">*</span></label>
                <asp:TextBox ID="txtConfirmPassword" runat="server" TextMode="Password" CssClass="w-full px-4 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 transition duration-150 ease-in-out"></asp:TextBox>
                <asp:RequiredFieldValidator ID="rfvConfirmPassword" runat="server" ControlToValidate="txtConfirmPassword" ErrorMessage="Vui lòng xác nhận mật khẩu." Display="Dynamic" CssClass="text-red-600 text-xs mt-1"></asp:RequiredFieldValidator>
                <asp:CompareValidator ID="cvPassword" runat="server" ControlToValidate="txtConfirmPassword" ControlToCompare="txtPassword" ErrorMessage="Mật khẩu không trùng khớp." Display="Dynamic" CssClass="text-red-600 text-xs mt-1"></asp:CompareValidator>
            </div>

             <%-- Đường kẻ phân cách --%>
            <hr class="my-6 border-gray-200" />

            <%-- Họ Tên --%>
            <div>
                <label for="<%=txtTen.ClientID %>" class="block text-sm font-medium text-gray-700 mb-1">Họ Tên</label>
                <asp:TextBox ID="txtTen" runat="server" CssClass="w-full px-4 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 transition duration-150 ease-in-out"></asp:TextBox>
            </div>

            <%-- Điện Thoại --%>
            <div>
                <label for="<%=txtDienThoai.ClientID %>" class="block text-sm font-medium text-gray-700 mb-1">Điện Thoại</label>
                <asp:TextBox ID="txtDienThoai" runat="server" TextMode="Phone" CssClass="w-full px-4 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 transition duration-150 ease-in-out"></asp:TextBox>
                <%-- Có thể thêm RegularExpressionValidator cho số điện thoại Việt Nam nếu cần
                <asp:RegularExpressionValidator ID="revDienThoai" runat="server" ControlToValidate="txtDienThoai" ErrorMessage="Số điện thoại không hợp lệ." ValidationExpression="(0[3|5|7|8|9])+([0-9]{8})\b" Display="Dynamic" CssClass="text-red-600 text-xs mt-1"></asp:RegularExpressionValidator>
                 --%>
            </div>

            <%-- Vai trò --%>
            <div>
                <label for="<%=ddlVaiTro.ClientID %>" class="block text-sm font-medium text-gray-700 mb-1">Vai trò <span class="text-red-500">*</span></label>
                <asp:DropDownList ID="ddlVaiTro" runat="server" CssClass="w-full px-4 py-2 border border-gray-300 rounded-md shadow-sm bg-white focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 transition duration-150 ease-in-out"> <%-- Thêm bg-white để đảm bảo nền trắng --%>
                    <asp:ListItem Text="User" Value="1" Selected="True"></asp:ListItem>
                    <asp:ListItem Text="Admin" Value="0"></asp:ListItem>
                 </asp:DropDownList>
                 <asp:RequiredFieldValidator InitialValue="" ID="rfvVaiTro" runat="server" ControlToValidate="ddlVaiTro" ErrorMessage="Vui lòng chọn vai trò." Display="Dynamic" CssClass="text-red-600 text-xs mt-1"></asp:RequiredFieldValidator>
            </div>

            <%-- Nút bấm --%>
            <div class="mt-8 flex justify-end space-x-4"> <%-- Tăng khoảng cách trên, tăng khoảng cách giữa nút --%>
                <asp:Button ID="btnLuu" runat="server" Text="Lưu Người Dùng" CssClass="px-6 py-2 bg-indigo-600 hover:bg-indigo-700 text-white font-semibold rounded-md shadow-md focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 transition duration-150 ease-in-out" OnClick="btnLuu_Click" /> <%-- Màu indigo, bo tròn vừa, shadow, hiệu ứng focus rõ hơn, transition --%>
                <asp:Button ID="btnHuy" runat="server" Text="Hủy" CssClass="px-6 py-2 bg-gray-200 hover:bg-gray-300 text-gray-800 font-semibold rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-gray-400 transition duration-150 ease-in-out" OnClick="btnHuy_Click" CausesValidation="false" /> <%-- Màu xám nhạt hơn, hover đậm hơn chút, bo tròn vừa, shadow nhỏ, hiệu ứng focus, transition --%>
            </div>
        </div>
    </div>
</asp:Content>