using System.Data;
using Npgsql;
using InventoryManagement.Data;

namespace InventoryManagement.ApiServer;

/// <summary>
/// Các truy vấn API phục vụ màn hình tổng hợp và cảnh báo tồn kho.
/// </summary>
public sealed class InventoryApiQueries
{
    private readonly DatabaseHelper _db = new DatabaseHelper();

    /// <summary>
    /// Lấy danh sách hàng hóa có số lượng tồn nhỏ hơn hoặc bằng ngưỡng truyền vào.
    /// </summary>
    public DataTable GetTonKhoThap(int soLuongToiDa)
    {
        const string sql = @"SELECT ma_hanghoa AS ""Ma Hang"", ten_hanghoa AS ""Ten Hang Hoa"",
                                    so_luong_ton AS ""Ton Kho"", don_vi_tinh AS ""DVT""
                             FROM hanghoa
                             WHERE is_deleted = false AND so_luong_ton <= @soLuongToiDa
                             ORDER BY so_luong_ton ASC, ten_hanghoa ASC";
        return _db.ExecuteQuery(sql, new[] { new NpgsqlParameter("@soLuongToiDa", soLuongToiDa) });
    }
}
