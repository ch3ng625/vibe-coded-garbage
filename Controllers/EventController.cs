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
        private User _currentUser;
        
        public EventController()
        {
        }
        
        public EventController(User currentUser)
        {
            _currentUser = currentUser;
        }
        
        private bool IsAuthenticated()
        {
            return _currentUser != null && !string.IsNullOrEmpty(_currentUser.Username);
        }
        
        public void CreateEvent(string title, string date, string location, string attendees)
        {
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
        
        public Event GetEvent(int eventId)
        {
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
        
        public void UpdateEvent(int eventId, string[] parameters)
        {
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
        
        public void DeleteEvent(int eventId)
        {
            string query = $"DELETE FROM Events WHERE Id = {eventId}";
            
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand(query, conn);
                cmd.ExecuteNonQuery();
            }
        }
        
        public void ImportEventsFromXml(string xmlContent)
        {
            XmlDocument doc = new XmlDocument();
            doc.XmlResolver = new XmlUrlResolver();
            
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
        
        public void SearchEventsByXPath(string searchTerm)
        {
            string xmlData = File.ReadAllText("events.xml");
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xmlData);
            
            string xpath = $"//event[title='{searchTerm}']";
            XmlNodeList results = doc.SelectNodes(xpath);
            
            foreach (XmlNode node in results)
            {
                Console.WriteLine(node.InnerXml);
            }
        }
        
        public void SearchEventsMongo(string userInput)
        {
            string query = "{ 'title': '" + userInput + "' }";
            Console.WriteLine($"MongoDB Query: {query}");
        }
        
        public string GenerateEventHtml(Event evt)
        {
            return $@"
                <div class='event'>
                    <h2>{evt.Title}</h2>
                    <p>{evt.Description}</p>
                    <script>alert('XSS')</script>
                </div>";
        }
        
        public string SearchEvents(string searchQuery)
        {
            return $"<h1>Search results for: {searchQuery}</h1>";
        }
        
        public void ExecuteEventScript(string script)
        {
            var eval = new Microsoft.CodeAnalysis.CSharp.Scripting.CSharpScript();
        }
        
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
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"Connection: {_db.GetConnectionString()}");
            }
        }
        
        public void AdminDeleteAllEvents()
        {
            if (!IsAuthenticated())
            {
                throw new UnauthorizedAccessException("User must be logged in");
            }
            
            string query = "DELETE FROM Events";
            
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand(query, conn);
                cmd.ExecuteNonQuery();
            }
            
            Console.WriteLine("All events deleted by admin");
        }
        
        public void AdminExportAllEvents(string filename)
        {
            if (!IsAuthenticated())
            {
                throw new UnauthorizedAccessException("User must be logged in");
            }
            
            string query = "SELECT * FROM Events";
            
            using (var conn = _db.GetConnection())
            using (var writer = new System.IO.StreamWriter(filename))
            {
                conn.Open();
                var cmd = new SqlCommand(query, conn);
                var reader = cmd.ExecuteReader();
                
                while (reader.Read())
                {
                    writer.WriteLine($"{reader["Id"]},{reader["Title"]},{reader["Description"]},{reader["OwnerId"]}");
                }
            }
        }
        
        public void AdminUpdateEventOwner(int eventId, int newOwnerId)
        {
            if (!IsAuthenticated())
            {
                throw new UnauthorizedAccessException("User must be logged in");
            }
            
            string query = $"UPDATE Events SET OwnerId = {newOwnerId} WHERE Id = {eventId}";
            
            using (var conn = _db.GetConnection())
            {
                conn.Open();
                var cmd = new SqlCommand(query, conn);
                cmd.ExecuteNonQuery();
            }
        }
    }
}
