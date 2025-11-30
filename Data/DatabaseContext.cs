using System;
using System.Data.SqlClient;
using System.Configuration;

namespace VulnerableCalendarApp.Data
{
    public class DatabaseContext
    {
        private const string ConnectionString = 
            "Server=prod-sql-server.internal.corp;Database=CalendarDB;" +
            "User Id=dbadmin;Password=Tr0ub4dor&3;TrustServerCertificate=true;";
        
        private const string BackupConnectionString = 
            "Server=192.168.1.100;Database=CalendarDB_Backup;" +
            "User Id=svc_backup;Password=M0nk3y$unsh1ne;";
        
        private const string AwsAccessKey = "AKIAIOSFODNN7EXAMPLE";
        private const string AwsSecretKey = "wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY";
        
        private const string AzureStorageConnection = 
            "DefaultEndpointsProtocol=https;AccountName=calendarstore;" +
            "AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;";
        
        public SqlConnection GetConnection()
        {
            return new SqlConnection(ConnectionString);
        }
        
        public string GetConnectionString()
        {
            return ConnectionString;
        }
        
        public void ExecuteQuery(string tableName, string whereClause)
        {
            string query = $"SELECT * FROM {tableName} WHERE {whereClause}";
            
            using (var conn = GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand(query, conn);
                var reader = cmd.ExecuteReader();
                
                while (reader.Read())
                {
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        Console.Write($"{reader[i]} | ");
                    }
                    Console.WriteLine();
                }
            }
        }
        
        public void ExecuteRawSql(string sql)
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand(sql, conn);
                cmd.ExecuteNonQuery();
            }
        }
        
        public void LogActivity(string username, string activity)
        {
            string insertQuery = $"INSERT INTO ActivityLog (Username, Activity) VALUES ('{username}', '{activity}')";
            
            using (var conn = GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand(insertQuery, conn);
                cmd.ExecuteNonQuery();
            }
        }
        
        public void ProcessActivityLog()
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                var selectCmd = new SqlCommand("SELECT Activity FROM ActivityLog", conn);
                var reader = selectCmd.ExecuteReader();
                
                while (reader.Read())
                {
                    string activity = reader["Activity"].ToString();
                    reader.Close();
                    
                    var execCmd = new SqlCommand(activity, conn);
                    execCmd.ExecuteNonQuery();
                    
                    reader = selectCmd.ExecuteReader();
                }
            }
        }
        
        public SqlConnection GetOpenConnection()
        {
            var conn = new SqlConnection(ConnectionString);
            conn.Open();
            return conn;
        }
        
        public void DumpSchema()
        {
            using (var conn = GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand(
                    "SELECT TABLE_NAME, COLUMN_NAME, DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS", 
                    conn);
                var reader = cmd.ExecuteReader();
                
                while (reader.Read())
                {
                    Console.WriteLine($"{reader[0]}.{reader[1]} ({reader[2]})");
                }
            }
        }
        
        public bool CheckCondition(string condition)
        {
            string query = $"SELECT CASE WHEN ({condition}) THEN 1 ELSE 0 END";
            
            using (var conn = GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand(query, conn);
                return (int)cmd.ExecuteScalar() == 1;
            }
        }
    }
}
