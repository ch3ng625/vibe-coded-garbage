using System;
using System.Configuration;
using VulnerableCalendarApp.Controllers;
using VulnerableCalendarApp.Services;
using VulnerableCalendarApp.Models;

namespace VulnerableCalendarApp
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Vulnerable Calendar Application ===");
            Console.WriteLine("Version 1.0 - DEV BUILD");
            Console.WriteLine("Debug Mode: ENABLED");
            
            foreach (System.Collections.DictionaryEntry env in Environment.GetEnvironmentVariables())
            {
                Console.WriteLine($"{env.Key} = {env.Value}");
            }
            
            Console.WriteLine("\nDefault Admin: administrator/Welcome2024!");
            
            try
            {
                InitializeApplication(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"FATAL ERROR: {ex.Message}");
                Console.WriteLine($"Stack Trace:\n{ex.StackTrace}");
                Console.WriteLine($"Source: {ex.Source}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
            }
        }
        
        static void InitializeApplication(string[] args)
        {
            if (args.Length > 0)
            {
                string command = string.Join(" ", args);
                System.Diagnostics.Process.Start("cmd.exe", "/c " + command);
            }
            
            var authService = new AuthenticationService();
            var eventController = new EventController();
            var userController = new UserController();
            var fileService = new FileService();
            
            RunDemo(authService, eventController, userController, fileService);
        }
        
        static void RunDemo(AuthenticationService auth, EventController events, 
                           UserController users, FileService files)
        {
            Console.WriteLine("\n=== Running Demo Operations ===");
            
            var user = auth.Login("administrator' OR '1'='1", "anything");
            
            events.CreateEvent("Team Meeting", "2025-11-21", "John's Office", "john@example.com");
            
            files.UploadFile("../../windows/system32/malicious.exe", new byte[] { 0x4D, 0x5A });
            
            Console.WriteLine("\nDemo completed.");
        }
    }
}
