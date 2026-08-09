using System.Data;
using Npgsql;
using InventoryManagement.Data;
using InventoryManagement.Models;

namespace InventoryManagement.Repositories;

/// <summary>
/// Truy vấn các số liệu tổng hợp dùng riêng cho trang thống kê.
/// </summary>
public sealed class DashboardRepository
{
    private readonly DatabaseHelper _db = new DatabaseHelper();

    /// <summary>
    /// Lấy các chỉ số tổng quan của hệ thống trong một truy vấn.
    /// </summary>
    public DashboardSummary GetSummary()
    {
        const string sql = """
            SELECT
                (SELECT COUNT(*) FROM hanghoa WHERE COALESCE(is_deleted, false) = false) AS tong_hang_hoa,
                (SELECT COUNT(*) FROM khachhang) AS tong_khach_hang,
                (SELECT COUNT(*) FROM nhanvien WHERE COALESCE(is_deleted, false) = false) AS tong_nhan_vien,
                COALESCE((SELECT SUM(so_luong) FROM chitietphieunhap), 0) AS tong_nhap_kho,
                COALESCE((SELECT SUM(so_luong) FROM chitietphieuxuat), 0) AS tong_xuat_kho,
                COALESCE((
                    SELECT SUM(so_luong_ton * gia_nhap)
                    FROM hanghoa
                    WHERE COALESCE(is_deleted, false) = false
                ), 0) AS gia_tri_ton_kho,
                (SELECT COUNT(*)
                 FROM hanghoa
                 WHERE COALESCE(is_deleted, false) = false
                   AND so_luong_ton <= 10) AS so_hang_sap_het
            """;

        DataTable table = _db.ExecuteQuery(sql);
        if (table.Rows.Count == 0)
        {
            return new DashboardSummary();
        }

        DataRow row = table.Rows[0];
        return new DashboardSummary
        {
            TongHangHoa = Convert.ToInt32(row["tong_hang_hoa"]),
            TongKhachHang = Convert.ToInt32(row["tong_khach_hang"]),
            TongNhanVien = Convert.ToInt32(row["tong_nhan_vien"]),
            TongNhapKho = Convert.ToInt32(row["tong_nhap_kho"]),
            TongXuatKho = Convert.ToInt32(row["tong_xuat_kho"]),
            GiaTriTonKho = Convert.ToDecimal(row["gia_tri_ton_kho"]),
            SoHangSapHet = Convert.ToInt32(row["so_hang_sap_het"])
        };
    }

    /// <summary>
    /// Lấy tổng số lượng nhập và xuất của 12 tháng gần nhất, kể cả tháng không phát sinh.
    /// </summary>
    public List<DashboardMonthlyPoint> GetMonthlyMovement()
    {
        const string sql = """
            WITH thang AS (
                SELECT generate_series(
                    date_trunc('month', CURRENT_DATE) - INTERVAL '11 months',
                    date_trunc('month', CURRENT_DATE),
                    INTERVAL '1 month'
                ) AS bat_dau_thang
            ),
            nhap AS (
                SELECT date_trunc('month', pn.ngay_nhap) AS bat_dau_thang,
                       COALESCE(SUM(ct.so_luong), 0) AS so_luong
                FROM phieunhap pn
                JOIN chitietphieunhap ct ON ct.ma_phieunhap = pn.ma_phieunhap
                GROUP BY date_trunc('month', pn.ngay_nhap)
            ),
            xuat AS (
                SELECT date_trunc('month', px.ngay_xuat) AS bat_dau_thang,
                       COALESCE(SUM(ct.so_luong), 0) AS so_luong
                FROM phieuxuat px
                JOIN chitietphieuxuat ct ON ct.ma_phieuxuat = px.ma_phieuxuat
                GROUP BY date_trunc('month', px.ngay_xuat)
            )
            SELECT EXTRACT(MONTH FROM t.bat_dau_thang)::int AS thang,
                   EXTRACT(YEAR FROM t.bat_dau_thang)::int AS nam,
                   COALESCE(n.so_luong, 0)::int AS so_luong_nhap,
                   COALESCE(x.so_luong, 0)::int AS so_luong_xuat
            FROM thang t
            LEFT JOIN nhap n ON n.bat_dau_thang = t.bat_dau_thang
            LEFT JOIN xuat x ON x.bat_dau_thang = t.bat_dau_thang
            ORDER BY t.bat_dau_thang
            """;

        DataTable table = _db.ExecuteQuery(sql);
        return table.Rows.Cast<DataRow>()
            .Select(row => new DashboardMonthlyPoint
            {
                Thang = Convert.ToInt32(row["thang"]),
                Nam = Convert.ToInt32(row["nam"]),
                SoLuongNhap = Convert.ToInt32(row["so_luong_nhap"]),
                SoLuongXuat = Convert.ToInt32(row["so_luong_xuat"])
            })
            .ToList();
    }

    /// <summary>
    /// Lấy tỷ lệ hàng hóa theo loại hàng.
    /// </summary>
    public List<DashboardCategoryPoint> GetProductsByCategory()
    {
        const string sql = """
            SELECT lh.ten_loaihang,
                   COUNT(h.ma_hanghoa)::int AS so_luong_hang_hoa,
                   COALESCE(SUM(h.so_luong_ton), 0)::int AS tong_ton_kho
            FROM loaihang lh
            LEFT JOIN hanghoa h
                   ON h.ma_loaihang = lh.ma_loaihang
                  AND COALESCE(h.is_deleted, false) = false
            GROUP BY lh.ma_loaihang, lh.ten_loaihang
            ORDER BY so_luong_hang_hoa DESC, lh.ten_loaihang
            """;

        DataTable table = _db.ExecuteQuery(sql);
        return table.Rows.Cast<DataRow>()
            .Select(row => new DashboardCategoryPoint
            {
                TenLoaiHang = Convert.ToString(row["ten_loaihang"]) ?? string.Empty,
                SoLuongHangHoa = Convert.ToInt32(row["so_luong_hang_hoa"]),
                TongTonKho = Convert.ToInt32(row["tong_ton_kho"])
            })
            .ToList();
    }

    /// <summary>
    /// Lấy các mặt hàng đang còn nhiều nhất.
    /// </summary>
    public List<DashboardStockPoint> GetTopStock(int limit)
    {
        const string sql = """
            SELECT ten_hanghoa, so_luong_ton
            FROM hanghoa
            WHERE COALESCE(is_deleted, false) = false
            ORDER BY so_luong_ton DESC, ten_hanghoa
            LIMIT @limit
            """;

        DataTable table = _db.ExecuteQuery(sql, new[]
        {
            new NpgsqlParameter("@limit", limit)
        });

        return table.Rows.Cast<DataRow>()
            .Select(row => new DashboardStockPoint
            {
                TenHangHoa = Convert.ToString(row["ten_hanghoa"]) ?? string.Empty,
                SoLuongTon = Convert.ToInt32(row["so_luong_ton"])
            })
            .ToList();
    }

    /// <summary>
    /// Lấy các khách hàng có tổng giá trị xuất kho cao nhất.
    /// </summary>
    public List<DashboardCustomerPoint> GetTopCustomers(int limit)
    {
        const string sql = """
            SELECT kh.ten_khachhang,
                   COUNT(px.ma_phieuxuat)::int AS so_lan_mua,
                   COALESCE(SUM(px.tong_tien), 0) AS tong_gia_tri
            FROM khachhang kh
            JOIN phieuxuat px ON px.ma_khachhang = kh.ma_khachhang
            GROUP BY kh.ma_khachhang, kh.ten_khachhang
            ORDER BY tong_gia_tri DESC, so_lan_mua DESC, kh.ten_khachhang
            LIMIT @limit
            """;

        DataTable table = _db.ExecuteQuery(sql, new[]
        {
            new NpgsqlParameter("@limit", limit)
        });

        return table.Rows.Cast<DataRow>()
            .Select(row => new DashboardCustomerPoint
            {
                TenKhachHang = Convert.ToString(row["ten_khachhang"]) ?? string.Empty,
                SoLanMua = Convert.ToInt32(row["so_lan_mua"]),
                TongGiaTri = Convert.ToDecimal(row["tong_gia_tri"])
            })
            .ToList();
    }
}
