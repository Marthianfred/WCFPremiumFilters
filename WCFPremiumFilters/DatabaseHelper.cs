using System;
using System.Data;
using System.Data.OleDb;
using System.Configuration;

namespace WCFPremiumFilters
{
    public class DatabaseHelper : IDisposable
    {
        private readonly string _connectionString =
            ConfigurationManager.ConnectionStrings["AccessDBCloud"].ConnectionString;

        public DataTable ExecuteStoredProcedure(string storedProcedureName, params OleDbParameter[] parameters)
        {
            using (var connection = new OleDbConnection(_connectionString))
            {
                connection.Open();
                using (var cmd = new OleDbCommand(storedProcedureName, connection))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    if (parameters != null)
                    {
                        cmd.Parameters.AddRange(parameters);
                    }

                    using (var sda = new OleDbDataAdapter(cmd))
                    {
                        var dt = new DataTable();
                        sda.Fill(dt);
                        return dt;
                    }
                }
            }
        }

        public void Dispose()
        {
        }
    }
}