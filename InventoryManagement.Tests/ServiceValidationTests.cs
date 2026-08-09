using InventoryManagement.ApiServer.DTOs;
using InventoryManagement.ApiServer.Services;
using InventoryManagement.Models;
using InventoryManagement.Repositories;

namespace InventoryManagement.Tests;

public sealed class ServiceValidationTests
{
    /// <summary>
    /// Kiem tra AuthService tu choi request dang nhap thieu ten tai khoan va mat khau.
    /// </summary>
    [Fact]
    public void AuthService_ShouldRejectEmptyLoginRequest()
    {
        AuthService service = new AuthService(new TaiKhoanRepository());

        InvalidOperationException ex = Assert.Throws<InvalidOperationException>(() =>
            service.CheckLogin(new LoginRequest
            {
                Username = "",
                Password = ""
            }));

        Assert.Contains("Vui long nhap ten tai khoan", ex.Message);
    }

    /// <summary>
    /// Kiem tra HangHoaService tu choi mat hang thieu ten.
    /// </summary>
    [Fact]
    public void HangHoaService_ShouldRejectMissingName()
    {
        HangHoaService service = new HangHoaService(new HangHoaRepository());

        ApiValidationException ex = Assert.Throws<ApiValidationException>(() =>
            service.Them(new HangHoa
            {
                TenHangHoa = "",
                MaLoaiHang = 1,
                MaNhaCungCap = 1,
                GiaNhap = 1000,
                GiaBan = 1500,
                SoLuongTon = 10,
                DonViTinh = "Cai"
            }));

        Assert.Contains(ex.Errors, error => error.Contains("tenHangHoa"));
    }

    /// <summary>
    /// Kiem tra HangHoaService tu choi so luong ton am.
    /// </summary>
    [Fact]
    public void HangHoaService_ShouldRejectNegativeStock()
    {
        HangHoaService service = new HangHoaService(new HangHoaRepository());

        ApiValidationException ex = Assert.Throws<ApiValidationException>(() =>
            service.Them(new HangHoa
            {
                TenHangHoa = "Laptop",
                MaLoaiHang = 1,
                MaNhaCungCap = 1,
                GiaNhap = 1000,
                GiaBan = 1500,
                SoLuongTon = -1,
                DonViTinh = "Cai"
            }));

        Assert.Contains(ex.Errors, error => error.Contains("soLuongTon"));
    }

    /// <summary>
    /// Kiem tra PhieuNhapService tu choi phieu nhap khong co dong chi tiet.
    /// </summary>
    [Fact]
    public void PhieuNhapService_ShouldRejectEmptyDetails()
    {
        PhieuNhapService service = new PhieuNhapService(new PhieuNhapRepository());

        ApiValidationException ex = Assert.Throws<ApiValidationException>(() =>
            service.LuuPhieuNhap(new LuuPhieuNhapRequest
            {
                PhieuNhap = new PhieuNhap
                {
                    MaNhaCungCap = 1,
                    MaNhanVien = 1,
                    TongTien = 1000
                },
                ChiTietList = new List<ChiTietPhieuNhap>()
            }));

        Assert.Contains(ex.Errors, error => error.Contains("it nhat mot mat hang"));
    }

    /// <summary>
    /// Kiem tra PhieuNhapService tu choi dong chi tiet co so luong khong hop le.
    /// </summary>
    [Fact]
    public void PhieuNhapService_ShouldRejectInvalidDetailQuantity()
    {
        PhieuNhapService service = new PhieuNhapService(new PhieuNhapRepository());

        ApiValidationException ex = Assert.Throws<ApiValidationException>(() =>
            service.LuuPhieuNhap(new LuuPhieuNhapRequest
            {
                PhieuNhap = new PhieuNhap
                {
                    MaNhaCungCap = 1,
                    MaNhanVien = 1,
                    TongTien = 1000
                },
                ChiTietList = new List<ChiTietPhieuNhap>
                {
                    new ChiTietPhieuNhap
                    {
                        MaHangHoa = 1,
                        SoLuong = -5,
                        DonGiaNhap = 1000,
                        ThanhTien = -5000
                    }
                }
            }));

        Assert.Contains(ex.Errors, error => error.Contains("soLuong"));
    }

    /// <summary>
    /// Kiem tra PhieuXuatService tu choi phieu xuat khong co dong chi tiet.
    /// </summary>
    [Fact]
    public void PhieuXuatService_ShouldRejectEmptyDetails()
    {
        PhieuXuatService service = new PhieuXuatService(new PhieuXuatRepository());

        ApiValidationException ex = Assert.Throws<ApiValidationException>(() =>
            service.LuuPhieuXuat(new LuuPhieuXuatRequest
            {
                PhieuXuat = new PhieuXuat
                {
                    MaKhachHang = 1,
                    MaNhanVien = 1,
                    TongTien = 1000
                },
                ChiTietList = new List<ChiTietPhieuXuat>()
            }));

        Assert.Contains(ex.Errors, error => error.Contains("it nhat mot mat hang"));
    }

    /// <summary>
    /// Kiem tra PhieuXuatService tu choi dong chi tiet co so luong khong hop le.
    /// </summary>
    [Fact]
    public void PhieuXuatService_ShouldRejectInvalidDetailQuantity()
    {
        PhieuXuatService service = new PhieuXuatService(new PhieuXuatRepository());

        ApiValidationException ex = Assert.Throws<ApiValidationException>(() =>
            service.LuuPhieuXuat(new LuuPhieuXuatRequest
            {
                PhieuXuat = new PhieuXuat
                {
                    MaKhachHang = 1,
                    MaNhanVien = 1,
                    TongTien = 1000
                },
                ChiTietList = new List<ChiTietPhieuXuat>
                {
                    new ChiTietPhieuXuat
                    {
                        MaHangHoa = 1,
                        SoLuong = -5,
                        DonGiaXuat = 1000,
                        ThanhTien = -5000
                    }
                }
            }));

        Assert.Contains(ex.Errors, error => error.Contains("soLuong"));
    }
}
