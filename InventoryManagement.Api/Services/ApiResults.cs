using Microsoft.AspNetCore.Http;
using Npgsql;

namespace InventoryManagement.ApiServer.Services;

/// <summary>
/// Lớp tiện ích chuẩn hóa response và xử lý lỗi chung cho các endpoint API.
/// </summary>
public static class ApiResults
{
    /// <summary>
    /// Bọc xử lý endpoint trong try/catch để API luôn trả lỗi dạng JSON thống nhất.
    /// </summary>
    public static IResult Safe(Func<IResult> action)
    {
        try
        {
            return action();
        }
        catch (ApiValidationException ex)
        {
            return Results.BadRequest(new { message = ex.Message, errors = ex.Errors });
        }
        catch (InvalidOperationException ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
        catch (PostgresException ex) when (IsDataRuleViolation(ex))
        {
            return Results.BadRequest(new { message = ToDataRuleMessage(ex) });
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            return Results.Problem(
                title: "Loi xu ly API",
                detail: "Da xay ra loi he thong. Hay kiem tra log backend.",
                statusCode: StatusCodes.Status500InternalServerError);
        }
    }

    /// <summary>
    /// Chuyển DataTable từ repository thành danh sách object JSON và trả HTTP 200.
    /// </summary>
    public static IResult OkTable(System.Data.DataTable table)
    {
        return Results.Ok(DataTableJson.ToRows(table));
    }

    /// <summary>
    /// Trả kết quả chuẩn cho thao tác thêm mới dữ liệu.
    /// </summary>
    public static IResult Created(int affectedRows)
    {
        return Results.Ok(new { message = "Da them du lieu.", affectedRows });
    }

    /// <summary>
    /// Trả kết quả chuẩn cho thao tác cập nhật, bao gồm trường hợp không tìm thấy bản ghi.
    /// </summary>
    public static IResult Updated(int affectedRows)
    {
        return affectedRows == 0
            ? Results.NotFound(new { message = "Khong tim thay du lieu can cap nhat." })
            : Results.Ok(new { message = "Da cap nhat du lieu.", affectedRows });
    }

    /// <summary>
    /// Trả kết quả chuẩn cho thao tác xóa, bao gồm trường hợp không tìm thấy bản ghi.
    /// </summary>
    public static IResult Deleted(int affectedRows)
    {
        return affectedRows == 0
            ? Results.NotFound(new { message = "Khong tim thay du lieu can xoa." })
            : Results.Ok(new { message = "Da xoa du lieu.", affectedRows });
    }

    /// <summary>
    /// Xac dinh loi PostgreSQL co phai la loi rang buoc du lieu co the tra ve 400 hay khong.
    /// </summary>
    private static bool IsDataRuleViolation(PostgresException ex)
    {
        return ex.SqlState == PostgresErrorCodes.UniqueViolation
            || ex.SqlState == PostgresErrorCodes.ForeignKeyViolation
            || ex.SqlState == PostgresErrorCodes.CheckViolation
            || ex.SqlState == PostgresErrorCodes.NotNullViolation;
    }

    /// <summary>
    /// Chuyen ma loi rang buoc PostgreSQL thanh thong bao ngan gon cho client.
    /// </summary>
    private static string ToDataRuleMessage(PostgresException ex)
    {
        if (ex.SqlState == PostgresErrorCodes.UniqueViolation)
        {
            return "Du lieu bi trung voi ban ghi da co.";
        }

        if (ex.SqlState == PostgresErrorCodes.ForeignKeyViolation)
        {
            return "Du lieu lien ket khong ton tai hoac dang duoc su dung.";
        }

        if (ex.SqlState == PostgresErrorCodes.CheckViolation)
        {
            return "Du lieu khong thoa man rang buoc an toan cua database.";
        }

        if (ex.SqlState == PostgresErrorCodes.NotNullViolation)
        {
            return "Du lieu bat buoc khong duoc de trong.";
        }

        return "Du lieu khong hop le.";
    }
}
