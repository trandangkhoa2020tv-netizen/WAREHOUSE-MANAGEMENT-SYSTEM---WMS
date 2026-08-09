using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using InventoryManagement.ApiServer.Config;

namespace InventoryManagement.ApiServer.Services;

/// <summary>
/// Service phát hành và kiểm tra JWT HMAC SHA-256 cho API.
/// </summary>
public sealed class JwtTokenService
{
    private readonly JwtSettings _settings;

    /// <summary>
    /// Khởi tạo service JWT với cấu hình token.
    /// </summary>
    public JwtTokenService(JwtSettings settings)
    {
        _settings = settings;
    }

    /// <summary>
    /// Tạo JWT cho tài khoản đã đăng nhập thành công.
    /// </summary>
    public string CreateToken(string username, string role, out DateTime expiresAt)
    {
        expiresAt = DateTime.UtcNow.AddMinutes(Math.Max(1, _settings.ExpirationMinutes));

        Dictionary<string, object> header = new Dictionary<string, object>
        {
            ["alg"] = "HS256",
            ["typ"] = "JWT"
        };

        Dictionary<string, object> payload = new Dictionary<string, object>
        {
            ["sub"] = username,
            ["role"] = role,
            ["iss"] = _settings.Issuer,
            ["aud"] = _settings.Audience,
            ["exp"] = new DateTimeOffset(expiresAt).ToUnixTimeSeconds()
        };

        string unsignedToken = Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(header))
            + "."
            + Base64UrlEncode(JsonSerializer.SerializeToUtf8Bytes(payload));

        return unsignedToken + "." + Sign(unsignedToken);
    }

    /// <summary>
    /// Kiểm tra token có đúng chữ ký, issuer, audience và còn hạn hay không.
    /// </summary>
    public bool IsValid(string token)
    {
        return TryReadUser(token, out _, out _);
    }

    /// <summary>
    /// Kiem tra token va lay ten tai khoan/vai tro dung cho phan quyen backend.
    /// </summary>
    public bool TryReadUser(string token, out string username, out string role)
    {
        username = string.Empty;
        role = string.Empty;

        if (string.IsNullOrWhiteSpace(token))
        {
            return false;
        }

        string[] parts = token.Split('.');
        if (parts.Length != 3)
        {
            return false;
        }

        string unsignedToken = parts[0] + "." + parts[1];
        if (!ConstantTimeEquals(Sign(unsignedToken), parts[2]))
        {
            return false;
        }

        try
        {
            byte[] payloadBytes = Base64UrlDecode(parts[1]);
            using JsonDocument document = JsonDocument.Parse(payloadBytes);
            JsonElement payload = document.RootElement;

            bool isValid = HasString(payload, "iss", _settings.Issuer)
                && HasString(payload, "aud", _settings.Audience)
                && payload.TryGetProperty("exp", out JsonElement exp)
                && DateTimeOffset.FromUnixTimeSeconds(exp.GetInt64()) > DateTimeOffset.UtcNow;

            if (!isValid)
            {
                return false;
            }

            username = GetString(payload, "sub");
            role = GetString(payload, "role");
            return !string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(role);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Lấy token từ header Authorization dạng Bearer.
    /// </summary>
    public static string GetBearerToken(HttpRequest request)
    {
        if (!request.Headers.TryGetValue("Authorization", out var values))
        {
            return string.Empty;
        }

        string authorization = values.ToString();
        const string bearerPrefix = "Bearer ";
        return authorization.StartsWith(bearerPrefix, StringComparison.OrdinalIgnoreCase)
            ? authorization.Substring(bearerPrefix.Length).Trim()
            : string.Empty;
    }

    /// <summary>
    /// Kiểm tra path hiện tại có được bỏ qua xác thực JWT hay không.
    /// </summary>
    public static bool IsPublicPath(PathString path)
    {
        return path == "/"
            || path.StartsWithSegments("/api/health")
            || path.StartsWithSegments("/api/chuc-nang")
            || path.StartsWithSegments("/api/docs")
            || path.StartsWithSegments("/api/auth/login")
            || path.StartsWithSegments("/swagger");
    }

    /// <summary>
    /// Tao chu ky HMAC SHA-256 cho phan header va payload cua JWT.
    /// </summary>
    private string Sign(string unsignedToken)
    {
        using HMACSHA256 hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_settings.SecretKey));
        return Base64UrlEncode(hmac.ComputeHash(Encoding.UTF8.GetBytes(unsignedToken)));
    }

    /// <summary>
    /// Kiem tra mot claim dang chuoi co ton tai va khop gia tri mong doi hay khong.
    /// </summary>
    private static bool HasString(JsonElement payload, string propertyName, string expected)
    {
        return payload.TryGetProperty(propertyName, out JsonElement value)
            && string.Equals(value.GetString(), expected, StringComparison.Ordinal);
    }

    /// <summary>
    /// Doc mot claim dang chuoi tu payload, tra chuoi rong neu khong co.
    /// </summary>
    private static string GetString(JsonElement payload, string propertyName)
    {
        return payload.TryGetProperty(propertyName, out JsonElement value)
            ? value.GetString() ?? string.Empty
            : string.Empty;
    }

    /// <summary>
    /// So sanh hai chuoi chu ky theo constant-time de giam ro ri thoi gian.
    /// </summary>
    private static bool ConstantTimeEquals(string expected, string actual)
    {
        byte[] expectedBytes = Encoding.UTF8.GetBytes(expected);
        byte[] actualBytes = Encoding.UTF8.GetBytes(actual);
        return expectedBytes.Length == actualBytes.Length
            && CryptographicOperations.FixedTimeEquals(expectedBytes, actualBytes);
    }

    /// <summary>
    /// Ma hoa byte array sang Base64Url theo dinh dang JWT.
    /// </summary>
    private static string Base64UrlEncode(byte[] bytes)
    {
        return Convert.ToBase64String(bytes).TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    /// <summary>
    /// Giai ma chuoi Base64Url cua JWT ve byte array.
    /// </summary>
    private static byte[] Base64UrlDecode(string value)
    {
        string base64 = value.Replace('-', '+').Replace('_', '/');
        switch (base64.Length % 4)
        {
            case 2:
                base64 += "==";
                break;
            case 3:
                base64 += "=";
                break;
        }

        return Convert.FromBase64String(base64);
    }
}
