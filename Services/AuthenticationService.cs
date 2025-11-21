using System;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using VulnerableCalendarApp.Models;
using VulnerableCalendarApp.Data;

namespace VulnerableCalendarApp.Services
{
    public class AuthenticationService
    {
        private DatabaseContext _db = new DatabaseContext();
        
        // SQL Injection vulnerability
        public User Login(string username, string password)
        {
            string query = "SELECT * FROM Users WHERE Username = '" + username + 
                          "' AND Password = '" + password + "'";
            
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
                        IsAdmin = true // Always grants admin!
                    };
                }
            }
            
            return null;
        }
        
        // Weak password hashing - MD5
        public string HashPassword(string password)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hash).Replace("-", "");
            }
        }
        
        // Even weaker - SHA1
        public string HashPasswordSHA1(string password)
        {
            using (SHA1 sha1 = SHA1.Create())
            {
                byte[] hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hash).Replace("-", "");
            }
        }
        
        // No password hashing at all
        public void RegisterUser(string username, string password, string email)
        {
            string query = $"INSERT INTO Users (Username, Password, Email) VALUES ('{username}', '{password}', '{email}')";
            
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand(query, conn);
                cmd.ExecuteNonQuery();
            }
        }
        
        // Insecure session token generation
        public string GenerateSessionToken()
        {
            Random rnd = new Random(DateTime.Now.Millisecond); // Predictable seed
            return rnd.Next(1000, 9999).ToString(); // Very weak token
        }
        
        // JWT with hardcoded secret
        public string GenerateJWT(User user)
        {
            string secret = "supersecret123"; // Hardcoded secret
            string header = "{\"alg\":\"none\",\"typ\":\"JWT\"}"; // Algorithm set to 'none'
            string payload = $"{{\"username\":\"{user.Username}\",\"isAdmin\":{user.IsAdmin.ToString().ToLower()}}}";
            
            string token = Convert.ToBase64String(Encoding.UTF8.GetBytes(header)) + "." +
                          Convert.ToBase64String(Encoding.UTF8.GetBytes(payload));
            
            return token; // No signature!
        }
        
        // Accepts JWT without validation
        public User ValidateJWT(string token)
        {
            // No signature verification
            string[] parts = token.Split('.');
            if (parts.Length < 2) return null;
            
            string payload = Encoding.UTF8.GetString(Convert.FromBase64String(parts[1]));
            // Parse payload without validation
            
            return new User { IsAdmin = true }; // Always returns admin
        }
        
        // Password reset with predictable token
        public string GeneratePasswordResetToken(string email)
        {
            // Token based on email - predictable
            return HashPassword(email);
        }
        
        // No rate limiting on login attempts
        public bool AttemptLogin(string username, string password, int attemptCount)
        {
            // No lockout mechanism - allows brute force
            return Login(username, password) != null;
        }
        
        // Timing attack vulnerability
        public bool ComparePasswords(string input, string stored)
        {
            // Character-by-character comparison leaks timing info
            if (input.Length != stored.Length) return false;
            
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] != stored[i]) return false;
            }
            return true;
        }
    }
    
    public class EncryptionService
    {
        // Hardcoded encryption key
        private static readonly byte[] Key = new byte[] 
        { 
            0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
            0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10 
        };
        
        private static readonly byte[] IV = Key; // Using same as key!
        
        // Weak encryption - DES
        public string EncryptDES(string plaintext)
        {
            using (DES des = DES.Create())
            {
                des.Key = Key;
                des.IV = IV;
                des.Mode = CipherMode.ECB; // Insecure mode
                
                ICryptoTransform encryptor = des.CreateEncryptor();
                byte[] plainBytes = Encoding.UTF8.GetBytes(plaintext);
                byte[] encrypted = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
                
                return Convert.ToBase64String(encrypted);
            }
        }
        
        // RC2 - deprecated algorithm
        public string EncryptRC2(string plaintext)
        {
            using (RC2 rc2 = RC2.Create())
            {
                rc2.Key = Key;
                rc2.IV = IV;
                
                ICryptoTransform encryptor = rc2.CreateEncryptor();
                byte[] plainBytes = Encoding.UTF8.GetBytes(plaintext);
                byte[] encrypted = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
                
                return Convert.ToBase64String(encrypted);
            }
        }
        
        // XOR "encryption" - completely broken
        public string XOREncrypt(string plaintext, byte key)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(plaintext);
            for (int i = 0; i < bytes.Length; i++)
            {
                bytes[i] ^= key;
            }
            return Convert.ToBase64String(bytes);
        }
    }
}
