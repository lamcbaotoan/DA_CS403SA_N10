<%@ Page Title="Đánh Giá Sách" Language="C#" MasterPageFile="~/WebForm/User/User.Master" AutoEventWireup="true" CodeBehind="danhgia.aspx.cs" Inherits="Webebook.WebForm.User.danhgia" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.0.0/css/all.min.css" />
    <style>
        /* CSS giữ nguyên */
        .star-rating { display: inline-flex; vertical-align: middle; }
        .star-rating input[type="radio"] { position: absolute; opacity: 0; pointer-events: none; }
        .star-rating label { font-size: 2em; color: #d1d5db; cursor: pointer; transition: color 0.2s ease-in-out; padding: 0 0.1em; order: 1; }
        .star-rating input[id^="rating1"] + label { order: 5; }
        .star-rating input[id^="rating2"] + label { order: 4; }
        .star-rating input[id^="rating3"] + label { order: 3; }
        .star-rating input[id^="rating4"] + label { order: 2; }
        .star-rating input[id^="rating5"] + label { order: 1; }
        .star-rating label:hover, .star-rating label:hover ~ label { color: #facc15; }
        .star-rating input[type="radio"]:checked ~ label { color: #f59e0b; }
        .star-rating:hover input[type="radio"]:not(:checked) ~ label { color: #d1d5db; }
        .star-rating:hover label:hover, .star-rating:hover label:hover ~ label { color: #facc15 !important; }
        .message-error { display: block; margin-bottom: 1rem; padding: 0.75rem; border-radius: 0.375rem; border: 1px solid #fca5a5; background-color: #fef2f2; color: #b91c1c; font-size: 0.875rem; }
        .message-success { display: block; margin-bottom: 1rem; padding: 0.75rem; border-radius: 0.375rem; border: 1px solid #6ee7b7; background-color: #ecfdf5; color: #047857; font-size: 0.875rem; }
        .validation-error { color: #dc2626; font-size: 0.75rem; margin-top: 0.25rem; }
        #selectedRatingDisplay { margin-left: 0.5rem; font-size: 0.9rem; color: #4b5563; vertical-align: middle; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="container mx-auto px-4 lg:px-8 py-8">
        <h2 class="text-3xl font-bold text-gray-800 mb-6 border-b pb-3">Viết Đánh Giá</h2>
        <asp:Label ID="lblMessage" runat="server" EnableViewState="false" Visible="false"></asp:Label>

        <asp:Panel ID="pnlReviewForm" runat="server" Visible="true" CssClass="max-w-2xl mx-auto bg-white p-6 rounded-lg shadow-md border border-gray-200">
            <%-- Thông tin sách --%>
            <div class="flex items-start space-x-4 mb-6 p-4 bg-gray-50 rounded-md border border-gray-200">
                <asp:Image ID="imgBookCover" runat="server" CssClass="w-16 h-24 object-cover rounded border border-gray-300 flex-shrink-0" AlternateText="Bìa sách" />
                <div>
                    <p class="text-xs text-gray-500">Bạn đang đánh giá cho sách:</p>
                    <h3 class="text-lg font-semibold text-gray-800 mt-1">
                        <asp:Label ID="lblBookTitle" runat="server" Text="Đang tải..."></asp:Label>
                    </h3>
                </div>
            </div>

            <%-- Form đánh giá --%>
            <div class="space-y-4">
                <%-- Phần chọn điểm --%>
                <div>
                    <label class="block text-sm font-medium text-gray-700 mb-2">Điểm đánh giá của bạn:</label>
                    <div>
                        <div class="star-rating">
                            <%-- **** BỎ runat="server" và ClientIDMode khỏi radio buttons **** --%>
                            <input type="radio" id="rating5" name="bookRating" value="5" /><label for="rating5" title="Tuyệt vời (5 sao)">&#9733;</label>
                            <input type="radio" id="rating4" name="bookRating" value="4" /><label for="rating4" title="Rất tốt (4 sao)">&#9733;</label>
                            <input type="radio" id="rating3" name="bookRating" value="3" /><label for="rating3" title="Bình thường (3 sao)">&#9733;</label>
                            <input type="radio" id="rating2" name="bookRating" value="2" /><label for="rating2" title="Không tệ (2 sao)">&#9733;</label>
                            <input type="radio" id="rating1" name="bookRating" value="1" /><label for="rating1" title="Tệ (1 sao)">&#9733;</label>
                        </div>
                        <span id="selectedRatingDisplay" class="ml-2 text-sm text-gray-600 align-middle">(Chưa chọn)</span>
                    </div>

                     <%-- CustomValidator (Giữ nguyên) --%>
                    <asp:CustomValidator ID="cvRating" runat="server"
                        ErrorMessage="Vui lòng chọn điểm đánh giá (1-5 sao)."
                        CssClass="validation-error" Display="Dynamic" SetFocusOnError="true"
                        ClientValidationFunction="validateRatingSelection"
                        OnServerValidate="cvRating_ServerValidate">
                    </asp:CustomValidator>

                    <%-- JavaScript (Giữ nguyên với các log debug) --%>
                    <script type="text/javascript">
                        console.log('SCRIPT START: Script block is executing.');
                        function validateRatingSelection(source, args) {
                            console.log('VALIDATE FUNC: validateRatingSelection called.');
                            const visibleRadios = document.querySelectorAll('input[type="radio"][name="bookRating"]');
                            let isSelected = false;
                            visibleRadios.forEach(radio => { if (radio.checked) { isSelected = true; } });
                            args.IsValid = isSelected;
                            if (typeof (ValidatorUpdateDisplay) === 'function') { ValidatorUpdateDisplay(source); }
                            else { console.warn('VALIDATE FUNC: ValidatorUpdateDisplay function not found.'); }
                            console.log('VALIDATE FUNC: Setting args.IsValid to:', isSelected);
                        }
                        document.addEventListener('DOMContentLoaded', function () {
                            console.log('DOM LOADED: DOMContentLoaded event fired.');
                            const ratingContainer = document.querySelector('.star-rating');
                            if (!ratingContainer) { console.error('DOM LOADED ERROR: Cannot find .star-rating container!'); return; }
                            const visibleRadios = ratingContainer.querySelectorAll('input[type="radio"][name="bookRating"]');
                            const displaySpan = document.getElementById('selectedRatingDisplay');
                            const cvValidator = document.getElementById('<%= cvRating.ClientID %>'); // Vẫn cần ClientID của validator
                            console.log(`DOM LOADED: Found ${visibleRadios.length} radio buttons with name="bookRating".`); // <<<< QUAN SÁT SỐ LƯỢNG NÀY
                            if (!displaySpan) { console.error('DOM LOADED ERROR: Cannot find #selectedRatingDisplay span!'); } else { console.log('DOM LOADED: Found displaySpan.'); }
                            if (!cvValidator) { console.warn('DOM LOADED WARN: Cannot find validator #<%= cvRating.ClientID %>.'); } else { console.log('DOM LOADED: Found cvValidator.'); }

                            function updateRatingDisplay() { /* ... Giữ nguyên hàm này ... */
                                console.log('UPDATE DISPLAY: updateRatingDisplay called.');
                                if (!displaySpan) { console.error('UPDATE DISPLAY ERROR: displaySpan is null!'); return; }
                                let selectedValue = ''; let checkedRadioId = null;
                                visibleRadios.forEach(radio => { if (radio.checked) { selectedValue = radio.value; checkedRadioId = radio.id; } });
                                console.log(`UPDATE DISPLAY: Found checked value: '${selectedValue}', ID: ${checkedRadioId}`);
                                if (selectedValue) { displaySpan.textContent = `(Đã chọn: ${selectedValue} sao)`; console.log('UPDATE DISPLAY: Text updated.'); }
                                else { displaySpan.textContent = '(Chưa chọn)'; console.log('UPDATE DISPLAY: Text reset to default.'); }
                            }

                            if (visibleRadios.length > 0) {
                                visibleRadios.forEach(radio => {
                                    console.log(`ATTACH LISTENER: Adding change listener to #${radio.id}`);
                                    radio.addEventListener('change', function () {
                                        console.log(`EVENT FIRED: Change event on #${this.id}, Value: ${this.value}, Checked: ${this.checked}`);
                                        updateRatingDisplay();
                                        if (cvValidator && typeof cvValidator.isvalid !== 'undefined' && cvValidator.isvalid === false) {
                                            console.log('EVENT FIRED: Resetting validator state.'); cvValidator.isvalid = true;
                                            if (typeof (ValidatorUpdateDisplay) === 'function') { try { ValidatorUpdateDisplay(cvValidator); } catch (e) { console.error('EVENT FIRED ERROR: Calling ValidatorUpdateDisplay failed:', e); } }
                                            else { console.warn('EVENT FIRED WARN: ValidatorUpdateDisplay function not found.'); }
                                        } else if (!cvValidator) { console.warn('EVENT FIRED WARN: Cannot reset validator - cvValidator not found.'); }
                                    });
                                });
                            } else { console.error('ATTACH LISTENER ERROR: No radio buttons found to attach listeners to!'); }
                            console.log('INIT DISPLAY: Updating initial rating display.'); updateRatingDisplay();
                        });
                        console.log('SCRIPT END: Script block finished execution.');
                    </script>
                </div>

                <%-- Phần nhập nhận xét --%>
                <div>
                    <label for="<%=txtComment.ClientID %>" class="block text-sm font-medium text-gray-700">Nhận xét chi tiết (tuỳ chọn):</label>
                    <asp:TextBox ID="txtComment" runat="server" TextMode="MultiLine" Rows="5" MaxLength="2000"
                        CssClass="mt-1 block w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-indigo-500 focus:border-indigo-500 sm:text-sm"></asp:TextBox>
                </div>

                <%-- Nút Gửi --%>
                <div class="pt-4 text-right border-t border-gray-200 mt-4">
                    <asp:Button ID="btnSubmitReview" runat="server" Text="Gửi Đánh Giá"
                        OnClick="btnSubmitReview_Click"
                        CssClass="inline-flex justify-center py-2 px-4 border border-transparent shadow-sm text-sm font-medium rounded-md text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-indigo-500 disabled:opacity-50 disabled:cursor-not-allowed"
                        CausesValidation="true"
                        />
                </div>
            </div>
        </asp:Panel>

        <%-- Link quay lại --%>
        <div class="mt-6 text-center">
            <asp:HyperLink ID="hlBack" runat="server" NavigateUrl="~/WebForm/User/lichsumuahang.aspx"
                CssClass="text-sm text-indigo-600 hover:text-indigo-800 hover:underline">
                <i class="fas fa-arrow-left mr-1"></i> Quay lại
            </asp:HyperLink>
        </div>
    </div>
</asp:Content>