namespace InventoryManagement.ApiServer.Config;

/// <summary>
/// Cau hinh chung cua API: URL lang nghe, API key va danh sach origin duoc phep goi CORS.
/// </summary>
public sealed class ApiSettings
{
    /// <summary>
    /// URL backend dùng để lắng nghe request HTTP.
    /// </summary>
    public string Url { get; set; } = "http://localhost:8088";

    /// <summary>
    /// Bật/tắt yêu cầu API key cho các request gửi vào backend.
    /// </summary>
    public bool RequireApiKey { get; set; }

    /// <summary>
    /// Khóa API hợp lệ khi cấu hình RequireApiKey được bật.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// Danh sách origin được phép gọi API qua CORS.
    /// Rỗng hoặc "*" nghĩa là cho phép tất cả origin.
    /// </summary>
    public string[] AllowedOrigins { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Kiểm tra cấu hình CORS có đang cho phép mọi origin gọi API hay không.
    /// </summary>
    public bool AllowsAnyOrigin()
    {
        return AllowedOrigins == null || AllowedOrigins.Length == 0 || AllowedOrigins.Contains("*");
    }
}
