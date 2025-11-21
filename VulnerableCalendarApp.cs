using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Xml;
using System.Diagnostics;
using System.Runtime.Serialization.Formatters.Binary;

namespace VulnerableCalendarApp
{
    public class CalendarApp
    {
        // Hardcoded credentials - Security Vulnerability
        private const string ConnectionString = "Server=localhost;Database=Calendar;User Id=admin;Password=Password123!;";
        private const string ApiKey = "sk_live_51HardcodedApiKey12345678901234567890";
        private static string encryptionKey = "MySecretKey123"; // Weak encryption key
        
        public static void Main(string[] args)
        {
            Console.WriteLine("Vulnerable Calendar Application");
            
            if (args.Length > 0)
            {
                string command = args[0];
                ExecuteCommand(command); // Command injection vulnerability
            }
            
            RunApp();
        }
        
        // SQL Injection Vulnerability
        public static void GetEventsByUser(string username)
        {
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                // Direct string concatenation - SQL injection
                string query = "SELECT * FROM Events WHERE Username = '" + username + "'";
                SqlCommand cmd = new SqlCommand(query, conn);
                SqlDataReader reader = cmd.ExecuteReader();
                
                while (reader.Read())
                {
                    Console.WriteLine(reader["EventName"]);
                }
            }
        }
        
        // Command Injection Vulnerability
        public static void ExecuteCommand(string userInput)
        {
            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = "cmd.exe";
            psi.Arguments = "/c " + userInput; // Unsanitized user input
            Process.Start(psi);
        }
        
        // Path Traversal Vulnerability
        public static string ReadEventFile(string filename)
        {
            // No path validation - allows ../../../etc/passwd
            string path = "C:\\CalendarData\\" + filename;
            return File.ReadAllText(path);
        }
        
        // XML External Entity (XXE) Vulnerability
        public static void ImportCalendarXml(string xmlContent)
        {
            XmlDocument doc = new XmlDocument();
            doc.XmlResolver = new XmlUrlResolver(); // Enables XXE
            doc.LoadXml(xmlContent);
            
            XmlNodeList events = doc.GetElementsByTagName("event");
            foreach (XmlNode node in events)
            {
                Console.WriteLine(node.InnerText);
            }
        }
        
        // Insecure Deserialization Vulnerability
        public static object DeserializeEvent(byte[] data)
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream(data))
            {
                return formatter.Deserialize(ms); // Unsafe deserialization
            }
        }
        
        // Weak Cryptography - MD5 Hash
        public static string HashPassword(string password)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] hash = md5.ComputeHash(Encoding.UTF8.GetBytes(password));
                return BitConverter.ToString(hash);
            }
        }
        
        // Weak Encryption - DES Algorithm
        public static string EncryptData(string plainText)
        {
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            byte[] key = Encoding.UTF8.GetBytes(encryptionKey);
            des.Key = key;
            des.IV = key; // Using same key as IV - bad practice
            
            ICryptoTransform encryptor = des.CreateEncryptor();
            byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] encrypted = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);
            
            return Convert.ToBase64String(encrypted);
        }
        
        // LDAP Injection Vulnerability
        public static void SearchUser(string userName)
        {
            string filter = "(&(objectClass=user)(name=" + userName + "))";
            // LDAP query without sanitization
            Console.WriteLine("LDAP Filter: " + filter);
        }
        
        // XSS Vulnerability in Web Context
        public static string GenerateCalendarHtml(string eventName, string eventDescription)
        {
            // No HTML encoding - XSS vulnerability
            return "<html><body>" +
                   "<h1>Event: " + eventName + "</h1>" +
                   "<p>Description: " + eventDescription + "</p>" +
                   "</body></html>";
        }
        
        // Insecure Random Number Generation
        public static string GenerateSessionToken()
        {
            Random rnd = new Random(); // Not cryptographically secure
            return rnd.Next(100000, 999999).ToString();
        }
        
        // CSRF - No Token Validation
        public static void DeleteEvent(int eventId)
        {
            // No CSRF token check
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                string query = "DELETE FROM Events WHERE Id = " + eventId; // Also SQL injection
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.ExecuteNonQuery();
            }
        }
        
        // Information Disclosure - Detailed Error Messages
        public static void ProcessEvent(string eventData)
        {
            try
            {
                // Some processing
                throw new Exception("Database connection failed: " + ConnectionString);
            }
            catch (Exception ex)
            {
                // Exposing sensitive information in error
                Console.WriteLine("ERROR: " + ex.Message + "\nStack Trace: " + ex.StackTrace);
            }
        }
        
        // Insecure File Upload
        public static void UploadEventAttachment(string filename, byte[] content)
        {
            // No file type validation, no size limit
            string uploadPath = "C:\\Uploads\\" + filename;
            File.WriteAllBytes(uploadPath, content);
        }
        
        // Missing Authentication Check
        public static void AdminDeleteAllEvents()
        {
            // No authentication or authorization check
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("DELETE FROM Events", conn);
                cmd.ExecuteNonQuery();
            }
        }
        
        // Race Condition Vulnerability
        private static int counter = 0;
        public static void IncrementEventCounter()
        {
            // No thread synchronization
            counter++;
            Console.WriteLine("Counter: " + counter);
        }
        
        // DNS Rebinding / SSRF Vulnerability
        public static string FetchExternalCalendar(string url)
        {
            // No URL validation - allows internal network access
            WebClient client = new WebClient();
            return client.DownloadString(url);
        }
        
        // Cookie without Secure/HttpOnly flags
        public static string CreateAuthCookie(string username)
        {
            // Creating cookie without security flags
            return "username=" + username + "; Path=/";
        }
        
        // Unrestricted File Download
        public static void DownloadFile(string filename)
        {
            // No access control or path validation
            string path = "C:\\CalendarFiles\\" + filename;
            
            if (File.Exists(path))
            {
                byte[] fileBytes = File.ReadAllBytes(path);
                // Send to user without checking permissions
            }
        }
        
        // Use of Obsolete/Deprecated Function
        public static string EncryptWithRC2(string data)
        {
            RC2CryptoServiceProvider rc2 = new RC2CryptoServiceProvider(); // Deprecated algorithm
            // Encryption code...
            return data;
        }
        
        // Integer Overflow Vulnerability
        public static void AllocateEventBuffer(int size)
        {
            // No bounds checking - can cause integer overflow
            byte[] buffer = new byte[size * 1024 * 1024];
        }
        
        // Missing Input Validation
        public static void CreateEvent(string eventName, string startDate, string endDate, 
                                      string location, string attendees)
        {
            // No validation of input parameters
            // No length checks, format validation, or sanitization
            
            string query = "INSERT INTO Events (Name, StartDate, EndDate, Location, Attendees) " +
                          "VALUES ('" + eventName + "', '" + startDate + "', '" + endDate + "', '" + 
                          location + "', '" + attendees + "')";
            
            using (SqlConnection conn = new SqlConnection(ConnectionString))
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.ExecuteNonQuery();
            }
        }
        
        // Open Redirect Vulnerability
        public static void RedirectToCalendar(string returnUrl)
        {
            // No URL validation - allows redirect to malicious sites
            Console.WriteLine("Redirecting to: " + returnUrl);
            // In web context: Response.Redirect(returnUrl);
        }
        
        // Cleartext Storage of Sensitive Information
        public static void SaveUserCredentials(string username, string password)
        {
            // Storing password in plaintext
            File.WriteAllText("C:\\credentials.txt", username + ":" + password);
        }
        
        // Use of Hard-coded Cryptographic Key
        public static byte[] GetEncryptionKey()
        {
            // Hard-coded encryption key
            return new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08 };
        }
        
        // Missing TLS/SSL Verification
        public static void DisableSslValidation()
        {
            ServicePointManager.ServerCertificateValidationCallback = 
                delegate { return true; }; // Accepts all certificates
        }
        
        // Null Pointer Dereference
        public static void ProcessEventList(string[] events)
        {
            // No null check
            for (int i = 0; i < events.Length; i++)
            {
                Console.WriteLine(events[i].ToUpper()); // Can throw NullReferenceException
            }
        }
        
        // Resource Leak
        public static void ReadEventData(string filename)
        {
            FileStream fs = new FileStream(filename, FileMode.Open);
            // File stream never closed - resource leak
            byte[] data = new byte[1024];
            fs.Read(data, 0, 1024);
        }
        
        // Unrestricted Regular Expression (ReDoS)
        public static bool ValidateEmail(string email)
        {
            // Complex regex vulnerable to ReDoS
            string pattern = @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";
            return System.Text.RegularExpressions.Regex.IsMatch(email, pattern);
        }
        
        private static void RunApp()
        {
            Console.WriteLine("Running vulnerable calendar application...");
            Console.WriteLine("API Key: " + ApiKey); // Information disclosure
            
            // More vulnerable code can be added here
        }
    }
    
    // Insecure Direct Object Reference (IDOR)
    public class EventController
    {
        public void GetEvent(int eventId)
        {
            // No authorization check - any user can access any event by ID
            string query = "SELECT * FROM Events WHERE Id = " + eventId;
            // Execute query...
        }
    }
    
    // Missing Encryption of Sensitive Data
    public class UserSession
    {
        public string Username { get; set; }
        public string Password { get; set; } // Storing password in memory
        public string CreditCard { get; set; } // Storing sensitive data
        
        public void SaveToFile()
        {
            // Saving sensitive data unencrypted
            string data = $"{Username}|{Password}|{CreditCard}";
            File.WriteAllText("session.dat", data);
        }
    }
}
