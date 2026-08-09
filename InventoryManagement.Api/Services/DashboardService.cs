using InventoryManagement.Models;
using InventoryManagement.Repositories;

namespace InventoryManagement.ApiServer.Services;

/// <summary>
/// Hợp đồng xử lý dữ liệu cho trang thống kê.
/// </summary>
public interface IDashboardService
{
    DashboardData GetDashboard();
    DashboardSummary GetSummary();
    List<DashboardMonthlyPoint> GetMonthlyMovement();
    List<DashboardCategoryPoint> GetProductsByCategory();
    List<DashboardStockPoint> GetTopStock(int? limit);
    List<DashboardCustomerPoint> GetTopCustomers(int? limit);
}

/// <summary>
/// Tổng hợp các nhóm số liệu dashboard và giới hạn số dòng hiển thị.
/// </summary>
public sealed class DashboardService : IDashboardService
{
    private readonly DashboardRepository _repository;

    public DashboardService(DashboardRepository repository)
    {
        _repository = repository;
    }

    public DashboardData GetDashboard()
    {
        return new DashboardData
        {
            Summary = GetSummary(),
            NhapXuatTheoThang = GetMonthlyMovement(),
            HangTheoDanhMuc = GetProductsByCategory(),
            TonKhoNhieu = GetTopStock(8),
            KhachHangMuaNhieu = GetTopCustomers(8)
        };
    }

    public DashboardSummary GetSummary() => _repository.GetSummary();

    public List<DashboardMonthlyPoint> GetMonthlyMovement() => _repository.GetMonthlyMovement();

    public List<DashboardCategoryPoint> GetProductsByCategory() => _repository.GetProductsByCategory();

    public List<DashboardStockPoint> GetTopStock(int? limit) => _repository.GetTopStock(NormalizeLimit(limit));

    public List<DashboardCustomerPoint> GetTopCustomers(int? limit) => _repository.GetTopCustomers(NormalizeLimit(limit));

    private static int NormalizeLimit(int? limit)
    {
        return Math.Clamp(limit ?? 8, 1, 20);
    }
}
