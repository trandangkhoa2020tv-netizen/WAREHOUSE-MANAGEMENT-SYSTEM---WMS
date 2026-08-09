using System.Text.Json;
using Npgsql;
using InventoryManagement.Data;

namespace InventoryManagement.ApiServer.Services;

/// <summary>
/// Ghi audit log cho cac thao tac lam thay doi du lieu.
/// </summary>
public sealed class AuditLogService
{
    private readonly DatabaseHelper _dbHelper = new DatabaseHelper();

    /// <summary>
    /// Ghi mot dong audit log cho thao tac tao, sua hoac xoa du lieu.
    /// </summary>
    public void Record(HttpContext context, string action, string tableName, int? recordId, object details)
    {
        try
        {
            string sql = @"INSERT INTO auditlog
                           (ten_taikhoan, vai_tro, hanh_dong, bang_du_lieu, ma_ban_ghi, noi_dung, ip_address)
                           VALUES (@username, @role, @action, @table, @recordId, @details, @ip)";

            NpgsqlParameter[] parameters =
            {
                new NpgsqlParameter("@username", GetUsername(context)),
                new NpgsqlParameter("@role", GetRole(context)),
                new NpgsqlParameter("@action", action),
                new NpgsqlParameter("@table", tableName),
                new NpgsqlParameter("@recordId", recordId.HasValue ? recordId.Value : DBNull.Value),
                new NpgsqlParameter("@details", SerializeDetails(details)),
                new NpgsqlParameter("@ip", context.Connection.RemoteIpAddress?.ToString() ?? string.Empty)
            };

            _dbHelper.ExecuteNonQuery(sql, parameters);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine("Khong ghi duoc audit log: " + ex.Message);
        }
    }

    /// <summary>
    /// Lay ten tai khoan tu context hien tai de gan vao audit log.
    /// </summary>
    private static string GetUsername(HttpContext context)
    {
        return context.User?.Identity?.Name
            ?? Convert.ToString(context.Items[ApiAuthorization.UserNameItemKey])
            ?? string.Empty;
    }

    /// <summary>
    /// Lay vai tro nguoi dung tu claim hoac context item de gan vao audit log.
    /// </summary>
    private static string GetRole(HttpContext context)
    {
        return context.User?.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value
            ?? Convert.ToString(context.Items[ApiAuthorization.UserRoleItemKey])
            ?? string.Empty;
    }

    /// <summary>
    /// Chuyen payload chi tiet thao tac thanh JSON luu trong bang auditlog.
    /// </summary>
    private static string SerializeDetails(object details)
    {
        return JsonSerializer.Serialize(details, new JsonSerializerOptions
        {
            WriteIndented = false
        });
    }
}
