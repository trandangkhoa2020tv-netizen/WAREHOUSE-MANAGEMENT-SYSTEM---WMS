using InventoryManagement.ApiServer.Services;

namespace InventoryManagement.ApiServer.Endpoints;

/// <summary>
/// Nhóm endpoint tra cứu tồn kho.
/// </summary>
public static class KhoEndpoints
{
    /// <summary>
    /// Đăng ký route tồn kho vào ứng dụng Minimal API.
    /// </summary>
    public static IEndpointRouteBuilder MapKhoEndpoints(this IEndpointRouteBuilder app)
    {
        // Lấy danh sách hàng hóa có số lượng tồn thấp hơn hoặc bằng ngưỡng truyền vào.
        app.MapGet("/api/ton-kho/thap", (int? soLuongToiDa, IKhoService service) =>
            ApiResults.Safe(() => ApiResults.OkTable(service.GetTonKhoThap(soLuongToiDa))));

        return app;
    }
}
