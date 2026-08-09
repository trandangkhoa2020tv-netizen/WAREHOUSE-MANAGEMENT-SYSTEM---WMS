namespace InventoryManagement.ApiServer.DTOs;

/// <summary>
/// DTO trả về thông tin hàng hóa cho API hiện đại.
/// </summary>
public sealed class HangHoaDto
{
    public int MaHangHoa { get; set; }
    public string TenHangHoa { get; set; }
    public string TenLoaiHang { get; set; }
    public string TenNhaCungCap { get; set; }
    public decimal GiaNhap { get; set; }
    public decimal GiaBan { get; set; }
    public int SoLuongTon { get; set; }
    public string DonViTinh { get; set; }
    public string GhiChu { get; set; }
}

/// <summary>
/// DTO trả về thông tin loại hàng.
/// </summary>
public sealed class LoaiHangDto
{
    public int MaLoaiHang { get; set; }
    public string TenLoaiHang { get; set; }
    public string GhiChu { get; set; }
}

/// <summary>
/// DTO trả về thông tin nhà cung cấp.
/// </summary>
public sealed class NhaCungCapDto
{
    public int MaNhaCungCap { get; set; }
    public string TenNhaCungCap { get; set; }
    public string DiaChi { get; set; }
    public string SoDienThoai { get; set; }
    public string Email { get; set; }
    public string GhiChu { get; set; }
}

/// <summary>
/// DTO trả về thông tin khách hàng.
/// </summary>
public sealed class KhachHangDto
{
    public int MaKhachHang { get; set; }
    public string TenKhachHang { get; set; }
    public string DiaChi { get; set; }
    public string SoDienThoai { get; set; }
    public string Email { get; set; }
    public string GhiChu { get; set; }
}

/// <summary>
/// DTO trả về thông tin nhân viên.
/// </summary>
public sealed class NhanVienDto
{
    public int MaNhanVien { get; set; }
    public string TenNhanVien { get; set; }
    public string DiaChi { get; set; }
    public string SoDienThoai { get; set; }
    public string Email { get; set; }
    public string ChucVu { get; set; }
    public string GhiChu { get; set; }
}

/// <summary>
/// DTO trả về thông tin phiếu nhập.
/// </summary>
public sealed class PhieuNhapDto
{
    public int MaPhieuNhap { get; set; }
    public string TenNhaCungCap { get; set; }
    public string TenNhanVien { get; set; }
    public DateTime NgayNhap { get; set; }
    public decimal TongTien { get; set; }
    public string GhiChu { get; set; }
}

/// <summary>
/// DTO trả về chi tiết một dòng hàng trong phiếu nhập.
/// </summary>
public sealed class ChiTietPhieuNhapDto
{
    public string TenHangHoa { get; set; }
    public int SoLuong { get; set; }
    public decimal DonGiaNhap { get; set; }
    public decimal ThanhTien { get; set; }
}

/// <summary>
/// DTO trả về thông tin phiếu xuất.
/// </summary>
public sealed class PhieuXuatDto
{
    public int MaPhieuXuat { get; set; }
    public string TenKhachHang { get; set; }
    public string TenNhanVien { get; set; }
    public DateTime NgayXuat { get; set; }
    public decimal TongTien { get; set; }
    public string GhiChu { get; set; }
}

/// <summary>
/// DTO trả về chi tiết một dòng hàng trong phiếu xuất.
/// </summary>
public sealed class ChiTietPhieuXuatDto
{
    public string TenHangHoa { get; set; }
    public int SoLuong { get; set; }
    public string DonViTinh { get; set; }
    public decimal DonGiaXuat { get; set; }
    public decimal ThanhTien { get; set; }
}

/// <summary>
/// DTO trả về thông tin chung của phiếu xuất để in hoặc xuất file.
/// </summary>
public sealed class ThongTinPhieuXuatDto
{
    public int MaPhieuXuat { get; set; }
    public string TenKhachHang { get; set; }
    public string SoDienThoai { get; set; }
    public string DiaChi { get; set; }
    public string TenNhanVien { get; set; }
    public DateTime NgayXuat { get; set; }
    public decimal TongTien { get; set; }
    public string GhiChu { get; set; }
}
