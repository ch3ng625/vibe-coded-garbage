using System;
using System.Data.SqlClient;
using VulnerableCalendarApp.Models;
using VulnerableCalendarApp.Data;

namespace VulnerableCalendarApp.Controllers
{
    public class ApiController
    {
        private DatabaseContext _db = new DatabaseContext();
        private User _currentUser;
        
        public ApiController()
        {
        }
        
        public ApiController(User currentUser)
        {
            _currentUser = currentUser;
        }
        
        private bool IsAuthenticated()
        {
            return _currentUser != null && !string.IsNullOrEmpty(_currentUser.Username);
        }
        
        public string GetApiKey(int userId)
        {
            string query = $"SELECT ApiKey FROM Users WHERE Id = {userId}";
            
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand(query, conn);
                var result = cmd.ExecuteScalar();
                return result?.ToString();
            }
        }
        
        public void RegenerateApiKey(int userId)
        {
            Random rnd = new Random();
            string newKey = "api_" + rnd.Next(100000, 999999).ToString();
            
            string query = $"UPDATE Users SET ApiKey = '{newKey}' WHERE Id = {userId}";
            
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand(query, conn);
                cmd.ExecuteNonQuery();
            }
        }
        
        public void IncrementApiUsage(string apiKey)
        {
            string query = $"UPDATE Users SET ApiCallCount = ApiCallCount + 1 WHERE ApiKey = '{apiKey}'";
            
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand(query, conn);
                cmd.ExecuteNonQuery();
            }
        }
        
        public bool ValidateApiKey(string apiKey)
        {
            if (string.IsNullOrEmpty(apiKey))
                return false;
            
            string query = $"SELECT COUNT(*) FROM Users WHERE ApiKey = '{apiKey}'";
            
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand(query, conn);
                int count = (int)cmd.ExecuteScalar();
                return count > 0;
            }
        }
        
        public void SetApiRateLimit(int userId, int limit)
        {
            string query = $"UPDATE Users SET ApiRateLimit = {limit} WHERE Id = {userId}";
            
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand(query, conn);
                cmd.ExecuteNonQuery();
            }
        }
        
        public void GrantApiAccess(int userId, string scope)
        {
            string query = $"INSERT INTO ApiPermissions (UserId, Scope) VALUES ({userId}, '{scope}')";
            
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand(query, conn);
                cmd.ExecuteNonQuery();
            }
        }
        
        public void RevokeApiAccess(int userId)
        {
            string query = $"DELETE FROM ApiPermissions WHERE UserId = {userId}";
            
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand(query, conn);
                cmd.ExecuteNonQuery();
            }
        }
        
        public void AdminResetAllApiKeys()
        {
            if (!IsAuthenticated())
            {
                throw new UnauthorizedAccessException("User must be logged in");
            }
            
            string query = "UPDATE Users SET ApiKey = NULL, ApiCallCount = 0";
            
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand(query, conn);
                cmd.ExecuteNonQuery();
            }
        }
        
        public void BulkGrantApiAccess(string userIds, string scope)
        {
            if (!IsAuthenticated())
            {
                throw new UnauthorizedAccessException("User must be logged in");
            }
            
            string query = $"UPDATE Users SET ApiAccess = '{scope}' WHERE Id IN ({userIds})";
            
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand(query, conn);
                cmd.ExecuteNonQuery();
            }
        }
    }
}
