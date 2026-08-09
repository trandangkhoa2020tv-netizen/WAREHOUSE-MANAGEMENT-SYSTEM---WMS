namespace InventoryManagement.ApiServer.Services;

/// <summary>
/// Exception đại diện cho lỗi validate dữ liệu đầu vào của API.
/// </summary>
public sealed class ApiValidationException : Exception
{
    /// <summary>
    /// Tạo exception validate với danh sách lỗi chi tiết.
    /// </summary>
    public ApiValidationException(List<string> errors)
        : base("Du lieu khong hop le.")
    {
        Errors = errors;
    }

    /// <summary>
    /// Danh sách lỗi validate để trả về client.
    /// </summary>
    public IReadOnlyList<string> Errors { get; }
}
