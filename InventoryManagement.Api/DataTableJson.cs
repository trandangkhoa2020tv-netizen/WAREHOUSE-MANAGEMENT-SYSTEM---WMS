using System.Data;

namespace InventoryManagement.ApiServer;

/// <summary>
/// Chuyển dữ liệu DataTable sang cấu trúc dictionary để minimal API serialize thành JSON.
/// </summary>
public static class DataTableJson
{
    /// <summary>
    /// Biến từng dòng DataTable thành Dictionary với key là tên cột và value là giá trị của ô.
    /// </summary>
    public static List<Dictionary<string, object>> ToRows(DataTable table)
    {
        List<Dictionary<string, object>> rows = new List<Dictionary<string, object>>();

        foreach (DataRow row in table.Rows)
        {
            Dictionary<string, object> item = new Dictionary<string, object>();
            foreach (DataColumn column in table.Columns)
            {
                object value = row[column];
                item[column.ColumnName] = value == DBNull.Value ? null : value;
            }

            rows.Add(item);
        }

        return rows;
    }
}
