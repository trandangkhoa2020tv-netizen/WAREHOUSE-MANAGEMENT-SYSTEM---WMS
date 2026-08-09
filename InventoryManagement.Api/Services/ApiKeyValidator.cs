using System.Security.Cryptography;
using System.Text;

namespace InventoryManagement.ApiServer.Services;

/// <summary>
/// Lớp tiện ích kiểm tra API key gửi lên từ client.
/// </summary>
public static class ApiKeyValidator
{
    /// <summary>
    /// Kiểm tra API key trong header X-API-Key hoặc Authorization Bearer có khớp cấu hình hay không.
    /// </summary>
    public static bool HasValidApiKey(HttpRequest request, string configuredApiKey)
    {
        if (string.IsNullOrWhiteSpace(configuredApiKey))
        {
            return false;
        }

        string providedApiKey = null;
        if (request.Headers.TryGetValue("X-API-Key", out var apiKeyValues))
        {
            providedApiKey = apiKeyValues.ToString();
        }

        if (string.IsNullOrWhiteSpace(providedApiKey) && request.Headers.TryGetValue("Authorization", out var authValues))
        {
            string authorization = authValues.ToString();
            const string bearerPrefix = "Bearer ";
            if (authorization.StartsWith(bearerPrefix, StringComparison.OrdinalIgnoreCase))
            {
                providedApiKey = authorization.Substring(bearerPrefix.Length).Trim();
            }
        }

        if (string.IsNullOrWhiteSpace(providedApiKey))
        {
            return false;
        }

        byte[] expectedBytes = Encoding.UTF8.GetBytes(configuredApiKey);
        byte[] providedBytes = Encoding.UTF8.GetBytes(providedApiKey);
        return expectedBytes.Length == providedBytes.Length && CryptographicOperations.FixedTimeEquals(expectedBytes, providedBytes);
    }
}
