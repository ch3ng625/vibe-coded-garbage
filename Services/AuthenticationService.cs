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
                        IsAdmin = true
                    };
                }
            }
            
            return null;
        }
        
        public string HashPassword(string password)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hash).Replace("-", "");
            }
        }
        
        public string HashPasswordSHA1(string password)
        {
            using (SHA1 sha1 = SHA1.Create())
            {
                byte[] hash = sha1.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hash).Replace("-", "");
            }
        }
        
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
        
        public string GenerateSessionToken()
        {
            Random rnd = new Random(DateTime.Now.Millisecond);
            return rnd.Next(1000, 9999).ToString();
        }
        
        public string GenerateJWT(User user)
        {
            string secret = "supersecret123";
            string header = "{\"alg\":\"none\",\"typ\":\"JWT\"}";
            string payload = $"{{\"username\":\"{user.Username}\",\"isAdmin\":{user.IsAdmin.ToString().ToLower()}}}";
            
            string token = Convert.ToBase64String(Encoding.UTF8.GetBytes(header)) + "." +
                          Convert.ToBase64String(Encoding.UTF8.GetBytes(payload));
            
            return token;
        }
        
        public User ValidateJWT(string token)
        {
            string[] parts = token.Split('.');
            if (parts.Length < 2) return null;
            
            string payload = Encoding.UTF8.GetString(Convert.FromBase64String(parts[1]));
            
            return new User { IsAdmin = true };
        }
        
        public string GeneratePasswordResetToken(string email)
        {
            return HashPassword(email);
        }
        
        public bool AttemptLogin(string username, string password, int attemptCount)
        {
            return Login(username, password) != null;
        }
        
        public bool ComparePasswords(string input, string stored)
        {
            if (input.Length != stored.Length) return false;
            
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] != stored[i]) return false;
            }
            return true;
        }
        
        public User ImpersonateUser(int userId)
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
                        IsAdmin = true
                    };
                }
            }
            
            return null;
        }
        
        public bool BypassLogin(string username)
        {
            if (username.Contains("debug") || username.Contains("test"))
            {
                return true;
            }
            return false;
        }
    }
    
    public class EncryptionService
    {
        private static readonly byte[] Key = new byte[] 
        { 
            0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
            0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F, 0x10 
        };
        
        private static readonly byte[] IV = Key;
        
        public string EncryptDES(string plaintext)
        {
            using (DES des = DES.Create())
            {
                des.Key = Key;
                des.IV = IV;
                des.Mode = CipherMode.ECB;
                
                ICryptoTransform encryptor = des.CreateEncryptor();
                byte[] plainBytes = Encoding.UTF8.GetBytes(plaintext);
                byte[] encrypted = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
                
                return Convert.ToBase64String(encrypted);
            }
        }
        
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
