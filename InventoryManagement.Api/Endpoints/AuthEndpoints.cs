using InventoryManagement.ApiServer.DTOs;
using InventoryManagement.ApiServer.Services;

namespace InventoryManagement.ApiServer.Endpoints;

/// <summary>
/// Nhóm endpoint xác thực tài khoản người dùng.
/// </summary>
public static class AuthEndpoints
{
    /// <summary>
    /// Đăng ký route đăng nhập vào ứng dụng Minimal API.
    /// </summary>
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        // Nhận username/password, gọi service xác thực và trả vai trò nếu đăng nhập thành công.
        app.MapPost("/api/auth/login", (LoginRequest input, IAuthService service, JwtTokenService tokenService) => ApiResults.Safe(() =>
        {
            string vaiTro = service.CheckLogin(input);
            if (string.IsNullOrEmpty(vaiTro))
            {
                return Results.Unauthorized();
            }

            string token = tokenService.CreateToken(input.Username.Trim(), vaiTro, out DateTime expiresAt);
            return Results.Ok(new { tenTaiKhoan = input.Username.Trim(), vaiTro, token, expiresAt });
        }))
            .RequireRateLimiting("Login");

        return app;
    }
}
