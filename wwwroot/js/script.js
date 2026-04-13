////Xử lý AJAX
//document.getElementById('searchForm').addEventListener('submit', function (e) {
//    // Ngăn chặn hành vi load lại trang mặc định của Form
//    e.preventDefault();

//    // Hiện chữ "Đang tìm..." trong lúc chờ Server xử lý
//    document.getElementById('resultsContainer').innerHTML = '<p class="text-center">Đang tìm phòng...</p>';

//    // Lấy URL và gom dữ liệu từ Form
//    const form = e.target;
//    const url = form.action;
//    const formData = new FormData(form);
//    const queryString = new URLSearchParams(formData).toString();

//    // Gửi dữ liệu ngầm (AJAX) bằng Fetch API
//    fetch(`${url}?${queryString}`)
//        .then(response => response.text()) // Nhận kết quả dạng HTML từ PartialView
//        .then(html => {
//            // Đổ đoạn HTML nhận được vào cái khay
//            document.getElementById('resultsContainer').innerHTML = html;
//        })
//        .catch(error => {
//            console.error('Lỗi rồi:', error);
//            document.getElementById('resultsContainer').innerHTML = '<p class="text-danger text-center">Có lỗi xảy ra khi tìm kiếm.</p>';
//        });
//});