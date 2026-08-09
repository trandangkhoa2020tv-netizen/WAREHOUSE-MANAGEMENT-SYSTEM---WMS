using InventoryManagement.Models;

namespace InventoryManagement.ApiServer.DTOs;

/// <summary>
/// Dữ liệu body khi lưu phiếu nhập kèm danh sách chi tiết mặt hàng.
/// </summary>
public sealed class LuuPhieuNhapRequest
{
    /// <summary>
    /// Thông tin phiếu nhập chính: nhà cung cấp, nhân viên, tổng tiền và ghi chú.
    /// </summary>
    public PhieuNhap PhieuNhap { get; set; }

    /// <summary>
    /// Danh sách các dòng hàng hóa trong phiếu nhập.
    /// </summary>
    public List<ChiTietPhieuNhap> ChiTietList { get; set; } = new List<ChiTietPhieuNhap>();
}

/// <summary>
/// Dữ liệu body khi lưu phiếu xuất kèm danh sách chi tiết mặt hàng.
/// </summary>
public sealed class LuuPhieuXuatRequest
{
    /// <summary>
    /// Thông tin phiếu xuất chính: khách hàng, nhân viên, tổng tiền và ghi chú.
    /// </summary>
    public PhieuXuat PhieuXuat { get; set; }

    /// <summary>
    /// Danh sách các dòng hàng hóa trong phiếu xuất.
    /// </summary>
    public List<ChiTietPhieuXuat> ChiTietList { get; set; } = new List<ChiTietPhieuXuat>();
}
