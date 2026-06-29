using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace WpfApp1.Models
{
    public static class DatabaseHelper
    {
        private static string connectionString =
            @"Data Source=DESKTOP-159LLU1\KAMI0YASU;Initial Catalog=MunicipalOlympiads;Integrated Security=True";

        public static void SetConnectionString(string connStr)
        {
            connectionString = connStr;
        }

        public static SqlConnection GetConnection()
        {
            return new SqlConnection(connectionString);
        }

        public static DataTable ExecuteQuery(string query, SqlParameter[] parameters = null)
        {
            try
            {
                using (var conn = GetConnection())
                using (var cmd = CreateCommand(query, conn, parameters))
                using (var adapter = new SqlDataAdapter(cmd))
                {
                    var dt = new DataTable();
                    adapter.Fill(dt);
                    return dt;
                }
            }
            catch (SqlException ex)
            {
                LogError("ExecuteQuery", query, ex);
                throw;
            }
        }
        public static async Task<DataTable> ExecuteQueryAsync(string query, SqlParameter[] parameters = null)
        {
            try
            {
                using (var conn = GetConnection())
                using (var cmd = CreateCommand(query, conn, parameters))
                {
                    await conn.OpenAsync();
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        var dt = new DataTable();
                        dt.Load(reader);
                        return dt;
                    }
                }
            }
            catch (SqlException ex)
            {
                LogError("ExecuteQueryAsync", query, ex);
                throw;
            }
        }

        public static int ExecuteNonQuery(string query, SqlParameter[] parameters = null)
        {
            try
            {
                using (var conn = GetConnection())
                using (var cmd = CreateCommand(query, conn, parameters))
                {
                    conn.Open();
                    return cmd.ExecuteNonQuery();
                }
            }
            catch (SqlException ex)
            {
                LogError("ExecuteNonQuery", query, ex);
                throw;
            }
        }
        public static async Task<int> ExecuteNonQueryAsync(string query, SqlParameter[] parameters = null)
        {
            try
            {
                using (var conn = GetConnection())
                using (var cmd = CreateCommand(query, conn, parameters))
                {
                    await conn.OpenAsync();
                    return await cmd.ExecuteNonQueryAsync();
                }
            }
            catch (SqlException ex)
            {
                LogError("ExecuteNonQueryAsync", query, ex);
                throw;
            }
        }

        public static object ExecuteScalar(string query, SqlParameter[] parameters = null)
        {
            try
            {
                using (var conn = GetConnection())
                using (var cmd = CreateCommand(query, conn, parameters))
                {
                    conn.Open();
                    return cmd.ExecuteScalar();
                }
            }
            catch (SqlException ex)
            {
                LogError("ExecuteScalar", query, ex);
                throw;
            }
        }

        public static async Task<object> ExecuteScalarAsync(string query, SqlParameter[] parameters = null)
        {
            try
            {
                using (var conn = GetConnection())
                using (var cmd = CreateCommand(query, conn, parameters))
                {
                    await conn.OpenAsync();
                    return await cmd.ExecuteScalarAsync();
                }
            }
            catch (SqlException ex)
            {
                LogError("ExecuteScalarAsync", query, ex);
                throw;
            }
        }

        public static bool TestConnection()
        {
            try
            {
                using (var conn = GetConnection())
                {
                    conn.Open();
                    return true;
                }
            }
            catch (Exception ex)
            {
                LogError("TestConnection", "", ex);
                return false;
            }
        }

        private static SqlCommand CreateCommand(string query, SqlConnection conn, SqlParameter[] parameters)
        {
            var cmd = new SqlCommand(query, conn);
            cmd.CommandTimeout = 30;
            if (parameters != null)
                cmd.Parameters.AddRange(parameters);
            return cmd;
        }

        private static void LogError(string method, string query, Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(
                $"[DatabaseHelper.{method}] Ошибка: {ex.Message}\n" +
                $"Запрос: {(query.Length > 200 ? query.Substring(0, 200) + "..." : query)}");
        }
    }
}