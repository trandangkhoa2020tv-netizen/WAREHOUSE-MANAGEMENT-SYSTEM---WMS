namespace InventoryManagement.ApiServer.Services;

/// <summary>
/// Lớp tiện ích validate dữ liệu đầu vào dùng chung cho các service API.
/// </summary>
internal static class ValidationHelper
{
    /// <summary>
    /// Kiểm tra body JSON có bị null hay không trước khi đọc các property bên trong.
    /// </summary>
    public static List<string> RequireBody<T>(T input)
    {
        List<string> errors = new List<string>();
        if (input == null)
        {
            errors.Add("Body JSON khong duoc de trong.");
        }

        return errors;
    }

    /// <summary>
    /// Ném ApiValidationException nếu danh sách lỗi đang có ít nhất một lỗi.
    /// </summary>
    public static void ThrowIfAny(List<string> errors)
    {
        if (errors.Count > 0)
        {
            throw new ApiValidationException(errors);
        }
    }

    /// <summary>
    /// Bắt buộc chuỗi phải có nội dung và không vượt quá độ dài tối đa.
    /// </summary>
    public static void RequireText(List<string> errors, string value, string fieldName, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            errors.Add(fieldName + " khong duoc de trong.");
            return;
        }

        OptionalMaxLength(errors, value, fieldName, maxLength);
    }

    /// <summary>
    /// Kiểm tra độ dài tối đa cho chuỗi không bắt buộc nhập.
    /// </summary>
    public static void OptionalMaxLength(List<string> errors, string value, string fieldName, int maxLength)
    {
        if (!string.IsNullOrEmpty(value) && value.Length > maxLength)
        {
            errors.Add(fieldName + " khong duoc vuot qua " + maxLength + " ky tu.");
        }
    }

    /// <summary>
    /// Kiểm tra email không bắt buộc: nếu có nhập thì phải có dạng email cơ bản và không quá dài.
    /// </summary>
    public static void OptionalEmail(List<string> errors, string value, string fieldName, int maxLength)
    {
        OptionalMaxLength(errors, value, fieldName, maxLength);
        if (!string.IsNullOrWhiteSpace(value) && (!value.Contains('@') || value.StartsWith("@") || value.EndsWith("@")))
        {
            errors.Add(fieldName + " khong dung dinh dang email.");
        }
    }

    /// <summary>
    /// Bắt buộc giá trị số nguyên phải lớn hơn 0.
    /// </summary>
    public static void RequirePositive(List<string> errors, int value, string fieldName)
    {
        if (value <= 0)
        {
            errors.Add(fieldName + " phai lon hon 0.");
        }
    }

    /// <summary>
    /// Bắt buộc giá trị số nguyên không được âm.
    /// </summary>
    public static void RequireNonNegative(List<string> errors, int value, string fieldName)
    {
        if (value < 0)
        {
            errors.Add(fieldName + " khong duoc am.");
        }
    }

    /// <summary>
    /// Bắt buộc giá trị số thập phân hoặc tiền tệ không được âm.
    /// </summary>
    public static void RequireNonNegativeDecimal(List<string> errors, decimal value, string fieldName)
    {
        if (value < 0)
        {
            errors.Add(fieldName + " khong duoc am.");
        }
    }
}
