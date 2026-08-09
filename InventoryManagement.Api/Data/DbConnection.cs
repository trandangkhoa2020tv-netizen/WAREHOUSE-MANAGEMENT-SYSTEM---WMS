using Npgsql;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace InventoryManagement.Data
{
    /// <summary>
    /// Quản lý chuỗi kết nối PostgreSQL dùng chung cho toàn bộ chương trình.
    /// Lớp này đọc cấu hình từ Config/appsettings.json, nếu không đọc được thì dùng cấu hình mặc định.
    /// </summary>
    public class DbConnection
    {
        // Chuỗi kết nối dự phòng để chương trình vẫn có thể chạy khi thiếu file cấu hình.
        private const string DefaultConnectionString =
            "Host=localhost;Port=5432;Database=quanlyhanghoa;Username=postgres;Password=";

        // Chuỗi kết nối cuối cùng được tính một lần khi ứng dụng khởi động.
        public static string ConnectionString { get; } = BuildConnectionString();

        /// <summary>
        /// Tạo đối tượng kết nối mới. Mỗi lần gọi sẽ trả về connection chưa mở.
        /// </summary>
        public static NpgsqlConnection GetConnection()
        {
            return new NpgsqlConnection(ConnectionString);
        }

        /// <summary>
        /// Kiểm tra nhanh database có kết nối được hay không. API /api/health dùng hàm này.
        /// </summary>
        public static bool TestConnection()
        {
            try
            {
                using (NpgsqlConnection conn = GetConnection())
                {
                    conn.Open();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Đọc file appsettings.json và dựng chuỗi kết nối PostgreSQL.
        /// Hàm cố tình bắt lỗi và trả về cấu hình mặc định để tránh crash khi thiếu file config.
        /// </summary>
        private static string BuildConnectionString()
        {
            try
            {
                string host = "localhost";
                int port = 5432;
                string database = "quanlyhanghoa";
                string username = "postgres";
                string password = string.Empty;

                string configPath = Path.Combine(AppContext.BaseDirectory, "Config", "appsettings.json");
                if (!File.Exists(configPath))
                {
                    configPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
                }

                if (File.Exists(configPath))
                {
                    using FileStream stream = File.OpenRead(configPath);
                    using JsonDocument document = JsonDocument.Parse(stream);
                    JsonElement db = document.RootElement.GetProperty("DatabaseSettings");

                    host = GetString(db, "Host", host);
                    port = GetInt(db, "Port", port);
                    database = GetString(db, "Database", database);
                    username = GetString(db, "Username", username);
                    password = GetString(db, "Password", password);
                }

                Dictionary<string, string> dotEnv = LoadDotEnv();

                host = GetConfigurationString("QLKH_DB_HOST", host, dotEnv);
                port = GetConfigurationInt("QLKH_DB_PORT", port, dotEnv);
                database = GetConfigurationString("QLKH_DB_NAME", database, dotEnv);
                username = GetConfigurationString("QLKH_DB_USER", username, dotEnv);
                password = GetConfigurationString("QLKH_DB_PASSWORD", password, dotEnv);

                return new NpgsqlConnectionStringBuilder
                {
                    Host = host,
                    Port = port,
                    Database = database,
                    Username = username,
                    Password = password,
                    GssEncryptionMode = GssEncryptionMode.Disable
                }.ConnectionString;
            }
            catch
            {
                return DefaultConnectionString;
            }
        }

        /// <summary>
        /// Lấy giá trị chuỗi trong JSON; nếu thiếu key thì dùng giá trị fallback.
        /// </summary>
        private static string GetString(JsonElement parent, string propertyName, string fallback)
        {
            return parent.TryGetProperty(propertyName, out JsonElement value)
                ? value.GetString() ?? fallback
                : fallback;
        }

        /// <summary>
        /// Lấy giá trị số nguyên trong JSON; nếu thiếu hoặc sai kiểu thì dùng giá trị fallback.
        /// </summary>
        private static int GetInt(JsonElement parent, string propertyName, int fallback)
        {
            return parent.TryGetProperty(propertyName, out JsonElement value) && value.TryGetInt32(out int result)
                ? result
                : fallback;
        }

        /// <summary>
        /// Lấy giá trị chuỗi từ biến môi trường; nếu không có thì giữ nguyên giá trị fallback.
        /// </summary>
        private static string GetConfigurationString(
            string variableName,
            string fallback,
            Dictionary<string, string> dotEnv)
        {
            string value = Environment.GetEnvironmentVariable(variableName);
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            return dotEnv.TryGetValue(variableName, out value) && !string.IsNullOrWhiteSpace(value)
                ? value
                : fallback;
        }

        private static int GetConfigurationInt(
            string variableName,
            int fallback,
            Dictionary<string, string> dotEnv)
        {
            string value = Environment.GetEnvironmentVariable(variableName);
            if (string.IsNullOrWhiteSpace(value) && dotEnv.TryGetValue(variableName, out string dotEnvValue))
            {
                value = dotEnvValue;
            }

            return int.TryParse(value, out int result) ? result : fallback;
        }

        private static Dictionary<string, string> LoadDotEnv()
        {
            Dictionary<string, string> values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            string envPath = FindDotEnvPath();
            if (string.IsNullOrWhiteSpace(envPath))
            {
                return values;
            }

            foreach (string line in File.ReadAllLines(envPath))
            {
                string trimmed = line.Trim();
                if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("#", StringComparison.Ordinal))
                {
                    continue;
                }

                int separatorIndex = trimmed.IndexOf('=');
                if (separatorIndex <= 0)
                {
                    continue;
                }

                string key = trimmed.Substring(0, separatorIndex).Trim();
                string value = trimmed.Substring(separatorIndex + 1).Trim().Trim('"');
                if (!string.IsNullOrWhiteSpace(key))
                {
                    values[key] = value;
                }
            }

            return values;
        }

        private static string FindDotEnvPath()
        {
            DirectoryInfo directory = new DirectoryInfo(AppContext.BaseDirectory);
            while (directory != null)
            {
                string envPath = Path.Combine(directory.FullName, ".env");
                if (File.Exists(envPath))
                {
                    return envPath;
                }

                directory = directory.Parent;
            }

            return string.Empty;
        }

        private static string GetEnvironmentString(string variableName, string fallback)
        {
            string value = Environment.GetEnvironmentVariable(variableName);
            return string.IsNullOrWhiteSpace(value) ? fallback : value;
        }

        /// <summary>
        /// Lấy giá trị số nguyên từ biến môi trường; nếu thiếu hoặc không parse được thì dùng fallback.
        /// </summary>
        private static int GetEnvironmentInt(string variableName, int fallback)
        {
            string value = Environment.GetEnvironmentVariable(variableName);
            return int.TryParse(value, out int result) ? result : fallback;
        }
    }
}
