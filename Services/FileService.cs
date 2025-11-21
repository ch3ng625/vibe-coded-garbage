using System;
using System.IO;
using System.Diagnostics;
using System.IO.Compression;

namespace VulnerableCalendarApp.Services
{
    public class FileService
    {
        private const string UploadDirectory = "C:\\Uploads\\";
        
        // Path traversal vulnerability
        public void UploadFile(string filename, byte[] content)
        {
            // No path validation - allows ../../../
            string fullPath = UploadDirectory + filename;
            
            // No file type restriction
            // No size limit
            File.WriteAllBytes(fullPath, content);
            
            Console.WriteLine($"File uploaded: {fullPath}");
        }
        
        // Arbitrary file read
        public byte[] DownloadFile(string filename)
        {
            // No authorization check
            // No path validation
            string fullPath = UploadDirectory + filename;
            
            if (File.Exists(fullPath))
            {
                return File.ReadAllBytes(fullPath);
            }
            
            // Error message reveals file system structure
            throw new Exception($"File not found: {fullPath}");
        }
        
        // Directory traversal in file listing
        public string[] ListFiles(string directory)
        {
            // No validation - can list any directory
            string fullPath = UploadDirectory + directory;
            return Directory.GetFiles(fullPath);
        }
        
        // Command injection via filename
        public void ProcessFile(string filename)
        {
            // Executes command with user-controlled filename
            string command = $"type {UploadDirectory}{filename}";
            
            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = "cmd.exe",
                Arguments = $"/c {command}",
                RedirectStandardOutput = true,
                UseShellExecute = false
            };
            
            Process proc = Process.Start(psi);
            string output = proc.StandardOutput.ReadToEnd();
            Console.WriteLine(output);
        }
        
        // Zip slip vulnerability
        public void ExtractZip(string zipPath, string extractPath)
        {
            // No validation of zip entry paths
            using (ZipArchive archive = ZipFile.OpenRead(zipPath))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    // Doesn't check for path traversal in entry names
                    string destinationPath = Path.Combine(extractPath, entry.FullName);
                    
                    // Can write outside intended directory
                    entry.ExtractToFile(destinationPath, true);
                }
            }
        }
        
        // Unrestricted file upload with execution
        public void UploadAndExecute(string filename, byte[] content)
        {
            // Uploads file
            string fullPath = UploadDirectory + filename;
            File.WriteAllBytes(fullPath, content);
            
            // Immediately executes it!
            if (filename.EndsWith(".exe") || filename.EndsWith(".bat") || filename.EndsWith(".ps1"))
            {
                Process.Start(fullPath);
            }
        }
        
        // Symlink attack vulnerability
        public void CreateBackup(string sourceFile, string backupFile)
        {
            // Doesn't check for symlinks
            File.Copy(sourceFile, backupFile, true);
        }
        
        // Temp file with predictable name
        public string CreateTempFile(string data)
        {
            // Predictable temp filename
            string tempFile = $"C:\\Temp\\temp_{DateTime.Now.Ticks}.txt";
            File.WriteAllText(tempFile, data);
            
            // File never cleaned up - resource leak
            return tempFile;
        }
        
        // Race condition in file operations
        public void SecureDelete(string filename)
        {
            string fullPath = UploadDirectory + filename;
            
            if (File.Exists(fullPath))
            {
                // Race condition: file could be modified between check and delete
                System.Threading.Thread.Sleep(100); // Simulates delay
                File.Delete(fullPath);
            }
        }
        
        // XML bomb / Billion laughs attack
        public void ParseXmlFile(string filename)
        {
            string fullPath = UploadDirectory + filename;
            
            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            doc.XmlResolver = new System.Xml.XmlUrlResolver();
            
            // No size limit, no entity expansion limit
            doc.Load(fullPath);
        }
        
        // Deserialization of untrusted data
        public object DeserializeFromFile(string filename)
        {
            string fullPath = UploadDirectory + filename;
            
            using (FileStream fs = new FileStream(fullPath, FileMode.Open))
            {
                var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                return formatter.Deserialize(fs); // Unsafe!
            }
        }
        
        // File inclusion vulnerability
        public string IncludeFile(string filename)
        {
            // Can include any file from filesystem
            string content = File.ReadAllText(filename);
            return content;
        }
        
        // Doesn't validate MIME types
        public void UploadImage(string filename, byte[] content)
        {
            // Only checks extension, not actual content
            if (filename.EndsWith(".jpg") || filename.EndsWith(".png"))
            {
                File.WriteAllBytes(UploadDirectory + filename, content);
            }
            // But .jpg.exe would bypass this check
        }
    }
}
