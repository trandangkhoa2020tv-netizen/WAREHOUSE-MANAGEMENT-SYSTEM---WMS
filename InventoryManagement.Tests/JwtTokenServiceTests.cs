using InventoryManagement.ApiServer.Config;
using InventoryManagement.ApiServer.Services;

namespace InventoryManagement.Tests;

public sealed class JwtTokenServiceTests
{
    /// <summary>
    /// Kiem tra token moi tao co han su dung va validate duoc.
    /// </summary>
    [Fact]
    public void CreateToken_ShouldReturnTokenThatCanBeValidated()
    {
        JwtTokenService service = CreateService();

        string token = service.CreateToken("admin", "Admin", out DateTime expiresAt);

        Assert.False(string.IsNullOrWhiteSpace(token));
        Assert.True(expiresAt > DateTime.UtcNow);
        Assert.True(service.IsValid(token));
    }

    /// <summary>
    /// Kiem tra service doc dung username va role tu token hop le.
    /// </summary>
    [Fact]
    public void TryReadUser_ShouldReturnUsernameAndRole()
    {
        JwtTokenService service = CreateService();

        string token = service.CreateToken("admin", "Admin", out _);

        Assert.True(service.TryReadUser(token, out string username, out string role));
        Assert.Equal("admin", username);
        Assert.Equal("Admin", role);
    }

    /// <summary>
    /// Kiem tra token sai dinh dang bi tu choi.
    /// </summary>
    [Fact]
    public void IsValid_ShouldRejectInvalidToken()
    {
        JwtTokenService service = CreateService();

        Assert.False(service.IsValid("invalid.token.value"));
    }

    /// <summary>
    /// Tao JwtTokenService voi cau hinh on dinh dung rieng cho unit test.
    /// </summary>
    private static JwtTokenService CreateService()
    {
        return new JwtTokenService(new JwtSettings
        {
            Issuer = "TestIssuer",
            Audience = "TestAudience",
            SecretKey = "Unit-Test-Secret-Key-For-Jwt-Validation",
            ExpirationMinutes = 30
        });
    }
}
