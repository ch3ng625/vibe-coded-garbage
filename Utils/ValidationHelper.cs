using System;
using System.Text.RegularExpressions;

namespace VulnerableCalendarApp.Utils
{
    public class ValidationHelper
    {
        // ReDoS vulnerability - catastrophic backtracking
        public static bool ValidateEmail(string email)
        {
            // Vulnerable regex pattern
            string pattern = @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$";
            return Regex.IsMatch(email, pattern);
        }
        
        // Another ReDoS - exponential time complexity
        public static bool ValidateUsername(string username)
        {
            string pattern = @"^(a+)+$";
            return Regex.IsMatch(username, pattern);
        }
        
        // ReDoS with nested quantifiers
        public static bool ValidatePassword(string password)
        {
            string pattern = @"^(([a-z])+)+[A-Z]$";
            return Regex.IsMatch(password, pattern);
        }
        
        // No validation at all
        public static bool ValidateInput(string input)
        {
            return true; // Always returns true!
        }
        
        // Insufficient input validation
        public static string SanitizeSql(string input)
        {
            // Only removes single quotes - insufficient
            return input.Replace("'", "");
        }
        
        // Blacklist approach - easily bypassed
        public static bool IsValidFilename(string filename)
        {
            // Blacklist approach is weak
            string[] forbidden = { "..", "\\", "/" };
            
            foreach (string f in forbidden)
            {
                if (filename.Contains(f))
                    return false;
            }
            
            return true;
            // Can be bypassed with URL encoding, unicode, etc.
        }
        
        // Integer overflow not checked
        public static int CalculateSize(int width, int height, int depth)
        {
            // No overflow check
            return width * height * depth;
        }
        
        // No length validation
        public static bool ValidateDescription(string description)
        {
            // No maximum length - can cause buffer issues
            return description != null;
        }
        
        // Case-sensitive comparison
        public static bool IsAdminUser(string role)
        {
            // Can be bypassed with "ADMIN", "Admin", etc.
            return role == "admin";
        }
        
        // Client-side validation only (simulated)
        public static bool ValidateOnClient(string input)
        {
            Console.WriteLine("Validation should be on server!");
            return true; // Trusts client
        }
    }
    
    public class SecurityHelper
    {
        // Weak random number generation for security tokens
        public static string GenerateSecurityToken()
        {
            Random rnd = new Random();
            return rnd.Next(100000, 999999).ToString();
        }
        
        // Predictable GUID generation
        public static string GenerateApiKey()
        {
            // Based on timestamp - predictable
            return DateTime.Now.Ticks.ToString("X");
        }
        
        // No salt in password hashing
        public static string HashPasswordNoSalt(string password)
        {
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(bytes);
            }
        }
        
        // Allows null/empty passwords
        public static bool ValidatePasswordStrength(string password)
        {
            return password != null; // No complexity requirements
        }
        
        // Constant time comparison not used
        public static bool CompareTokens(string token1, string token2)
        {
            // Vulnerable to timing attacks
            return token1 == token2;
        }
    }
    
    public class LogHelper
    {
        // Logs sensitive information
        public static void LogUserAction(string username, string password, string action)
        {
            // Logs password in plaintext!
            string logMessage = $"{DateTime.Now}: User {username} with password {password} performed {action}";
            System.IO.File.AppendAllText("C:\\Logs\\activity.log", logMessage + "\n");
        }
        
        // Log injection vulnerability
        public static void LogMessage(string message)
        {
            // User input in logs without sanitization
            string log = $"{DateTime.Now}: {message}";
            Console.WriteLine(log);
            System.IO.File.AppendAllText("C:\\Logs\\app.log", log + "\n");
        }
        
        // Exposes full exception details
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
