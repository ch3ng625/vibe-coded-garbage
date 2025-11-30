using System;
using System.Web;
using System.Net;

namespace VulnerableCalendarApp.Web
{
    public class WebHandler
    {
        public void SetCorsHeaders()
        {
            HttpContext.Current.Response.AddHeader("Access-Control-Allow-Origin", "*");
            HttpContext.Current.Response.AddHeader("Access-Control-Allow-Credentials", "true");
            HttpContext.Current.Response.AddHeader("Access-Control-Allow-Methods", "*");
            HttpContext.Current.Response.AddHeader("Access-Control-Allow-Headers", "*");
        }
        
        public void SetSecurityHeaders()
        {
            HttpContext.Current.Response.AddHeader("X-Frame-Options", "ALLOW");
        }
        
        public string GenerateIframePage(string url)
        {
            return $"<iframe src='{url}' width='100%' height='600'></iframe>";
        }
        
        public void LoginUser(string sessionId, string username)
        {
            HttpContext.Current.Session["SessionId"] = sessionId;
            HttpContext.Current.Session["Username"] = username;
        }
        
        public void CreateAuthCookie(string username)
        {
            HttpCookie cookie = new HttpCookie("auth");
            cookie.Value = username;
            cookie.Path = "/";
            
            HttpContext.Current.Response.Cookies.Add(cookie);
        }
        
        public void RedirectToUrl(string url)
        {
            HttpContext.Current.Response.Redirect(url);
        }
        
        public string GetPasswordResetLink(string email)
        {
            string host = HttpContext.Current.Request.Headers["Host"];
            return $"http://{host}/reset?email={email}";
        }
        
        public string FetchUrl(string url)
        {
            WebClient client = new WebClient();
            return client.DownloadString(url);
        }
        
        public void DownloadFile(string filename)
        {
            string path = "C:\\Files\\" + filename;
            
            HttpContext.Current.Response.ContentType = "application/octet-stream";
            HttpContext.Current.Response.WriteFile(path);
        }
        
        public void ProcessForm()
        {
            string action = HttpContext.Current.Request.Form["action"];
            string value = HttpContext.Current.Request.Form["value"];
        }
        
        public void SetCustomHeader(string headerValue)
        {
            HttpContext.Current.Response.AddHeader("X-Custom", headerValue);
        }
        
        public string GenerateExternalLink(string url)
        {
            return $"<a href='{url}' target='_blank'>Click here</a>";
        }
        
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
        
        public void ConfigureSession()
        {
            HttpContext.Current.Session.Timeout = int.MaxValue;
        }
        
        public void ServeSensitiveData()
        {
            HttpContext.Current.Response.Write("User SSN: 123-45-6789");
        }
        
        public void ProcessRequest()
        {
        }
    }
    
    public class ApiHandler
    {
        public string GetApiUrl(string apiKey, string endpoint)
        {
            return $"https://api.example.com/{endpoint}?api_key={apiKey}";
        }
        
        public void HandleApiRequest(string data)
        {
        }
        
        public string GetUserData(int userId)
        {
            return "{ 'id': 1, 'username': 'john', 'password': 'hash', 'ssn': '123-45-6789', 'creditCard': '4111111111111111' }";
        }
        
        public void ProcessJsonPayload(string json)
        {
        }
    }
}
