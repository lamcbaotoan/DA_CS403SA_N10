<%@ Page Title="Quản Lý Đánh Giá" Language="C#" MasterPageFile="~/WebForm/Admin/Admin.Master" AutoEventWireup="true" CodeBehind="QuanLyDanhGia.aspx.cs" Inherits="Webebook.WebForm.Admin.QuanLyDanhGia" %>
<%@ MasterType VirtualPath="~/WebForm/Admin/Admin.Master" %>

<asp:Content ID="Content1" ContentPlaceHolderID="head" runat="server">
    <%-- Các thư viện cần thiết --%>
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.0.0/css/all.min.css" />
    <script src="https://cdn.jsdelivr.net/npm/chart.js@3.7.0/dist/chart.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/sweetalert2@11"></script>

    <%-- CSS tùy chỉnh cho trang --%>
    <style>
        /* === Star Rating Edit Styles === */
        .star-rating-edit { display: inline-flex; flex-direction: row-reverse; justify-content: flex-end; }
        .star-rating-edit input[type="radio"] { display: none; }
        .star-rating-edit label { font-size: 1.8em; color: #d1d5db; cursor: pointer; transition: color 0.2s ease-in-out; padding: 0 0.1em; }
        .star-rating-edit input[type="radio"]:checked ~ label { color: #f59e0b; }
        .star-rating-edit:hover label { color: #f59e0b; }
        .star-rating-edit label:hover ~ label { color: #d1d5db; }
        .star-rating-edit:not(:hover) input[type="radio"]:checked ~ label { color: #f59e0b; }

        /* === Tooltip for Truncated Text === */
        .truncate-comment[title] { position: relative; cursor: help; }
        .truncate-comment[title]:hover::after {
            content: attr(title);
            position: absolute; left: 50%; transform: translateX(-50%); bottom: 110%;
            z-index: 10; background: rgba(17, 24, 39, 0.9); color: white;
            padding: 6px 10px; border-radius: 4px; font-size: 0.8rem;
            white-space: normal; width: max-content; max-width: 350px;
            pointer-events: none; box-shadow: 0 2px 5px rgba(0,0,0,0.2);
            opacity: 0; visibility: hidden; transition: opacity 0.2s ease-in-out, visibility 0.2s ease-in-out;
        }
        .truncate-comment[title]:hover::after { opacity: 1; visibility: visible; }
        .truncate-comment { display: block; max-width: 300px; white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }

        /* === Edit Panel Animation === */
        #<%= pnlEditReview.ClientID %> {
            opacity: 0; transform: translateY(15px) scale(0.98);
            transition: opacity 0.3s ease-out, transform 0.3s ease-out;
            will-change: opacity, transform;
        }
        #<%= pnlEditReview.ClientID %>.panel-visible {
            opacity: 1; transform: translateY(0) scale(1);
        }

        /* === Subtle Button Hover Effect === */
        .btn-hover-effect { transition: transform 0.2s ease-out, box-shadow 0.2s ease-out; }
        .btn-hover-effect:hover { transform: translateY(-2px) scale(1.03); box-shadow: 0 4px 10px rgba(0, 0, 0, 0.15); }

        /* === GridView Row Hover === */
        #<%= gvReviews.ClientID %> tbody tr:hover { background-color: #f9fafb; }
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="ContentPlaceHolder1" runat="server">
    <div class="container mx-auto px-4 py-8">
        <h2 class="text-3xl font-bold text-gray-800 mb-6 border-b pb-3">Quản Lý & Thống Kê Đánh Giá</h2>
        
        <asp:Panel ID="pnlAdminMessage" runat="server" Visible="false" CssClass="mb-4">
            <asp:Label ID="lblAdminMessage" runat="server" EnableViewState="false"></asp:Label>
        </asp:Panel>

        <%-- === SECTION 1: THỐNG KÊ === --%>
        <asp:Panel ID="pnlStatistics" runat="server" CssClass="mb-8 transition-opacity duration-300 ease-in-out">
            <h3 class="text-xl font-semibold text-gray-700 mb-4">Tổng Quan</h3>
            <div class="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-5 mb-6">
                <div class="bg-white p-5 rounded-xl shadow-md border border-gray-200 text-center transform transition duration-300 hover:scale-105 hover:shadow-lg">
                    <div class="text-sm font-medium text-gray-500 uppercase tracking-wider">Tổng đánh giá</div>
                    <div class="mt-2 text-4xl font-semibold text-blue-600 flex items-center justify-center gap-2">
                        <i class="fas fa-comments text-2xl text-blue-400"></i>
                        <asp:Label ID="lblTotalReviews" runat="server" Text="0"></asp:Label>
                    </div>
                </div>
                <div class="bg-white p-5 rounded-xl shadow-md border border-gray-200 text-center transform transition duration-300 hover:scale-105 hover:shadow-lg">
                    <div class="text-sm font-medium text-gray-500 uppercase tracking-wider">Điểm TB</div>
                    <div class="mt-2 text-4xl font-semibold text-green-600 flex items-center justify-center gap-2">
                        <i class="fas fa-star text-3xl text-yellow-400"></i>
                        <asp:Label ID="lblOverallAverage" runat="server" Text="N/A"></asp:Label>
                    </div>
                </div>
            </div>
            <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
                <asp:Panel ID="pnlAveragePerBook" runat="server" Visible="false" CssClass="bg-white p-5 rounded-xl shadow-md border border-gray-200">
                    <h4 class="text-lg font-semibold text-gray-700 mb-3">Top Sách Theo Đánh Giá</h4>
                    <div class="max-h-72 overflow-y-auto text-sm">
                        <asp:Repeater ID="rptAveragePerBook" runat="server">
                            <HeaderTemplate><ul class="divide-y divide-gray-100"></HeaderTemplate>
                            <ItemTemplate>
                                <li class="py-2.5 flex justify-between items-center gap-3 hover:bg-gray-50 px-1 rounded">
                                    <span class="text-gray-800 truncate pr-2 flex-grow" title='<%# Server.HtmlEncode(Eval("TenSach").ToString()) %>'><%# TruncateString(Eval("TenSach"), 45) %></span>
                                    <span class="font-semibold text-amber-500 whitespace-nowrap flex-shrink-0 flex items-center gap-1">
                                        <%# Eval("AvgRating", "{0:N1}") %> <i class='fas fa-star fa-xs'></i>
                                        <span class="text-xs text-gray-400 ml-1 font-normal">(<%# Eval("ReviewCount") %> lượt)</span>
                                    </span>
                                </li>
                            </ItemTemplate>
                            <FooterTemplate></ul></FooterTemplate>
                        </asp:Repeater>
                    </div>
                </asp:Panel>
                <asp:Panel ID="pnlChart" runat="server" Visible="false" CssClass="bg-white p-5 rounded-xl shadow-md border border-gray-200">
                    <h4 class="text-lg font-semibold text-gray-700 mb-3">Phân Bố Điểm</h4>
                    <div class="relative h-72"><canvas id="ratingsChartCanvas"></canvas></div>
                </asp:Panel>
            </div>
        </asp:Panel>

        <%-- === SECTION 2: BỘ LỌC === --%>
        <asp:Panel ID="pnlFilters" runat="server" CssClass="bg-gray-50 p-5 rounded-xl border border-gray-200 mb-6 shadow-sm transition-opacity duration-300 ease-in-out">
            <div class="grid grid-cols-1 md:grid-cols-3 gap-4 items-end">
                <div>
                    <label for="<%= txtSearchUserBook.ClientID %>" class="block text-sm font-medium text-gray-700 mb-1">Tìm Tên sách/Người dùng</label>
                    <div class="relative">
                        <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                            <i class="fas fa-search text-gray-400"></i>
                        </div>
                        <asp:TextBox ID="txtSearchUserBook" runat="server" CssClass="pl-10 w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500" placeholder="Nhập từ khóa..."></asp:TextBox>
                    </div>
                </div>
                <div>
                    <label for="<%= ddlRatingFilter.ClientID %>" class="block text-sm font-medium text-gray-700 mb-1">Lọc theo điểm</label>
                    <asp:DropDownList ID="ddlRatingFilter" runat="server" CssClass="w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 bg-white"></asp:DropDownList>
                </div>
                <div class="flex gap-3 items-center justify-start md:justify-end pt-4 md:pt-0">
                    <asp:Button ID="btnSearch" runat="server" Text="Lọc" OnClick="btnSearch_Click" CssClass="btn-hover-effect inline-flex justify-center items-center px-5 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-indigo-600 hover:bg-indigo-700 focus:outline-none focus:ring-2 focus:ring-offset-1 focus:ring-indigo-500" />
                    <asp:Button ID="btnReset" runat="server" Text="Reset" OnClick="btnReset_Click" CausesValidation="false" CssClass="btn-hover-effect inline-flex justify-center items-center px-5 py-2 border border-gray-300 rounded-md shadow-sm text-sm font-medium text-gray-700 bg-white hover:bg-gray-100 focus:outline-none focus:ring-2 focus:ring-offset-1 focus:ring-gray-400" />
                </div>
            </div>
        </asp:Panel>

        <%-- === SECTION 3: DANH SÁCH ĐÁNH GIÁ === --%>
        <asp:Panel ID="pnlReviewList" runat="server" CssClass="bg-white shadow-md border border-gray-200 rounded-xl overflow-hidden transition-opacity duration-300 ease-in-out">
            <h3 class="text-xl font-semibold text-gray-700 p-5 border-b border-gray-200">Danh Sách Đánh Giá</h3>
            <div class="overflow-x-auto">
                <asp:GridView ID="gvReviews" runat="server" AutoGenerateColumns="False" CssClass="min-w-full divide-y divide-gray-200"
                    AllowPaging="True" PageSize="10" DataKeyNames="IDDanhGia"
                    OnPageIndexChanging="gvReviews_PageIndexChanging"
                    OnRowCommand="gvReviews_RowCommand"
                    OnRowDataBound="gvReviews_RowDataBound"
                    EmptyDataText="<div class='text-center py-10 text-gray-500'><i class='fas fa-inbox fa-2x mb-2'></i><p>Không có đánh giá nào phù hợp.</p></div>" GridLines="None">
                    <HeaderStyle CssClass="bg-gray-100 text-xs font-semibold text-gray-600 uppercase tracking-wider" />
                    <RowStyle CssClass="bg-white text-sm" />
                    <AlternatingRowStyle CssClass="bg-gray-50 text-sm" />
                    <PagerStyle CssClass="bg-gray-50 px-4 py-3 border-t border-gray-200 text-sm" HorizontalAlign="Right" />
                    <EmptyDataRowStyle CssClass="bg-white" />
                    <Columns>
                        <asp:BoundField DataField="IDDanhGia" HeaderText="ID" ItemStyle-CssClass="px-3 py-3 whitespace-nowrap text-xs text-gray-500 text-center" HeaderStyle-CssClass="px-3 py-3 text-center w-12" />
                        <asp:TemplateField HeaderText="Người dùng" HeaderStyle-CssClass="px-4 py-3 text-left">
                            <ItemTemplate><span class="font-medium text-gray-800"><%# Server.HtmlEncode(Eval("Ten")?.ToString() ?? "N/A") %></span></ItemTemplate>
                            <ItemStyle CssClass="px-4 py-3 whitespace-nowrap" />
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Sách" HeaderStyle-CssClass="px-4 py-3 text-left">
                            <ItemTemplate><span class="text-gray-700 truncate-comment" title='<%# Server.HtmlEncode(Eval("TenSach").ToString()) %>'><%# Server.HtmlEncode(Eval("TenSach").ToString()) %></span></ItemTemplate>
                            <ItemStyle CssClass="px-4 py-3" />
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Điểm" HeaderStyle-CssClass="px-4 py-3 text-center w-28">
                            <ItemTemplate><%# GetStarRatingHtml(Eval("Diem")) %></ItemTemplate>
                            <ItemStyle CssClass="px-4 py-3 whitespace-nowrap text-center" />
                        </asp:TemplateField>
                        <asp:TemplateField HeaderText="Nhận xét" HeaderStyle-CssClass="px-4 py-3 text-left min-w-[250px]">
                            <ItemTemplate><span class="text-gray-600 truncate-comment" title='<%# Server.HtmlEncode(Eval("NhanXet")?.ToString() ?? "") %>'><%# TruncateString(Eval("NhanXet"), 100) %></span></ItemTemplate>
                            <ItemStyle CssClass="px-4 py-3" />
                        </asp:TemplateField>
                        <asp:BoundField DataField="NgayDanhGia" HeaderText="Ngày" DataFormatString="{0:dd/MM/yy HH:mm}" ItemStyle-CssClass="px-4 py-3 whitespace-nowrap text-xs text-gray-500" HeaderStyle-CssClass="px-4 py-3 text-left" />
                        <asp:TemplateField HeaderText="Hành động" ItemStyle-CssClass="px-4 py-3 whitespace-nowrap text-right text-sm font-medium" HeaderStyle-CssClass="px-4 py-3 text-right">
                            <ItemTemplate>
                                <asp:LinkButton ID="lnkEdit" runat="server" CommandName="EditReview" CommandArgument='<%# Eval("IDDanhGia") %>' CssClass="text-indigo-600 hover:text-indigo-800 mr-3 transition duration-150 ease-in-out" ToolTip="Sửa"><i class="fas fa-pencil-alt fa-fw"></i></asp:LinkButton>
                                <asp:LinkButton ID="lnkDelete" runat="server" CommandName="CustomDelete" CommandArgument='<%# Eval("IDDanhGia") %>' CssClass="text-red-600 hover:text-red-800 transition duration-150 ease-in-out" ToolTip="Xóa" CausesValidation="false" UseSubmitBehavior="false">
                                    <i class="fas fa-trash-alt fa-fw"></i>
                                </asp:LinkButton>
                            </ItemTemplate>
                        </asp:TemplateField>
                    </Columns>
                </asp:GridView>
            </div>
        </asp:Panel>

        <%-- === SECTION 4: PANEL SỬA ĐÁNH GIÁ === --%>
        <asp:Panel ID="pnlEditReview" runat="server" Visible="false" CssClass="mt-8 bg-white p-6 rounded-xl shadow-xl border border-gray-300 max-w-2xl mx-auto">
            <h3 class="text-xl font-semibold text-gray-800 mb-5 border-b pb-3">Chỉnh Sửa Đánh Giá</h3>
            <asp:HiddenField ID="hfEditReviewId" runat="server" />
            <div class="space-y-5">
                <div class="text-sm p-3 bg-gray-100 rounded-lg border border-gray-200">
                    <span class="font-medium text-gray-600">Người dùng:</span>
                    <asp:Label ID="lblEditUser" runat="server" CssClass="ml-2 text-gray-900 font-semibold"></asp:Label><br />
                    <span class="font-medium text-gray-600 mt-1 inline-block">Sách:</span>
                    <asp:Label ID="lblEditBook" runat="server" CssClass="ml-2 text-gray-900 font-semibold"></asp:Label>
                </div>
                <div>
                    <label class="block text-sm font-medium text-gray-700 mb-2">Điểm đánh giá:</label>
                    <div class="star-rating-edit">
                        <asp:RadioButtonList ID="rblEditRating" runat="server" RepeatDirection="Horizontal" RepeatLayout="Flow" CssClass="star-rating-inner flex flex-row-reverse">
                            <asp:ListItem Value="5" Text="<i class='far fa-star'></i>" title="5 stars" CssClass="hidden"></asp:ListItem>
                            <asp:ListItem Value="4" Text="<i class='far fa-star'></i>" title="4 stars" CssClass="hidden"></asp:ListItem>
                            <asp:ListItem Value="3" Text="<i class='far fa-star'></i>" title="3 stars" CssClass="hidden"></asp:ListItem>
                            <asp:ListItem Value="2" Text="<i class='far fa-star'></i>" title="2 stars" CssClass="hidden"></asp:ListItem>
                            <asp:ListItem Value="1" Text="<i class='far fa-star'></i>" title="1 star" CssClass="hidden"></asp:ListItem>
                        </asp:RadioButtonList>
                    </div>
                    <asp:RequiredFieldValidator ID="rfvEditRating" runat="server" ControlToValidate="rblEditRating" InitialValue="" ErrorMessage="Vui lòng chọn điểm." CssClass="text-red-600 text-xs mt-1 block" Display="Dynamic" ValidationGroup="EditReviewGroup" />
                </div>
                <div>
                    <label for="<%=txtEditComment.ClientID %>" class="block text-sm font-medium text-gray-700 mb-1">Nội dung nhận xét:</label>
                    <asp:TextBox ID="txtEditComment" runat="server" TextMode="MultiLine" Rows="5" CssClass="mt-1 w-full px-3 py-2 border border-gray-300 rounded-md shadow-sm focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500"></asp:TextBox>
                </div>
                <div class="flex justify-end gap-4 pt-5 border-t mt-5">
                    <asp:Button ID="btnSaveChanges" runat="server" Text="Lưu Thay Đổi" OnClick="btnSaveChanges_Click" CssClass="btn-hover-effect inline-flex justify-center py-2 px-6 border border-transparent shadow-sm text-sm font-medium rounded-md text-white bg-green-600 hover:bg-green-700 focus:outline-none focus:ring-2 focus:ring-offset-1 focus:ring-green-500" ValidationGroup="EditReviewGroup" />
                    <asp:Button ID="btnCancelEdit" runat="server" Text="Hủy Bỏ" OnClick="btnCancelEdit_Click" CausesValidation="false" CssClass="btn-hover-effect inline-flex justify-center py-2 px-6 border border-gray-300 shadow-sm text-sm font-medium rounded-md text-gray-700 bg-white hover:bg-gray-100 focus:outline-none focus:ring-2 focus:ring-offset-1 focus:ring-gray-400" />
                </div>
            </div>
        </asp:Panel>
    </div>

    <%-- JavaScript for Animations & Confirmations --%>
    <script type="text/javascript">
        function showEditPanelAnimated() {
            const panel = document.getElementById('<%= pnlEditReview.ClientID %>');
            if (panel) {
                setTimeout(() => { panel.classList.add('panel-visible'); }, 50);
            }
        }

        function showReviewDeleteConfirmation(reviewId, username, bookTitle, comment, sourceControlUniqueId) {
            const truncatedComment = comment.length > 150 ? comment.substring(0, 150) + '...' : comment;
            Swal.fire({
                title: 'Bạn chắc chắn muốn xóa?',
                html: `
                    <div class="text-left text-sm space-y-2 border-t border-b py-4">
                        <p><strong>Người dùng:</strong> ${username}</p>
                        <p><strong>Sách:</strong> ${bookTitle}</p>
                        <p><strong>Nội dung:</strong></p>
                        <blockquote class="border-l-4 border-gray-300 pl-3 italic text-gray-600">
                            ${truncatedComment || '(Không có nhận xét)'}
                        </blockquote>
                    </div>
                `,
                icon: 'warning',
                showCancelButton: true,
                confirmButtonColor: '#d33',
                cancelButtonColor: '#3085d6',
                confirmButtonText: '<i class="fas fa-trash-alt"></i> Có, xóa ngay!',
                cancelButtonText: 'Hủy bỏ'
            }).then((result) => {
                if (result.isConfirmed) {
                    __doPostBack(sourceControlUniqueId, '');
                }
            });
        }

        // ================= BẮT ĐẦU: THÊM HÀM VẼ BIỂU ĐỒ =================
        // Hàm này sẽ được gọi bởi script từ C# sau khi dữ liệu đã sẵn sàng
        function renderRatingsChart() {
            // Kiểm tra xem dữ liệu đã được C# đẩy xuống chưa
            if (!window.chartLabels || !window.chartData) {
                console.error('Dữ liệu biểu đồ chưa sẵn sàng.');
                return;
            }

            var ctx = document.getElementById('ratingsChartCanvas').getContext('2d');

            // Hủy biểu đồ cũ nếu tồn tại để tránh vẽ đè lên nhau
            if (window.ratingsChart instanceof Chart) {
                window.ratingsChart.destroy();
            }

            // Vẽ biểu đồ mới với dữ liệu đã được cung cấp
            window.ratingsChart = new Chart(ctx, {
                type: 'bar',
                data: {
                    labels: window.chartLabels, // Sử dụng dữ liệu từ biến toàn cục
                    datasets: [{
                        label: 'Số lượng đánh giá',
                        data: window.chartData, // Sử dụng dữ liệu từ biến toàn cục
                        backgroundColor: [
                            'rgba(239, 68, 68, 0.6)',  // Red-500
                            'rgba(249, 115, 22, 0.6)', // Orange-500
                            'rgba(234, 179, 8, 0.6)',  // Amber-500
                            'rgba(132, 204, 22, 0.6)', // Lime-500
                            'rgba(34, 197, 94, 0.6)'   // Green-500
                        ],
                        borderColor: [
                            'rgba(239, 68, 68, 1)', 'rgba(249, 115, 22, 1)',
                            'rgba(234, 179, 8, 1)', 'rgba(132, 204, 22, 1)',
                            'rgba(34, 197, 94, 1)'
                        ],
                        borderWidth: 1
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    scales: {
                        y: {
                            beginAtZero: true,
                            ticks: { stepSize: 1, precision: 0 }
                        }
                    },
                    plugins: {
                        legend: { display: false },
                        tooltip: {
                            callbacks: {
                                label: function (context) {
                                    let label = context.dataset.label || '';
                                    if (label) { label += ': '; }
                                    if (context.parsed.y !== null) { label += context.parsed.y; }
                                    return label;
                                }
                            }
                        }
                    }
                }
            });
        }
        // ================= KẾT THÚC: THÊM HÀM VẼ BIỂU ĐỒ =================
    </script>
</asp:Content>