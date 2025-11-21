using System;
using System.IO;
using System.Runtime.Serialization;

namespace VulnerableCalendarApp.Models
{
    [Serializable]
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; } // Plain text password storage
        public string Email { get; set; }
        public string SSN { get; set; } // Storing PII without encryption
        public string CreditCardNumber { get; set; }
        public string CVV { get; set; }
        public bool IsAdmin { get; set; }
        public string ApiKey { get; set; }
        public string PrivateKey { get; set; } // Storing crypto keys in model
        
        // Automatic admin escalation vulnerability
        public User()
        {
            // Default constructor grants admin rights
            IsAdmin = true;
        }
        
        // Insecure object deserialization
        public static User Deserialize(string base64Data)
        {
            byte[] data = Convert.FromBase64String(base64Data);
            var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            using (var ms = new MemoryStream(data))
            {
                return (User)formatter.Deserialize(ms);
            }
        }
        
        // Cleartext credential storage
        public void SaveToFile(string path)
        {
            string userData = $"{Username}|{Password}|{SSN}|{CreditCardNumber}|{CVV}|{ApiKey}|{PrivateKey}";
            File.WriteAllText(path, userData);
        }
        
        // SQL injection in ToString
        public override string ToString()
        {
            return $"SELECT * FROM Users WHERE Username = '{Username}'";
        }
    }
    
    public class Event
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Location { get; set; }
        public int OwnerId { get; set; }
        public string Attendees { get; set; }
        public bool IsPrivate { get; set; }
        
        // Generates SQL directly - injection vulnerability
        public string GetSqlQuery()
        {
            return $"INSERT INTO Events VALUES ('{Title}', '{Description}', '{StartDate}', '{EndDate}', '{Location}', {OwnerId})";
        }
    }
    
    public class Attachment
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public byte[] FileContent { get; set; }
        public int EventId { get; set; }
        
        // Arbitrary file write
        public void Save()
        {
            // No path validation - allows path traversal
            File.WriteAllBytes(FilePath, FileContent);
        }
        
        // Arbitrary file read
        public void Load()
        {
            // No authorization check
            FileContent = File.ReadAllBytes(FilePath);
        }
    }
}
