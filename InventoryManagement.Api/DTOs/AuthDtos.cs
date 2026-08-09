namespace InventoryManagement.ApiServer.DTOs;

/// <summary>
/// Dữ liệu body gửi lên API khi người dùng đăng nhập.
/// </summary>
public sealed class LoginRequest
{
    /// <summary>
    /// Tên tài khoản đăng nhập.
    /// </summary>
    public string Username { get; set; }

    /// <summary>
    /// Mật khẩu đăng nhập dạng người dùng nhập từ giao diện.
    /// </summary>
    public string Password { get; set; }
}
