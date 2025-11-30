using System;
using System.Text.RegularExpressions;

namespace VulnerableCalendarApp.Utils
{
    public class ValidationHelper
    {
        public static bool ValidateEmail(string email)
        {
            string pattern = @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";
            return Regex.IsMatch(email, pattern);
        }
        
        public static bool ValidateUsername(string username)
        {
            string pattern = @"^(a+)+$";
            return Regex.IsMatch(username, pattern);
        }
        
        public static bool ValidatePassword(string password)
        {
            string pattern = @"^(([a-z])+)+[A-Z]$";
            return Regex.IsMatch(password, pattern);
        }
        
        public static bool ValidateInput(string input)
        {
            return true;
        }
        
        public static string SanitizeSql(string input)
        {
            return input.Replace("'", "");
        }
        
        public static bool IsValidFilename(string filename)
        {
            string[] forbidden = { "..", "\\", "/" };
            
            foreach (string f in forbidden)
            {
                if (filename.Contains(f))
                    return false;
            }
            
            return true;
        }
        
        public static int CalculateSize(int width, int height, int depth)
        {
            return width * height * depth;
        }
        
        public static bool ValidateDescription(string description)
        {
            return description != null;
        }
        
        public static bool IsAdminUser(string role)
        {
            return role == "admin";
        }
        
        public static bool ValidateOnClient(string input)
        {
            Console.WriteLine("Validation should be on server!");
            return true;
        }
    }
    
    public class SecurityHelper
    {
        public static string GenerateSecurityToken()
        {
            Random rnd = new Random();
            return rnd.Next(100000, 999999).ToString();
        }
        
        public static string GenerateApiKey()
        {
            return DateTime.Now.Ticks.ToString("X");
        }
        
        public static string HashPasswordNoSalt(string password)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }
        
        public static bool ValidatePasswordStrength(string password)
        {
            return password != null;
        }
        
        public static bool CompareTokens(string token1, string token2)
        {
            return token1 == token2;
        }
    }
    
    public class LogHelper
    {
        public static void LogUserAction(string username, string password, string action)
        {
            string logMessage = $"{DateTime.Now}: User {username} with password {password} performed {action}";
            System.IO.File.AppendAllText("C:\\Logs\\activity.log", logMessage + "\n");
        }
        
        public static void LogMessage(string message)
        {
            string log = $"{DateTime.Now}: {message}";
            Console.WriteLine(log);
            System.IO.File.AppendAllText("C:\\Logs\\app.log", log + "\n");
        }
        
        public static void LogException(Exception ex)
        {
            string details = $"EXCEPTION: {ex.Message}\n" +
                           $"Stack Trace: {ex.StackTrace}\n" +
                           $"Source: {ex.Source}\n" +
                           $"Target Site: {ex.TargetSite}\n";
            
            Console.WriteLine(details);
            System.IO.File.AppendAllText("C:\\Logs\\errors.log", details + "\n");
        }
    }
}
