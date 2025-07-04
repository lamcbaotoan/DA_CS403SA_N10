<%-- File: ~/WebForm/VangLai/gioithieu.aspx --%>
<%@ Page Title="Giới Thiệu" Language="C#" MasterPageFile="~/WebForm/VangLai/Site.Master" AutoEventWireup="true" CodeBehind="gioithieu.aspx.cs" Inherits="Webebook.WebForm.VangLai.gioithieu" %>

<asp:Content ID="Content1" ContentPlaceHolderID="HeadContent" runat="server">
    <style>
        /* Optional: Smooth scrolling */
        html { scroll-behavior: smooth; }
         /* Tab styles */
        .tab-button { transition: background-color 0.2s, color 0.2s, border-color 0.2s; }
        /* Define styles for the ACTIVE state */
        .tab-button.active {
            border-color: #3b82f6; /* Tailwind blue-500 */
            color: #3b82f6; /* Tailwind blue-600 */
            background-color: #eff6ff; /* Tailwind blue-50 */
        }
        .tab-content { display: none; }
        .tab-content.active { display: block; animation: fadeIn 0.5s; }

        @keyframes fadeIn {
          from { opacity: 0; }
          to { opacity: 1; }
        }
        /* Style for prose */
        .prose h2 { margin-bottom: 1rem; padding-bottom: 0.5rem; border-bottom: 1px solid #e5e7eb; font-size: 1.5rem; font-weight: 600; color: #1f2937; }
        .prose h3 { margin-top: 1.5em; margin-bottom: 0.5em; font-weight: 600; font-size: 1.25rem; color: #374151;}
        .prose ul { list-style-type: disc; margin-left: 1.5em; margin-bottom: 1em;}
        .prose li { margin-bottom: 0.5em; }
        .prose p { margin-bottom: 1em; line-height: 1.6;}
        .prose a { color: #3b82f6; text-decoration: none; }
        .prose a:hover { text-decoration: underline; }
        .prose strong { font-weight: 600; }
        .prose em { font-style: italic; }
        /* Ensure social icons have alignment */
        .inline-flex { display: inline-flex; align-items: center; }
        .mr-2 { margin-right: 0.5rem; }
        .text-lg { font-size: 1.125rem; } /* Adjust icon size if needed */
    </style>
    <%-- Add Font Awesome link here or ensure it's in Site.Master --%>
    <%-- <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/5.15.4/css/all.min.css" integrity="sha512-..." crossorigin="anonymous" referrerpolicy="no-referrer" /> --%>
</asp:Content>

<asp:Content ID="Content2" ContentPlaceHolderID="MainContent" runat="server">
    <div class="container mx-auto px-4 py-8 max-w-4xl">
        <h1 class="text-3xl font-bold text-center text-gray-800 mb-8">Webebook - Thế Giới Sách Trong Tầm Tay</h1>

<%-- Tab Navigation --%>
        <div class="mb-6 border-b border-gray-300">
            <nav class="-mb-px flex flex-wrap sm:space-x-6 justify-center sm:justify-start" aria-label="Tabs">
                <%-- Add type="button" to prevent form submission --%>
                <button type="button" id="tab-about-btn" class="tab-button active whitespace-nowrap py-3 px-4 border-b-2 font-medium text-sm text-gray-500 hover:text-gray-700 hover:border-gray-400" onclick="switchTab('about')">
                    Về Chúng Tôi
                </button>
                 <%-- Add type="button" to prevent form submission --%>
                <button type="button" id="tab-contact-btn" class="tab-button whitespace-nowrap py-3 px-4 border-b-2 font-medium text-sm text-gray-500 hover:text-gray-700 hover:border-gray-400" onclick="switchTab('contact')">
                    Liên Hệ
                </button>
                 <%-- Add type="button" to prevent form submission --%>
                <button type="button" id="tab-terms-btn" class="tab-button whitespace-nowrap py-3 px-4 border-b-2 font-medium text-sm text-gray-500 hover:text-gray-700 hover:border-gray-400" onclick="switchTab('terms')">
                    Điều Khoản Dịch Vụ
                </button>
            </nav>
        </div>

        <%-- Nội dung các Tab --%>
        <div class="bg-white p-6 md:p-8 rounded-lg shadow-md prose max-w-none">

            <%-- ==================== TAB VỀ CHÚNG TÔI ==================== --%>
            <%-- The 'active' class here is the INITIAL state before JS runs --%>
            <div id="tab-about" class="tab-content active">
                <h2 class="text-2xl font-semibold text-gray-800 mb-4 border-b pb-2">Giới Thiệu Về Webebook</h2>
                <p><strong>Webebook</strong> được xây dựng với niềm đam mê mãnh liệt dành cho sách và khát vọng lan tỏa tri thức đến cộng đồng. Chúng tôi tin rằng trong thời đại số, việc tiếp cận sách cần trở nên thuận tiện, nhanh chóng và không giới hạn về không gian hay thời gian.</p>
                <h3 class="text-xl font-semibold text-gray-700 mt-6 mb-3">Sứ Mệnh Của Chúng Tôi</h3>
                <p>Sứ mệnh của Webebook là trở thành cầu nối vững chắc giữa tác giả, nhà xuất bản và độc giả yêu sách tại Việt Nam và trên thế giới. Chúng tôi mong muốn xây dựng một nền tảng đọc sách điện tử toàn diện, nơi mọi người có thể dễ dàng tìm kiếm, mua và thưởng thức những cuốn sách yêu thích, từ đó nuôi dưỡng tình yêu đọc sách và phát triển văn hóa đọc trong cộng đồng.</p>
                <h3 class="text-xl font-semibold text-gray-700 mt-6 mb-3">Tại Sao Chọn Webebook?</h3>
                <ul>
                    <li><strong>Kho Sách Phong Phú:</strong> Hàng ngàn đầu sách thuộc mọi thể loại (văn học, kinh tế, kỹ năng, thiếu nhi, ngoại ngữ, truyện tranh...) được cập nhật liên tục, đáp ứng mọi nhu cầu đọc của bạn.</li>
                    <li><strong>Bản Quyền Đảm Bảo:</strong> Chúng tôi tôn trọng quyền tác giả và hợp tác chặt chẽ với các nhà xuất bản uy tín để mang đến cho bạn những ấn phẩm chất lượng và hợp pháp.</li>
                    <li><strong>Trải Nghiệm Đọc Tối Ưu:</strong> Giao diện thân thiện, dễ sử dụng, tùy chỉnh phông chữ, cỡ chữ, màu nền linh hoạt. Đọc sách mượt mà trên mọi thiết bị (máy tính, điện thoại, máy tính bảng).</li>
                    <li><strong>Tiện Lợi và Linh Hoạt:</strong> Mua sách chỉ với vài cú nhấp chuột, sách của bạn sẽ được lưu trữ an toàn trong "Tủ Sách" cá nhân và có thể truy cập mọi lúc, mọi nơi.</li>
                    <li><strong>Cộng Đồng Yêu Sách:</strong> Tham gia bình luận, đánh giá sách, chia sẻ cảm nhận và kết nối với những người cùng đam mê.</li>
                </ul>
                <p>Webebook không chỉ là một cửa hàng sách trực tuyến, mà còn là một không gian văn hóa, nơi tri thức được sẻ chia và tình yêu sách được lan tỏa. Hãy đồng hành cùng Webebook trên hành trình khám phá thế giới kỳ diệu của những trang sách!</p>
                 <%-- Replace placeholder with actual developer name if desired --%>
                <p><em>Được phát triển bởi <strong>[Lâm Chu Bảo Toàn]</strong> với tâm huyết mang sách đến gần hơn với mọi người.</em></p>
            </div>

            <%-- ==================== TAB LIÊN HỆ ==================== --%>
            <div id="tab-contact" class="tab-content">
                 <h2 class="text-2xl font-semibold text-gray-800 mb-4 border-b pb-2">Thông Tin Liên Hệ</h2>
                 <p>Webebook luôn trân trọng mọi ý kiến đóng góp và sẵn lòng hỗ trợ quý độc giả. Nếu bạn có bất kỳ câu hỏi, thắc mắc hoặc cần trợ giúp, vui lòng liên hệ với chúng tôi qua các kênh sau:</p>
                 <h3 class="text-xl font-semibold text-gray-700 mt-6 mb-3">Thông Tin Chung</h3>
                 <ul>
                     <li><strong>Email Hỗ Trợ Khách Hàng:</strong> <a href="mailto:hotro@webebook.com">hotro@webebook.com</a> (Phản hồi trong vòng 24h làm việc)</li>
                     <li><strong>Hotline Chăm Sóc Khách Hàng:</strong> <a href="tel:19001234">1900 1234</a> (Cước phí 1000đ/phút, hoạt động từ 8:00 - 17:30, Thứ 2 - Thứ 6)</li>
                     <li><strong>Hợp Tác Nội Dung & Bản Quyền:</strong> <a href="mailto:banquyen@webebook.com">banquyen@webebook.com</a></li>
                     <li><strong>Địa chỉ Văn Phòng (Chỉ tiếp khách có lịch hẹn):</strong> Số 123, Đường ABC, Quận Hải Châu, Thành phố Đà Nẵng, Việt Nam</li>
                 </ul>
                  <h3 class="text-xl font-semibold text-gray-700 mt-6 mb-3">Kết Nối Với Chúng Tôi Trên Mạng Xã Hội</h3>
                  <%-- Ensure Font Awesome CSS is linked in your MasterPage or <head> for these icons --%>
                  <ul class="list-none pl-0">
                      <li class="mb-2">
                          <a href="#" target="_blank" class="inline-flex items-center text-blue-600 hover:text-blue-800">
                              <i class="fab fa-facebook-square mr-2 text-lg"></i> Facebook Webebook
                          </a>
                      </li>
                      <li class="mb-2">
                           <a href="#" target="_blank" class="inline-flex items-center text-purple-600 hover:text-purple-800">
                               <i class="fab fa-instagram-square mr-2 text-lg"></i> Instagram Webebook
                           </a>
                      </li>
                       <li>
                           <a href="#" target="_blank" class="inline-flex items-center text-red-600 hover:text-red-800">
                               <i class="fab fa-youtube-square mr-2 text-lg"></i> Youtube Webebook
                           </a>
                       </li>
                  </ul>
                 <p class="mt-6">Chúng tôi luôn nỗ lực để cải thiện dịch vụ và mang đến trải nghiệm tốt nhất cho bạn. Mọi phản hồi của bạn đều quý giá!</p>
            </div>

            <%-- ==================== TAB ĐIỀU KHOẢN DỊCH VỤ ==================== --%>
             <div id="tab-terms" class="tab-content">
                 <h2 class="text-2xl font-semibold text-gray-800 mb-4 border-b pb-2">Điều Khoản Dịch Vụ Webebook</h2>
                 <p>Chào mừng bạn đến với Webebook! Vui lòng đọc kỹ các Điều Khoản Dịch Vụ ("Điều Khoản") dưới đây trước khi truy cập hoặc sử dụng trang web Webebook.com và các dịch vụ liên quan (gọi chung là "Dịch Vụ"). Việc bạn sử dụng Dịch Vụ đồng nghĩa với việc bạn chấp nhận và đồng ý bị ràng buộc bởi các Điều Khoản này.</p>

                 <h3>1. Chấp Thuận Điều Khoản</h3>
                 <p>Bằng cách đăng ký tài khoản hoặc sử dụng Dịch Vụ dưới bất kỳ hình thức nào, bạn xác nhận rằng bạn đã đọc, hiểu và đồng ý tuân thủ các Điều Khoản này cũng như <a href="/PrivacyPolicy.aspx" target="_blank">Chính Sách Quyền Riêng Tư</a> của chúng tôi (được tích hợp vào Điều Khoản này). Nếu bạn không đồng ý, vui lòng không sử dụng Dịch Vụ.</p>

                 <h3>2. Tài Khoản Người Dùng</h3>
                 <ul>
                     <li>Bạn cần đăng ký tài khoản để truy cập một số tính năng của Dịch Vụ, đặc biệt là mua và đọc sách.</li>
                     <li>Bạn cam kết cung cấp thông tin đăng ký chính xác, cập nhật và đầy đủ.</li>
                     <li>Bạn chịu hoàn toàn trách nhiệm về việc bảo mật mật khẩu và mọi hoạt động diễn ra dưới tài khoản của mình. Thông báo ngay cho Webebook nếu phát hiện bất kỳ hành vi sử dụng trái phép nào.</li>
                     <li>Webebook có quyền từ chối cung cấp dịch vụ, chấm dứt tài khoản hoặc hủy đơn hàng theo quyết định riêng của mình nếu phát hiện vi phạm.</li>
                 </ul>

                 <h3>3. Mua Sách và Sử Dụng Nội Dung</h3>
                 <ul>
                     <li>Khi bạn mua một cuốn sách điện tử (ebook) trên Webebook, bạn được cấp giấy phép có giới hạn, không độc quyền, không thể chuyển nhượng để truy cập và đọc nội dung đó cho mục đích cá nhân, phi thương mại thông qua Dịch Vụ của chúng tôi.</li>
                     <li>Bạn không được phép sao chép, sửa đổi, phân phối lại, bán, cho thuê, cho mượn, phát sóng, tạo sản phẩm phái sinh hoặc sử dụng nội dung cho bất kỳ mục đích nào khác ngoài quy định rõ ràng trong Điều Khoản này.</li>
                     <li>Mọi hành vi vi phạm bản quyền sẽ bị xử lý theo quy định của pháp luật.</li>
                 </ul>

                  <h3>4. Giá Cả và Thanh Toán</h3>
                 <ul>
                     <li>Giá sách được niêm yết rõ ràng trên trang chi tiết sản phẩm và có thể thay đổi mà không cần báo trước.</li>
                     <li>Chúng tôi chấp nhận các phương thức thanh toán được liệt kê tại trang thanh toán. Bạn đồng ý cung cấp thông tin thanh toán hợp lệ và ủy quyền cho chúng tôi hoặc đối tác xử lý thanh toán của chúng tôi thu phí cho các giao dịch bạn thực hiện.</li>
                     <li>Chính sách hoàn tiền (nếu có) sẽ được quy định cụ thể và công bố trên website.</li>
                 </ul>

                 <h3>5. Quyền Sở Hữu Trí Tuệ</h3>
                 <p>Toàn bộ nội dung trên Dịch Vụ, bao gồm nhưng không giới hạn ở sách điện tử, văn bản, đồ họa, logo, biểu tượng, hình ảnh, giao diện người dùng, mã nguồn và phần mềm, là tài sản của Webebook hoặc các nhà cung cấp nội dung của chúng tôi và được bảo vệ bởi luật sở hữu trí tuệ của Việt Nam và quốc tế.</p>

                 <h3>6. Hành Vi Bị Cấm</h3>
                 <p>Bạn đồng ý không sử dụng Dịch Vụ để:</p>
                 <ul>
                     <li>Thực hiện bất kỳ hành vi bất hợp pháp nào hoặc vi phạm Điều Khoản này.</li>
                     <li>Vi phạm quyền sở hữu trí tuệ hoặc các quyền khác của bên thứ ba.</li>
                     <li>Truyền bá virus, mã độc hoặc các nội dung gây hại khác.</li>
                     <li>Can thiệp hoặc phá vỡ hoạt động của Dịch Vụ hoặc máy chủ.</li>
                     <li>Thu thập thông tin cá nhân của người dùng khác một cách trái phép.</li>
                 </ul>

                  <h3>7. Miễn Trừ Trách Nhiệm và Giới Hạn Trách Nhiệm</h3>
                 <ul>
                     <li>Dịch Vụ được cung cấp trên cơ sở "nguyên trạng" ("as is") và "như sẵn có" ("as available") mà không có bất kỳ bảo đảm nào.</li>
                     <li>Webebook không đảm bảo Dịch Vụ sẽ không bị gián đoạn, không có lỗi hoặc hoàn toàn an toàn.</li>
                     <li>Trong phạm vi tối đa được pháp luật cho phép, Webebook sẽ không chịu trách nhiệm cho bất kỳ thiệt hại gián tiếp, ngẫu nhiên, đặc biệt, do hậu quả hoặc mang tính trừng phạt nào phát sinh từ việc bạn sử dụng hoặc không thể sử dụng Dịch Vụ.</li>
                 </ul>

                 <h3>8. Thay Đổi Điều Khoản</h3>
                 <p>Webebook có quyền sửa đổi các Điều Khoản này vào bất kỳ lúc nào. Chúng tôi sẽ thông báo về những thay đổi quan trọng bằng cách đăng phiên bản cập nhật trên website hoặc qua email. Việc bạn tiếp tục sử dụng Dịch Vụ sau khi các thay đổi có hiệu lực đồng nghĩa với việc bạn chấp nhận các Điều Khoản đã sửa đổi.</p>

                  <h3>9. Luật Áp Dụng và Giải Quyết Tranh Chấp</h3>
                 <p>Các Điều Khoản này sẽ được điều chỉnh và giải thích theo pháp luật Việt Nam. Mọi tranh chấp phát sinh sẽ được giải quyết thông qua thương lượng hòa giải. Nếu không thể hòa giải, tranh chấp sẽ được đưa ra giải quyết tại Tòa án có thẩm quyền tại Đà Nẵng, Việt Nam.</p>

                  <h3>10. Thông Tin Liên Lạc</h3>
                 <p>Nếu bạn có bất kỳ câu hỏi nào về Điều Khoản Dịch Vụ này, vui lòng liên hệ với chúng tôi qua email: <a href="mailto:phaply@webebook.com">phaply@webebook.com</a>.</p>

                 <%-- Dynamically display the current date --%>
                 <p class="mt-6 italic"><em>Cập nhật lần cuối: Ngày <%= DateTime.Now.ToString("dd 'tháng' MM 'năm' yyyy", new System.Globalization.CultureInfo("vi-VN")) %></em></p>
             </div>

        </div> <%-- End .bg-white --%>
    </div> <%-- End .container --%>

    <%-- Script chuyển tab --%>
     <script>
         function switchTab(tabId) {
             // Update the URL hash. This allows bookmarking and persists state on refresh.
             window.location.hash = tabId;

             // Hide all tab contents by removing the 'active' class
             document.querySelectorAll('.tab-content').forEach(content => {
                 content.classList.remove('active');
             });

             // Deactivate all tab buttons by removing the 'active' class
             document.querySelectorAll('.tab-button').forEach(button => {
                 button.classList.remove('active');
             });

             // Show the selected tab content by adding the 'active' class
             const selectedContent = document.getElementById('tab-' + tabId);
             if (selectedContent) {
                 selectedContent.classList.add('active');
             } else {
                 console.error('Content element not found for tabId:', tabId);
             }

             // Activate the selected tab button by adding the 'active' class
             const selectedButton = document.getElementById('tab-' + tabId + '-btn');
             if (selectedButton) {
                 selectedButton.classList.add('active');
             } else {
                 console.error('Button element not found for tabId:', tabId);
             }
         }

         // Run this code after the DOM is fully loaded
         document.addEventListener('DOMContentLoaded', () => {
             const hash = window.location.hash.substring(1); // Get hash value without the '#'
             const validTabs = ['about', 'contact', 'terms'];
             let tabToActivate = 'about'; // Default tab

             // Check if the hash corresponds to a valid tab
             if (hash && validTabs.includes(hash)) {
                 tabToActivate = hash;
             } else {
                 // Optional: If you want the URL to always reflect the active tab,
                 // uncomment the next line to set the hash to 'about' if it's missing or invalid.
                 // window.location.hash = 'about';
             }

             // Activate the determined tab. Use setTimeout to ensure rendering is complete.
             setTimeout(() => {
                 switchTab(tabToActivate);
             }, 0);
         });
     </script>
</asp:Content>