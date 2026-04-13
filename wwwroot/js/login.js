

//Toast biến mất 
document.addEventListener("DOMContentLoaded", function () {
    // Tìm thẻ thông báo theo ID đã đặt ở bước trước
    const toast = document.getElementById("auto-close-toast");

    if (toast) {
        // 1. Sau 3 giây (3000ms), bắt đầu hiệu ứng mờ dần
        setTimeout(() => {
            toast.style.transition = "opacity 0.6s ease, transform 0.6s ease";
            toast.style.opacity = "0";
            toast.style.transform = "translate(-50%, -20px)"; // Bay ngược lên nhẹ một chút

            // 2. Sau khi mờ hẳn (đợi thêm 600ms của hiệu ứng transition), xóa hẳn thẻ khỏi HTML
            setTimeout(() => { toast.remove(); }, 600); // 4000ms = 4 giây hiển thị trên màn hình
        }, 4000);
    }
});