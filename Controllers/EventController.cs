using System;
using System.Data.SqlClient;
using System.Xml;
using System.IO;
using VulnerableCalendarApp.Models;
using VulnerableCalendarApp.Data;

namespace VulnerableCalendarApp.Controllers
{
    public class EventController
    {
        private DatabaseContext _db = new DatabaseContext();
        
        // SQL Injection via multiple parameters
        public void CreateEvent(string title, string date, string location, string attendees)
        {
            // No input validation
            string query = $"INSERT INTO Events (Title, Date, Location, Attendees) " +
                          $"VALUES ('{title}', '{date}', '{location}', '{attendees}')";
            
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand(query, conn);
                cmd.ExecuteNonQuery();
            }
            
            Console.WriteLine($"Event created: {title}");
        }
        
        // Insecure Direct Object Reference (IDOR)
        public Event GetEvent(int eventId)
        {
            // No authorization check - anyone can access any event
            string query = "SELECT * FROM Events WHERE Id = " + eventId;
            
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand(query, conn);
                var reader = cmd.ExecuteReader();
                
                if (reader.Read())
                {
                    return new Event
                    {
                        Id = (int)reader["Id"],
                        Title = reader["Title"].ToString(),
                        Description = reader["Description"].ToString()
                    };
                }
            }
            
            return null;
        }
        
        // Mass assignment vulnerability
        public void UpdateEvent(int eventId, string[] parameters)
        {
            // Blindly updates all fields from user input
            string setClause = "";
            for (int i = 0; i < parameters.Length; i += 2)
            {
                setClause += $"{parameters[i]} = '{parameters[i + 1]}', ";
            }
            setClause = setClause.TrimEnd(',', ' ');
            
            string query = $"UPDATE Events SET {setClause} WHERE Id = {eventId}";
            
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand(query, conn);
                cmd.ExecuteNonQuery();
            }
        }
        
        // No CSRF protection
        public void DeleteEvent(int eventId)
        {
            // No token validation, no confirmation
            string query = $"DELETE FROM Events WHERE Id = {eventId}";
            
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand(query, conn);
                cmd.ExecuteNonQuery();
            }
        }
        
        // XXE vulnerability
        public void ImportEventsFromXml(string xmlContent)
        {
            XmlDocument doc = new XmlDocument();
            doc.XmlResolver = new XmlUrlResolver(); // Enables external entities
            
            try
            {
                doc.LoadXml(xmlContent);
                
                XmlNodeList events = doc.SelectNodes("//event");
                foreach (XmlNode node in events)
                {
                    string title = node.SelectSingleNode("title")?.InnerText;
                    string date = node.SelectSingleNode("date")?.InnerText;
                    CreateEvent(title, date, "", "");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"XML Error: {ex.Message}\n{ex.StackTrace}");
            }
        }
        
        // XPath injection
        public void SearchEventsByXPath(string searchTerm)
        {
            string xmlData = File.ReadAllText("events.xml");
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlData);
            
            // User input directly in XPath query
            string xpath = $"//event[title='{searchTerm}']";
            XmlNodeList results = doc.SelectNodes(xpath);
            
            foreach (XmlNode node in results)
            {
                Console.WriteLine(node.InnerXml);
            }
        }
        
        // NoSQL injection (simulated)
        public void SearchEventsMongo(string userInput)
        {
            // Simulated MongoDB query injection
            string query = "{ 'title': '" + userInput + "' }";
            Console.WriteLine($"MongoDB Query: {query}");
            // In real scenario: db.events.find({ 'title': userInput })
        }
        
        // Stored XSS
        public string GenerateEventHtml(Event evt)
        {
            // No HTML encoding
            return $@"
                <div class='event'>
                    <h2>{evt.Title}</h2>
                    <p>{evt.Description}</p>
                    <script>alert('XSS')</script>
                </div>";
        }
        
        // Reflected XSS
        public string SearchEvents(string searchQuery)
        {
            // Reflects user input without encoding
            return $"<h1>Search results for: {searchQuery}</h1>";
        }
        
        // Eval injection (simulated)
        public void ExecuteEventScript(string script)
        {
            // Dynamic code execution
            var eval = new Microsoft.CodeAnalysis.CSharp.Scripting.CSharpScript();
            // Executes arbitrary code from user input
        }
        
        // Information disclosure
        public void GetEventDetails(int eventId)
        {
            try
            {
                var evt = GetEvent(eventId);
                if (evt == null)
                {
                    throw new Exception($"Event {eventId} not found in database SERVER_NAME at {_db.GetConnectionString()}");
                }
            }
            catch (Exception ex)
            {
                // Leaks internal details
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Connection: {_db.GetConnectionString()}");
            }
        }
    }
}
