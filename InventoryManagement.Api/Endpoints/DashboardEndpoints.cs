using InventoryManagement.ApiServer.Services;

namespace InventoryManagement.ApiServer.Endpoints;

/// <summary>
/// Các endpoint cung cấp số liệu cho trang thống kê.
/// </summary>
public static class DashboardEndpoints
{
    public static IEndpointRouteBuilder MapDashboardEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/api/dashboard", (IDashboardService service) =>
            ApiResults.Safe(() => Results.Ok(service.GetDashboard())));

        app.MapGet("/api/dashboard/summary", (IDashboardService service) =>
            ApiResults.Safe(() => Results.Ok(service.GetSummary())));

        app.MapGet("/api/dashboard/monthly", (IDashboardService service) =>
            ApiResults.Safe(() => Results.Ok(service.GetMonthlyMovement())));

        app.MapGet("/api/dashboard/products-by-category", (IDashboardService service) =>
            ApiResults.Safe(() => Results.Ok(service.GetProductsByCategory())));

        app.MapGet("/api/dashboard/top-stock", (int? limit, IDashboardService service) =>
            ApiResults.Safe(() => Results.Ok(service.GetTopStock(limit))));

        app.MapGet("/api/dashboard/top-customers", (int? limit, IDashboardService service) =>
            ApiResults.Safe(() => Results.Ok(service.GetTopCustomers(limit))));

        return app;
    }
}
