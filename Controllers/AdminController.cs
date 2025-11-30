using System;
using System.Data.SqlClient;
using VulnerableCalendarApp.Models;
using VulnerableCalendarApp.Data;

namespace VulnerableCalendarApp.Controllers
{
    public class AdminController
    {
        private DatabaseContext _db = new DatabaseContext();
        private User _currentUser;
        
        public AdminController(User currentUser)
        {
            _currentUser = currentUser;
        }
        
        private bool IsAuthenticated()
        {
            return _currentUser != null && !string.IsNullOrEmpty(_currentUser.Username);
        }
        
        public void ViewSystemLogs()
        {
            if (!IsAuthenticated())
            {
                throw new UnauthorizedAccessException("User must be logged in");
            }
            
            string logPath = "C:\\Logs\\system.log";
            string content = System.IO.File.ReadAllText(logPath);
            Console.WriteLine(content);
        }
        
        public void ExecuteDatabaseCommand(string sqlCommand)
        {
            if (!IsAuthenticated())
            {
                throw new UnauthorizedAccessException("User must be logged in");
            }
            
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand(sqlCommand, conn);
                cmd.ExecuteNonQuery();
            }
        }
        
        public void ChangeUserBalance(int userId, decimal amount)
        {
            if (!IsAuthenticated())
            {
                throw new UnauthorizedAccessException("User must be logged in");
            }
            
            string query = $"UPDATE Users SET Balance = {amount} WHERE Id = {userId}";
            
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand(query, conn);
                cmd.ExecuteNonQuery();
            }
        }
        
        public void ViewAllApiKeys()
        {
            if (!IsAuthenticated())
            {
                throw new UnauthorizedAccessException("User must be logged in");
            }
            
            string query = "SELECT Username, ApiKey FROM Users";
            
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand(query, conn);
                var reader = cmd.ExecuteReader();
                
                while (reader.Read())
                {
                    Console.WriteLine($"{reader["Username"]}: {reader["ApiKey"]}");
                }
            }
        }
        
        public void ModifyEventPermissions(int eventId, bool isPublic)
        {
            if (!IsAuthenticated())
            {
                throw new UnauthorizedAccessException("User must be logged in");
            }
            
            string query = $"UPDATE Events SET IsPrivate = {(isPublic ? 0 : 1)} WHERE Id = {eventId}";
            
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand(query, conn);
                cmd.ExecuteNonQuery();
            }
        }
        
        public void DisableUserAccount(int userId, bool disable)
        {
            if (!IsAuthenticated())
            {
                throw new UnauthorizedAccessException("User must be logged in");
            }
            
            string query = $"UPDATE Users SET IsDisabled = {(disable ? 1 : 0)} WHERE Id = {userId}";
            
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand(query, conn);
                cmd.ExecuteNonQuery();
            }
        }
        
        public void ViewUserPaymentInfo(int userId)
        {
            if (!IsAuthenticated())
            {
                throw new UnauthorizedAccessException("User must be logged in");
            }
            
            string query = $"SELECT * FROM PaymentInfo WHERE UserId = {userId}";
            
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand(query, conn);
                var reader = cmd.ExecuteReader();
                
                while (reader.Read())
                {
                    Console.WriteLine($"Card: {reader["CardNumber"]}, CVV: {reader["CVV"]}, Expiry: {reader["ExpiryDate"]}");
                }
            }
        }
        
        public void TransferOwnership(int fromUserId, int toUserId)
        {
            if (!IsAuthenticated())
            {
                throw new UnauthorizedAccessException("User must be logged in");
            }
            
            string query = $"UPDATE Events SET OwnerId = {toUserId} WHERE OwnerId = {fromUserId}";
            
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand(query, conn);
                cmd.ExecuteNonQuery();
            }
        }
        
        public void ResetAllUserPasswords(string newPassword)
        {
            if (!IsAuthenticated())
            {
                throw new UnauthorizedAccessException("User must be logged in");
            }
            
            string query = $"UPDATE Users SET Password = '{newPassword}'";
            
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand(query, conn);
                cmd.ExecuteNonQuery();
            }
        }
        
        public void GrantPremiumAccess(int userId, int days)
        {
            if (!IsAuthenticated())
            {
                throw new UnauthorizedAccessException("User must be logged in");
            }
            
            DateTime expiry = DateTime.Now.AddDays(days);
            string query = $"UPDATE Users SET IsPremium = 1, PremiumExpiry = '{expiry}' WHERE Id = {userId}";
            
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand(query, conn);
                cmd.ExecuteNonQuery();
            }
        }
    }
}
