<%-- ThemSach.aspx --%>
<%@ Page Title="Thêm Sách Mới" Language="C#" MasterPageFile="~/WebForm/Admin/Admin.Master" AutoEventWireup="true" CodeBehind="ThemSach.aspx.cs" Inherits="Webebook.WebForm.Admin.ThemSach" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <style>
        .validation-error { color: #dc3545; font-size: 0.875em; margin-top: 0.25rem; display: block; }
        .image-preview-container { margin-top: 0.5rem; min-height: 100px; /* Ensure space even when hidden */ }
        .image-preview {
            width: 68px;
            height: 96px;
            border: 1px solid #ddd;
            background-color: #f8f9fa;
            object-fit: contain; /* Giữ tỷ lệ khung hình */
            display: none; /* Ẩn ban đầu */
            vertical-align: middle;
        }
        .preview-error { color: #dc3545; font-size: 0.875em; display: block; margin-top: 0.25rem; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="container mx-auto px-4 max-w-3xl">
        <h2 class="text-2xl font-semibold text-gray-800 mb-6">Thêm Sách Mới</h2>
        <asp:Label ID="lblMessage" runat="server" CssClass="block mb-4 p-3 rounded-md border" EnableViewState="false" Visible="false"></asp:Label>

        <div class="bg-white p-6 md:p-8 rounded-lg shadow-md">
            <div class="grid grid-cols-1 md:grid-cols-2 gap-x-6 gap-y-4">
                <%-- Cột Trái --%>
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
                            <asp:ListItem Text="Hoàn thành" Value="Hoàn thành" Selected="True"></asp:ListItem>
                            <asp:ListItem Text="Đang cập nhật" Value="Đang cập nhật"></asp:ListItem>
                            <asp:ListItem Text="Tạm dừng" Value="Tạm dừng"></asp:ListItem>
                        </asp:DropDownList>
                    </div>
                </div>

                <%-- Cột Phải --%>
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
                        <%-- **** MODIFIED: Cover Image Upload **** --%>
                        <label for="<%=fuBiaSach.ClientID %>" class="block text-sm font-medium text-gray-700 mb-1">Ảnh Bìa <span class="text-red-500">*</span></label>
                        <asp:FileUpload ID="fuBiaSach" runat="server" 
                            CssClass="w-full text-sm text-gray-500 file:mr-4 file:py-2 file:px-4 file:rounded-md file:border-0 file:text-sm file:font-semibold file:bg-indigo-50 file:text-indigo-700 hover:file:bg-indigo-100 cursor-pointer" 
                            onchange="previewImage(this);" /> <%-- **** ADDED: onchange event for preview **** --%>
                        
                        <%-- **** ADDED: Image Preview Area **** --%>
                        <div class="image-preview-container">
                             <asp:Image ID="imgPreview" runat="server" CssClass="image-preview" AlternateText="Xem trước ảnh bìa" />
                             <asp:Label ID="lblPreviewError" runat="server" CssClass="preview-error" EnableViewState="false"></asp:Label>
                        </div>
                        
                        <%-- Validators --%>
                        <asp:RequiredFieldValidator ID="rfvBiaSach" runat="server" ControlToValidate="fuBiaSach" ErrorMessage="Ảnh bìa là bắt buộc." InitialValue="" Display="Dynamic" CssClass="validation-error"></asp:RequiredFieldValidator>
                        <asp:RegularExpressionValidator ID="revBiaSach" runat="server" ControlToValidate="fuBiaSach" ErrorMessage="Chỉ chấp nhận file ảnh (jpg, jpeg, png, gif, webp, bmp)." Display="Dynamic" ValidationExpression="^.*\.(jpg|JPG|jpeg|JPEG|png|PNG|gif|GIF|bmp|BMP|webp|WEBP)$" CssClass="validation-error"></asp:RegularExpressionValidator>
                       
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
                     <div>
                        <%-- **** NEW: TheLoaiChuoi Textbox **** --%>
                        <label for="<%=txtTheLoaiChuoi.ClientID %>" class="block text-sm font-medium text-gray-700 mb-1">Thể Loại (Cách nhau bởi dấu phẩy)</label>
                        <asp:TextBox ID="txtTheLoaiChuoi" runat="server" CssClass="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500" placeholder="Hành động, Phiêu lưu, Lãng mạn"></asp:TextBox>
                    </div>
                </div>
            </div>

            <%-- Mô Tả --%>
            <div class="mt-6">
                <label for="<%=txtMoTa.ClientID %>" class="block text-sm font-medium text-gray-700 mb-1">Mô Tả</label>
                <asp:TextBox ID="txtMoTa" runat="server" TextMode="MultiLine" Rows="4" CssClass="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500"></asp:TextBox>
            </div>

            <%-- Nút Bấm --%>
            <div class="mt-8 flex justify-end space-x-3 border-t pt-6">
                <asp:Button ID="btnLuu" runat="server" Text="Lưu Sách" CssClass="px-4 py-2 bg-green-600 hover:bg-green-700 text-white font-medium rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-green-500" OnClick="btnLuu_Click" />
                <asp:Button ID="btnHuy" runat="server" Text="Hủy" CssClass="px-4 py-2 bg-gray-200 hover:bg-gray-300 text-gray-800 font-medium rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-gray-400" OnClick="btnHuy_Click" CausesValidation="false" />
            </div>
        </div>
    </div>

    <%-- **** ADDED: JavaScript for Image Preview **** --%>
    <script type="text/javascript">
        function previewImage(fileInput) {
            // Lấy các element cần thiết, sử dụng ClientID để đảm bảo ID đúng phía client
            const preview = document.getElementById('<%= imgPreview.ClientID %>');
            const errorLabel = document.getElementById('<%= lblPreviewError.ClientID %>');

            // Xóa xem trước cũ và thông báo lỗi
            preview.style.display = 'none';
            preview.src = '#'; // Hoặc đặt một ảnh placeholder nếu muốn
            if (errorLabel) errorLabel.textContent = '';

            // Kiểm tra xem người dùng đã chọn file chưa
            if (fileInput.files && fileInput.files[0]) {
                const file = fileInput.files[0];
                const allowedTypes = ['image/jpeg', 'image/png', 'image/gif', 'image/bmp', 'image/webp'];

                // Kiểm tra loại file (phía client để có phản hồi nhanh)
                if (!allowedTypes.includes(file.type)) {
                    if (errorLabel) errorLabel.textContent = 'Lỗi xem trước: Chỉ chấp nhận file ảnh (jpg, png, gif, bmp, webp).';
                    fileInput.value = ''; // Xóa file đã chọn nếu không hợp lệ
                    return; // Dừng lại
                }

                // Kiểm tra kích thước file (ví dụ: giới hạn 5MB phía client)
                const maxSizeMB = 5;
                if (file.size > maxSizeMB * 1024 * 1024) {
                    if (errorLabel) errorLabel.textContent = `Lỗi xem trước: Kích thước file quá lớn (tối đa ${maxSizeMB}MB).`;
                    fileInput.value = ''; // Xóa file đã chọn
                    return; // Dừng lại
                }

                // Sử dụng FileReader để đọc file
                const reader = new FileReader();

                reader.onload = function (e) {
                    // Gán kết quả đọc (dưới dạng Data URL) vào src của thẻ img
                    preview.src = e.target.result;
                    // Hiển thị thẻ img lên
                    preview.style.display = 'inline-block'; // Hoặc 'block' tùy layout
                }
                reader.onerror = function () {
                    if (errorLabel) errorLabel.textContent = 'Lỗi xem trước: Không thể đọc file ảnh.';
                }

                // Bắt đầu đọc file
                reader.readAsDataURL(file);
            }
        }
    </script>
</asp:Content>