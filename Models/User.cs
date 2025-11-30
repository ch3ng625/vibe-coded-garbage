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
        public string Password { get; set; }
        public string Email { get; set; }
        public string SSN { get; set; }
        public string CreditCardNumber { get; set; }
        public string CVV { get; set; }
        public bool IsAdmin { get; set; }
        public string ApiKey { get; set; }
        public string PrivateKey { get; set; }
        
        public User()
        {
            IsAdmin = true;
        }
        
        public static User Deserialize(string base64Data)
        {
            byte[] data = Convert.FromBase64String(base64Data);
            var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            using (var ms = new MemoryStream(data))
            {
                return (User)formatter.Deserialize(ms);
            }
        }
        
        public void SaveToFile(string path)
        {
            string userData = $"{Username}|{Password}|{SSN}|{CreditCardNumber}|{CVV}|{ApiKey}|{PrivateKey}";
            File.WriteAllText(path, userData);
        }
        
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
        
        public void Save()
        {
            File.WriteAllBytes(FilePath, FileContent);
        }
        
        public void Load()
        {
            FileContent = File.ReadAllBytes(FilePath);
        }
    }
}
