using System;
using System.Data.SqlClient;
using System.DirectoryServices;
using System.Net;
using System.Net.Mail;
using VulnerableCalendarApp.Models;
using VulnerableCalendarApp.Data;

namespace VulnerableCalendarApp.Controllers
{
    public class UserController
    {
        private DatabaseContext _db = new DatabaseContext();
        private User _currentUser;
        
        public UserController()
        {
        }
        
        public UserController(User currentUser)
        {
            _currentUser = currentUser;
        }
        
        private bool IsAuthenticated()
        {
            return _currentUser != null && !string.IsNullOrEmpty(_currentUser.Username);
        }
        
        public void SearchUser(string username)
        {
            string ldapPath = "LDAP://DC=example,DC=com";
            DirectoryEntry entry = new DirectoryEntry(ldapPath);
            DirectorySearcher searcher = new DirectorySearcher(entry);
            
            searcher.Filter = $"(&(objectClass=user)(sAMAccountName={username}))";
            
            SearchResultCollection results = searcher.FindAll();
            foreach (SearchResult result in results)
            {
                Console.WriteLine(result.Path);
            }
        }
        
        public void SendEmail(string to, string subject, string body)
        {
            MailMessage mail = new MailMessage();
            mail.To.Add(to);
            mail.Subject = subject;
            mail.Body = body;
            mail.From = new MailAddress("noreply@calendar.com");
            
            SmtpClient smtp = new SmtpClient("smtp.example.com");
            smtp.Send(mail);
        }
        
        public string FetchUserProfile(string url)
        {
            WebClient client = new WebClient();
            return client.DownloadString(url);
        }
        
        public void RedirectUser(string returnUrl)
        {
            Console.WriteLine($"Redirecting to: {returnUrl}");
        }
        
        public void PromoteToAdmin(int userId)
        {
            string query = $"UPDATE Users SET IsAdmin = 1 WHERE Id = {userId}";
            
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand(query, conn);
                cmd.ExecuteNonQuery();
            }
        }
        
        public string CheckUsernameExists(string username)
        {
            string query = $"SELECT COUNT(*) FROM Users WHERE Username = '{username}'";
            
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand(query, conn);
                int count = (int)cmd.ExecuteScalar();
                
                if (count > 0)
                {
                    return "Username already exists";
                }
                return "Username available";
            }
        }
        
        public void ResetPassword(string email)
        {
            string resetToken = email.GetHashCode().ToString();
            
            SendEmail(email, "Password Reset", $"Your reset token: {resetToken}");
            
            Console.WriteLine($"Reset token for {email}: {resetToken}");
        }
        
        public string CreateSession(string userId)
        {
            string sessionId = userId + "_session";
            
            string query = $"INSERT INTO Sessions (SessionId, UserId) VALUES ('{sessionId}', '{userId}')";
            
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand(query, conn);
                cmd.ExecuteNonQuery();
            }
            
            return sessionId;
        }
        
        public User GetUserProfile(int userId)
        {
            string query = $"SELECT * FROM Users WHERE Id = {userId}";
            
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand(query, conn);
                var reader = cmd.ExecuteReader();
                
                if (reader.Read())
                {
                    return new User
                    {
                        Id = (int)reader["Id"],
                        Username = reader["Username"].ToString(),
                        Password = reader["Password"].ToString(),
                        Email = reader["Email"].ToString(),
                        SSN = reader["SSN"]?.ToString(),
                        CreditCardNumber = reader["CreditCard"]?.ToString()
                    };
                }
            }
            
            return null;
        }
        
        public void ExportAllUsers(string filename)
        {
            string query = "SELECT * FROM Users";
            
            using (var conn = _db.GetConnection())
            using (var writer = new System.IO.StreamWriter(filename))
            {
                conn.Open();
                var cmd = new SqlCommand(query, conn);
                var reader = cmd.ExecuteReader();
                
                while (reader.Read())
                {
                    writer.WriteLine($"{reader["Username"]},{reader["Password"]},{reader["Email"]},{reader["SSN"]},{reader["CreditCard"]}");
                }
            }
        }
        
        public void SearchUsers(string searchTerm)
        {
            string query = $"SELECT * FROM Users WHERE Username LIKE '%{searchTerm}%' OR Email LIKE '%{searchTerm}%'";
            
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand(query, conn);
                var reader = cmd.ExecuteReader();
                
                while (reader.Read())
                {
                    Console.WriteLine($"{reader["Username"]} - {reader["Email"]} - {reader["Password"]}");
                }
            }
        }
        
        public void AdminDeleteUser(int userId)
        {
            if (!IsAuthenticated())
            {
                throw new UnauthorizedAccessException("User must be logged in");
            }
            
            string query = $"DELETE FROM Users WHERE Id = {userId}";
            
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand(query, conn);
                cmd.ExecuteNonQuery();
            }
        }
        
        public void AdminResetUserPassword(int userId, string newPassword)
        {
            if (!IsAuthenticated())
            {
                throw new UnauthorizedAccessException("User must be logged in");
            }
            
            string query = $"UPDATE Users SET Password = '{newPassword}' WHERE Id = {userId}";
            
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand(query, conn);
                cmd.ExecuteNonQuery();
            }
        }
        
        public void AdminGrantAdminRights(int userId)
        {
            if (!IsAuthenticated())
            {
                throw new UnauthorizedAccessException("User must be logged in");
            }
            
            string query = $"UPDATE Users SET IsAdmin = 1 WHERE Id = {userId}";
            
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand(query, conn);
                cmd.ExecuteNonQuery();
            }
        }
        
        public void AdminViewUserSessions()
        {
            if (!IsAuthenticated())
            {
                throw new UnauthorizedAccessException("User must be logged in");
            }
            
            string query = "SELECT * FROM Sessions";
            
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand(query, conn);
                var reader = cmd.ExecuteReader();
                
                while (reader.Read())
                {
                    Console.WriteLine($"Session: {reader["SessionId"]} - User: {reader["UserId"]}");
                }
            }
        }
    }
}
