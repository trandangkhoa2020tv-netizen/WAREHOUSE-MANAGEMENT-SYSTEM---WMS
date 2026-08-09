using System.Data;
using System.Text.Json.Serialization;
using InventoryManagement.Models;

namespace InventoryManagement.ApiClients
{
    /// <summary>
    /// Client gọi API nghiệp vụ nhập kho.
    /// </summary>
    public sealed class PhieuNhapApiClient
    {
        /// <summary>Lấy lịch sử phiếu nhập để hiển thị trên form nhập kho.</summary>
        public DataTable GetAllPhieuNhap() => ApiHttpClient.GetTable("api/phieu-nhap");

        /// <summary>Lấy chi tiết các mặt hàng của một phiếu nhập theo mã phiếu.</summary>
        public DataTable GetChiTietTheoMaPhieu(int id) => ApiHttpClient.GetTable($"api/phieu-nhap/{id}/chi-tiet");

        /// <summary>
        /// Lưu phiếu nhập cùng danh sách chi tiết và yêu cầu backend cộng tồn kho trong transaction.
        /// </summary>
        public void LuuPhieuNhap(PhieuNhap phieuNhap, List<ChiTietPhieuNhap> chiTietList)
        {
            ApiHttpClient.Post("api/phieu-nhap", new { phieuNhap, chiTietList });
        }
    }

    /// <summary>
    /// Client gọi API nghiệp vụ xuất kho.
    /// </summary>
    public sealed class PhieuXuatApiClient
    {
        /// <summary>Lấy lịch sử phiếu xuất để hiển thị trên form xuất kho.</summary>
        public DataTable GetAllPhieuXuat() => ApiHttpClient.GetTable("api/phieu-xuat");

        /// <summary>Lấy chi tiết các mặt hàng của một phiếu xuất theo mã phiếu.</summary>
        public DataTable GetChiTietTheoMaPhieu(int id) => ApiHttpClient.GetTable($"api/phieu-xuat/{id}/chi-tiet");

        /// <summary>Lấy thông tin tổng quát của phiếu xuất để in báo cáo hoặc xuất file.</summary>
        public DataTable GetThongTinPhieuXuat(int id) => ApiHttpClient.GetTable($"api/phieu-xuat/{id}/thong-tin");

        /// <summary>
        /// Lưu phiếu xuất cùng danh sách chi tiết và yêu cầu backend trừ tồn kho trong transaction.
        /// </summary>
        public void LuuPhieuXuat(PhieuXuat phieuXuat, List<ChiTietPhieuXuat> chiTietList)
        {
            ApiHttpClient.Post("api/phieu-xuat", new { phieuXuat, chiTietList });
        }
    }

    /// <summary>
    /// Client gọi API đăng nhập và trả về vai trò người dùng sau khi xác thực.
    /// </summary>
    public sealed class AuthApiClient
    {
        /// <summary>
        /// Kiểm tra tài khoản mật khẩu; trả về chuỗi rỗng nếu đăng nhập thất bại.
        /// </summary>
        public string CheckLogin(string username, string password)
        {
            try
            {
                LoginResponse response = ApiHttpClient.PostForJson<object, LoginResponse>(
                    "api/auth/login",
                    new { username, password });

                if (!string.IsNullOrWhiteSpace(response?.Token))
                {
                    ApiHttpClient.SetBearerToken(response.Token);
                }

                return response?.VaiTro ?? string.Empty;
            }
            catch (UnauthorizedAccessException)
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Kiểu dữ liệu nhận về từ endpoint đăng nhập.
        /// </summary>
        private sealed class LoginResponse
        {
            /// <summary>Tên tài khoản đã đăng nhập.</summary>
            [JsonPropertyName("tenTaiKhoan")]
            public string TenTaiKhoan { get; set; }

            /// <summary>Vai trò phân quyền của tài khoản.</summary>
            [JsonPropertyName("vaiTro")]
            public string VaiTro { get; set; }

            /// <summary>JWT dùng để gọi các endpoint nghiệp vụ nếu backend bật JwtSettings.RequireJwt.</summary>
            [JsonPropertyName("token")]
            public string Token { get; set; }

            /// <summary>Thời điểm token hết hạn theo giờ UTC từ API.</summary>
            [JsonPropertyName("expiresAt")]
            public DateTime ExpiresAt { get; set; }
        }
    }
}
