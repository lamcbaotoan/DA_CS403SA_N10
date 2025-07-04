<%-- SuaSach.aspx --%>
<%@ Page Title="Sửa Thông Tin Sách" Language="C#" MasterPageFile="~/WebForm/Admin/Admin.Master" AutoEventWireup="true" CodeBehind="SuaSach.aspx.cs" Inherits="Webebook.WebForm.Admin.SuaSach" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .validation-error { color: #dc3545; font-size: 0.875em; margin-top: 0.25rem; display: block; }
        .info-link i { margin-right: 4px; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="container mx-auto px-4 py-6">
        <div class="bg-white p-6 md:p-8 rounded-lg shadow-md mb-8">
            <div class="flex justify-between items-center mb-6 border-b pb-3">
                <h2 class="text-2xl font-semibold text-gray-800">
                    Sửa Thông Tin Sách (ID: <asp:Label ID="lblSachID" runat="server" CssClass="font-bold"></asp:Label>)
                </h2>
                <asp:Button ID="btnManageContent" runat="server" Text="Quản Lý Nội Dung"
                    CssClass="px-4 py-2 bg-purple-600 hover:bg-purple-700 text-white font-medium rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-purple-500"
                    OnClick="btnManageContent_Click" CausesValidation="false" />
            </div>
            <%-- Unified Message Label --%>
            <asp:Label ID="lblMessage" runat="server" CssClass="block mb-4 p-3 rounded-md border" EnableViewState="false" Visible="false"></asp:Label>

            <asp:HiddenField ID="hfSachID" runat="server" />
            <%-- **** REMOVED: hfTheLoai (replaced by LoaiSach/TheLoaiChuoi) **** --%>
            <%-- **** RENAMED: hfCurrentBiaSach to hfCurrentDuongDanBiaSach **** --%>
            <asp:HiddenField ID="hfCurrentDuongDanBiaSach" runat="server" />

            <div class="grid grid-cols-1 md:grid-cols-2 gap-x-6 gap-y-4">
                <div class="space-y-4">
                    <div>
                        <label for="<%=txtTenSach.ClientID %>" class="block text-sm font-medium text-gray-700 mb-1">Tên Sách <span class="text-red-500">*</span></label>
                        <asp:TextBox ID="txtTenSach" runat="server" CssClass="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="rfvTenSach" runat="server" ControlToValidate="txtTenSach" ErrorMessage="Tên sách là bắt buộc." Display="Dynamic" CssClass="validation-error"></asp:RequiredFieldValidator>
                    </div>
                    <div>
                        <label for="<%=txtTacGia.ClientID %>" class="block text-sm font-medium text-gray-700 mb-1">Tác Giả</label>
                        <asp:TextBox ID="txtTacGia" runat="server" CssClass="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"></asp:TextBox>
                    </div>
                    <div>
                        <label for="<%=txtGiaSach.ClientID %>" class="block text-sm font-medium text-gray-700 mb-1">Giá Sách (VNĐ) <span class="text-red-500">*</span></label>
                        <asp:TextBox ID="txtGiaSach" runat="server" CssClass="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500" TextMode="Number" step="1000"></asp:TextBox>
                        <asp:RequiredFieldValidator ID="rfvGiaSach" runat="server" ControlToValidate="txtGiaSach" ErrorMessage="Giá sách là bắt buộc." Display="Dynamic" CssClass="validation-error"></asp:RequiredFieldValidator>
                        <asp:CompareValidator ID="cvGiaSach" runat="server" ControlToValidate="txtGiaSach" Operator="DataTypeCheck" Type="Currency" ErrorMessage="Giá sách phải là số." Display="Dynamic" CssClass="validation-error"></asp:CompareValidator>
                        <asp:CompareValidator ID="cvGiaSachNonNegative" runat="server" ControlToValidate="txtGiaSach" Operator="GreaterThanEqual" Type="Currency" ValueToCompare="0" ErrorMessage="Giá sách không được âm." Display="Dynamic" CssClass="validation-error"></asp:CompareValidator>
                    </div>
                    <div>
                        <label for="<%=ddlTrangThaiNoiDung.ClientID %>" class="block text-sm font-medium text-gray-700 mb-1">Trạng Thái Nội Dung</label>
                        <asp:DropDownList ID="ddlTrangThaiNoiDung" runat="server" CssClass="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 bg-white">
                            <asp:ListItem Text="Hoàn thành" Value="Hoàn thành"></asp:ListItem>
                            <asp:ListItem Text="Đang cập nhật" Value="Đang cập nhật"></asp:ListItem>
                            <asp:ListItem Text="Tạm dừng" Value="Tạm dừng"></asp:ListItem>
                        </asp:DropDownList>
                    </div>
                     <div>
                        <%-- **** NEW: LoaiSach Dropdown **** --%>
                        <label for="<%=ddlLoaiSach.ClientID %>" class="block text-sm font-medium text-gray-700 mb-1">Loại Sách <span class="text-red-500">*</span></label>
                        <asp:DropDownList ID="ddlLoaiSach" runat="server" CssClass="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 bg-white">
                            <asp:ListItem Text="-- Chọn loại sách --" Value=""></asp:ListItem>
                            <asp:ListItem Text="Truyện Tranh" Value="Truyện Tranh"></asp:ListItem>
                            <asp:ListItem Text="Truyện Chữ" Value="Truyện Chữ"></asp:ListItem>
                        </asp:DropDownList>
                         <asp:RequiredFieldValidator ID="rfvLoaiSach" runat="server" ControlToValidate="ddlLoaiSach" ErrorMessage="Loại sách là bắt buộc." InitialValue="" Display="Dynamic" CssClass="validation-error"></asp:RequiredFieldValidator>
                    </div>
                </div>
                <div class="space-y-4">
                    <div>
                        <label for="<%=txtNhaXuatBan.ClientID %>" class="block text-sm font-medium text-gray-700 mb-1">Nhà Xuất Bản</label>
                        <asp:TextBox ID="txtNhaXuatBan" runat="server" CssClass="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"></asp:TextBox>
                    </div>
                    <div>
                        <label for="<%=txtNhomDich.ClientID %>" class="block text-sm font-medium text-gray-700 mb-1">Nhóm Dịch</label>
                        <asp:TextBox ID="txtNhomDich" runat="server" CssClass="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"></asp:TextBox>
                    </div>
                    <div>
                        <%-- **** MODIFIED: Display Current Image from Path **** --%>
                        <label class="block text-sm font-medium text-gray-700 mb-1">Ảnh Bìa Hiện Tại</label>
                        <div class="mb-2 min-h-[6rem] flex items-center justify-start">
                             <asp:Image ID="imgCurrentBiaSach" runat="server" CssClass="max-h-24 w-auto object-contain border rounded bg-gray-50 shadow-sm" Visible="false" AlternateText="Ảnh bìa hiện tại" />
                             <asp:Label ID="lblNoCurrentImage" runat="server" Text="Chưa có ảnh bìa" Visible="true" CssClass="text-sm text-gray-500 italic ml-2"></asp:Label>
                        </div>
                        <%-- **** MODIFIED: Upload New Image (Optional on Edit) **** --%>
                        <label for="<%=fuBiaSach.ClientID %>" class="block text-sm font-medium text-gray-700 mb-1">Tải Ảnh Bìa Mới (Để trống nếu không đổi)</label>
                        <asp:FileUpload ID="fuBiaSach" runat="server" CssClass="w-full text-sm text-gray-500 file:mr-4 file:py-2 file:px-4 file:rounded-md file:border-0 file:text-sm file:font-semibold file:bg-indigo-50 file:text-indigo-700 hover:file:bg-indigo-100 cursor-pointer" />
                        <asp:RegularExpressionValidator ID="revBiaSach" runat="server" ControlToValidate="fuBiaSach" ErrorMessage="Chỉ chấp nhận file ảnh (jpg, jpeg, png, gif, webp, bmp)." Display="Dynamic" ValidationExpression="^.*\.(jpg|JPG|jpeg|JPEG|png|PNG|gif|GIF|bmp|BMP|webp|WEBP)$" CssClass="validation-error"></asp:RegularExpressionValidator>
                        <%-- Optional: Add server-side size validation if needed --%>
                    </div>
                    <div>
                         <%-- **** NEW: TheLoaiChuoi Textbox **** --%>
                        <label for="<%=txtTheLoaiChuoi.ClientID %>" class="block text-sm font-medium text-gray-700 mb-1">Thể Loại (Cách nhau bởi dấu phẩy)</label>
                        <asp:TextBox ID="txtTheLoaiChuoi" runat="server" CssClass="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500" placeholder="Hành động, Phiêu lưu, Lãng mạn"></asp:TextBox>
                    </div>
                </div>
            </div>
            <div class="mt-6">
                <label for="<%=txtMoTa.ClientID %>" class="block text-sm font-medium text-gray-700 mb-1">Mô Tả</label>
                <asp:TextBox ID="txtMoTa" runat="server" TextMode="MultiLine" Rows="5" CssClass="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"></asp:TextBox>
            </div>
            <div class="mt-8 flex justify-end space-x-3 border-t pt-6">
                <asp:Button ID="btnLuuThongTin" runat="server" Text="Lưu Thông Tin Sách" CssClass="px-4 py-2 bg-blue-600 hover:bg-blue-700 text-white font-medium rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-blue-500" OnClick="btnLuuThongTin_Click" />
                <asp:Button ID="btnHuy" runat="server" Text="Quay Lại Danh Sách" CssClass="px-4 py-2 bg-gray-200 hover:bg-gray-300 text-gray-800 font-medium rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-gray-400" OnClick="btnHuy_Click" CausesValidation="false" />
            </div>
        </div>
    </div>
</asp:Content>

