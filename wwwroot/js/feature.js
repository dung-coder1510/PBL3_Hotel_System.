
/**
 * Hàm gọi Luxury Confirm dùng chung cho mọi nút bấm
 * @param {HTMLElement} buttonElement - Nút vừa được bấm (truyền 'this')
 * @param {string} title - Tiêu đề của Popup
 * @param {string} message - Nội dung chi tiết của Popup
 * @param {string} type - Loại (warning, danger, info) để đổi màu Icon
 */
function triggerToast(message, type = 'success') {
    // 1. Kiểm tra/Tạo container nếu chưa có
    let container = document.getElementById('luxury-toast-container');
    if (!container) {
        container = document.createElement('div');
        container.id = 'luxury-toast-container';
        document.body.appendChild(container);
    }

    // 2. Tạo phần tử Toast
    const toast = document.createElement('div');
    toast.className = `luxury-toast-item toast-${type}`;

    // Chọn icon tương ứng
    const icon = type === 'success' ? 'fa-check-circle' :
        type === 'error' ? 'fa-exclamation-circle' : 'fa-exclamation-triangle';

    toast.innerHTML = `
        <i class="fas ${icon}" style="font-size: 1.2rem;"></i>
        <span>${message}</span>
    `;

    // 3. Thêm vào container
    container.appendChild(toast);

    // 4. Tự động xóa sau 4 giây
    setTimeout(() => {
        toast.style.opacity = '0';
        toast.style.transform = 'translateY(-20px)';
        setTimeout(() => toast.remove(), 400);
    }, 4000);
}





// Dùng var để tránh lỗi "already declared"
var currentFormToSubmit = null;

function showLuxuryConfirm(buttonElement, title, message, type = 'warning') {
    // Alert kiểm tra đầu vào
    console.log("Đang mở Luxury Confirm cho nút:", buttonElement);

    // 1. Tìm Form bao quanh nút bấm
    currentFormToSubmit = buttonElement.closest("form");
    if (!currentFormToSubmit) {
        alert("LỖI: Nút bấm này không nằm trong một thẻ <form> nào cả!");
        return;
    }

    // 2. Lấy các phần tử giao diện
    const overlay = document.getElementById("luxuryConfirmOverlay");
    const titleEl = document.getElementById("luxuryConfirmTitle");
    const messageEl = document.getElementById("luxuryConfirmMessage");
    const btnAccept = document.getElementById("btnConfirmAccept");

    if (!overlay || !btnAccept) {
        alert("LỖI: Không tìm thấy khung Modal (Overlay hoặc Button Accept) trong Layout!");
        return;
    }

    // 3. Đổ dữ liệu vào Modal
    titleEl.innerText = title;
    messageEl.innerText = message;

    // 4. HIỂN THỊ MODAL
    overlay.style.display = "flex";

    // 5. GÁN SỰ KIỆN (Dùng addEventListener thay vì onclick để an toàn hơn)
    // Xóa bỏ các sự kiện cũ để tránh bị lặp
    const newBtnAccept = btnAccept.cloneNode(true);
    btnAccept.parentNode.replaceChild(newBtnAccept, btnAccept);

    newBtnAccept.addEventListener("click", function (e) {
        e.preventDefault(); // CHẶN TUYỆT ĐỐI hành vi mặc định

        console.log("Đã bấm nút ĐỒNG Ý!");

        if (currentFormToSubmit.classList.contains("ajax-form")) {
            // NHÁNH AJAX (Gửi ngầm)
            const formData = new FormData(currentFormToSubmit);
            fetch(currentFormToSubmit.action, {
                method: "POST",
                body: formData
            })
                .then(res => res.json()) // ĐỌC DỮ LIỆU DƯỚI DẠNG JSON
                .then(data => {
                    // Luôn hiện thông báo từ Server
                    if (typeof triggerToast === "function") {
                        triggerToast(data.message, data.success ? "success" : "error");
                    }

                    if (data.success) {

                        // KIỂM TRA LUỒNG 1: Nếu đang ở trang Quản lý Nhân viên (Duyệt ca)
                        const staffIdInput = currentFormToSubmit.querySelector('input[name="staffId"]');
                        if (staffIdInput && typeof loadStaffShifts === "function") {
                            loadStaffShifts(staffIdInput.value);
                            return; // Đã chạy xong luồng 1 thì thoát hàm ngay
                        }

                        // KIỂM TRA LUỒNG 2: Nếu đang ở trang Quản lý Lương (Chốt sổ / Thanh toán)
                        if (typeof window.reloadPayrollTable === "function") {
                            closeAdminModal(); // Đóng Modal Phiếu lương
                            window.reloadPayrollTable(); // Gọi hàm load lại bảng lương
                            return; // Thoát hàm
                        }

                        // LUỒNG DỰ PHÒNG: Nếu form thành công mà không thuộc 2 luồng trên
                        // Thì đợi 1 giây cho hiện Toast rồi tự động nạp lại trang
                        setTimeout(() => {
                            location.reload();
                        }, 1000);
                    }
                })
                .catch(err => {
                    // Nếu vẫn văng lỗi, in lỗi cụ thể ra Console thay vì chỉ hiện dòng Toast đỏ chung chung
                    console.error("LỖI JS TRONG NHÁNH FETCH:", err);
                    if (typeof triggerToast === "function") {
                        triggerToast("Lỗi hệ thống: Xem Console (F12) để biết chi tiết!", "error");
                    }
                });
        }
        else {
            // NHÁNH THƯỜNG (Ví dụ: Xóa phòng)
            currentFormToSubmit.submit();
        }

        closeLuxuryConfirm();
    });
}

function closeLuxuryConfirm() {
    const overlay = document.getElementById("luxuryConfirmOverlay");
    if (overlay) overlay.style.display = "none";
}

// Lắng nghe sự kiện Đồng ý và Click ra ngoài vùng đen
document.addEventListener("DOMContentLoaded", function () {

    // Nút đồng ý
    const btnAccept = document.getElementById("btnConfirmAccept");
    if (btnAccept) {
        btnAccept.addEventListener("click", function () {
            if (currentFormToSubmit) {
                currentFormToSubmit.submit(); // Đẩy dữ liệu lên Server
            }
            closeLuxuryConfirm();
        });
    }

    // Click vùng đen
    const overlay = document.getElementById("luxuryConfirmOverlay");
    if (overlay) {
        overlay.addEventListener("click", function (e) {
            if (e.target === this) closeLuxuryConfirm();
        });
    }
});

// ========================================================
// HỆ THỐNG TÍNH GIÁ ĐỘNG CHO MODAL PHÒNG (DELEGATION EVENT)
// ========================================================

// 1. Hàm tính toán logic (Core)
function calculateRoomPrice() {
    const typeSelect = document.getElementById('LoaiPhong');
    const sizeInput = document.getElementById('Size');
    const hiddenInput = document.getElementById('GiaPhong');
    const displayLabel = document.getElementById('giaPhongDisplay');

    // Nếu không mở Modal thêm phòng thì bỏ qua
    if (!typeSelect || !sizeInput) return;

    const type = typeSelect.value;
    const size = parseInt(sizeInput.value) || 1;

    let basePrice = 0;
    if (type === 'Standard') basePrice = 500000;
    else if (type === 'Deluxe') basePrice = 800000;
    else if (type === 'Suite') basePrice = 1200000;

    let extra = 0;
    if (size > 1) extra = (size - 2) * 150000;

    const finalPrice = basePrice + extra;

    if (hiddenInput) hiddenInput.value = finalPrice;
    if (displayLabel) displayLabel.innerText = finalPrice.toLocaleString('vi-VN') + ' ₫';
}

['input', 'change'].forEach(function (evt) {
    document.addEventListener(evt, function (e) {

        // 1. NẾU NGƯỜI DÙNG TƯƠNG TÁC VỚI Ô "SIZE" (Sức chứa)
        if (e.target && e.target.id === 'Size') {
            let val = Math.floor(parseFloat(e.target.value));

            if (isNaN(val)) {
                // Nếu rỗng thì để trống cho người dùng nhập lại, NHƯNG vẫn gọi tính giá (sẽ lấy mặc định là 1)
                calculateRoomPrice();
                return;
            }

            if (val > 5) e.target.value = 5;
            else if (val < 1) e.target.value = 1;
            else e.target.value = val;

            // Tính lại tiền sau khi đã ép chuẩn số
            calculateRoomPrice();
        }

        // 2. NẾU NGƯỜI DÙNG TƯƠNG TÁC VỚI Ô "LOAIPHONG" (Hạng phòng)
        if (e.target && e.target.id === 'LoaiPhong') {
            // Khi đổi loại phòng, không cần validate gì cả, chỉ việc tính lại tiền
            calculateRoomPrice();
        }

    });
});