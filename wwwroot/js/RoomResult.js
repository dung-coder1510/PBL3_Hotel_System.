//function loadRoomDetail(SoPhong) {
//    const modalOverlay = document.getElementById("roomModalOverlay");
//    const modalContent = document.getElementById("roomModalContent");

//    // Hiển thị khung Modal lên, cho chữ Loading xoay xoay
//    modalOverlay.style.display = "flex";
//    modalContent.innerHTML = '<div style="text-align:center; padding: 50px;"><i class="fas fa-spinner fa-spin fa-2x"></i> Đang tải thông tin...</div>';

//    // Gọi AJAX lên Controller
//    fetch(`/Room/Detail/${SoPhong}`) // Thay /Room/ bằng Controller của bạn nếu khác
//        .then(response => {
//            if (!response.ok) throw new Error("Lỗi tải dữ liệu");
//            return response.text();
//        })
//        .then(html => {
//            // Đổ HTML nhận được vào cái hộp trắng
//            modalContent.innerHTML = html;
//        })
//        .catch(error => {
//            modalContent.innerHTML = '<div style="color:red; text-align:center; padding:30px;">Có lỗi xảy ra khi lấy thông tin phòng!</div>';
//        });
//}

//// Hàm đóng Modal
//function closeRoomModal() {
//    document.getElementById("roomModalOverlay").style.display = "none";
//}

//// Click ra ngoài vùng đen để đóng Modal (UX xịn xò)
//document.getElementById("roomModalOverlay").addEventListener("click", function (e) {
//    if (e.target === this) {
//        closeRoomModal();
//    }
//});


document.addEventListener("DOMContentLoaded", function () {
    // --- 1. XỬ LÝ FORM TÌM KIẾM (CHUYỂN TRANG HOẶC AJAX) ---
    const searchForm = document.getElementById("searchForm");
    const resultsContainer = document.getElementById("resultsContainer");

    if (searchForm) {
        searchForm.addEventListener("submit", function (e) {
            // KIỂM TRA: Nếu có resultsContainer trên trang này (Nghĩa là đang ở trang Room/Index)
            if (resultsContainer) {
                e.preventDefault(); // Chặn hành vi chuyển trang mặc định

                // Hiển thị trạng thái đang tải
                resultsContainer.innerHTML = `
                    <div style="text-align:center; padding: 3rem;">
                        <i class="fas fa-spinner fa-spin fa-3x" style="color: #d4af37;"></i>
                        <p style="margin-top: 15px; color: #666;">Đang tìm kiếm phòng trống phù hợp...</p>
                    </div>`;

                // Lấy dữ liệu từ Form
                const formData = new FormData(searchForm);
                const queryString = new URLSearchParams(formData).toString();

                // Gọi AJAX đến hàm Search (trả về Partial View _RoomResults)
                const ajaxUrl = "/SearchRoom/Search?" + queryString;

                fetch(ajaxUrl, {
                    headers: { 'X-Requested-With': 'XMLHttpRequest' }
                })
                    .then(response => {
                        if (!response.ok) throw new Error("Lỗi mạng hoặc Server");
                        return response.text();
                    })
                    .then(html => {
                        // Đổ kết quả vào hộp chứa
                        resultsContainer.innerHTML = html;
                    })
                    .catch(error => {
                        console.error("Lỗi AJAX Search:", error);
                        resultsContainer.innerHTML = '<div style="color:red; text-align:center; padding:3rem;">Có lỗi xảy ra khi lấy danh sách phòng!</div>';
                    });
            }
            // NGƯỢC LẠI: Nếu không có resultsContainer (Đang ở Trang Chủ)
            // Chúng ta KHÔNG gọi e.preventDefault() -> Trình duyệt sẽ tự động gửi form và chuyển trang đến /Room/Index
        });

        // MẸO UX: Nếu khách vừa từ trang chủ nhảy sang (URL có chứa tham số tìm kiếm)
        // và đang đứng ở trang có kết quả, tự động kích hoạt tìm kiếm luôn cho khách xem
        if (resultsContainer && window.location.search.length > 0) {
            // Tạo một sự kiện submit giả để kích hoạt AJAX ngay khi trang vừa tải xong
            searchForm.dispatchEvent(new Event('submit'));
        }
    }

    // --- 2. XỬ LÝ CLICK RA NGOÀI ĐỂ ĐÓNG MODAL (Code của bạn) ---
    const modalOverlay = document.getElementById("roomModalOverlay");
    if (modalOverlay) {
        modalOverlay.addEventListener("click", function (e) {
            if (e.target === this) {
                closeRoomModal();
            }
        });
    }
});

// --- 3. HÀM MỞ CHI TIẾT PHÒNG (Global - để gọi từ onclick) ---
function loadRoomDetail(SoPhong) {

    const modalOverlay = document.getElementById("roomModalOverlay");
    const modalContent = document.getElementById("roomModalContent");

    if (!modalOverlay || !modalContent) return;

    modalOverlay.style.display = "flex";
    modalContent.innerHTML = `
        <div style="text-align:center; padding: 50px;">
            <i class="fas fa-spinner fa-spin fa-2x" style="color: #d4af37;"></i>
            <p style="margin-top: 10px;">Đang tải thông tin...</p>
        </div>`;

    fetch(`/Room/Detail/${SoPhong}`)
        .then(response => {
            if (!response.ok) throw new Error("Lỗi tải dữ liệu");
            return response.text();
        })
        .then(html => {
            modalContent.innerHTML = html;
        })
        .catch(error => {
            console.error("Lỗi chi tiết phòng:", error);
            modalContent.innerHTML = '<div style="color:red; text-align:center; padding:30px;">Không thể tải thông tin phòng lúc này.</div>';
        });
}

// --- 4. HÀM ĐÓNG MODAL ---
function closeRoomModal() {
    const modalOverlay = document.getElementById("roomModalOverlay");
    if (modalOverlay) {
        modalOverlay.style.display = "none";
    }
}