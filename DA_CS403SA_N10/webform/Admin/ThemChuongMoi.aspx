<%@ Page Title="Thêm Chương Mới" Language="C#" MasterPageFile="~/WebForm/Admin/Admin.Master" AutoEventWireup="true" CodeBehind="ThemChuongMoi.aspx.cs" Inherits="Webebook.WebForm.Admin.ThemChuongMoi" %>
<%@ OutputCache Duration="1" VaryByParam="none" Location="None" NoStore="true" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <%-- Dropzone CSS - Đã bỏ integrity, crossorigin --%>
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/dropzone/5.9.3/min/dropzone.min.css" />
    <%-- SortableJS --%>
    <script src="https://cdnjs.cloudflare.com/ajax/libs/Sortable/1.15.0/Sortable.min.js"></script>
    <%-- Dropzone JS - Đã bỏ integrity, crossorigin (vẫn dùng defer) --%>
    <script src="https://cdnjs.cloudflare.com/ajax/libs/dropzone/5.9.3/min/dropzone.min.js" defer></script>

    <style>
        /* --- CSS cho Message Panel --- */
        .message-panel { @apply mb-4 p-3 rounded-lg flex items-center text-sm; }
        .message-success { @apply bg-green-100 border border-green-400 text-green-700; }
        .message-error { @apply bg-red-100 border border-red-400 text-red-700; }
        .message-panel i { @apply mr-2; }
        /* --- CSS cho Dropzone --- */
        .dropzone { @apply border-2 border-dashed border-gray-300 rounded-lg p-6 text-center bg-gray-50 hover:bg-gray-100 transition-colors cursor-pointer min-h-[100px]; }
        .dropzone .dz-message { @apply text-gray-500 font-medium m-0; }
        /* --- CSS cho Preview Items --- */
        #dropzonePreviewContainer { @apply mt-4 space-y-2 max-h-[400px] overflow-y-auto border border-dashed border-gray-300 p-2 rounded-lg min-h-[100px] bg-gray-50; }
        .dropzone .dz-preview { @apply flex items-center p-3 mb-2 bg-white border border-gray-200 rounded-md shadow-sm relative cursor-grab; min-height: 90px; }
        .dropzone .dz-preview .dz-image { @apply w-20 h-20 rounded border border-gray-200 flex-shrink-0 bg-gray-100 flex items-center justify-center; }
        .dropzone .dz-preview .dz-image img { @apply object-contain w-full h-full; }
        .dropzone .dz-preview .dz-details { @apply ml-4 flex-1 text-left overflow-hidden; }
        .dropzone .dz-preview .dz-filename { @apply font-medium text-sm block truncate; }
        .dropzone .dz-preview .dz-size { @apply text-xs text-gray-500 block; }
        .dropzone .dz-preview .dz-progress { @apply h-1 bg-gray-200 rounded mt-1 overflow-hidden w-full absolute bottom-1 left-3 right-3; }
        .dropzone .dz-preview .dz-progress .dz-upload { @apply bg-blue-500 h-full block transition-width duration-300 ease-linear; }
        .dropzone .dz-preview .dz-error-message { @apply text-red-600 text-xs mt-1 font-medium block; max-height: 5em; overflow: hidden;}
        .dropzone .dz-preview .dz-remove { @apply absolute top-1 right-1 bg-red-500 text-white rounded-full w-5 h-5 text-xs font-bold flex items-center justify-center opacity-70 hover:opacity-100 transition-opacity; text-decoration: none; z-index: 10; }
        .dropzone .dz-preview.dz-error .dz-error-mark, .dropzone .dz-preview.dz-success .dz-success-mark { @apply absolute top-1/2 left-1/2 transform -translate-x-1/2 -translate-y-1/2 opacity-0 transition-opacity; font-size: 2rem; display: block; }
         .dropzone .dz-preview.dz-success .dz-success-mark { @apply text-green-500; opacity: 1;}
         .dropzone .dz-preview.dz-error .dz-error-mark { @apply text-red-500; opacity:1;}
        /* --- SortableJS Styling --- */
        .dropzone .dz-preview.sortable-ghost { @apply opacity-40 bg-blue-100 border-blue-300; }
        .dropzone .dz-preview.sortable-chosen { @apply shadow-lg ring-2 ring-offset-1 ring-blue-500; }
        /* --- Loading Spinner --- */
        .loading-spinner::after { content: ''; @apply inline-block w-4 h-4 border-2 border-t-transparent border-white rounded-full animate-spin ml-2; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="container mx-auto px-4 py-6 max-w-4xl">
        <div class="bg-white p-4 rounded-lg shadow mb-6 border border-gray-100">
             <h2 class="text-xl font-semibold text-gray-800 mb-2"><asp:Label ID="lblPageModeTitle" runat="server" Text="Thêm Chương Mới"></asp:Label></h2>
             <p class="text-sm text-gray-600">
                 <strong>Sách:</strong> <asp:Label ID="lblBookTitleContext" runat="server" Text="[Sách]" CssClass="font-medium text-gray-800"></asp:Label>
                 (ID: <asp:Label ID="lblSachIDContext" runat="server" CssClass="font-mono"></asp:Label>) -
                 <strong>Loại:</strong> <asp:Label ID="lblLoaiSachContext" runat="server" Text="[Loại]" CssClass="font-medium text-gray-800"></asp:Label>
             </p>
             <asp:HyperLink ID="hlBackToList" runat="server" CssClass="mt-2 inline-block text-sm text-blue-600 hover:text-blue-800 hover:underline transition-colors"><i class="fas fa-arrow-left mr-1"></i> Quay lại Danh sách chương</asp:HyperLink>
        </div>

        <asp:Panel ID="pnlMessage" runat="server" Visible="false" CssClass="message-panel" EnableViewState="false"><asp:Label ID="lblFormMessage" runat="server"></asp:Label></asp:Panel>
        <asp:ValidationSummary ID="vsChapterForm" runat="server" CssClass="bg-red-50 border border-red-200 p-3 rounded-md mb-4 text-sm text-red-700" HeaderText="Vui lòng sửa các lỗi sau:" ValidationGroup="AddChapterValidation" DisplayMode="BulletList" ShowSummary="true" style="display: none;" />

        <div class="bg-white p-6 rounded-lg shadow-md">
            <asp:HiddenField ID="hfSachID" runat="server" />
            <asp:HiddenField ID="hfLoaiSach" runat="server" />
            <asp:HiddenField ID="hfComicImageOrder" runat="server" />

            <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
                 <div>
                     <label for="<%=txtSoChuong.ClientID %>" class="block text-sm font-medium text-gray-700 mb-1">Số Chương <span class="text-red-500">*</span></label>
                     <asp:TextBox ID="txtSoChuong" runat="server" TextMode="Number" min="1" CssClass="w-full p-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500 focus:border-blue-500"></asp:TextBox>
                     <asp:RequiredFieldValidator ID="rfvSoChuong" runat="server" ControlToValidate="txtSoChuong" ErrorMessage="Số chương là bắt buộc." Display="Dynamic" CssClass="text-red-600 text-xs mt-1" ValidationGroup="AddChapterValidation" Enabled="true"></asp:RequiredFieldValidator>
                     <asp:CompareValidator ID="cvSoChuongType" runat="server" ControlToValidate="txtSoChuong" Operator="DataTypeCheck" Type="Integer" ErrorMessage="Số chương phải là số nguyên dương." Display="Dynamic" CssClass="text-red-600 text-xs mt-1" ValidationGroup="AddChapterValidation" Enabled="true"></asp:CompareValidator>
                     <asp:CompareValidator ID="cvSoChuongPositive" runat="server" ControlToValidate="txtSoChuong" Operator="GreaterThan" Type="Integer" ValueToCompare="0" ErrorMessage="Số chương phải lớn hơn 0." Display="Dynamic" CssClass="text-red-600 text-xs mt-1" ValidationGroup="AddChapterValidation" Enabled="true"></asp:CompareValidator>
                     <asp:CustomValidator ID="cvSoChuongExists" runat="server" ControlToValidate="txtSoChuong" ErrorMessage="Số chương này đã tồn tại cho sách." Display="Dynamic" CssClass="text-red-600 text-xs mt-1" ValidationGroup="AddChapterValidation" OnServerValidate="cvSoChuongExists_ServerValidate" Enabled="true" />
                 </div>
                 <div class="md:col-span-2">
                     <label for="<%=txtTenChuong.ClientID %>" class="block text-sm font-medium text-gray-700 mb-1">Tên Chương (Tùy chọn)</label>
                     <asp:TextBox ID="txtTenChuong" runat="server" CssClass="w-full p-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500 focus:border-blue-500" MaxLength="255"></asp:TextBox>
                 </div>
            </div>

            <hr class="my-6 border-gray-200" />

            <asp:Panel ID="pnlNovelContent" runat="server" Visible="false">
                 <h4 class="text-lg font-medium text-gray-800 mb-4">Nội Dung Truyện Chữ</h4>
                 <div class="mb-4">
                     <label for="<%=fuFileTieuThuyet.ClientID %>" class="block text-sm font-medium text-gray-700 mb-1">Tùy chọn 1: Tải File Mới (TXT)</label>
                     <asp:FileUpload ID="fuFileTieuThuyet" runat="server" CssClass="w-full p-2 border border-gray-300 rounded-md file:mr-4 file:py-2 file:px-4 file:rounded-md file:border-0 file:bg-blue-500 file:text-white file:hover:bg-blue-600" />
                     <asp:RegularExpressionValidator ID="revFileTieuThuyet" runat="server" ControlToValidate="fuFileTieuThuyet" ErrorMessage="Chỉ chấp nhận:Text (.txt)." ValidationExpression="^.*\.(txt|TXT)$" Display="Dynamic" CssClass="text-red-600 text-xs mt-1" ValidationGroup="AddChapterValidation" Enabled="false"></asp:RegularExpressionValidator>
                 </div>
                 <div class="mb-4">
                     <label for="<%=txtNoiDungChu.ClientID %>" class="block text-sm font-medium text-gray-700 mb-1">Tùy chọn 2: Nhập Nội Dung Trực Tiếp</label>
                     <asp:TextBox ID="txtNoiDungChu" runat="server" TextMode="MultiLine" Rows="10" CssClass="w-full p-2 border border-gray-300 rounded-md focus:ring-2 focus:ring-blue-500 focus:border-blue-500 resize-y"></asp:TextBox>
                 </div>
                 <asp:CustomValidator ID="cvNovelContentRequired" runat="server" ErrorMessage="Phải cung cấp nội dung (tải file hoặc nhập)." Display="Dynamic" CssClass="text-red-600 text-xs" ValidationGroup="AddChapterValidation" OnServerValidate="cvNovelContentRequired_ServerValidate" ClientValidationFunction="validateNovelContent_Client" Enabled="false"/>
            </asp:Panel>

            <asp:Panel ID="pnlComicContent" runat="server" Visible="false">
                <h4 class="text-lg font-medium text-gray-800 mb-1">Nội Dung Truyện Tranh</h4>
                <p class="text-sm text-gray-500 mb-4">Kéo thả hoặc chọn nhiều ảnh. Kéo ảnh đã tải lên để sắp xếp thứ tự.</p>

                 <%-- Nút chọn file riêng biệt --%>
                 <button type="button" id="selectFilesBtn" class="mb-2 px-4 py-2 bg-indigo-600 text-white text-sm font-medium rounded-md hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500">
                     <i class="fas fa-images mr-2"></i> Chọn Ảnh Từ Máy Tính
                 </button>

                <div id="comicUploaderWrapper" class="mb-4">
                    <div id="comicUploader" class="dropzone"></div>
                </div>

                 <label class="block text-sm font-medium text-gray-700 mb-1 mt-4">Ảnh đã tải lên (Kéo thả để sắp xếp):</label>
                 <div id="dropzonePreviewContainer" class="dropzone-previews border border-dashed border-gray-300 p-2 rounded-lg min-h-[100px] bg-gray-50"></div>

                 <asp:CustomValidator ID="cvAnhTruyenRequired" runat="server"
                                    ErrorMessage="Bạn phải tải lên ít nhất một file ảnh hợp lệ."
                                    CssClass="text-red-600 text-xs mt-1" Display="Dynamic"
                                    ValidationGroup="AddChapterValidation"
                                    ClientValidationFunction="validateDropzoneFiles"
                                    OnServerValidate="cvAnhTruyenRequired_ServerValidate" Enabled="false"/>
            </asp:Panel>

            <div class="mt-8 flex justify-end space-x-4 border-t pt-6">
                <asp:Button ID="btnCancel" runat="server" Text="Hủy Bỏ" CssClass="px-4 py-2 bg-gray-500 text-white rounded-md hover:bg-gray-600 transition-colors" OnClick="btnCancel_Click" CausesValidation="false" />
                <asp:Button ID="btnAdd" runat="server" Text="Thêm Chương" CssClass="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 transition-colors" OnClick="btnAdd_Click" ValidationGroup="AddChapterValidation" OnClientClick="return prepareDropzoneSubmit();" />
            </div>
        </div>
    </div>

     <script type="text/javascript">

         // --- Configuration ---
         const comicUploadPanel = document.getElementById('<%= pnlComicContent.ClientID %>');
         const imageOrderHiddenField = document.getElementById('<%= hfComicImageOrder.ClientID %>');
         const dropzoneElementSelector = '#comicUploader';
         const selectFilesButtonSelector = '#selectFilesBtn';
         const dropzonePreviewContainer = document.getElementById('dropzonePreviewContainer');
         const uploadHandlerUrl = '<%= ResolveUrl("~/Handlers/UploadHandler.ashx") %>';
         const maxFilesizeMB = <%= MaxFileSizePerImageMb %>;
         const allowedImageExtensions = ('<%= string.Join(",", AllowedImageExtensions) %>').split(',');
         const combinedAcceptedFiles = allowedImageExtensions.map(ext => ext + ",image/" + ext.substring(1)).join(',');

         // --- State ---
         let uploadedFilesMap = {}; // { dropzoneFileUUID: tempFileName }
         let myDropzoneInstance = null; // Sử dụng tên này thay vì myDropzoneInstanceAdd
         let sortableInstance = null;

         // --- Initialization ---
         document.addEventListener('DOMContentLoaded', () => {
             console.log("DOM Content Loaded.");
             const isComicPanelVisible = comicUploadPanel && comicUploadPanel.offsetParent !== null;
             console.log("Comic Panel Visible on Load:", isComicPanelVisible);
             if (isComicPanelVisible) {
                 initializeDropzoneAndSortable(); // Gọi hàm này
             }
             const validationSummary = document.getElementById('<%= vsChapterForm.ClientID %>');
    if (validationSummary) validationSummary.style.display = 'none';
});

         // --- Dropzone & Sortable Setup ---
         // Hàm này được sử dụng cho trang Thêm Mới (dựa vào logic DOMContentLoaded)
         function initializeDropzoneAndSortable() {
             if (myDropzoneInstance) return;
             Dropzone.autoDiscover = false;

             const dzElement = document.querySelector(dropzoneElementSelector);
             const selectBtn = document.querySelector(selectFilesButtonSelector);
             if (!dzElement || !dropzonePreviewContainer || !selectBtn) { console.error('Dropzone/Preview/Button element not found.'); return; }

             try {
                 console.log('Initializing Dropzone...');
                 // Khởi tạo instance Dropzone (sử dụng myDropzoneInstance)
                 myDropzoneInstance = new Dropzone(dropzoneElementSelector, {
                     url: uploadHandlerUrl,
                     paramName: "file",
                     maxFilesize: maxFilesizeMB,
                     acceptedFiles: combinedAcceptedFiles,
                     addRemoveLinks: true,
                     dictDefaultMessage: `Kéo thả hoặc nhấp vào đây (Tối đa: ${maxFilesizeMB}MB/ảnh)`,
                     dictRemoveFile: "×",
                     dictCancelUpload: "Hủy",
                     dictInvalidFileType: "Loại file không hợp lệ.",
                     dictFileTooBig: `File quá lớn ({{filesize}}MB). Tối đa: {{maxFilesize}}MB.`,
                     dictResponseError: 'Lỗi server: {{statusCode}}',
                     previewsContainer: "#" + dropzonePreviewContainer.id,
                     clickable: [dropzoneElementSelector, selectFilesButtonSelector],
                     previewTemplate: `<div class="dz-preview dz-file-preview flex items-center p-3 mb-2 bg-white border border-gray-200 rounded-md shadow-sm relative cursor-grab min-h-[90px]"><div class="dz-image w-20 h-20 rounded border border-gray-200 flex-shrink-0 bg-gray-100 flex items-center justify-center"><img class="object-contain w-full h-full" data-dz-thumbnail /></div><div class="dz-details ml-4 flex-1 text-left overflow-hidden"><div class="dz-filename font-medium text-sm block truncate"><span data-dz-name></span></div><div class="dz-size text-xs text-gray-500 block"><span data-dz-size></span></div><div class="dz-error-message text-red-600 text-xs mt-1 font-medium block"><span data-dz-errormessage></span></div></div><div class="dz-progress h-1 bg-gray-200 rounded mt-1 overflow-hidden w-full absolute bottom-1 left-3 right-3"><span class="dz-upload bg-blue-500 h-full block transition-width duration-300 ease-linear" data-dz-uploadprogress style="width: 0%;"></span></div><div class="dz-success-mark text-green-500 absolute top-1/2 left-1/2 transform -translate-x-1/2 -translate-y-1/2 opacity-0 transition-opacity" style="display:none; font-size: 2rem;"><span>✔</span></div><div class="dz-error-mark text-red-500 absolute top-1/2 left-1/2 transform -translate-x-1/2 -translate-y-1/2 opacity-0 transition-opacity" style="display:none; font-size: 2rem;"><span>✘</span></div><a class="dz-remove" href="javascript:undefined;" data-dz-remove>×</a></div>`,

                     // *** THÊM DÒNG NÀY VÀO ĐÂY ***
                     parallelUploads: 1, // Chỉ upload 1 file mỗi lần
                     // -----------------------------

                     init: function () {
                         const dz = this;
                         console.log('Dropzone init function started.');
                         initializeSortable(dropzonePreviewContainer);

                         this.on("addedfile", file => { /* Reset UI */ if (file.previewElement) { file.previewElement.classList.remove('dz-success', 'dz-error'); const sm = file.previewElement.querySelector('.dz-success-mark'); const em = file.previewElement.querySelector('.dz-error-mark'); if (sm) { sm.style.display = 'none'; sm.style.opacity = '0'; } if (em) { em.style.display = 'none'; em.style.opacity = '0'; } const erm = file.previewElement.querySelector("[data-dz-errormessage]"); if (erm) erm.textContent = ''; const p = file.previewElement.querySelector('.dz-progress .dz-upload'); if (p) p.style.width = '0%'; p.parentNode.style.display = 'block'; } });
                         this.on("success", function (file, response) {
                             console.log("Event: success -", file.name, response);
                             if (response && response.fileName && !response.error && file.upload && file.upload.uuid) {
                                 file.serverFileName = response.fileName;
                                 uploadedFilesMap[file.upload.uuid] = response.fileName;
                                 // *** LƯU UUID VÀO ELEMENT DOM ***
                                 if (file.previewElement) {
                                     file.previewElement.setAttribute('data-dz-uuid', file.upload.uuid);
                                     console.log(`   -> Added data-dz-uuid=${file.upload.uuid} to preview element`);
                                     // Hiển thị UI thành công
                                     file.previewElement.classList.add('dz-success'); const mark = file.previewElement.querySelector('.dz-success-mark'); if (mark) { mark.style.display = 'block'; setTimeout(() => mark.style.opacity = '1', 10); } const p = file.previewElement.querySelector('.dz-progress'); if (p) p.style.display = 'none';
                                 }
                                 // --- Gọi update sau khi đã gán xong uuid ---
                                 console.log("   -> Calling updateHiddenFieldOrder from success event");
                                 updateHiddenFieldOrder();
                             } else { const msg = (response && response.error) || "Lỗi server response hoặc thiếu uuid"; console.error("   -> Invalid server response or missing uuid:", response, file.upload); dz.emit("error", file, msg); }
                         });
                         this.on("removedfile", function (file) {
                             console.log("Event: removedfile -", file.name);
                             if (file.upload && file.upload.uuid && uploadedFilesMap[file.upload.uuid]) {
                                 delete uploadedFilesMap[file.upload.uuid];
                                 console.log("   -> Calling updateHiddenFieldOrder from removedfile event");
                                 updateHiddenFieldOrder();
                             } else if (file.serverFileName) { // Fallback cho trường hợp không có UUID nhưng có serverFileName (hiếm)
                                 console.warn("   -> Removed file without UUID, attempting fallback using serverFileName:", file.serverFileName);
                                 let keyToDelete = null;
                                 for (const uuid in uploadedFilesMap) {
                                     if (uploadedFilesMap[uuid] === file.serverFileName) {
                                         keyToDelete = uuid;
                                         break;
                                     }
                                 }
                                 if (keyToDelete) {
                                     delete uploadedFilesMap[keyToDelete];
                                     console.log("   -> Fallback successful. Calling updateHiddenFieldOrder.");
                                     updateHiddenFieldOrder();
                                 } else {
                                     console.error("   -> Fallback failed. Could not find file in map.");
                                 }
                             } else {
                                 console.warn("   -> Removed file without UUID or serverFileName. Cannot update map reliably.");
                                 // Có thể cần gọi updateHiddenFieldOrder để ít nhất đồng bộ lại DOM
                                 updateHiddenFieldOrder();
                             }
                         });
                         this.on("error", function (file, errorMessage, xhr) {
                             console.error("Event: error -", file.name, errorMessage, xhr);
                             // Đảm bảo file lỗi cũng bị xóa khỏi map nếu nó đã được thêm vào
                             if (file.upload && file.upload.uuid && uploadedFilesMap[file.upload.uuid]) {
                                 console.warn("   -> Removing errored file from map:", file.upload.uuid);
                                 delete uploadedFilesMap[file.upload.uuid];
                                 updateHiddenFieldOrder(); // Cập nhật lại hidden field
                             }
                             // Hiển thị lỗi trên UI
                             if (file.previewElement) {
                                 file.previewElement.classList.add('dz-error');
                                 const errorMark = file.previewElement.querySelector('.dz-error-mark');
                                 if (errorMark) { errorMark.style.display = 'block'; setTimeout(() => errorMark.style.opacity = '1', 10); }
                                 const progress = file.previewElement.querySelector('.dz-progress');
                                 if (progress) progress.style.display = 'none';
                                 const msgContainer = file.previewElement.querySelector("[data-dz-errormessage]");
                                 if (msgContainer) {
                                     let displayError = errorMessage;
                                     // Cố gắng phân tích cú pháp lỗi JSON từ server nếu có
                                     if (typeof errorMessage === 'string') {
                                         try {
                                             const parsedError = JSON.parse(errorMessage);
                                             if (parsedError && parsedError.error) {
                                                 displayError = parsedError.error;
                                             }
                                         } catch (e) { /* Không phải JSON, giữ nguyên errorMessage */ }
                                     } else if (errorMessage.error) { // Nếu errorMessage là object có property error
                                         displayError = errorMessage.error;
                                     }
                                     msgContainer.textContent = displayError;
                                 }
                             }
                         });
                         this.on("queuecomplete", () => {
                             console.log("Event: queuecomplete");
                             // Đảm bảo rằng updateHiddenFieldOrder được gọi sau khi hàng đợi hoàn thành
                             // để phản ánh đúng trạng thái cuối cùng của các file thành công.
                             console.log("   -> Calling updateHiddenFieldOrder from queuecomplete event");
                             updateHiddenFieldOrder();
                         });
                         console.log('Dropzone init function finished.');
                     } // end init
                 });
                 console.log('Dropzone initialized successfully.');
             } catch (e) { console.error("Error initializing Dropzone:", e); alert("Lỗi khởi tạo khu vực upload ảnh."); }
         }


         function initializeSortable(container) {
             if (!container || typeof Sortable === 'undefined') return;
             if (sortableInstance) { sortableInstance.destroy(); sortableInstance = null; }
             try {
                 console.log('Initializing SortableJS on:', container);
                 sortableInstance = Sortable.create(container, {
                     animation: 150, draggable: ".dz-preview", filter: ".dz-error", preventOnFilter: false, ghostClass: 'sortable-ghost', chosenClass: 'sortable-chosen',
                     onEnd: () => { console.log('Event: Sortable onEnd'); console.log("   -> Calling updateHiddenFieldOrder from Sortable onEnd event"); updateHiddenFieldOrder(); }
                 });
             } catch (e) { console.error("Failed to initialize Sortable:", e); }
         }

         // --- Helpers ---
         // *** HÀM UPDATE ĐÃ SỬA LẠI CÁCH LẤY FILENAME ***
         function updateHiddenFieldOrder() {
             console.log("--- updateHiddenFieldOrder START ---");
             if (!imageOrderHiddenField || !dropzonePreviewContainer) { console.error("updateHiddenFieldOrder: Critical elements missing."); return; }
             let orderedTempFileNames = [];
             // Lấy các preview element thành công và không bị lỗi THEO THỨ TỰ DOM
             const previewElements = dropzonePreviewContainer.querySelectorAll('.dz-preview.dz-success');
             console.log(`updateHiddenFieldOrder: Found ${previewElements.length} successful preview elements.`);

             previewElements.forEach((element, index) => {
                 // **Lấy UUID từ data attribute**
                 const fileUUID = element.getAttribute('data-dz-uuid');
                 let tempFileName = null;

                 if (fileUUID) {
                     // **Tra cứu tên file tạm trong map bằng UUID**
                     if (uploadedFilesMap[fileUUID]) {
                         tempFileName = uploadedFilesMap[fileUUID];
                         console.log(`  Element ${index}: Found UUID ${fileUUID}, got serverFileName '${tempFileName}' from map.`);
                         orderedTempFileNames.push(tempFileName);
                     } else {
                         // Trường hợp lạ: có UUID nhưng không có trong map (có thể là file lỗi đã bị xóa khỏi map?)
                         console.warn(`  Element ${index}: Found UUID ${fileUUID} but file not found in uploadedFilesMap. Skipping.`);
                     }
                 } else {
                     // Preview element này không có UUID (lỗi hoặc đang upload?)
                     console.warn(`  Element ${index}: Could not find data-dz-uuid attribute. Skipping.`);
                 }
             });

             const newValue = orderedTempFileNames.join(',');
             if (imageOrderHiddenField.value !== newValue) {
                 imageOrderHiddenField.value = newValue;
                 console.log('===> Hidden field UPDATED:', imageOrderHiddenField.value);
                 // Trigger client validation for the dropzone (important!)
                 if (typeof Page_ClientValidate === 'function') {
                     const validator = document.getElementById('<%= cvAnhTruyenRequired.ClientID %>');
             if (validator) {
                 console.log('   -> Re-validating Dropzone validator.');
                 ValidatorValidate(validator); // Validate lại custom validator
                 // Cập nhật hiển thị của ValidationSummary nếu cần
                 const vs = document.getElementById('<%= vsChapterForm.ClientID %>');
                 if (vs && typeof ValidationSummaryOnSubmit === 'function') {
                      ValidationSummaryOnSubmit(); // Cập nhật Validation Summary
                      vs.style.display = vs.style.display === 'none' ? 'none' : 'block'; // Giữ nguyên trạng thái hiển thị
                 }
             }
         }
    } else {
         console.log('Hidden field value UNCHANGED:', imageOrderHiddenField.value);
    }
    console.log("--- updateHiddenFieldOrder END ---");
}


// --- Client Validation ---
function validateDropzoneFiles(source, args) {
     console.log("validateDropzoneFiles called.");
     if (!imageOrderHiddenField) {
         console.error("Dropzone validator: Hidden field not found.");
         args.IsValid = false;
         return;
     }
     const hasFiles = imageOrderHiddenField.value.trim() !== '';
     args.IsValid = hasFiles;
     console.log("Dropzone validation result:", args.IsValid, "Value:", imageOrderHiddenField.value);
     // Không cần gọi updateHiddenFieldOrder ở đây, nó được gọi từ các event khác
}

function validateNovelContent_Client(source, args) {
     console.log("validateNovelContent_Client called.");
     const fileUpload = document.getElementById('<%= fuFileTieuThuyet.ClientID %>');
     const textArea = document.getElementById('<%= txtNoiDungChu.ClientID %>');
     args.IsValid = (fileUpload && fileUpload.value !== '') || (textArea && textArea.value.trim() !== '');
     console.log("Novel content validation result:", args.IsValid);
}

// --- Form Submission ---
function prepareDropzoneSubmit() {
    // Bỏ Alert đi khi đã chắc chắn nó chạy
    // alert("prepareDropzoneSubmit function called!");
    console.log('--- prepareDropzoneSubmit START ---');
    console.log('   -> Calling updateHiddenFieldOrder before validation...');
    updateHiddenFieldOrder(); // Cập nhật lần cuối TRƯỚC KHI validate

    console.log('   -> Checking Page_ClientValidate...');
    if (typeof Page_ClientValidate === 'function') {
        if (!Page_ClientValidate('AddChapterValidation')) {
            console.log('   -> Client validation FAILED.');
            const vs = document.getElementById('<%= vsChapterForm.ClientID %>'); if(vs) vs.style.display = 'block'; // Hiển thị summary nếu lỗi
            // Kích hoạt lại nút nếu validation fail
            const btnAddFailed = document.getElementById('<%= btnAdd.ClientID %>'); const btnCancelFailed = document.getElementById('<%= btnCancel.ClientID %>');
            if(btnAddFailed) { btnAddFailed.disabled = false; btnAddFailed.classList.remove('loading-spinner'); btnAddFailed.value = 'Thêm Chương'; }
            if(btnCancelFailed) { btnCancelFailed.disabled = false; }
            return false; // Ngăn chặn submit
        }
        console.log('   -> Client validation PASSED.');
        const vs = document.getElementById('<%= vsChapterForm.ClientID %>'); if(vs) vs.style.display = 'none'; // Ẩn summary nếu pass
    } else { console.warn('   -> Page_ClientValidate function not found.'); }

    // Disable buttons và hiển thị loading
    console.log('   -> Disabling buttons and showing spinner...');
    const btnAdd = document.getElementById('<%= btnAdd.ClientID %>'); const btnCancel = document.getElementById('<%= btnCancel.ClientID %>');
             // Dùng setTimeout để đảm bảo các tác vụ UI (như ẩn/hiện validation summary) hoàn thành trước khi disable nút
             setTimeout(() => {
                 if (btnAdd) {
                     btnAdd.disabled = true;
                     btnAdd.classList.add('loading-spinner');
                     // Cập nhật giá trị của nút (Text) thay vì value cho <asp:Button>
                     btnAdd.setAttribute('value', 'Đang thêm...'); // Hoặc btnAdd.textContent nếu render ra <button>
                     // Kiểm tra xem thực sự render ra thẻ gì
                     if (btnAdd.tagName === 'INPUT') {
                         btnAdd.value = 'Đang thêm...';
                     } else {
                         // Nếu là <button>, thường sẽ có text bên trong
                         // btnAdd.innerHTML = 'Đang thêm... <span class="loading-spinner-inner"></span>'; // Cần định nghĩa spinner-inner
                     }
                 }
                 if (btnCancel) { btnCancel.disabled = true; }
             }, 0);

             console.log("   -> Final hidden field value before submit:", imageOrderHiddenField ? imageOrderHiddenField.value : 'N/A');
             console.log('--- prepareDropzoneSubmit END - Allowing submit ---');
             return true; // Cho phép submit
         }

         // FormatFileSize (Giữ nguyên)
         function formatFileSize(bytes) {
             if (bytes < 0 || typeof bytes !== 'number') {
                 return 'N/A';
             }
             if (bytes === 0) {
                 return '0 Bytes';
             }
             const k = 1024;
             const sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB', 'PB', 'EB', 'ZB', 'YB'];
             const i = Math.max(0, Math.min(Math.floor(Math.log(bytes) / Math.log(k)), sizes.length - 1));
             const formattedSize = parseFloat((bytes / Math.pow(k, i)).toFixed(1));
             return formattedSize + ' ' + sizes[i];
         }

     </script>


</asp:Content>