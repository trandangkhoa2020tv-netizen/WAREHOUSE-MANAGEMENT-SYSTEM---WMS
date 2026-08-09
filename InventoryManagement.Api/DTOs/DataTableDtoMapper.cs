using System.Data;

namespace InventoryManagement.ApiServer.DTOs;

/// <summary>
/// Chuyển DataTable hiện có sang DTO typed object cho các endpoint /api/v2.
/// Giữ mapper ở API layer để route cũ vẫn tương thích WinForms, route mới có JSON rõ field.
/// </summary>
public static class DataTableDtoMapper
{
    /// <summary>
    /// Chuyen bang hang hoa sang danh sach DTO cho API v2.
    /// </summary>
    public static List<HangHoaDto> ToHangHoaDtos(DataTable table)
    {
        return table.AsEnumerable().Select(row => new HangHoaDto
        {
            MaHangHoa = ToInt(row, 0),
            TenHangHoa = ToString(row, 1),
            TenLoaiHang = ToString(row, 2),
            TenNhaCungCap = ToString(row, 3),
            GiaNhap = ToDecimal(row, 4),
            GiaBan = ToDecimal(row, 5),
            SoLuongTon = ToInt(row, 6),
            DonViTinh = ToString(row, 7),
            GhiChu = ToString(row, 8)
        }).ToList();
    }

    /// <summary>
    /// Chuyen bang loai hang sang danh sach DTO cho API v2.
    /// </summary>
    public static List<LoaiHangDto> ToLoaiHangDtos(DataTable table)
    {
        return table.AsEnumerable().Select(row => new LoaiHangDto
        {
            MaLoaiHang = ToInt(row, 0),
            TenLoaiHang = ToString(row, 1),
            GhiChu = ToString(row, 2)
        }).ToList();
    }

    /// <summary>
    /// Chuyen bang nha cung cap sang danh sach DTO cho API v2.
    /// </summary>
    public static List<NhaCungCapDto> ToNhaCungCapDtos(DataTable table)
    {
        return table.AsEnumerable().Select(row => new NhaCungCapDto
        {
            MaNhaCungCap = ToInt(row, 0),
            TenNhaCungCap = ToString(row, 1),
            DiaChi = ToString(row, 2),
            SoDienThoai = ToString(row, 3),
            Email = ToString(row, 4),
            GhiChu = ToString(row, 5)
        }).ToList();
    }

    /// <summary>
    /// Chuyen bang khach hang sang danh sach DTO cho API v2.
    /// </summary>
    public static List<KhachHangDto> ToKhachHangDtos(DataTable table)
    {
        return table.AsEnumerable().Select(row => new KhachHangDto
        {
            MaKhachHang = ToInt(row, 0),
            TenKhachHang = ToString(row, 1),
            DiaChi = ToString(row, 2),
            SoDienThoai = ToString(row, 3),
            Email = ToString(row, 4),
            GhiChu = ToString(row, 5)
        }).ToList();
    }

    /// <summary>
    /// Chuyen bang nhan vien sang danh sach DTO cho API v2.
    /// </summary>
    public static List<NhanVienDto> ToNhanVienDtos(DataTable table)
    {
        return table.AsEnumerable().Select(row => new NhanVienDto
        {
            MaNhanVien = ToInt(row, 0),
            TenNhanVien = ToString(row, 1),
            DiaChi = ToString(row, 2),
            SoDienThoai = ToString(row, 3),
            Email = ToString(row, 4),
            ChucVu = ToString(row, 5),
            GhiChu = ToString(row, 6)
        }).ToList();
    }

    /// <summary>
    /// Chuyen bang phieu nhap sang danh sach DTO cho API v2.
    /// </summary>
    public static List<PhieuNhapDto> ToPhieuNhapDtos(DataTable table)
    {
        return table.AsEnumerable().Select(row => new PhieuNhapDto
        {
            MaPhieuNhap = ToInt(row, 0),
            TenNhaCungCap = ToString(row, 1),
            TenNhanVien = ToString(row, 2),
            NgayNhap = ToDateTime(row, 3),
            TongTien = ToDecimal(row, 4),
            GhiChu = ToString(row, 5)
        }).ToList();
    }

    /// <summary>
    /// Chuyen bang chi tiet phieu nhap sang danh sach DTO cho API v2.
    /// </summary>
    public static List<ChiTietPhieuNhapDto> ToChiTietPhieuNhapDtos(DataTable table)
    {
        return table.AsEnumerable().Select(row => new ChiTietPhieuNhapDto
        {
            TenHangHoa = ToString(row, 0),
            SoLuong = ToInt(row, 1),
            DonGiaNhap = ToDecimal(row, 2),
            ThanhTien = ToDecimal(row, 3)
        }).ToList();
    }

    /// <summary>
    /// Chuyen bang phieu xuat sang danh sach DTO cho API v2.
    /// </summary>
    public static List<PhieuXuatDto> ToPhieuXuatDtos(DataTable table)
    {
        return table.AsEnumerable().Select(row => new PhieuXuatDto
        {
            MaPhieuXuat = ToInt(row, 0),
            TenKhachHang = ToString(row, 1),
            TenNhanVien = ToString(row, 2),
            NgayXuat = ToDateTime(row, 3),
            TongTien = ToDecimal(row, 4),
            GhiChu = ToString(row, 5)
        }).ToList();
    }

    /// <summary>
    /// Chuyen bang chi tiet phieu xuat sang danh sach DTO cho API v2.
    /// </summary>
    public static List<ChiTietPhieuXuatDto> ToChiTietPhieuXuatDtos(DataTable table)
    {
        return table.AsEnumerable().Select(row => new ChiTietPhieuXuatDto
        {
            TenHangHoa = ToString(row, 0),
            SoLuong = ToInt(row, 1),
            DonViTinh = ToString(row, 2),
            DonGiaXuat = ToDecimal(row, 3),
            ThanhTien = ToDecimal(row, 4)
        }).ToList();
    }

    /// <summary>
    /// Chuyen bang thong tin phieu xuat sang danh sach DTO cho API v2.
    /// </summary>
    public static List<ThongTinPhieuXuatDto> ToThongTinPhieuXuatDtos(DataTable table)
    {
        return table.AsEnumerable().Select(row => new ThongTinPhieuXuatDto
        {
            MaPhieuXuat = ToInt(row, 0),
            TenKhachHang = ToString(row, 1),
            SoDienThoai = ToString(row, 2),
            DiaChi = ToString(row, 3),
            TenNhanVien = ToString(row, 4),
            NgayXuat = ToDateTime(row, 5),
            TongTien = ToDecimal(row, 6),
            GhiChu = ToString(row, 7)
        }).ToList();
    }

    /// <summary>
    /// Doc gia tri chuoi tai mot cot, tra chuoi rong neu database tra DBNull.
    /// </summary>
    private static string ToString(DataRow row, int index)
    {
        object value = row[index];
        return value == DBNull.Value ? string.Empty : Convert.ToString(value) ?? string.Empty;
    }

    /// <summary>
    /// Doc gia tri so nguyen tai mot cot, tra 0 neu database tra DBNull.
    /// </summary>
    private static int ToInt(DataRow row, int index)
    {
        object value = row[index];
        return value == DBNull.Value ? 0 : Convert.ToInt32(value);
    }

    /// <summary>
    /// Doc gia tri decimal tai mot cot, tra 0 neu database tra DBNull.
    /// </summary>
    private static decimal ToDecimal(DataRow row, int index)
    {
        object value = row[index];
        return value == DBNull.Value ? 0 : Convert.ToDecimal(value);
    }

    /// <summary>
    /// Doc gia tri thoi gian tai mot cot, tra DateTime.MinValue neu database tra DBNull.
    /// </summary>
    private static DateTime ToDateTime(DataRow row, int index)
    {
        object value = row[index];
        return value == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(value);
    }
}
