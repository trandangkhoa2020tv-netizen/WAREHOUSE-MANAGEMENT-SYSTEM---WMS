namespace InventoryManagement.Models
{
    /// <summary>
    /// Bộ nhớ phiên đăng nhập hiện tại.
    /// Sau khi đăng nhập thành công, FrmDangNhap lưu tên tài khoản và vai trò vào đây để FrmMain phân quyền menu.
    /// </summary>
    public static class UserSession
    {
        /// <summary>Tên tài khoản đang đăng nhập.</summary>
        public static string TenTaiKhoan { get; set; } = string.Empty;

        /// <summary>Vai trò hiện tại, ví dụ Admin hoặc NhanVien.</summary>
        public static string VaiTro { get; set; } = string.Empty;
    }
}
