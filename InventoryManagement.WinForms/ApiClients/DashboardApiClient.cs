using InventoryManagement.Models;

namespace InventoryManagement.ApiClients
{
    /// <summary>
    /// Client lấy toàn bộ số liệu cần thiết cho trang thống kê.
    /// </summary>
    public sealed class DashboardApiClient
    {
        public DashboardData GetDashboard()
        {
            return ApiHttpClient.GetJson<DashboardData>("api/dashboard") ?? new DashboardData();
        }
    }
}
