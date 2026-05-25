

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

// HÀM HIỂN THỊ FORM CHECK-OUT
function toggleCheckOutForm() {
    console.log("Đang mở form check-out..."); // Dòng này để bạn kiểm tra trong F12
    const form = document.getElementById("formCheckOut");
    const btn = document.getElementById("btnShowCheckOut");

    if (form && btn) {
        form.style.display = "block"; // Hiện form
        btn.style.display = "none";    // Ẩn nút bấm cũ đi cho đỡ vướng
    } else {
        console.error("Không tìm thấy thẻ có ID formCheckOut hoặc btnShowCheckOut");
    }
}

// HÀM ĐÓNG (NÚT HỦY)
function closeCheckOutForm() {
    const form = document.getElementById("formCheckOut");
    const btn = document.getElementById("btnShowCheckOut");

    if (form && btn) {
        form.style.display = "none";
        btn.style.display = "flex";
    }
}

function loadBookingDetail(id) {
    const overlay = document.getElementById("bookingModalOverlay");
    const content = document.getElementById("bookingModalContent");

    overlay.style.display = "flex";
    content.innerHTML = '<div style="text-align:center; padding: 50px;"><i class="fas fa-spinner fa-spin fa-2x"></i> Đang tải...</div>';

    fetch(`/Booking/GetBookingDetailPartial/${id}`)
        .then(res => {
            if (!res.ok) throw new Error("Lỗi");
            return res.text();
        })
        .then(html => content.innerHTML = html)
        .catch(err => content.innerHTML = '<div style="color:red; text-align:center; padding:30px;">Có lỗi xảy ra!</div>');
}

function closeBookingModal() {
    document.getElementById("bookingModalOverlay").style.display = "none";
}


//Hien thi quan ly tai khoan cua Admin
function loadEditAccount(username) {
    console.log("--- BẮT ĐẦU DEBUG ---");
    console.log("1. Đã nhận Username:", username);

    const overlay = document.getElementById("adminModalOverlay");
    const content = document.getElementById("adminModalContent");

    console.log("2. Kiểm tra Overlay:", overlay);
    console.log("3. Kiểm tra Content:", content);

    if (!overlay || !content) {
        console.error("DỪNG LẠI: Không tìm thấy ID 'adminModalOverlay' hoặc 'adminModalContent' trên trang này!");
        alert("Lỗi: Trang web thiếu khung hiển thị Modal!");
        return;
    }
    overlay.style.display = "flex";
    content.innerHTML = '<div style="text-align:center; padding: 40px;"><i class="fas fa-spinner fa-spin fa-2x text-gold"></i></div>';

    // Dùng AJAX lên C# lấy form
    fetch(`/Admin/GetEditAccountPartial?username=${username}`)
        .then(res => {
            if (!res.ok) throw new Error("Lỗi");
            return res.text();
        })
        .then(html => content.innerHTML = html)
        .catch(err => content.innerHTML = '<div style="color:red; padding: 20px; text-align:center;">Có lỗi xảy ra!</div>');
}

function closeAdminModal() {
    document.getElementById("adminModalOverlay").style.display = "none";
}

function loadCreateAccount() {
    const overlay = document.getElementById("adminModalOverlay");
    const content = document.getElementById("adminModalContent");

    if (!overlay || !content) return;

    overlay.style.display = "flex";
    content.innerHTML = '<div style="text-align:center; padding: 40px;"><i class="fas fa-spinner fa-spin fa-2x text-gold"></i></div>';

    // Gọi hàm GetCreateAccountPartial từ AdminController
    fetch(`/Admin/GetCreateAccountPartial`)
        .then(res => {
            if (!res.ok) throw new Error("Lỗi mạng");
            return res.text();
        })
        .then(html => content.innerHTML = html)
        .catch(err => content.innerHTML = '<div style="color:red; padding: 20px; text-align:center;">Có lỗi xảy ra khi tải form!</div>');
}

function loadEditNhanVien(id) {
    const overlay = document.getElementById("adminModalOverlay");
    const content = document.getElementById("adminModalContent");

    if (!overlay || !content) return;
    overlay.style.display = "flex";
    content.innerHTML = '<div style="text-align:center; padding: 40px;"><i class="fas fa-spinner fa-spin fa-2x text-gold"></i></div>';

    // GỌI VỀ ADMIN CONTROLLER ĐỂ LẤY FORM
    fetch(`/Admin/GetEditNhanVienPartial?id=${id}`)
        .then(res => res.text())
        .then(html => content.innerHTML = html);
}

// UX: Bấm ra ngoài vùng đen để đóng Modal
var adminOverlay = document.getElementById("adminModalOverlay");
if (adminOverlay) { // <--- THÊM DÒNG NÀY
    adminOverlay.addEventListener("click", function (e) {
        if (e.target === this) closeAdminModal();
    });
}


function loadStaffShifts(id) {
    const overlay = document.getElementById("adminModalOverlay");
    const content = document.getElementById("adminModalContent");

    if (!overlay || !content) {
        console.error("Thiếu khung Modal adminModalOverlay trong Layout!");
        return;
    }

    overlay.style.display = "flex";
    content.innerHTML = '<div style="text-align:center; padding: 40px;"><i class="fas fa-spinner fa-spin fa-2x text-gold"></i><p>Đang tải lịch làm việc...</p></div>';

    // Gọi AJAX về Controller Admin
    fetch(`/Admin/GetStaffShiftsPartial?id=${id}`)
        .then(res => {
            if (!res.ok) throw new Error("Lỗi tải dữ liệu");
            return res.text();
        })
        .then(html => {
            content.innerHTML = html;
        })
        .catch(err => {
            content.innerHTML = '<div style="color:red; text-align:center; padding: 20px;">Không thể tải lịch làm lúc này!</div>';
        });
}