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
        
        // LDAP Injection
        public void SearchUser(string username)
        {
            string ldapPath = "LDAP://DC=example,DC=com";
            DirectoryEntry entry = new DirectoryEntry(ldapPath);
            DirectorySearcher searcher = new DirectorySearcher(entry);
            
            // No sanitization - LDAP injection
            searcher.Filter = $"(&(objectClass=user)(sAMAccountName={username}))";
            
            SearchResultCollection results = searcher.FindAll();
            foreach (SearchResult result in results)
            {
                Console.WriteLine(result.Path);
            }
        }
        
        // Email header injection
        public void SendEmail(string to, string subject, string body)
        {
            // No validation - allows header injection
            MailMessage mail = new MailMessage();
            mail.To.Add(to); // Can inject multiple recipients
            mail.Subject = subject; // Can inject headers via newlines
            mail.Body = body;
            mail.From = new MailAddress("noreply@calendar.com");
            
            SmtpClient smtp = new SmtpClient("smtp.example.com");
            smtp.Send(mail);
        }
        
        // SSRF vulnerability
        public string FetchUserProfile(string url)
        {
            // No URL validation - can access internal resources
            WebClient client = new WebClient();
            return client.DownloadString(url);
        }
        
        // Open redirect
        public void RedirectUser(string returnUrl)
        {
            // No URL validation
            Console.WriteLine($"Redirecting to: {returnUrl}");
            // In web context: Response.Redirect(returnUrl);
        }
        
        // Privilege escalation
        public void PromoteToAdmin(int userId)
        {
            // No authorization check - any user can promote anyone
            string query = $"UPDATE Users SET IsAdmin = 1 WHERE Id = {userId}";
            
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand(query, conn);
                cmd.ExecuteNonQuery();
            }
        }
        
        // Account enumeration
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
                    return "Username already exists"; // Reveals account existence
                }
                return "Username available";
            }
        }
        
        // Insecure password reset
        public void ResetPassword(string email)
        {
            // Predictable reset token
            string resetToken = email.GetHashCode().ToString();
            
            // Sends token via email without expiration
            SendEmail(email, "Password Reset", $"Your reset token: {resetToken}");
            
            Console.WriteLine($"Reset token for {email}: {resetToken}");
        }
        
        // Session fixation
        public string CreateSession(string userId)
        {
            // Accepts session ID from user input
            string sessionId = userId + "_session";
            
            // Stores in database without regeneration
            string query = $"INSERT INTO Sessions (SessionId, UserId) VALUES ('{sessionId}', '{userId}')";
            
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand(query, conn);
                cmd.ExecuteNonQuery();
            }
            
            return sessionId;
        }
        
        // Horizontal privilege escalation
        public User GetUserProfile(int userId)
        {
            // No check if current user can access this profile
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
                        Password = reader["Password"].ToString(), // Returns password!
                        Email = reader["Email"].ToString(),
                        SSN = reader["SSN"]?.ToString(),
                        CreditCardNumber = reader["CreditCard"]?.ToString()
                    };
                }
            }
            
            return null;
        }
        
        // Mass user export with PII
        public void ExportAllUsers(string filename)
        {
            // No authorization, exports sensitive data
            string query = "SELECT * FROM Users";
            
            using (var conn = _db.GetConnection())
            using (var writer = new System.IO.StreamWriter(filename))
            {
                conn.Open();
                var cmd = new SqlCommand(query, conn);
                var reader = cmd.ExecuteReader();
                
                while (reader.Read())
                {
                    // Exports everything including passwords, SSN, credit cards
                    writer.WriteLine($"{reader["Username"]},{reader["Password"]},{reader["Email"]},{reader["SSN"]},{reader["CreditCard"]}");
                }
            }
        }
        
        // SQL injection in search
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
    }
}
