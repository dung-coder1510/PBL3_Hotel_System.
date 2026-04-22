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
function triggerToast(message, type = 'error') {
    const container = document.getElementById("global-toast-container");
    if (!container) return;

    // Tạo nhanh 1 cái thẻ div dùng đúng class Luxury trong base.css
    const toast = document.createElement("div");
    toast.className = `alert-luxury-toast ${type === 'success' ? 'alert-toast-success' : 'alert-toast-error'}`;

    // Copy y hệt cấu trúc HTML của Layout
    toast.innerHTML = `
        <i class="fas ${type === 'success' ? 'fa-check-circle' : 'fa-exclamation-triangle'}"></i>
        <span>${message}</span>
        <i class="fas fa-times" onclick="this.parentElement.remove()" style="cursor:pointer; margin-left:10px;"></i>
    `;

    // Ném nó vào trong hộp chứa
    container.appendChild(toast);

    // Tự động xóa sau 4 giây (Tái sử dụng logic cũ)
    setTimeout(() => {
        toast.style.opacity = "0";
        toast.style.transform = "translateY(-20px)";
        toast.style.transition = "all 0.6s ease";
        setTimeout(() => toast.remove(), 600);
    }, 4000);
}