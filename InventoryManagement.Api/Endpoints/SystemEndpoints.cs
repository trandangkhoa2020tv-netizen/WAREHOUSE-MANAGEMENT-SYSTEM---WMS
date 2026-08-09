using InventoryManagement.ApiServer.Services;
using InventoryManagement.Data;

namespace InventoryManagement.ApiServer.Endpoints;

/// <summary>
/// Nhóm endpoint hệ thống dùng để kiểm tra trạng thái API và xem tài liệu route nhanh.
/// </summary>
public static class SystemEndpoints
{
    /// <summary>
    /// Đăng ký các route hệ thống vào ứng dụng Minimal API.
    /// </summary>
    public static IEndpointRouteBuilder MapSystemEndpoints(this IEndpointRouteBuilder app)
    {
        // Route gốc chuyển về health check để người dùng biết API đang chạy.
        app.MapGet("/", () => Results.Redirect("/api/health"));

        // Kiểm tra trạng thái API và khả năng kết nối PostgreSQL.
        app.MapGet("/api/health", () => ApiResults.Safe(() => Results.Ok(new
        {
            status = "running",
            database = DbConnection.TestConnection() ? "connected" : "disconnected",
            time = DateTime.Now
        })));

        // Trả danh sách nhóm chức năng mà backend đang cung cấp.
        app.MapGet("/api/chuc-nang", () => ApiResults.Safe(() => Results.Ok(new[]
        {
            new { nhom = "Danh muc", moTa = "CRUD hang hoa, loai hang, nha cung cap, khach hang, nhan vien." },
            new { nhom = "Kho", moTa = "Tra cuu ton kho va canh bao hang ton thap." },
            new { nhom = "Nhap kho", moTa = "Tao phieu nhap, cong ton kho, tra cuu lich su va chi tiet." },
            new { nhom = "Xuat kho", moTa = "Tao phieu xuat, tru ton kho, tra cuu lich su va chi tiet." },
            new { nhom = "Dang nhap", moTa = "Xac thuc tai khoan va tra ve vai tro nguoi dung." }
        })));

        // Trả tài liệu route ngắn để test API nhanh bằng trình duyệt/Postman.
        app.MapGet("/api/docs", () => ApiResults.Safe(() => Results.Ok(new
        {
            basePath = "/api",
            auth = "Neu ApiSettings.RequireApiKey = true, gui header X-API-Key hoac Authorization: Bearer <key>.",
            endpoints = new[]
            {
                "GET /api/health",
                "GET /api/chuc-nang",
                "GET /api/docs",
                "GET /swagger",
                "GET /swagger/v1/swagger.json",
                "POST /api/auth/login",
                "GET,POST /api/hang-hoa",
                "GET /api/v2/hang-hoa",
                "PUT,DELETE /api/hang-hoa/{id}",
                "GET,POST /api/loai-hang",
                "GET /api/v2/loai-hang",
                "PUT,DELETE /api/loai-hang/{id}",
                "GET,POST /api/nha-cung-cap",
                "GET /api/v2/nha-cung-cap",
                "PUT,DELETE /api/nha-cung-cap/{id}",
                "GET,POST /api/khach-hang",
                "GET /api/v2/khach-hang",
                "PUT,DELETE /api/khach-hang/{id}",
                "GET,POST /api/nhan-vien",
                "GET /api/v2/nhan-vien",
                "PUT,DELETE /api/nhan-vien/{id}",
                "GET /api/ton-kho/thap?soLuongToiDa=10",
                "GET,POST /api/phieu-nhap",
                "GET /api/v2/phieu-nhap",
                "GET /api/phieu-nhap/{id}/chi-tiet",
                "GET /api/v2/phieu-nhap/{id}/chi-tiet",
                "GET,POST /api/phieu-xuat",
                "GET /api/v2/phieu-xuat",
                "GET /api/phieu-xuat/{id}/chi-tiet",
                "GET /api/v2/phieu-xuat/{id}/chi-tiet",
                "GET /api/phieu-xuat/{id}/thong-tin",
                "GET /api/v2/phieu-xuat/{id}/thong-tin"
            }
        })));

        return app;
    }
}
