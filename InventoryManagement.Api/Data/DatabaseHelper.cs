using System;
using System.Data;
using Npgsql;

namespace InventoryManagement.Data
{
    /// <summary>
    /// Lớp tiện ích truy cập database cấp thấp.
    /// Các repository dùng lớp này để chạy SQL có tham số và nhận kết quả dạng DataTable/object.
    /// </summary>
    public class DatabaseHelper
    {
        /// <summary>
        /// Tạo kết nối PostgreSQL dùng chung theo Config/appsettings.json.
        /// </summary>
        public NpgsqlConnection GetConnection()
        {
            return DbConnection.GetConnection();
        }

        /// <summary>
        /// Chạy INSERT, UPDATE, DELETE và trả về số dòng bị ảnh hưởng.
        /// </summary>
        public int ExecuteNonQuery(string sql, NpgsqlParameter[] parameters = null)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    if (parameters != null)
                    {
                        cmd.Parameters.AddRange(parameters);
                    }

                    return cmd.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Chạy SELECT và trả về DataTable để bind vào DataGridView hoặc chuyển thành JSON cho API.
        /// </summary>
        public DataTable ExecuteQuery(string sql, NpgsqlParameter[] parameters = null)
        {
            DataTable dt = new DataTable();
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    if (parameters != null)
                    {
                        cmd.Parameters.AddRange(parameters);
                    }

                    using (var adapter = new NpgsqlDataAdapter(cmd))
                    {
                        adapter.Fill(dt);
                    }
                }
            }

            return dt;
        }

        /// <summary>
        /// Chạy câu SQL trả về một giá trị duy nhất như COUNT, SUM hoặc RETURNING id.
        /// </summary>
        public object ExecuteScalar(string sql, NpgsqlParameter[] parameters = null)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                using (var cmd = new NpgsqlCommand(sql, conn))
                {
                    if (parameters != null)
                    {
                        cmd.Parameters.AddRange(parameters);
                    }

                    return cmd.ExecuteScalar();
                }
            }
        }
    }
}
