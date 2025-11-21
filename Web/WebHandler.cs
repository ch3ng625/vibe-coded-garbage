using System;
using System.Web;
using System.Net;

namespace VulnerableCalendarApp.Web
{
    public class WebHandler
    {
        // CORS misconfiguration - allows all origins
        public void SetCorsHeaders()
        {
            // Overly permissive CORS
            HttpContext.Current.Response.AddHeader("Access-Control-Allow-Origin", "*");
            HttpContext.Current.Response.AddHeader("Access-Control-Allow-Credentials", "true");
            HttpContext.Current.Response.AddHeader("Access-Control-Allow-Methods", "*");
            HttpContext.Current.Response.AddHeader("Access-Control-Allow-Headers", "*");
        }
        
        // Missing security headers
        public void SetSecurityHeaders()
        {
            // Most security headers missing or misconfigured
            HttpContext.Current.Response.AddHeader("X-Frame-Options", "ALLOW"); // Should be DENY
            // Missing: Content-Security-Policy
            // Missing: X-Content-Type-Options
            // Missing: Strict-Transport-Security
        }
        
        // Clickjacking vulnerability
        public string GenerateIframePage(string url)
        {
            // Allows embedding in iframes
            return $"<iframe src='{url}' width='100%' height='600'></iframe>";
        }
        
        // Session fixation
        public void LoginUser(string sessionId, string username)
        {
            // Reuses session ID from user input
            HttpContext.Current.Session["SessionId"] = sessionId;
            HttpContext.Current.Session["Username"] = username;
        }
        
        // Cookie without Secure flag
        public void CreateAuthCookie(string username)
        {
            HttpCookie cookie = new HttpCookie("auth");
            cookie.Value = username;
            cookie.Path = "/";
            // Missing: cookie.Secure = true;
            // Missing: cookie.HttpOnly = true;
            // Missing: cookie.SameSite = SameSiteMode.Strict;
            
            HttpContext.Current.Response.Cookies.Add(cookie);
        }
        
        // Unvalidated redirect
        public void RedirectToUrl(string url)
        {
            // No validation - open redirect
            HttpContext.Current.Response.Redirect(url);
        }
        
        // Host header injection
        public string GetPasswordResetLink(string email)
        {
            // Uses user-controlled Host header
            string host = HttpContext.Current.Request.Headers["Host"];
            return $"http://{host}/reset?email={email}";
        }
        
        // Server-side request forgery via proxy
        public string FetchUrl(string url)
        {
            // No URL validation
            WebClient client = new WebClient();
            return client.DownloadString(url);
        }
        
        // Insecure direct download
        public void DownloadFile(string filename)
        {
            // No access control, path validation
            string path = "C:\\Files\\" + filename;
            
            HttpContext.Current.Response.ContentType = "application/octet-stream";
            HttpContext.Current.Response.WriteFile(path);
        }
        
        // Missing anti-CSRF token
        public void ProcessForm()
        {
            // No CSRF validation
            string action = HttpContext.Current.Request.Form["action"];
            string value = HttpContext.Current.Request.Form["value"];
            
            // Processes form without token validation
        }
        
        // HTTP response splitting
        public void SetCustomHeader(string headerValue)
        {
            // Allows newlines - response splitting
            HttpContext.Current.Response.AddHeader("X-Custom", headerValue);
        }
        
        // Tabnabbing vulnerability
        public string GenerateExternalLink(string url)
        {
            // Missing rel="noopener noreferrer"
            return $"<a href='{url}' target='_blank'>Click here</a>";
        }
        
        // Information disclosure via verbose errors
        public void HandleError(Exception ex)
        {
            HttpContext.Current.Response.StatusCode = 500;
            HttpContext.Current.Response.Write($@"
                <h1>Error</h1>
                <p>Message: {ex.Message}</p>
                <p>Stack Trace: {ex.StackTrace}</p>
                <p>Source: {ex.Source}</p>
            ");
        }
        
        // Session timeout not set
        public void ConfigureSession()
        {
            // Infinite session timeout
            HttpContext.Current.Session.Timeout = int.MaxValue;
        }
        
        // Cacheable sensitive data
        public void ServeSensitiveData()
        {
            // No cache-control headers for sensitive data
            HttpContext.Current.Response.Write("User SSN: 123-45-6789");
        }
        
        // Missing rate limiting
        public void ProcessRequest()
        {
            // No rate limiting - allows brute force, DoS
        }
    }
    
    public class ApiHandler
    {
        // API key exposed in URL
        public string GetApiUrl(string apiKey, string endpoint)
        {
            // API key in query string - logged in server logs
            return $"https://api.example.com/{endpoint}?api_key={apiKey}";
        }
        
        // No API versioning
        public void HandleApiRequest(string data)
        {
            // Breaking changes affect all clients
        }
        
        // Excessive data exposure
        public string GetUserData(int userId)
        {
            // Returns all user fields including sensitive ones
            return "{ 'id': 1, 'username': 'john', 'password': 'hash', 'ssn': '123-45-6789', 'creditCard': '4111111111111111' }";
        }
        
        // No request size limit
        public void ProcessJsonPayload(string json)
        {
            // Can receive unlimited data - DoS vector
        }
    }
}
