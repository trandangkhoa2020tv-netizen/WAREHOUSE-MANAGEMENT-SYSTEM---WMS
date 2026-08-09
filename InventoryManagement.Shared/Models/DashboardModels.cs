namespace InventoryManagement.Models
{
    /// <summary>
    /// Các số liệu tổng quan hiển thị ở đầu trang thống kê.
    /// </summary>
    public sealed class DashboardSummary
    {
        public int TongHangHoa { get; set; }
        public int TongKhachHang { get; set; }
        public int TongNhanVien { get; set; }
        public int TongNhapKho { get; set; }
        public int TongXuatKho { get; set; }
        public int SoHangSapHet { get; set; }
        public decimal GiaTriTonKho { get; set; }
    }

    /// <summary>
    /// Tổng số lượng nhập và xuất trong một tháng.
    /// </summary>
    public sealed class DashboardMonthlyPoint
    {
        public int Thang { get; set; }
        public int Nam { get; set; }
        public int SoLuongNhap { get; set; }
        public int SoLuongXuat { get; set; }
    }

    /// <summary>
    /// Số lượng mặt hàng và tổng tồn kho theo loại hàng.
    /// </summary>
    public sealed class DashboardCategoryPoint
    {
        public string TenLoaiHang { get; set; } = string.Empty;
        public int SoLuongHangHoa { get; set; }
        public int TongTonKho { get; set; }
    }

    /// <summary>
    /// Số lượng tồn hiện tại của một mặt hàng.
    /// </summary>
    public sealed class DashboardStockPoint
    {
        public string TenHangHoa { get; set; } = string.Empty;
        public int SoLuongTon { get; set; }
    }

    /// <summary>
    /// Số lần mua và tổng giá trị mua của một khách hàng.
    /// </summary>
    public sealed class DashboardCustomerPoint
    {
        public string TenKhachHang { get; set; } = string.Empty;
        public int SoLanMua { get; set; }
        public decimal TongGiaTri { get; set; }
    }

    /// <summary>
    /// Toàn bộ dữ liệu cần để dựng một lần trang thống kê.
    /// </summary>
    public sealed class DashboardData
    {
        public DashboardSummary Summary { get; set; } = new DashboardSummary();
        public List<DashboardMonthlyPoint> NhapXuatTheoThang { get; set; } = new List<DashboardMonthlyPoint>();
        public List<DashboardCategoryPoint> HangTheoDanhMuc { get; set; } = new List<DashboardCategoryPoint>();
        public List<DashboardStockPoint> TonKhoNhieu { get; set; } = new List<DashboardStockPoint>();
        public List<DashboardCustomerPoint> KhachHangMuaNhieu { get; set; } = new List<DashboardCustomerPoint>();
    }
}
