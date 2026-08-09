using System.Data;
using InventoryManagement.ApiServer;

namespace InventoryManagement.ApiServer.Services;

/// <summary>
/// Hợp đồng xử lý các nghiệp vụ tra cứu kho.
/// </summary>
public interface IKhoService
{
    /// <summary>
    /// Lấy danh sách hàng hóa có tồn kho thấp hơn hoặc bằng ngưỡng truyền vào.
    /// </summary>
    DataTable GetTonKhoThap(int? soLuongToiDa);
}

/// <summary>
/// Service xử lý nghiệp vụ tra cứu tồn kho.
/// </summary>
public sealed class KhoService : IKhoService
{
    private readonly InventoryApiQueries _queries;

    /// <summary>
    /// Khởi tạo service kho với lớp truy vấn tồn kho.
    /// </summary>
    public KhoService(InventoryApiQueries queries)
    {
        _queries = queries;
    }

    /// <summary>
    /// Lấy hàng tồn kho thấp; nếu không truyền ngưỡng thì mặc định dùng 10.
    /// </summary>
    public DataTable GetTonKhoThap(int? soLuongToiDa)
    {
        return _queries.GetTonKhoThap(soLuongToiDa ?? 10);
    }
}
