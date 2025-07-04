<%-- /WebForm/VangLai/thanhvienvl.aspx --%>
<%@ Page Title="Hồ Sơ Thành Viên" Language="C#" MasterPageFile="~/WebForm/VangLai/Site.Master" AutoEventWireup="true" CodeBehind="thanhvienvl.aspx.cs" Inherits="Webebook.WebForm.VangLai.thanhvienvl" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <%-- Thêm CSS nếu cần, Font Awesome đã có trong Site.Master --%>
    <%-- <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.0.0/css/all.min.css" /> --%> <%-- Có thể bỏ nếu Site.Master đã có --%>
    <style>
        /* CSS tùy chỉnh riêng cho trang này nếu cần */
    </style>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container mx-auto px-4 lg:px-8 py-8">
        <h2 class="text-3xl font-bold text-gray-800 mb-6 border-b pb-3">Hồ Sơ Thành Viên</h2>
        <asp:Label ID="lblMemberMessage" runat="server" EnableViewState="false" Visible="false"></asp:Label>

        <%-- Panel chứa hồ sơ, chỉ hiện khi tìm thấy và hợp lệ --%>
        <asp:Panel ID="pnlMemberProfile" runat="server" Visible="false">
            <div class="bg-white p-6 rounded-lg shadow-md border border-gray-200 max-w-xl mx-auto">
                <div class="flex flex-col sm:flex-row items-center sm:items-start gap-6">
                    <%-- Avatar --%>
                    <div class="flex-shrink-0">
                        <asp:Image ID="imgMemberAvatar" runat="server" CssClass="w-24 h-24 md:w-32 md:h-32 rounded-full object-cover border-2 border-gray-300 shadow" />
                    </div>
                    <%-- Tên và Username --%>
                    <div class="flex-grow text-center sm:text-left mt-4 sm:mt-0">
                        <h3 class="text-2xl font-semibold text-gray-800">
                            <asp:Label ID="lblMemberDisplayName" runat="server"></asp:Label>
                        </h3>
                        <p class="text-sm text-gray-500">
                            <asp:Label ID="lblMemberUsername" runat="server"></asp:Label>
                        </p>
                        <%-- Có thể thêm các thông tin công khai khác ở đây nếu muốn --%>
                        <%-- Ví dụ: Ngày tham gia (nếu có cột trong DB) --%>
                        <%-- <p class="text-xs text-gray-400 mt-2">Đã tham gia: [Ngày]</p> --%>
                    </div>
                </div>
            </div>
        </asp:Panel>

        <%-- Nút quay lại và về trang chủ công cộng --%>
        <div class="text-center mt-8">
            <asp:HyperLink ID="hlBack" runat="server" NavigateUrl="javascript:history.back()" CssClass="text-sm text-blue-600 hover:text-blue-800 hover:underline">
                <i class="fas fa-arrow-left mr-1"></i> Quay lại trang trước
            </asp:HyperLink>
            <span class="mx-2 text-gray-300">|</span>
             <%-- Đổi NavigateUrl về trang chủ của khách (VangLai) --%>
            <asp:HyperLink ID="hlHome" runat="server" NavigateUrl="~/WebForm/VangLai/trangchu.aspx" CssClass="text-sm text-blue-600 hover:text-blue-800 hover:underline">
                 Về trang chủ <i class="fas fa-home ml-1"></i>
            </asp:HyperLink>
        </div>

    </div> <%-- End Container --%>
</asp:Content>