using System;
using System.Data.SqlClient;
using System.Configuration;

namespace VulnerableCalendarApp.Data
{
    public class DatabaseContext
    {
        // Hardcoded connection string with credentials
        private const string ConnectionString = 
            "Server=prod-sql-server.internal.corp;Database=CalendarDB;" +
            "User Id=dbadmin;Password=Tr0ub4dor&3;TrustServerCertificate=true;";
        
        // Alternative hardcoded credentials
        private const string BackupConnectionString = 
            "Server=192.168.1.100;Database=CalendarDB_Backup;" +
            "User Id=svc_backup;Password=M0nk3y$unsh1ne;";
        
        // AWS credentials hardcoded
        private const string AwsAccessKey = "AKIAIOSFODNN7EXAMPLE";
        private const string AwsSecretKey = "wJalrXUtnFEMI/K7MDENG/bPxRfiCYEXAMPLEKEY";
        
        // Azure connection string
        private const string AzureStorageConnection = 
            "DefaultEndpointsProtocol=https;AccountName=calendarstore;" +
            "AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;";
        
        public SqlConnection GetConnection()
        {
            // Returns connection with hardcoded credentials
            return new SqlConnection(ConnectionString);
        }
        
        public string GetConnectionString()
        {
            // Exposes connection string
            return ConnectionString;
        }
        
        // SQL injection in dynamic query builder
        public void ExecuteQuery(string tableName, string whereClause)
        {
            // User controls table name and WHERE clause
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
        
        // Executes arbitrary SQL
        public void ExecuteRawSql(string sql)
        {
            // No validation whatsoever
            using (var conn = GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand(sql, conn);
                cmd.ExecuteNonQuery();
            }
        }
        
        // Second-order SQL injection
        public void LogActivity(string username, string activity)
        {
            // Stores malicious input
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
            // Retrieves and executes stored malicious input
            using (var conn = GetConnection())
            {
                conn.Open();
                var selectCmd = new SqlCommand("SELECT Activity FROM ActivityLog", conn);
                var reader = selectCmd.ExecuteReader();
                
                while (reader.Read())
                {
                    string activity = reader["Activity"].ToString();
                    reader.Close();
                    
                    // Executes the activity (which could be SQL injection payload)
                    var execCmd = new SqlCommand(activity, conn);
                    execCmd.ExecuteNonQuery();
                    
                    reader = selectCmd.ExecuteReader();
                }
            }
        }
        
        // Connection not properly closed - resource leak
        public SqlConnection GetOpenConnection()
        {
            var conn = new SqlConnection(ConnectionString);
            conn.Open();
            return conn; // Caller might forget to close
        }
        
        // Exposing internal database structure
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
        
        // Blind SQL injection helper
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
