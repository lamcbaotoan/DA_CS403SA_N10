<%@ Page Title="Sửa Nội Dung Chương" Language="C#" MasterPageFile="~/WebForm/Admin/Admin.Master" AutoEventWireup="true" CodeBehind="SuaNoiDungChuong.aspx.cs" Inherits="Webebook.WebForm.Admin.SuaNoiDungChuong" %>
<%@ OutputCache Duration="1" VaryByParam="none" Location="None" NoStore="true" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <%-- Dropzone CSS --%>
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/dropzone/5.9.3/min/dropzone.min.css" />
    <%-- SortableJS --%>
    <script src="https://cdnjs.cloudflare.com/ajax/libs/Sortable/1.15.0/Sortable.min.js"></script>
    <%-- Dropzone JS (defer) --%>
    <script src="https://cdnjs.cloudflare.com/ajax/libs/dropzone/5.9.3/min/dropzone.min.js" defer></script>
    <%-- Font Awesome for Icons --%>
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.0.0/css/all.min.css" /> <%-- Added Font Awesome --%>

    <style>
        /* --- CSS Cơ bản --- */
        .message-panel { @apply mb-4 p-3 rounded-lg flex items-center text-sm; }
        .message-success { @apply bg-green-100 border border-green-400 text-green-700; }
        .message-error { @apply bg-red-100 border border-red-400 text-red-700; }
        .message-panel i { @apply mr-2; }
        .validation-error { @apply text-red-600 text-xs mt-1 block; }
        .form-label { @apply block text-sm font-medium text-gray-700 mb-1; }
        .form-control { @apply w-full p-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500 focus:border-blue-500 transition-colors duration-150; }
        .form-control[disabled], .form-control[readonly] { @apply bg-gray-100 cursor-not-allowed opacity-70; }
        textarea.form-control { @apply min-h-[400px] /* Cao hơn */ resize-vertical font-mono text-sm; }
        .file-input-styled { @apply w-full p-2 border border-gray-300 rounded-md file:mr-4 file:py-2 file:px-4 file:rounded-md file:border-0 file:bg-indigo-500 file:text-white file:hover:bg-indigo-600 cursor-pointer text-sm text-gray-500; }
        .btn-action { @apply inline-flex items-center justify-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-offset-2 disabled:opacity-50 disabled:cursor-not-allowed transition-colors duration-150 ease-in-out; }
        /* --- Cập nhật màu nút --- */
        .btn-primary { @apply text-white bg-blue-600 hover:bg-blue-700 focus:ring-blue-500; } /* Lưu Nội Dung */
        .btn-secondary { @apply text-white bg-gray-500 hover:bg-gray-600 focus:ring-gray-500; } /* Hủy, Quay lại */
        .btn-success { @apply text-white bg-green-600 hover:bg-green-700 focus:ring-green-500; } /* Lưu Tên */
        .btn-sm { @apply px-3 py-1.5 text-xs; } /* Nút nhỏ */
        .loading-spinner::after { content: ''; @apply inline-block w-4 h-4 border-2 border-t-transparent border-white rounded-full animate-spin ml-2; }

        /* --- CSS Dropzone và Edit Comic --- */
        #comicUploaderWrapper { @apply mb-4; }
        #comicUploader.dropzone { @apply border-2 border-dashed border-blue-400 rounded-lg p-4 text-center bg-blue-50 hover:bg-blue-100 transition-colors cursor-pointer min-h-[80px] flex items-center justify-center; }
        #comicUploader.dropzone .dz-message { @apply text-blue-700 font-medium text-sm; }
        #comicUploader.dropzone .dz-message i { @apply mr-2; }

        #editComicImagesContainer { @apply mt-1 grid grid-cols-3 sm:grid-cols-4 md:grid-cols-5 lg:grid-cols-6 gap-3 /* Tăng Gap */ max-h-[60vh] overflow-y-auto border border-gray-200 p-3 rounded-lg min-h-[150px] bg-white shadow-inner; } /* Border thường */
        #editComicImagesContainer:empty::before { content: 'Chưa có ảnh nào.'; @apply text-sm text-gray-500 italic block text-center py-10 w-full; }

        /* Item ảnh */
        .edit-comic-image-item { @apply flex flex-col items-center p-2 border bg-white rounded-lg shadow hover:shadow-md cursor-grab w-full; position: relative; transition: box-shadow 0.2s ease; aspect-ratio: 3 / 5; } /* Tăng chiều cao để chứa nút rõ hơn */
        .edit-comic-image-item .edit-image-thumb { @apply w-full h-full flex-1 rounded-t border-b bg-gray-100 flex items-center justify-center overflow-hidden; } /* Ảnh chiếm phần lớn */
        .edit-comic-image-item .edit-image-thumb img { @apply object-contain max-w-full max-h-full; }
        .edit-comic-image-item .file-info-actions { @apply w-full p-1.5 bg-gray-50 rounded-b; } /* Phần chứa tên và nút */
        .edit-comic-image-item .file-name-display { @apply text-[10px] leading-tight text-center text-gray-600 break-all h-6 overflow-hidden w-full; } /* 2 dòng tên file */
        .item-status-new .file-name-display::before { content: '\f058'; /* Icon check */ font-family: 'Font Awesome 6 Free'; @apply font-black text-green-500 mr-1 text-xs; }
        .item-status-replaced .file-name-display::before { content: '\f044'; /* Icon edit */ font-family: 'Font Awesome 6 Free'; @apply font-black text-yellow-500 mr-1 text-xs; }
        .edit-comic-image-item .image-actions { @apply flex justify-center space-x-4 w-full pt-1; } /* Nút luôn hiện */
        .edit-comic-image-item .action-btn { @apply p-0.5 text-gray-500 hover:text-blue-600 cursor-pointer text-base border-0 bg-transparent transition-colors; }
        .edit-comic-image-item .delete-btn { @apply hover:text-red-600 text-red-500; }

        .sortable-ghost { @apply opacity-40 bg-blue-100 border-blue-300; }
        .sortable-chosen { @apply shadow-lg ring-2 ring-offset-1 ring-blue-500; }
        .hidden { display: none !important; }

         /* Style cho khu vực text truyện chữ */
         #pnlNovelContent .novel-content-wrapper { @apply border rounded-md p-5 bg-gray-50 space-y-5; } /* Tăng padding, spacing */
         #pnlExistingNovelFile { @apply p-3 bg-white border border-gray-300 rounded shadow-sm; }
         #pnlExistingNovelFile .existing-file-link { @apply text-blue-600 hover:text-blue-800 hover:underline text-sm font-mono flex-1 truncate; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="container mx-auto px-4 py-6 max-w-4xl">
        <div class="bg-white p-4 rounded-lg shadow mb-6 border border-gray-100">
             <h2 class="text-xl font-semibold text-gray-800 mb-2"><asp:Label ID="lblPageModeTitle" runat="server" Text="Sửa Nội Dung Chương"></asp:Label></h2>
             <p class="text-sm text-gray-600"><strong>Sách:</strong> <asp:Label ID="lblBookTitleContext" runat="server" Text="[Sách]" CssClass="font-medium text-gray-800"></asp:Label> (ID: <asp:Label ID="lblSachIDContext" runat="server" CssClass="font-mono"></asp:Label>) - <strong>Loại:</strong> <asp:Label ID="lblLoaiSachContext" runat="server" Text="[Loại]" CssClass="font-medium text-gray-800"></asp:Label></p>
             <asp:HyperLink ID="hlBackToList" runat="server" CssClass="mt-2 inline-block text-sm text-blue-600 hover:text-blue-800 hover:underline transition-colors"><i class="fas fa-arrow-left mr-1"></i> Quay lại Danh sách chương</asp:HyperLink>
        </div>

        <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="message-panel" EnableViewState="false"><asp:Label ID="lblFormMessage" runat="server"></asp:Label></asp:Panel>
        <asp:ValidationSummary ID="vsChapterForm" runat="server" CssClass="bg-red-50 border border-red-200 p-3 rounded-md mb-4 text-sm text-red-700" HeaderText="Vui lòng sửa các lỗi sau:" ValidationGroup="ChapterValidation" DisplayMode="BulletList" ShowSummary="true" style="display: none;" />

        <div class="bg-white p-6 rounded-lg shadow-md">
            <%-- Hidden fields --%>
            <asp:HiddenField ID="hfSachID" runat="server" />
            <asp:HiddenField ID="hfIDNoiDung" runat="server" />
            <asp:HiddenField ID="hfLoaiSach" runat="server" />
            <asp:HiddenField ID="hfCurrentDuongDan" runat="server" />
            <asp:HiddenField ID="hfComicImageOrder" runat="server" />
            <asp:HiddenField ID="hfComicImagesToDelete" runat="server" />
            <asp:HiddenField ID="hfComicNewFiles" runat="server" />
            <asp:HiddenField ID="hfComicImagesToReplace" runat="server" />
            <asp:HiddenField ID="hfOriginalNovelText" runat="server" />
            <asp:HiddenField ID="hfOriginalTenChuong" runat="server" />

            <div class="mb-6 pb-6 border-b border-gray-200">
                 <h4 class="text-lg font-semibold text-gray-800 mb-4">Thông Tin Cơ Bản</h4> <%-- Tăng mb --%>
                 <div class="grid grid-cols-1 md:grid-cols-3 gap-4 items-baseline">
                     <div>
                         <label class="form-label">Số Chương</label>
                         <asp:TextBox ID="txtSoChuong" runat="server" TextMode="Number" CssClass="form-control" ReadOnly="true" Enabled="false"></asp:TextBox>
                     </div>
                     <div class="md:col-span-2">
                         <label for="<%=txtTenChuong.ClientID %>" class="form-label">Tên Chương (Tùy chọn)</label>
                         <div class="flex items-center gap-2">
                             <asp:TextBox ID="txtTenChuong" runat="server" CssClass="form-control flex-grow" MaxLength="255"></asp:TextBox>
                             <asp:Button ID="btnSaveTenChuong" runat="server" Text="Lưu Tên" ToolTip="Chỉ lưu lại tên chương" CssClass="btn-action btn-success btn-sm flex-shrink-0" OnClick="btnSaveTenChuong_Click" CausesValidation="false" />
                         </div>
                     </div>
                 </div>
            </div>

            <%-- ==================== Panel Truyện Chữ ==================== --%>
            <asp:Panel ID="pnlNovelContent" runat="server" Visible="false">
                 <h4 class="text-lg font-semibold text-gray-800 mb-4">Cập Nhật Nội Dung Truyện Chữ</h4>
                 <div class="novel-content-wrapper space-y-5">
                     <%-- Panel hiển thị file cũ --%>
                     <asp:Panel ID="pnlExistingNovelFile" runat="server" Visible="false" CssClass="p-3 bg-white border border-gray-300 rounded shadow-sm">
                        <p class="text-sm font-medium text-gray-700 mb-1">File nội dung hiện tại:</p>
                        <div class="flex items-center">
                            <i class="fas fa-file-alt text-gray-500 mr-2 text-lg"></i>
                            <%-- **** ĐÂY LÀ DÒNG ĐÃ ĐƯỢC THÊM VÀO **** --%>
                            <asp:HyperLink ID="hlCurrentNovelFile" runat="server" Target="_blank" CssClass="existing-file-link text-blue-600 hover:text-blue-800 hover:underline text-sm font-mono flex-1 truncate"></asp:HyperLink>
                            <%-- ****************************************** --%>
                        </div>
                        <p class="text-xs text-gray-500 mt-1.5">Tải file mới hoặc sửa nội dung bên dưới sẽ thay thế file này.</p>
                     </asp:Panel>

                     <%-- Các control upload và textbox khác --%>
                     <div>
                         <label for="<%=fuFileTieuThuyet.ClientID %>" class="form-label">Tùy chọn 1: Tải File Mới Thay Thế (Word/PDF/TXT)</label> <%-- Cập nhật label cho rõ ràng hơn --%>
                         <asp:FileUpload ID="fuFileTieuThuyet" runat="server" CssClass="file-input-styled" />
                         <asp:RegularExpressionValidator ID="revFileTieuThuyet" runat="server" ControlToValidate="fuFileTieuThuyet"
                            ErrorMessage="Chỉ chấp nhận file .doc, .docx, .pdf, .txt"
                            ValidationExpression="^.*\.(doc|DOC|docx|DOCX|pdf|PDF|txt|TXT)$" Display="Dynamic"
                            CssClass="validation-error" ValidationGroup="ChapterValidation" Enabled="false"></asp:RegularExpressionValidator>
                     </div>
                     <div>
                         <label for="<%=txtNoiDungChu.ClientID %>" class="form-label">Tùy chọn 2: Nhập/Sửa Nội Dung Trực Tiếp</label>
                         <%-- Tăng Rows và min-height --%>
                         <asp:Label ID="lblNovelFileReadError" runat="server" CssClass="validation-error" Visible="false" EnableViewState="false"></asp:Label>
                         <asp:TextBox ID="txtNoiDungChu" runat="server" CssClass="form-control min-h-[500px]" Rows="30" TextMode="MultiLine" Width="852px"></asp:TextBox>
                     </div>
                     <asp:CustomValidator ID="cvNovelContentRequired" runat="server"
                        ErrorMessage="Không thể xóa hết nội dung. Phải có nội dung chữ hoặc tải file mới."
                        Display="Dynamic" CssClass="validation-error" ValidationGroup="ChapterValidation"
                        OnServerValidate="cvNovelContentRequired_ServerValidate" ClientValidationFunction="validateNovelContent_Client_Edit" Enabled="false"/>
                 </div>
            </asp:Panel>

            <%-- ==================== Panel Truyện Tranh ==================== --%>
            <asp:Panel ID="pnlComicContent" runat="server" Visible="false">
                <h4 class="text-lg font-semibold text-gray-800 mb-4">Cập Nhật Nội Dung Truyện Tranh</h4>
                <div class="mb-4 p-4 border rounded-md bg-gray-50">
                    <label class="form-label mb-2 text-sm">Thêm ảnh mới vào cuối danh sách:</label>
                    <div id="comicUploaderWrapper">
                        <%-- Dropzone làm nút chính --%>
                        <div id="comicUploader" class="dropzone"></div>
                    </div>
                </div>
                <label class="form-label mt-4">Ảnh Hiện Tại & Ảnh Mới (Kéo thả sắp xếp, <i class="fas fa-sync-alt text-blue-600"></i> thay thế, <i class="fas fa-trash text-red-600"></i> xóa):</label>
                <div id="editComicImagesContainer" class="dropzone-previews"></div> <%-- Preview container --%>
                <asp:CustomValidator ID="cvAnhTruyenRequired" runat="server" ErrorMessage="Phải còn lại ít nhất một ảnh." CssClass="validation-error" Display="Dynamic" ValidationGroup="ChapterValidation" ClientValidationFunction="validateEditComicImageRequired" OnServerValidate="cvAnhTruyenRequired_ServerValidate" Enabled="false"/>
                <input type="file" id="fuEditComicImageReplace" class="hidden" accept="image/*" onchange="handleComicImageReplaceUpload(event)">
            </asp:Panel>

            <%-- ==================== Nút Actions ==================== --%>
            <div class="mt-8 flex flex-col sm:flex-row sm:justify-between items-center border-t border-gray-200 pt-6 gap-4">
                 <asp:Button ID="btnCancel" runat="server" Text="Hủy Bỏ" CssClass="btn-action btn-secondary w-full sm:w-auto order-3 sm:order-1" OnClick="btnCancel_Click" CausesValidation="false" />
                 <div class="flex flex-col sm:flex-row gap-2 w-full sm:w-auto order-1 sm:order-2">
                    <%-- Nút này trùng chức năng với nút trên phần Tên Chương, có thể xem xét bỏ đi --%>
                    <asp:Button ID="Button1" runat="server" Text="Lưu Chỉ Tên Chương" ToolTip="Chỉ lưu lại tên chương đã sửa" CssClass="btn-action btn-success btn-sm w-full sm:w-auto" OnClick="btnSaveTenChuong_Click" CausesValidation="false" />
                    <asp:Button ID="btnSaveNoiDung" runat="server" Text="Lưu Thay Đổi Nội Dung" ToolTip="Lưu tất cả thay đổi về nội dung (ảnh hoặc text)" CssClass="btn-action btn-primary w-full sm:w-auto" OnClick="btnSaveNoiDung_Click" ValidationGroup="ChapterValidation" OnClientClick="return prepareSubmitData_Edit();" />
                 </div>
            </div>
        </div>
    </div>

    <%-- ==================== JavaScript ==================== --%>
    <script type="text/javascript">
        // --- Configuration ---
        const comicUploadPanel = document.getElementById('<%= pnlComicContent.ClientID %>');
        const imageOrderHiddenField = document.getElementById('<%= hfComicImageOrder.ClientID %>');
        const imagesToDeleteHiddenField = document.getElementById('<%= hfComicImagesToDelete.ClientID %>');
        const imagesToReplaceHiddenField = document.getElementById('<%= hfComicImagesToReplace.ClientID %>');
        const newFilesHiddenField = document.getElementById('<%= hfComicNewFiles.ClientID %>');
        const dropzoneElementSelector = '#comicUploader';
        const editPreviewContainer = document.getElementById('editComicImagesContainer');
        const replaceFileInput = document.getElementById('fuEditComicImageReplace');
        const uploadHandlerUrl = '<%= ResolveUrl("~/Handlers/UploadHandler.ashx") %>';
        const maxFilesizeMB = <%= MaxFileSizePerImageMb %>; // Lấy từ code-behind
        const allowedImageExtensions = ('<%= string.Join(",", AllowedImageExtensions) %>').split('.'); // Sửa lại cách split
        const combinedAcceptedFiles = allowedImageExtensions.map(ext => "." + ext + ",image/" + ext).join(','); // Format cho dropzone
        const maxFileSizeClient = maxFilesizeMB * 1024 * 1024; // Kích thước tối đa tính bằng bytes cho client-side validation

        // --- State ---
        let currentComicImagesState = [];
        let myDropzoneInstanceEdit = null;
        let sortableInstanceEdit = null;
        let replaceTargetInfo = { itemId: null, element: null };

        // --- Initialization ---
        document.addEventListener('DOMContentLoaded', () => {
            const isComicPanelVisible = comicUploadPanel && comicUploadPanel.offsetParent !== null;
            if (isComicPanelVisible) {
                initializeNewFileDropzone();
                initializeEditSortable(); // Gọi Sortable sau khi có thể đã có ảnh ban đầu
            }
            // Ẩn Validation Summary ban đầu
            const validationSummary = document.getElementById('<%= vsChapterForm.ClientID %>');
            if (validationSummary) validationSummary.style.display = 'none';

            // Gọi hàm khởi tạo dữ liệu ảnh ban đầu (nếu có)
            // Chú ý: Cần gọi initializeComicEditor từ code-behind sau khi load dữ liệu
            // Ví dụ: ScriptManager.RegisterStartupScript(this, GetType(), "initComic", $"initializeComicEditor('{jsonString}');", true);
        });

        // --- Dropzone Setup (Chỉ để THÊM file mới) ---
        function initializeNewFileDropzone() {
            if (myDropzoneInstanceEdit) return; // Đã khởi tạo rồi
            Dropzone.autoDiscover = false;
            const dzElement = document.querySelector(dropzoneElementSelector);
            if (!dzElement) {
                console.error('Dropzone element not found for adding new files.');
                return;
            }
            try {
                myDropzoneInstanceEdit = new Dropzone(dzElement, {
                    url: uploadHandlerUrl,
                    paramName: "file",
                    maxFilesize: maxFilesizeMB, // MB
                    acceptedFiles: combinedAcceptedFiles,
                    clickable: true,
                    createImageThumbnails: false, // Không tạo thumbnail tự động trong khu vực này
                    previewsContainer: false, // Không dùng preview của dropzone ở đây
                    dictDefaultMessage: `<i class="fas fa-cloud-upload-alt mr-2"></i> Kéo thả hoặc nhấp vào đây để thêm ảnh mới`,
                    init: function () {
                        const dz = this;
                        this.on("success", function (file, response) {
                            // console.log("Upload success:", file, response);
                            // Phải có response hợp lệ và có fileName được trả về từ handler
                            if (response && response.fileName && !response.error && file.upload && file.upload.uuid) {
                                const newId = `new_${file.upload.uuid}`; // ID duy nhất cho file mới
                                let blobUrl = null;
                                try {
                                    blobUrl = URL.createObjectURL(file); // Tạo URL tạm để hiển thị ảnh
                                } catch (e) { console.warn("Cannot create blob URL for new file", e); }

                                const newItem = {
                                    id: newId,
                                    originalPath: null, // File mới chưa có path gốc
                                    url: blobUrl, // Dùng blob URL để preview
                                    isExisting: false,
                                    tempFileName: response.fileName, // Tên file tạm trên server
                                    displayName: file.name, // Tên gốc của file
                                    newFileBlobUrl: blobUrl // Lưu lại để revoke sau
                                };
                                currentComicImagesState.push(newItem); // Thêm vào cuối danh sách state
                                renderComicEditPreview(); // Vẽ lại toàn bộ preview
                                dz.removeFile(file); // Xóa file khỏi giao diện Dropzone gốc (nếu có)
                            } else {
                                const msg = (response && response.error) ? response.error : "Lỗi không xác định từ server.";
                                alert(`Lỗi tải ảnh mới '${file.name}': ${msg}`);
                                dz.removeFile(file);
                            }
                        });
                        this.on("error", function (file, errorMessage) {
                            // console.error("Upload error:", file, errorMessage);
                            const msg = (typeof errorMessage === "string") ? errorMessage : ((errorMessage && errorMessage.error) ? errorMessage.error : "Lỗi không xác định.");
                            alert(`Lỗi tải ảnh mới '${file.name}': ${msg}`);
                            dz.removeFile(file);
                        });
                    }
                });
                console.log('Dropzone for adding new files initialized.');
            } catch (e) {
                console.error("Error initializing new file Dropzone:", e);
                alert("Không thể khởi tạo khu vực tải ảnh mới. Vui lòng tải lại trang.");
            }
        }

        // --- Sortable Setup ---
        function initializeEditSortable() {
            if (!editPreviewContainer || typeof Sortable === 'undefined') {
                console.warn("Edit preview container or SortableJS not found.");
                return;
            }
            // Hủy instance cũ nếu có để tránh lỗi
            if (sortableInstanceEdit) {
                try { sortableInstanceEdit.destroy(); } catch (e) { console.warn("Could not destroy previous sortable instance", e); }
                sortableInstanceEdit = null;
            }
            // Chỉ khởi tạo Sortable nếu có ít nhất 1 ảnh
            if (currentComicImagesState.length >= 1) {
                try {
                    sortableInstanceEdit = Sortable.create(editPreviewContainer, {
                        animation: 150, // ms, animation speed moving items when sorting, `0` — without animation
                        draggable: ".edit-comic-image-item", // Specifies which items inside the element should be draggable
                        ghostClass: 'sortable-ghost', // Class name for the drop placeholder
                        chosenClass: 'sortable-chosen', // Class name for the chosen item
                        // handle: '.drag-handle', // Nếu muốn dùng handle riêng để kéo
                        onEnd: (evt) => {
                            // Cập nhật lại thứ tự trong state internal dựa trên DOM
                            updateStateOrderFromDOM();
                        }
                    });
                    // console.log("Sortable initialized for edit container.");
                } catch (e) {
                    console.error("Failed to initialize edit Sortable:", e);
                }
            } else {
                // console.log("No items to sort, Sortable not initialized.");
            }
        }

        // --- Edit Logic ---
        // Hàm này được gọi từ code-behind với dữ liệu ảnh hiện có
        function initializeComicEditor(existingImagesJson) {
            // console.log("Initializing comic editor with data:", existingImagesJson);
            currentComicImagesState = []; // Reset state
            resetHiddenFields(); // Reset hidden fields

            if (existingImagesJson && typeof existingImagesJson === 'string' && existingImagesJson !== "null" && existingImagesJson !== "[]") {
                try {
                    const existingImages = JSON.parse(existingImagesJson);
                    if (Array.isArray(existingImages)) {
                        existingImages.forEach((imgData, index) => {
                            if (!imgData || !imgData.path || !imgData.url) {
                                console.warn("Skipping invalid initial image data:", imgData);
                                return;
                            }
                            currentComicImagesState.push({
                                id: `existing_${index}_${Math.random().toString(36).substr(2, 5)}`, // ID duy nhất tạm thời
                                originalPath: imgData.path,
                                url: imgData.url, // URL để hiển thị
                                isExisting: true,
                                tempFileName: null, // Chưa có file thay thế
                                displayName: imgData.name || imgData.path.split('/').pop(), // Tên hiển thị
                                newFileBlobUrl: null // Chưa có blob URL
                            });
                        });
                    }
                } catch (e) {
                    console.error("Error parsing existing image JSON:", e);
                    alert("Lỗi đọc dữ liệu ảnh hiện có.");
                }
            }
            renderComicEditPreview(); // Render lại giao diện dựa trên state đã được khởi tạo
        }

        // Render lại toàn bộ danh sách ảnh trong khu vực edit
        function renderComicEditPreview() {
            if (!editPreviewContainer) return;
            editPreviewContainer.innerHTML = ''; // Xóa hết nội dung cũ

            if (currentComicImagesState.length === 0) {
                editPreviewContainer.classList.remove('has-images'); // Thêm class để style khi rỗng (nếu cần)
                // Hiển thị thông báo rỗng bằng CSS :empty::before
            } else {
                editPreviewContainer.classList.add('has-images');
                currentComicImagesState.forEach(item => {
                    const el = createComicEditItemElement(item);
                    if (el) editPreviewContainer.appendChild(el);
                });
            }
            initializeEditSortable(); // Khởi tạo lại Sortable sau khi render
            updateHiddenFieldsAndValidate(); // Cập nhật hidden fields và validate
        }

        // Tạo HTML cho một item ảnh
        function createComicEditItemElement(item) {
            const div = document.createElement('div');
            div.className = 'edit-comic-image-item';
            div.setAttribute('data-id', item.id);
            if (item.originalPath) div.setAttribute('data-original-path', item.originalPath);
            if (item.tempFileName) div.setAttribute('data-temp-filename', item.tempFileName);

            // Thêm class trạng thái
            let statusClass = '';
            if (!item.isExisting) {
                statusClass = 'item-status-new'; // Ảnh mới thêm
            } else if (item.tempFileName) {
                statusClass = 'item-status-replaced'; // Ảnh cũ bị thay thế
            }
            if (statusClass) div.classList.add(statusClass);

            // Thumbnail container
            const thumbDiv = document.createElement('div');
            thumbDiv.className = 'edit-image-thumb';

            const img = document.createElement('img');
            img.alt = `Ảnh ${item.displayName || '[Ảnh không tên]'}`;
            let imageUrl = item.newFileBlobUrl || item.url || ''; // Ưu tiên blob URL nếu có (ảnh mới/thay thế)
            img.src = imageUrl;
            // Xử lý lỗi load ảnh
            img.onerror = () => {
                img.alt = 'Lỗi tải ảnh';
                thumbDiv.innerHTML = '<i class="fas fa-image text-gray-300 text-2xl"></i>'; // Icon placeholder
                div.classList.add('status-error'); // Thêm class lỗi nếu cần style riêng
            };
            thumbDiv.appendChild(img);

            // Info & Actions container
            const infoActionsDiv = document.createElement('div');
            infoActionsDiv.className = 'file-info-actions';

            // File name display
            const nameDiv = document.createElement('div');
            nameDiv.className = 'file-name-display';
            nameDiv.textContent = item.displayName || (item.originalPath ? item.originalPath.split('/').pop() : '[Ảnh không tên]');
            nameDiv.title = nameDiv.textContent; // Tooltip cho tên dài
            infoActionsDiv.appendChild(nameDiv);

            // Actions (Replace, Delete)
            const actionsDiv = document.createElement('div');
            actionsDiv.className = 'image-actions';

            // Replace Button
            const replaceBtn = document.createElement('button');
            replaceBtn.type = 'button';
            replaceBtn.className = 'action-btn replace-btn';
            replaceBtn.title = 'Thay thế ảnh này';
            replaceBtn.innerHTML = '<i class="fas fa-sync-alt fa-fw"></i>';
            replaceBtn.onclick = (e) => {
                e.stopPropagation(); // Ngăn sự kiện click lan ra item (nếu có)
                triggerComicImageReplace(div, item.id);
            };
            actionsDiv.appendChild(replaceBtn);

            // Delete Button
            const deleteBtn = document.createElement('button');
            deleteBtn.type = 'button';
            deleteBtn.className = 'action-btn delete-btn';
            deleteBtn.title = 'Xóa ảnh này';
            deleteBtn.innerHTML = '<i class="fas fa-trash fa-fw"></i>';
            deleteBtn.onclick = (e) => {
                e.stopPropagation();
                deleteComicEditImage(item.id);
            };
            actionsDiv.appendChild(deleteBtn);
            infoActionsDiv.appendChild(actionsDiv);

            // Append parts to the main div
            div.appendChild(thumbDiv);
            div.appendChild(infoActionsDiv);

            return div;
        }

        // Kích hoạt input file ẩn để chọn file thay thế
        function triggerComicImageReplace(element, itemId) {
            // console.log(`Triggering replace for item ID: ${itemId}`);
            replaceTargetInfo = { itemId: itemId, element: element }; // Lưu thông tin item cần thay thế
            replaceFileInput.value = null; // Reset input để có thể chọn lại cùng file
            replaceFileInput.click();
        }

        // Xử lý sau khi người dùng chọn file thay thế
        function handleComicImageReplaceUpload(event) {
            if (!replaceTargetInfo || !replaceTargetInfo.itemId || !event.target.files || event.target.files.length === 0) {
                resetReplacementState();
                return;
            }

            const file = event.target.files[0];
            const targetItemId = replaceTargetInfo.itemId;
            // console.log(`Replacing item ${targetItemId} with file:`, file.name);

            // === Client-side Validation ===
            const allowedTypes = ['image/jpeg', 'image/png', 'image/gif', 'image/webp']; // Cần khớp với server
            if (!allowedTypes.includes(file.type)) {
                alert(`Lỗi: Định dạng file thay thế không hợp lệ (${file.type}). Chỉ chấp nhận JPG, PNG, GIF, WEBP.`);
                resetReplacementState();
                return;
            }
            if (file.size > maxFileSizeClient) {
                alert(`Lỗi: File thay thế '${file.name}' quá lớn (tối đa ${formatFileSize(maxFileSizeClient)}).`);
                resetReplacementState();
                return;
            }
            // === End Client-side Validation ===

            const itemIndex = currentComicImagesState.findIndex(item => item.id === targetItemId);
            if (itemIndex > -1) {
                const itemToUpdate = currentComicImagesState[itemIndex];
                const formData = new FormData();
                formData.append("file", file); // "file" phải khớp với paramName của handler

                // Hiển thị trạng thái đang tải (tùy chọn)
                if (replaceTargetInfo.element) replaceTargetInfo.element.style.opacity = '0.5';

                // Gọi Upload Handler để tải file thay thế lên và lấy tên tạm
                fetch(uploadHandlerUrl, {
                    method: 'POST',
                    body: formData
                })
                    .then(response => {
                        if (!response.ok) {
                            // Cố gắng đọc lỗi JSON từ server nếu có
                            return response.json().then(err => { throw new Error(err.error || `Lỗi server: ${response.status}`); });
                        }
                        return response.json(); // Parse JSON response
                    })
                    .then(data => {
                        // console.log("Replace upload response:", data);
                        if (data && data.fileName && !data.error) {
                            // --- Cập nhật state của item ---
                            // 1. Giải phóng blob URL cũ nếu có (của lần thay thế trước đó hoặc file mới)
                            if (itemToUpdate.newFileBlobUrl) {
                                try { URL.revokeObjectURL(itemToUpdate.newFileBlobUrl); } catch (e) { console.warn("Could not revoke old blob URL", e); }
                            }
                            // 2. Lưu tên file tạm mới từ server
                            itemToUpdate.tempFileName = data.fileName;
                            // 3. Cập nhật tên hiển thị
                            itemToUpdate.displayName = file.name;
                            // 4. Nếu item này là ảnh gốc (isExisting=true) và đã bị đánh dấu xóa trước đó,
                            //    thì bây giờ nó được thay thế -> hủy việc xóa path gốc.
                            if (itemToUpdate.isExisting && itemToUpdate.originalPath) {
                                removePathFromDeleteList(itemToUpdate.originalPath);
                            }
                            // 5. Tạo blob URL mới cho ảnh vừa tải lên để preview
                            try {
                                itemToUpdate.newFileBlobUrl = URL.createObjectURL(file);
                            } catch (e) {
                                console.error("Cannot create blob URL for replacement file", e);
                                itemToUpdate.newFileBlobUrl = null; // Không có preview nếu lỗi
                            }

                            // Render lại giao diện để hiển thị ảnh mới và trạng thái 'replaced'
                            renderComicEditPreview();
                        } else {
                            // Lỗi từ handler
                            const errorMsg = data.error || "Lỗi không xác định khi tải file thay thế.";
                            alert(`Lỗi tải file thay thế '${file.name}': ${errorMsg}`);
                            if (replaceTargetInfo.element) replaceTargetInfo.element.style.opacity = '1'; // Khôi phục opacity
                        }
                        resetReplacementState(); // Reset trạng thái thay thế
                    })
                    .catch(error => {
                        console.error("Fetch error during replacement upload:", error);
                        alert(`Lỗi mạng hoặc lỗi server khi tải file thay thế '${file.name}': ${error.message}`);
                        if (replaceTargetInfo.element) replaceTargetInfo.element.style.opacity = '1'; // Khôi phục opacity
                        resetReplacementState();
                    });
            } else {
                alert("Lỗi: Không tìm thấy ảnh cần thay thế trong danh sách.");
                resetReplacementState();
            }
        }

        // Xóa một item ảnh khỏi danh sách
        function deleteComicEditImage(itemId) {
            if (!confirm("Bạn chắc chắn muốn xóa ảnh này?")) return;

            const itemIndex = currentComicImagesState.findIndex(item => item.id === itemId);
            if (itemIndex > -1) {
                const deletedItem = currentComicImagesState.splice(itemIndex, 1)[0]; // Xóa khỏi state và lấy item đã xóa

                // Nếu item bị xóa là một ảnh đã tồn tại (isExisting=true)
                // VÀ nó chưa bị thay thế bởi file mới (tempFileName=null)
                // -> thì cần thêm đường dẫn gốc của nó vào danh sách cần xóa trên server.
                if (deletedItem.isExisting && deletedItem.originalPath && !deletedItem.tempFileName) {
                    addPathToDeleteList(deletedItem.originalPath);
                }

                // Nếu item bị xóa có file tạm (là file mới hoặc file thay thế) -> cần xử lý xóa file tạm này (optional, tùy logic handler)
                if (deletedItem.tempFileName) {
                    // console.log(`Need to consider deleting temp file on server (optional): ${deletedItem.tempFileName}`);
                    // Có thể gọi handler để xóa file tạm ngay, hoặc để server tự dọn dẹp sau.
                }

                // Giải phóng blob URL nếu có (của file mới/thay thế)
                if (deletedItem.newFileBlobUrl) {
                    try { URL.revokeObjectURL(deletedItem.newFileBlobUrl); } catch (e) { console.warn("Could not revoke blob URL of deleted item", e); }
                }
                // Cũng giải phóng URL gốc nếu nó là blob (trường hợp hiếm)
                if (deletedItem.url && deletedItem.url.startsWith('blob:')) {
                    try { URL.revokeObjectURL(deletedItem.url); } catch (e) { }
                }


                renderComicEditPreview(); // Vẽ lại giao diện sau khi xóa
            } else {
                // console.warn(`Item ID ${itemId} not found for deletion.`);
            }
        }

        // Cập nhật thứ tự state dựa trên thứ tự các element trong DOM (sau khi kéo thả)
        function updateStateOrderFromDOM() {
            if (!editPreviewContainer) return;
            const newOrderedState = [];
            const previewElements = editPreviewContainer.querySelectorAll('.edit-comic-image-item');

            previewElements.forEach(element => {
                const itemId = element.getAttribute('data-id');
                const foundItem = currentComicImagesState.find(item => item.id === itemId);
                if (foundItem) {
                    newOrderedState.push(foundItem);
                } else {
                    // console.warn(`Item element with ID ${itemId} found in DOM but not in state.`);
                }
            });

            currentComicImagesState = newOrderedState; // Cập nhật state với thứ tự mới
            // console.log("Internal state reordered based on DOM.");
            updateHiddenFieldsAndValidate(); // Cập nhật hidden fields dựa trên state mới
        }

        // --- Hidden Field Management ---
        function updateHiddenFieldsAndValidate() {
            // Đảm bảo các hidden fields tồn tại
            if (!imageOrderHiddenField || !imagesToDeleteHiddenField || !imagesToReplaceHiddenField || !newFilesHiddenField) {
                console.error("One or more hidden fields for comic data are missing!");
                return;
            }

            const finalOrderIdentifiers = []; // Mảng chứa path gốc hoặc tên file tạm theo đúng thứ tự cuối cùng
            const replacements = []; // Mảng chứa các cặp { originalPath, tempFileName }
            const newFileTempNames = []; // Mảng chứa tên file tạm của các ảnh mới được thêm
            // Lấy danh sách path cần xóa hiện tại từ hidden field (để không thêm path đã bị thay thế vào order)
            const pathsToDelete = imagesToDeleteHiddenField.value ? imagesToDeleteHiddenField.value.split(',').map(p => p.trim()).filter(p => p) : [];

            currentComicImagesState.forEach(item => {
                let identifierForOrder = null;

                if (item.tempFileName) {
                    // Nếu có file tạm (ảnh mới hoặc ảnh thay thế)
                    identifierForOrder = item.tempFileName; // Dùng tên file tạm làm định danh thứ tự

                    if (item.isExisting && item.originalPath) {
                        // Nếu là ảnh gốc bị thay thế, thêm vào danh sách replacements
                        replacements.push({ originalPath: item.originalPath, tempFileName: item.tempFileName });
                    } else if (!item.isExisting) {
                        // Nếu là ảnh mới hoàn toàn, thêm tên file tạm vào danh sách new files
                        newFileTempNames.push(item.tempFileName);
                    }
                } else if (item.isExisting && item.originalPath) {
                    // Nếu là ảnh gốc VÀ không bị thay thế VÀ không nằm trong danh sách bị xóa
                    if (!pathsToDelete.includes(item.originalPath)) {
                        identifierForOrder = item.originalPath; // Dùng path gốc làm định danh thứ tự
                    }
                }
                // Chỉ thêm vào danh sách thứ tự nếu có định danh hợp lệ
                if (identifierForOrder) {
                    finalOrderIdentifiers.push(identifierForOrder);
                }
            });

            // Cập nhật giá trị các hidden fields
            const orderString = finalOrderIdentifiers.join(',');
            const replaceString = JSON.stringify(replacements);
            const newFilesString = newFileTempNames.join(',');
            // Chỉ cập nhật nếu giá trị thay đổi để tránh trigger không cần thiết
            if (imageOrderHiddenField.value !== orderString) {
                imageOrderHiddenField.value = orderString;
                // console.log("Updated hfComicImageOrder:", orderString);
            }
            // Danh sách xóa được cập nhật riêng bởi addPathToDeleteList/removePathFromDeleteList
            // nhưng cần đồng bộ lại ở đây nếu state thay đổi cách khác
            const currentDeleteString = imagesToDeleteHiddenField.value; // Giữ nguyên giá trị hiện tại từ các hàm add/remove
            // if (imagesToDeleteHiddenField.value !== currentDeleteString) {
            //    imagesToDeleteHiddenField.value = currentDeleteString;
            //    console.log("Updated hfComicImagesToDelete:", currentDeleteString);
            // }
            if (imagesToReplaceHiddenField.value !== replaceString) {
                imagesToReplaceHiddenField.value = replaceString;
                // console.log("Updated hfComicImagesToReplace:", replaceString);
            }
            if (newFilesHiddenField.value !== newFilesString) {
                newFilesHiddenField.value = newFilesString;
                // console.log("Updated hfComicNewFiles:", newFilesString);
            }

            // Trigger client validation cho trường ảnh truyện
            validateEditComicImageRequired(null, { IsValid: true }); // Gọi hàm validator với args giả để kiểm tra lại
        }

        function addPathToDeleteList(originalPath) {
            if (!imagesToDeleteHiddenField || !originalPath) return;
            let paths = imagesToDeleteHiddenField.value ? imagesToDeleteHiddenField.value.split(',').map(p => p.trim()).filter(p => p) : [];
            if (!paths.includes(originalPath)) {
                paths.push(originalPath);
                imagesToDeleteHiddenField.value = paths.join(',');
                // console.log(`Added to delete list: ${originalPath}. Current list: ${imagesToDeleteHiddenField.value}`);
                updateHiddenFieldsAndValidate(); // Cập nhật lại các field khác nếu cần
            }
        }

        function removePathFromDeleteList(originalPath) {
            if (!imagesToDeleteHiddenField || !originalPath || !imagesToDeleteHiddenField.value) return;
            let paths = imagesToDeleteHiddenField.value.split(',').map(p => p.trim()).filter(p => p);
            const initialLength = paths.length;
            paths = paths.filter(p => p !== originalPath);
            // Chỉ cập nhật nếu thực sự có thay đổi
            if (paths.length < initialLength) {
                imagesToDeleteHiddenField.value = paths.join(',');
                // console.log(`Removed from delete list: ${originalPath}. Current list: ${imagesToDeleteHiddenField.value}`);
                updateHiddenFieldsAndValidate(); // Cập nhật lại các field khác nếu cần
            }
        }

        function resetHiddenFields() {
            if (imageOrderHiddenField) imageOrderHiddenField.value = '';
            if (imagesToDeleteHiddenField) imagesToDeleteHiddenField.value = '';
            if (imagesToReplaceHiddenField) imagesToReplaceHiddenField.value = '[]'; // Mảng JSON rỗng
            if (newFilesHiddenField) newFilesHiddenField.value = '';
        }

        function resetReplacementState() {
            replaceTargetInfo = { itemId: null, element: null };
            if (replaceFileInput) replaceFileInput.value = null; // Reset input file
        }

        // --- Client Validation ---
        function validateEditComicImageRequired(source, args) {
            const isPanelVisible = comicUploadPanel && comicUploadPanel.offsetParent !== null;
            const validator = document.getElementById('<%= cvAnhTruyenRequired.ClientID %>');
            args.IsValid = true; // Mặc định là hợp lệ

            // Chỉ validate nếu panel truyện tranh hiển thị và validator được bật
            if (isPanelVisible && validator && validator.enabled) {
                // Đếm số lượng ảnh cuối cùng sẽ còn lại sau khi lưu
                // Bao gồm: ảnh gốc không bị xóa, ảnh mới, ảnh thay thế
                const finalImageCount = currentComicImagesState.filter(item => {
                    // Nếu là ảnh gốc, nó không được nằm trong danh sách xóa
                    if (item.isExisting && item.originalPath) {
                        return !imagesToDeleteHiddenField.value.split(',').includes(item.originalPath);
                    }
                    // Nếu là ảnh mới hoặc ảnh thay thế (có tempFileName), thì nó luôn được tính
                    return !!item.tempFileName;
                }).length;

                args.IsValid = finalImageCount > 0; // Phải có ít nhất 1 ảnh

                // Cập nhật thông báo lỗi nếu không hợp lệ
                if (!args.IsValid && source) {
                    source.errormessage = "Phải còn lại ít nhất một ảnh trong chương truyện tranh.";
                }
            }
            // console.log(`validateEditComicImageRequired - IsValid: ${args.IsValid}`);
        }

        // Client validation cho nội dung truyện chữ (EDIT MODE)
        function validateNovelContent_Client_Edit(source, args) {
            const fileUpload = document.getElementById('<%= fuFileTieuThuyet.ClientID %>');
            const textBox = document.getElementById('<%= txtNoiDungChu.ClientID %>');
            const originalText = document.getElementById('<%= hfOriginalNovelText.ClientID %>').value;
            const currentDuongDan = document.getElementById('<%= hfCurrentDuongDan.ClientID %>').value; // Đường dẫn file cũ (nếu có)

            const textContent = textBox ? textBox.value.trim() : '';
            const hasNewFile = fileUpload && fileUpload.files && fileUpload.files.length > 0;
            const hasTextChanged = textBox && textContent !== originalText.trim();
            const hadExistingContent = !stringIsNullOrWhiteSpace(originalText) || !stringIsNullOrEmpty(currentDuongDan); // Có nội dung gốc hoặc file gốc

            // Điều kiện hợp lệ:
            // 1. Có tải file mới LÊN.
            // 2. Hoặc, có nội dung trong textbox.
            // 3. Hoặc, KHÔNG tải file mới VÀ KHÔNG thay đổi text VÀ trước đó ĐÃ CÓ nội dung/file cũ.
            //    (Trường hợp này nghĩa là người dùng không làm gì cả -> hợp lệ)
            args.IsValid = hasNewFile || (textContent.length > 0) || (!hasNewFile && !hasTextChanged && hadExistingContent);

            if (!args.IsValid) {
                source.errormessage = "Không thể xóa hết nội dung. Vui lòng nhập nội dung chữ hoặc tải lên file mới.";
            }
            // console.log(`validateNovelContent_Client_Edit - IsValid: ${args.IsValid}`);
        }

        // Helper function
        function stringIsNullOrWhiteSpace(str) {
            return str === null || typeof str !== 'string' || str.match(/^ *$/) !== null;
        }
        function stringIsNullOrEmpty(str) {
            return str === null || typeof str !== 'string' || str.length === 0;
        }

        // --- Form Submission ---
        function prepareSubmitData_Edit() {
            console.log('Chuẩn bị gửi dữ liệu (Edit)...');
            // 1. Cập nhật lần cuối các hidden fields từ state
            updateHiddenFieldsAndValidate();

            // 2. Chạy client validation của ASP.NET
            if (typeof Page_ClientValidate === 'function') {
                if (!Page_ClientValidate('ChapterValidation')) {
                    console.log('Client validation FAILED.');
                    // Hiển thị validation summary nếu có lỗi
                    const vs = document.getElementById('<%= vsChapterForm.ClientID %>');
                    if (vs) vs.style.display = 'block';
                    // Reset trạng thái các nút submit nếu cần
                    const btnSave = document.getElementById('<%= btnSaveNoiDung.ClientID %>');
                    const btnCancel = document.getElementById('<%= btnCancel.ClientID %>');
                    const btnSaveName = document.getElementById('<%= btnSaveTenChuong.ClientID %>'); // Nút lưu tên trên cùng
                    const btnSaveName2 = document.getElementById('<%= Button1.ClientID %>'); // Nút lưu tên dưới cùng
                    if (btnSave) {
                        btnSave.disabled = false;
                        btnSave.classList.remove('loading-spinner');
                        btnSave.value = 'Lưu Thay Đổi Nội Dung';
                    }
                    if (btnCancel) { btnCancel.disabled = false; }
                    if (btnSaveName) { btnSaveName.disabled = false; }
                    if (btnSaveName2) { btnSaveName2.disabled = false; }
                    return false; // Ngăn chặn submit
                } else {
                    console.log('Client validation PASSED.');
                    // Ẩn validation summary nếu không có lỗi
                    const vs = document.getElementById('<%= vsChapterForm.ClientID %>');
                    if (vs) vs.style.display = 'none';
                }
            } else {
                console.warn("Page_ClientValidate function not found. Skipping ASP.NET client validation.");
            }

            // 3. (Quan trọng) Vô hiệu hóa các nút và hiển thị spinner ĐỂ TRÁNH SUBMIT NHIỀU LẦN
            // Dùng setTimeout 0 để đảm bảo nó chạy sau khi luồng hiện tại hoàn tất (bao gồm cả return false ở trên nếu validation fail)
            const btnSave = document.getElementById('<%= btnSaveNoiDung.ClientID %>');
            const btnCancel = document.getElementById('<%= btnCancel.ClientID %>');
            const btnSaveName = document.getElementById('<%= btnSaveTenChuong.ClientID %>');
            const btnSaveName2 = document.getElementById('<%= Button1.ClientID %>');
            setTimeout(() => {
                if (btnSave) {
                    btnSave.disabled = true;
                    btnSave.classList.add('loading-spinner');
                    btnSave.value = 'Đang lưu...'; // Đổi text nút
                }
                if (btnCancel) { btnCancel.disabled = true; }
                if (btnSaveName) { btnSaveName.disabled = true; }
                if (btnSaveName2) { btnSaveName2.disabled = true; }
            }, 0);

            // Log giá trị cuối cùng của hidden fields trước khi submit
            console.log("Final Hidden Fields before submit:",
                `\n Order: ${imageOrderHiddenField ? imageOrderHiddenField.value : 'N/A'}`,
                `\n Delete: ${imagesToDeleteHiddenField ? imagesToDeleteHiddenField.value : 'N/A'}`,
                `\n Replace: ${imagesToReplaceHiddenField ? imagesToReplaceHiddenField.value : 'N/A'}`,
                `\n New: ${newFilesHiddenField ? newFilesHiddenField.value : 'N/A'}`
            );

            return true; // Cho phép form submit
        }

        // Helper Format File Size
        function formatFileSize(bytes) {
            if (bytes < 0) return 'N/A';
            if (bytes === 0) return '0 Bytes';
            const k = 1024;
            const sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB'];
            if (bytes < 1) return '0 Bytes'; // Xử lý trường hợp rất nhỏ
            try {
                // Đảm bảo index không vượt quá kích thước mảng sizes
                const i = Math.max(0, Math.min(Math.floor(Math.log(bytes) / Math.log(k)), sizes.length - 1));
                // Làm tròn đến 1 chữ số thập phân
                return parseFloat((bytes / Math.pow(k, i)).toFixed(1)) + ' ' + sizes[i];
            } catch (e) {
                // console.error("Error formatting file size:", e);
                return 'N/A'; // Trả về N/A nếu có lỗi (ví dụ: log(0))
            }
        }

    </script>
</asp:Content>