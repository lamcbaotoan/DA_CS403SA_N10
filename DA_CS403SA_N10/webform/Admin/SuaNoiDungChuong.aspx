<%@ Page Title="Sửa Nội Dung Chương" Language="C#" MasterPageFile="~/WebForm/Admin/Admin.Master" AutoEventWireup="true" CodeBehind="SuaNoiDungChuong.aspx.cs" Inherits="Webebook.WebForm.Admin.SuaNoiDungChuong" %>
<%@ OutputCache Duration="1" VaryByParam="none" Location="None" NoStore="true" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <%-- Libraries --%>
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/dropzone/5.9.3/min/dropzone.min.css" />
    <script src="https://cdnjs.cloudflare.com/ajax/libs/Sortable/1.15.0/Sortable.min.js"></script>
    <script src="https://cdnjs.cloudflare.com/ajax/libs/dropzone/5.9.3/min/dropzone.min.js" defer></script>
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.0.0/css/all.min.css" />

    <%-- CSS --%>
    <style>
        /* --- General & Forms --- */
        .form-label { display: block; font-size: 0.875rem; font-weight: 500; color: #374151; margin-bottom: 0.25rem; }
        .form-control {
            width: 100%; padding: 0.5rem; border: 1px solid #d1d5db; border-radius: 0.375rem;
            transition: border-color 0.15s ease-in-out, box-shadow 0.15s ease-in-out;
        }
        .form-control:focus { outline: none; border-color: #4f46e5; box-shadow: 0 0 0 2px rgba(79, 70, 229, 0.3); }
        .form-control[disabled], .form-control[readonly] { background-color: #f3f4f6; cursor: not-allowed; opacity: 0.7; }
        textarea.form-control { min-height: 400px; resize: vertical; font-family: ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, "Liberation Mono", "Courier New", monospace; font-size: 0.875rem; }
        .validation-error { color: #dc2626; font-size: 0.75rem; margin-top: 0.25rem; display: block; }
        
        /* --- Buttons --- */
        .btn-action {
            display: inline-flex; align-items: center; justify-content: center;
            padding: 0.5rem 1rem; border: 1px solid transparent; font-size: 0.875rem;
            font-weight: 500; border-radius: 0.375rem; box-shadow: 0 1px 2px 0 rgba(0, 0, 0, 0.05);
            cursor: pointer; transition: background-color 0.15s ease-in-out;
        }
        .btn-action:disabled { opacity: 0.5; cursor: not-allowed; }
        .btn-primary { color: #fff; background-color: #4f46e5; } .btn-primary:hover { background-color: #4338ca; }
        .btn-secondary { color: #374151; background-color: #fff; border-color: #d1d5db; } .btn-secondary:hover { background-color: #f9fafb; }
        .btn-success { color: #fff; background-color: #16a34a; } .btn-success:hover { background-color: #15803d; }
        .btn-sm { padding: 0.375rem 0.75rem; font-size: 0.75rem; }
        .loading-spinner::after { content: ''; display: inline-block; width: 1rem; height: 1rem; border: 2px solid; border-top-color: transparent; border-right-color: transparent; border-radius: 50%; animation: spin 0.75s linear infinite; margin-left: 0.5rem; }

        /* --- Dropzone & Comic Edit Area --- */
        #comicUploader { border: 2px dashed #60a5fa; border-radius: 0.5rem; padding: 1rem; text-align: center; background-color: #eff6ff; transition: background-color 0.2s ease; cursor: pointer; min-height: 80px; display: flex; align-items: center; justify-content: center; }
        #comicUploader:hover { background-color: #dbeafe; }
        #comicUploader .dz-message { color: #1d4ed8; font-weight: 500; }
        #comicUploader .dz-message i { margin-right: 0.5rem; }
        #editComicImagesContainer { margin-top: 0.25rem; display: grid; grid-template-columns: repeat(3, 1fr); gap: 0.75rem; max-height: 60vh; overflow-y: auto; border: 1px solid #e5e7eb; padding: 0.75rem; border-radius: 0.5rem; min-height: 150px; background-color: #f9fafb; box-shadow: inset 0 2px 4px 0 rgba(0,0,0,0.05); }
        @media (min-width: 640px) { #editComicImagesContainer { grid-template-columns: repeat(4, 1fr); } }
        @media (min-width: 768px) { #editComicImagesContainer { grid-template-columns: repeat(5, 1fr); } }
        @media (min-width: 1024px) { #editComicImagesContainer { grid-template-columns: repeat(6, 1fr); } }
        #editComicImagesContainer:empty::before { content: 'Chưa có ảnh nào. Kéo thả hoặc nhấn vào khu vực trên để thêm ảnh.'; font-size: 0.875rem; color: #6b7280; font-style: italic; display: block; text-align: center; padding: 4rem 0; width: 100%; grid-column: 1 / -1; }

        /* Comic Image Item */
        .edit-comic-image-item { display: flex; flex-direction: column; align-items: center; padding: 0.5rem; border: 1px solid #e5e7eb; background-color: #fff; border-radius: 0.5rem; box-shadow: 0 1px 2px 0 rgba(0,0,0,0.05); cursor: grab; position: relative; transition: box-shadow 0.2s ease; aspect-ratio: 3 / 5; }
        .edit-comic-image-item .edit-image-thumb { width: 100%; height: 100%; flex: 1; border-radius: 0.25rem 0.25rem 0 0; border-bottom: 1px solid #f3f4f6; background-color: #f9fafb; display: flex; align-items: center; justify-content: center; overflow: hidden; }
        .edit-comic-image-item .edit-image-thumb img { object-fit: contain; max-width: 100%; max-height: 100%; }
        .edit-comic-image-item .file-info-actions { width: 100%; padding: 0.375rem; background-color: #f9fafb; border-radius: 0 0 0.25rem 0.25rem; }
        .edit-comic-image-item .file-name-display { font-size: 11px; line-height: 1.2; text-align: center; color: #4b5563; overflow: hidden; display: -webkit-box; -webkit-line-clamp: 2; -webkit-box-orient: vertical; height: 30px; }
        .item-status-new .file-name-display::before, .item-status-replaced .file-name-display::before { font-family: 'Font Awesome 6 Free'; font-weight: 900; margin-right: 0.25rem; font-size: 0.75rem; }
        .item-status-new .file-name-display::before { content: '\f058'; color: #16a34a; }
        .item-status-replaced .file-name-display::before { content: '\f044'; color: #f59e0b; }
        .edit-comic-image-item .image-actions { display: flex; justify-content: center; gap: 1rem; width: 100%; padding-top: 0.25rem; }
        .edit-comic-image-item .action-btn { padding: 0.125rem; color: #6b7280; cursor: pointer; font-size: 1rem; border: none; background-color: transparent; transition: color 0.2s ease; }
        .edit-comic-image-item .action-btn:hover { color: #3b82f6; }
        .edit-comic-image-item .delete-btn { color: #ef4444; } .edit-comic-image-item .delete-btn:hover { color: #b91c1c; }
        .sortable-ghost { opacity: 0.4; background-color: #dbeafe; border: 1px dashed #93c5fd; }
        .sortable-chosen { box-shadow: 0 4px 6px -1px rgba(0,0,0,0.1), 0 2px 4px -2px rgba(0,0,0,0.1); outline: 2px solid #3b82f6; }

        /* Message Panel */
        .message-panel { padding: 1rem 1.25rem; margin-bottom: 1.5rem; border-radius: 0.375rem; border-width: 1px; font-weight: 500; }
        .message-success { color: #14532d; background-color: #dcfce7; border-color: #86efac; }
        .message-error { color: #991b1b; background-color: #fee2e2; border-color: #fca5a5; }
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
                 <h4 class="text-lg font-semibold text-gray-800 mb-4">Thông Tin Cơ Bản</h4>
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
                     <asp:Panel ID="pnlExistingNovelFile" runat="server" Visible="false" CssClass="p-3 bg-gray-50 border border-gray-200 rounded-md">
                        <p class="text-sm font-medium text-gray-700 mb-1">File nội dung hiện tại:</p>
                        <div class="flex items-center">
                            <i class="fas fa-file-alt text-gray-500 mr-2 text-lg"></i>
                            <asp:HyperLink ID="hlCurrentNovelFile" runat="server" Target="_blank" CssClass="text-blue-600 hover:text-blue-800 hover:underline text-sm font-mono flex-1 truncate"></asp:HyperLink>
                        </div>
                        <p class="text-xs text-gray-500 mt-1.5">Tải file mới hoặc sửa nội dung bên dưới sẽ thay thế file này.</p>
                     </asp:Panel>

                    <div>
                        <label for="<%=fuFileTieuThuyet.ClientID %>" class="form-label">Tùy chọn 1: Tải File Mới Thay Thế (chỉ .txt)</label>
                        <asp:FileUpload ID="fuFileTieuThuyet" runat="server" CssClass="file-input-styled" accept=".txt" />
                        <asp:RegularExpressionValidator ID="revFileTieuThuyet" runat="server" ControlToValidate="fuFileTieuThuyet"
                            ErrorMessage="Chỉ chấp nhận file .txt"
                            ValidationExpression="^.*\.(txt|TXT)$" Display="Dynamic"
                            CssClass="validation-error" ValidationGroup="ChapterValidation" Enabled="false"></asp:RegularExpressionValidator>
                    </div>
                     <div>
                         <label for="<%=txtNoiDungChu.ClientID %>" class="form-label">Tùy chọn 2: Nhập/Sửa Nội Dung Trực Tiếp</label>
                         <asp:Label ID="lblNovelFileReadError" runat="server" CssClass="validation-error" Visible="false" EnableViewState="false"></asp:Label>
                         <asp:TextBox ID="txtNoiDungChu" runat="server" CssClass="form-control" Rows="25" TextMode="MultiLine"></asp:TextBox>
                     </div>
                     <asp:CustomValidator ID="cvNovelContentRequired" runat="server"
                        ErrorMessage="Phải có nội dung chữ hoặc tải file mới."
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
                        <div id="comicUploader" class="dropzone"></div>
                    </div>
                </div>
                <label class="form-label mt-4">Ảnh Hiện Tại & Ảnh Mới (Kéo thả sắp xếp, <i class="fas fa-sync-alt text-blue-600"></i> thay thế, <i class="fas fa-trash text-red-600"></i> xóa):</label>
                <div id="editComicImagesContainer" class="dropzone-previews"></div>
                <asp:CustomValidator ID="cvAnhTruyenRequired" runat="server" ErrorMessage="Phải còn lại ít nhất một ảnh." CssClass="validation-error" Display="Dynamic" ValidationGroup="ChapterValidation" ClientValidationFunction="validateEditComicImageRequired" OnServerValidate="cvAnhTruyenRequired_ServerValidate" Enabled="false"/>
                <input type="file" id="fuEditComicImageReplace" class="hidden" accept="image/*" onchange="handleComicImageReplaceUpload(event)">
            </asp:Panel>

            <%-- ==================== Nút Actions ==================== --%>
            <div class="mt-8 flex flex-col sm:flex-row sm:justify-end items-center border-t border-gray-200 pt-6 gap-4">
                 <asp:Button ID="btnCancel" runat="server" Text="Hủy Bỏ" CssClass="btn-action btn-secondary w-full sm:w-auto" OnClick="btnCancel_Click" CausesValidation="false" />
                 <asp:Button ID="btnSaveNoiDung" runat="server" Text="Lưu Thay Đổi Nội Dung" ToolTip="Lưu tất cả thay đổi về Tên chương và Nội dung (ảnh/text)" CssClass="btn-action btn-primary w-full sm:w-auto" OnClick="btnSaveNoiDung_Click" ValidationGroup="ChapterValidation" OnClientClick="return prepareSubmitData_Edit();" />
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
        const maxFilesizeMB = <%= MaxFileSizePerImageMb %>;
        const allowedImageExtensions = '<%= string.Join(",", AllowedImageExtensions) %>';
        const combinedAcceptedFiles = allowedImageExtensions.split(',').map(ext => ext.trim()).map(ext => ext + ",image/" + ext.substring(1)).join(',');
        const maxFileSizeClient = maxFilesizeMB * 1024 * 1024;

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
                initializeEditSortable(); 
            }
            const validationSummary = document.getElementById('<%= vsChapterForm.ClientID %>');
            if (validationSummary) validationSummary.style.display = 'none';
        });
        
        // This function is called from code-behind (ScriptManager)
        function initializeComicEditor(existingImagesJson) {
            currentComicImagesState = []; 
            resetHiddenFields();

            if (existingImagesJson && Array.isArray(existingImagesJson)) {
                existingImagesJson.forEach((imgData, index) => {
                    if (!imgData || !imgData.path || !imgData.url) return;
                    currentComicImagesState.push({
                        id: `existing_${index}_${Math.random().toString(36).substr(2, 5)}`,
                        originalPath: imgData.path,
                        url: imgData.url,
                        isExisting: true,
                        tempFileName: null,
                        displayName: imgData.name || imgData.path.split('/').pop(),
                        newFileBlobUrl: null
                    });
                });
            }
            renderComicEditPreview();
        }

        // --- Dropzone Setup ---
        function initializeNewFileDropzone() {
            if (myDropzoneInstanceEdit) return;
            Dropzone.autoDiscover = false;
            try {
                myDropzoneInstanceEdit = new Dropzone(dropzoneElementSelector, {
                    url: uploadHandlerUrl,
                    paramName: "file",
                    maxFilesize: maxFilesizeMB,
                    acceptedFiles: combinedAcceptedFiles,
                    clickable: true,
                    createImageThumbnails: false,
                    previewsContainer: false,
                    dictDefaultMessage: `<i class="fas fa-cloud-upload-alt mr-2"></i> Kéo thả hoặc nhấp để thêm ảnh`,
                    init: function () {
                        this.on("success", function (file, response) {
                            if (response && response.fileName && !response.error) {
                                const newItem = {
                                    id: `new_${file.upload.uuid}`,
                                    originalPath: null,
                                    url: URL.createObjectURL(file),
                                    isExisting: false,
                                    tempFileName: response.fileName,
                                    displayName: file.name,
                                    newFileBlobUrl: URL.createObjectURL(file)
                                };
                                currentComicImagesState.push(newItem);
                                renderComicEditPreview();
                                this.removeFile(file);
                            } else {
                                alert(`Lỗi tải ảnh '${file.name}': ${response.error || 'Lỗi không xác định'}`);
                                this.removeFile(file);
                            }
                        });
                        this.on("error", (file, msg) => alert(`Lỗi tải ảnh '${file.name}': ${msg.error || msg}`));
                    }
                });
            } catch (e) {
                console.error("Error initializing Dropzone:", e);
                alert("Không thể khởi tạo khu vực tải ảnh. Vui lòng tải lại trang.");
            }
        }

        // --- Sortable Setup ---
        function initializeEditSortable() {
            if (!editPreviewContainer || typeof Sortable === 'undefined') return;
            if (sortableInstanceEdit) sortableInstanceEdit.destroy();
            
            sortableInstanceEdit = Sortable.create(editPreviewContainer, {
                animation: 150,
                draggable: ".edit-comic-image-item",
                ghostClass: 'sortable-ghost',
                chosenClass: 'sortable-chosen',
                onEnd: () => updateStateOrderFromDOM(),
            });
        }
        
        // --- Core UI Logic ---
        function renderComicEditPreview() {
            if (!editPreviewContainer) return;
            editPreviewContainer.innerHTML = ''; 

            currentComicImagesState.forEach(item => {
                const el = createComicEditItemElement(item);
                if (el) editPreviewContainer.appendChild(el);
            });
            
            initializeEditSortable(); // Re-init for new/removed items
            updateHiddenFieldsAndValidate();
        }

        function createComicEditItemElement(item) {
            const div = document.createElement('div');
            div.className = 'edit-comic-image-item';
            div.setAttribute('data-id', item.id);
            if (item.originalPath) div.setAttribute('data-original-path', item.originalPath);
            if (item.tempFileName) div.setAttribute('data-temp-filename', item.tempFileName);

            let statusClass = !item.isExisting ? 'item-status-new' : (item.tempFileName ? 'item-status-replaced' : '');
            if (statusClass) div.classList.add(statusClass);

            const thumbDiv = document.createElement('div');
            thumbDiv.className = 'edit-image-thumb';
            const img = document.createElement('img');
            img.src = item.newFileBlobUrl || item.url || '';
            img.onerror = () => { thumbDiv.innerHTML = '<i class="fas fa-image-slash text-gray-400 text-2xl"></i>'; };
            thumbDiv.appendChild(img);

            const infoActionsDiv = document.createElement('div');
            infoActionsDiv.className = 'file-info-actions';
            const nameDiv = document.createElement('div');
            nameDiv.className = 'file-name-display';
            nameDiv.textContent = item.displayName || '[Ảnh không tên]';
            nameDiv.title = nameDiv.textContent;
            infoActionsDiv.appendChild(nameDiv);

            const actionsDiv = document.createElement('div');
            actionsDiv.className = 'image-actions';
            actionsDiv.innerHTML = `
                <button type="button" class="action-btn replace-btn" title="Thay thế ảnh này" onclick="triggerComicImageReplace(this.closest('.edit-comic-image-item'), '${item.id}')"><i class="fas fa-sync-alt fa-fw"></i></button>
                <button type="button" class="action-btn delete-btn" title="Xóa ảnh này" onclick="deleteComicEditImage('${item.id}')"><i class="fas fa-trash fa-fw"></i></button>
            `;
            infoActionsDiv.appendChild(actionsDiv);

            div.appendChild(thumbDiv);
            div.appendChild(infoActionsDiv);
            return div;
        }

        // --- Actions ---
        function triggerComicImageReplace(element, itemId) {
            replaceTargetInfo = { itemId: itemId, element: element };
            replaceFileInput.value = null;
            replaceFileInput.click();
        }

        function deleteComicEditImage(itemId) {
            const itemToDelete = currentComicImagesState.find(item => item.id === itemId);
            if (!itemToDelete) return; // Không tìm thấy item để xóa

            Swal.fire({
                title: 'Xóa ảnh này?',
                html: `Bạn có chắc chắn muốn xóa ảnh:<br><strong>${itemToDelete.displayName}</strong>`,
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#d33',
                cancelButtonColor: '#6b7280',
                confirmButtonText: '<i class="fas fa-trash-alt"></i> Đồng ý, Xóa!',
                cancelButtonText: 'Hủy'
            }).then((result) => {
                if (result.isConfirmed) {
                    // --- Toàn bộ logic xóa cũ được chuyển vào đây ---
                    const itemIndex = currentComicImagesState.findIndex(item => item.id === itemId);
                    if (itemIndex > -1) {
                        const [deletedItem] = currentComicImagesState.splice(itemIndex, 1);

                        if (deletedItem.isExisting && deletedItem.originalPath && !deletedItem.tempFileName) {
                            addPathToDeleteList(deletedItem.originalPath);
                        }

                        if (deletedItem.newFileBlobUrl) {
                            URL.revokeObjectURL(deletedItem.newFileBlobUrl);
                        }

                        renderComicEditPreview(); // Vẽ lại giao diện sau khi xóa
                    }
                }
            });
        }
        
        function handleComicImageReplaceUpload(event) {
            if (!replaceTargetInfo.itemId || !event.target.files.length) return;
            const file = event.target.files[0];
            const targetItemId = replaceTargetInfo.itemId;
            
            // Client-side validation
            if (!file.type.startsWith('image/')) { alert(`Lỗi: Định dạng file thay thế không hợp lệ.`); return; }
            if (file.size > maxFileSizeClient) { alert(`Lỗi: File thay thế quá lớn (tối đa ${maxFilesizeMB}MB).`); return; }

            const itemIndex = currentComicImagesState.findIndex(item => item.id === targetItemId);
            if (itemIndex > -1) {
                const itemToUpdate = currentComicImagesState[itemIndex];
                const formData = new FormData();
                formData.append("file", file);
                
                replaceTargetInfo.element.style.opacity = '0.5';

                fetch(uploadHandlerUrl, { method: 'POST', body: formData })
                .then(response => response.json())
                .then(data => {
                    if (data && data.fileName && !data.error) {
                        if (itemToUpdate.newFileBlobUrl) URL.revokeObjectURL(itemToUpdate.newFileBlobUrl);
                        itemToUpdate.tempFileName = data.fileName;
                        itemToUpdate.displayName = file.name;
                        itemToUpdate.newFileBlobUrl = URL.createObjectURL(file);
                        if (itemToUpdate.isExisting && itemToUpdate.originalPath) {
                            removePathFromDeleteList(itemToUpdate.originalPath);
                        }
                        renderComicEditPreview();
                    } else {
                        alert(`Lỗi tải file thay thế: ${data.error || 'Lỗi server'}`);
                        replaceTargetInfo.element.style.opacity = '1';
                    }
                })
                .catch(error => {
                    alert(`Lỗi mạng khi tải file thay thế: ${error.message}`);
                    replaceTargetInfo.element.style.opacity = '1';
                });
            }
        }

        function updateStateOrderFromDOM() {
            const newOrderedState = [];
            editPreviewContainer.querySelectorAll('.edit-comic-image-item').forEach(element => {
                const itemId = element.getAttribute('data-id');
                const foundItem = currentComicImagesState.find(item => item.id === itemId);
                if (foundItem) newOrderedState.push(foundItem);
            });
            currentComicImagesState = newOrderedState;
            updateHiddenFieldsAndValidate();
        }

        // --- Hidden Field & Validation Management ---
        function updateHiddenFieldsAndValidate() {
            const finalOrder = currentComicImagesState.map(item => item.tempFileName || item.originalPath);
            const replacements = currentComicImagesState
                .filter(item => item.isExisting && item.tempFileName)
                .map(item => ({ originalPath: item.originalPath, tempFileName: item.tempFileName }));
            const newFiles = currentComicImagesState
                .filter(item => !item.isExisting)
                .map(item => item.tempFileName);

            imageOrderHiddenField.value = finalOrder.join(',');
            imagesToReplaceHiddenField.value = JSON.stringify(replacements);
            newFilesHiddenField.value = newFiles.join(',');
            
            // Trigger client validation for comic images
            if (typeof(ValidatorValidate) === "function") {
                ValidatorValidate(document.getElementById('<%= cvAnhTruyenRequired.ClientID %>'));
            }
        }

        function addPathToDeleteList(path) {
            let paths = imagesToDeleteHiddenField.value ? imagesToDeleteHiddenField.value.split(',') : [];
            if (!paths.includes(path)) {
                paths.push(path);
                imagesToDeleteHiddenField.value = paths.join(',');
            }
        }

        function removePathFromDeleteList(path) {
            let paths = imagesToDeleteHiddenField.value ? imagesToDeleteHiddenField.value.split(',') : [];
            imagesToDeleteHiddenField.value = paths.filter(p => p !== path).join(',');
        }

        function resetHiddenFields() {
            imageOrderHiddenField.value = '';
            imagesToDeleteHiddenField.value = '';
            imagesToReplaceHiddenField.value = '[]';
            newFilesHiddenField.value = '';
        }

        // --- Client Validation Functions ---
        function validateEditComicImageRequired(source, args) {
            args.IsValid = comicUploadPanel.offsetParent === null || (imageOrderHiddenField.value.trim() !== '');
        }

        function validateNovelContent_Client_Edit(source, args) {
            const fileUpload = document.getElementById('<%= fuFileTieuThuyet.ClientID %>');
            const textBox = document.getElementById('<%= txtNoiDungChu.ClientID %>');
            const hasNewFile = fileUpload && fileUpload.files.length > 0;
            const hasText = textBox && textBox.value.trim().length > 0;
            args.IsValid = hasNewFile || hasText;
        }

        // --- Form Submission ---
        function prepareSubmitData_Edit() {
            // Final update of hidden fields before submitting
            updateHiddenFieldsAndValidate();

            if (typeof Page_ClientValidate === 'function' && !Page_ClientValidate('ChapterValidation')) {
                const vs = document.getElementById('<%= vsChapterForm.ClientID %>');
                if (vs) vs.style.display = 'block';
                return false;
            }

            const btnSave = document.getElementById('<%= btnSaveNoiDung.ClientID %>');
            const btnCancel = document.getElementById('<%= btnCancel.ClientID %>');

            // Disable buttons to prevent double-submission
            setTimeout(() => {
                if (btnSave) {
                    btnSave.disabled = true;
                    btnSave.classList.add('loading-spinner');
                    btnSave.value = 'Đang lưu...';
                }
                if (btnCancel) { btnCancel.disabled = true; }
            }, 0);

            return true;
        }
    </script>
</asp:Content>