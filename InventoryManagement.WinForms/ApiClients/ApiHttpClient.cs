using System.Data;
using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace InventoryManagement.ApiClients
{
    /// <summary>
    /// Lớp dùng chung để gửi HTTP request tới backend và chuyển JSON trả về thành dữ liệu WinForms cần dùng.
    /// </summary>
    internal static class ApiHttpClient
    {
        private static readonly HttpClient Client = CreateClient();

        /// <summary>
        /// Gắn hoặc xóa JWT Bearer token cho các request sau khi đăng nhập.
        /// </summary>
        public static void SetBearerToken(string token)
        {
            Client.DefaultRequestHeaders.Authorization = string.IsNullOrWhiteSpace(token)
                ? null
                : new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        /// <summary>
        /// Gửi request GET tới endpoint và đọc mảng JSON trả về thành DataTable.
        /// </summary>
        public static DataTable GetTable(string path)
        {
            return SendAsync(async () =>
            {
                using HttpResponseMessage response = await Client.GetAsync(path).ConfigureAwait(false);
                await EnsureSuccess(response).ConfigureAwait(false);
                using Stream stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                return await ReadTable(stream).ConfigureAwait(false);
            });
        }

        /// <summary>
        /// Gửi request GET và đọc đối tượng JSON trả về theo kiểu dữ liệu mong muốn.
        /// </summary>
        public static TResponse GetJson<TResponse>(string path)
        {
            return SendAsync(async () =>
            {
                using HttpResponseMessage response = await Client.GetAsync(path).ConfigureAwait(false);
                await EnsureSuccess(response).ConfigureAwait(false);
                TResponse result = await response.Content.ReadFromJsonAsync<TResponse>().ConfigureAwait(false);
                return result;
            });
        }

        /// <summary>
        /// Gửi request POST có body JSON và đọc kết quả JSON trả về theo kiểu dữ liệu mong muốn.
        /// </summary>
        public static TResponse PostForJson<TRequest, TResponse>(string path, TRequest body)
        {
            return SendAsync(async () =>
            {
                using HttpResponseMessage response = await Client.PostAsJsonAsync(path, body).ConfigureAwait(false);
                await EnsureSuccess(response).ConfigureAwait(false);
                TResponse result = await response.Content.ReadFromJsonAsync<TResponse>().ConfigureAwait(false);
                return result;
            });
        }

        /// <summary>
        /// Gửi request POST để thêm mới dữ liệu, chỉ kiểm tra trạng thái thành công của API.
        /// </summary>
        public static void Post<TRequest>(string path, TRequest body)
        {
            SendAsync(async () =>
            {
                using HttpResponseMessage response = await Client.PostAsJsonAsync(path, body).ConfigureAwait(false);
                await EnsureSuccess(response).ConfigureAwait(false);
                return true;
            });
        }

        /// <summary>
        /// Gửi request PUT để cập nhật dữ liệu, chỉ kiểm tra trạng thái thành công của API.
        /// </summary>
        public static void Put<TRequest>(string path, TRequest body)
        {
            SendAsync(async () =>
            {
                using HttpResponseMessage response = await Client.PutAsJsonAsync(path, body).ConfigureAwait(false);
                await EnsureSuccess(response).ConfigureAwait(false);
                return true;
            });
        }

        /// <summary>
        /// Gửi request DELETE để xóa dữ liệu theo endpoint truyền vào.
        /// </summary>
        public static void Delete(string path)
        {
            SendAsync(async () =>
            {
                using HttpResponseMessage response = await Client.DeleteAsync(path).ConfigureAwait(false);
                await EnsureSuccess(response).ConfigureAwait(false);
                return true;
            });
        }

        /// <summary>
        /// Tạo HttpClient theo cấu hình BaseUrl và gắn API key nếu có.
        /// </summary>
        private static HttpClient CreateClient()
        {
            ApiClientSettings settings = ApiClientSettings.Load();
            HttpClient client = new HttpClient
            {
                BaseAddress = new Uri(EnsureTrailingSlash(settings.BaseUrl)),
                Timeout = TimeSpan.FromSeconds(30)
            };

            if (!string.IsNullOrWhiteSpace(settings.ApiKey))
            {
                client.DefaultRequestHeaders.Add("X-API-Key", settings.ApiKey);
            }

            return client;
        }

        /// <summary>
        /// Bảo đảm BaseUrl luôn kết thúc bằng dấu gạch chéo để ghép đường dẫn endpoint chính xác.
        /// </summary>
        private static string EnsureTrailingSlash(string value)
        {
            return value.EndsWith("/", StringComparison.Ordinal) ? value : value + "/";
        }

        /// <summary>
        /// Chạy thao tác HTTP bất đồng bộ theo kiểu đồng bộ để các form WinForms gọi được đơn giản.
        /// </summary>
        private static T SendAsync<T>(Func<Task<T>> action)
        {
            try
            {
                return action().GetAwaiter().GetResult();
            }
            catch (HttpRequestException ex)
            {
                throw new InvalidOperationException("Khong ket noi duoc API server. Hay kiem tra backend InventoryManagement.Api dang chay. Chi tiet: " + ex.Message, ex);
            }
            catch (TaskCanceledException ex)
            {
                throw new InvalidOperationException("API server phan hoi qua cham hoac khong san sang.", ex);
            }
        }

        /// <summary>
        /// Kiểm tra HTTP status code và chuyển lỗi API thành exception có thông báo dễ hiểu.
        /// </summary>
        private static async Task EnsureSuccess(HttpResponseMessage response)
        {
            if (response.IsSuccessStatusCode)
            {
                return;
            }

            string message = await ReadErrorMessage(response).ConfigureAwait(false);
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException(string.IsNullOrWhiteSpace(message) ? "Khong co quyen truy cap API." : message);
            }

            throw new InvalidOperationException(message);
        }

        /// <summary>
        /// Đọc nội dung lỗi từ API, ưu tiên các trường message, detail hoặc danh sách errors.
        /// </summary>
        private static async Task<string> ReadErrorMessage(HttpResponseMessage response)
        {
            string content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(content))
            {
                return $"API tra ve loi {(int)response.StatusCode} {response.ReasonPhrase}.";
            }

            try
            {
                using JsonDocument document = JsonDocument.Parse(content);
                JsonElement root = document.RootElement;
                if (root.TryGetProperty("message", out JsonElement message))
                {
                    string value = message.GetString();
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        return value;
                    }
                }

                if (root.TryGetProperty("detail", out JsonElement detail))
                {
                    string value = detail.GetString();
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        return value;
                    }
                }

                if (root.TryGetProperty("errors", out JsonElement errors) && errors.ValueKind == JsonValueKind.Array)
                {
                    return string.Join(Environment.NewLine, errors.EnumerateArray().Select(error => error.GetString()));
                }
            }
            catch (JsonException)
            {
                return content;
            }

            return content;
        }

        /// <summary>
        /// Chuyển mảng JSON từ API thành DataTable, tự suy luận kiểu dữ liệu từng cột.
        /// </summary>
        private static async Task<DataTable> ReadTable(Stream stream)
        {
            using JsonDocument document = await JsonDocument.ParseAsync(stream).ConfigureAwait(false);
            DataTable table = new DataTable();
            if (document.RootElement.ValueKind != JsonValueKind.Array)
            {
                return table;
            }

            List<JsonElement> rows = document.RootElement.EnumerateArray().ToList();
            foreach (JsonElement row in rows)
            {
                if (row.ValueKind != JsonValueKind.Object)
                {
                    continue;
                }

                foreach (JsonProperty property in row.EnumerateObject())
                {
                    if (!table.Columns.Contains(property.Name))
                    {
                        table.Columns.Add(property.Name, InferColumnType(rows, property.Name));
                    }
                }
            }

            foreach (JsonElement row in rows)
            {
                DataRow dataRow = table.NewRow();
                foreach (JsonProperty property in row.EnumerateObject())
                {
                    dataRow[property.Name] = ConvertValue(property.Value, table.Columns[property.Name].DataType) ?? DBNull.Value;
                }

                table.Rows.Add(dataRow);
            }

            return table;
        }

        /// <summary>
        /// Suy luận kiểu cột DataTable dựa trên giá trị JSON đầu tiên khác null của thuộc tính.
        /// </summary>
        private static Type InferColumnType(List<JsonElement> rows, string propertyName)
        {
            foreach (JsonElement row in rows)
            {
                if (!row.TryGetProperty(propertyName, out JsonElement value) || value.ValueKind == JsonValueKind.Null)
                {
                    continue;
                }

                if (value.ValueKind == JsonValueKind.Number)
                {
                    return value.TryGetInt32(out _) ? typeof(int) : typeof(decimal);
                }

                if (value.ValueKind == JsonValueKind.True || value.ValueKind == JsonValueKind.False)
                {
                    return typeof(bool);
                }

                if (value.ValueKind == JsonValueKind.String && DateTime.TryParse(value.GetString(), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out _))
                {
                    return typeof(DateTime);
                }

                return typeof(string);
            }

            return typeof(string);
        }

        /// <summary>
        /// Chuyển JsonElement sang kiểu dữ liệu tương ứng với cột DataTable.
        /// </summary>
        private static object ConvertValue(JsonElement value, Type targetType)
        {
            if (value.ValueKind == JsonValueKind.Null || value.ValueKind == JsonValueKind.Undefined)
            {
                return null;
            }

            if (targetType == typeof(int))
            {
                return value.ValueKind == JsonValueKind.Number ? value.GetInt32() : Convert.ToInt32(value.GetString());
            }

            if (targetType == typeof(decimal))
            {
                return value.ValueKind == JsonValueKind.Number ? value.GetDecimal() : Convert.ToDecimal(value.GetString(), CultureInfo.InvariantCulture);
            }

            if (targetType == typeof(bool))
            {
                return value.ValueKind == JsonValueKind.True || (value.ValueKind == JsonValueKind.String && bool.Parse(value.GetString()));
            }

            if (targetType == typeof(DateTime))
            {
                return value.ValueKind == JsonValueKind.String ? DateTime.Parse(value.GetString(), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind) : value.GetDateTime();
            }

            return value.ValueKind == JsonValueKind.String ? value.GetString() : value.ToString();
        }
    }
}
