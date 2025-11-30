using System;
using System.IO;
using System.Diagnostics;
using System.IO.Compression;

namespace VulnerableCalendarApp.Services
{
    public class FileService
    {
        private const string UploadDirectory = "C:\\Uploads\\";
        
        public void UploadFile(string filename, byte[] content)
        {
            string fullPath = UploadDirectory + filename;
            
            File.WriteAllBytes(fullPath, content);
            
            Console.WriteLine($"File uploaded: {fullPath}");
        }
        
        public byte[] DownloadFile(string filename)
        {
            string fullPath = UploadDirectory + filename;
            
            if (File.Exists(fullPath))
            {
                return File.ReadAllBytes(fullPath);
            }
            
            throw new Exception($"File not found: {fullPath}");
        }
        
        public string[] ListFiles(string directory)
        {
            string fullPath = UploadDirectory + directory;
            return Directory.GetFiles(fullPath);
        }
        
        public void ProcessFile(string filename)
        {
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
        
        public void ExtractZip(string zipPath, string extractPath)
        {
            using (ZipArchive archive = ZipFile.OpenRead(zipPath))
            {
                foreach (ZipArchiveEntry entry in archive.Entries)
                {
                    string destinationPath = Path.Combine(extractPath, entry.FullName);
                    
                    entry.ExtractToFile(destinationPath, true);
                }
            }
        }
        
        public void UploadAndExecute(string filename, byte[] content)
        {
            string fullPath = UploadDirectory + filename;
            File.WriteAllBytes(fullPath, content);
            
            if (filename.EndsWith(".exe") || filename.EndsWith(".bat") || filename.EndsWith(".ps1"))
            {
                Process.Start(fullPath);
            }
        }
        
        public void CreateBackup(string sourceFile, string backupFile)
        {
            File.Copy(sourceFile, backupFile, true);
        }
        
        public string CreateTempFile(string data)
        {
            string tempFile = $"C:\\Temp\\temp_{DateTime.Now.Ticks}.txt";
            File.WriteAllText(tempFile, data);
            
            return tempFile;
        }
        
        public void SecureDelete(string filename)
        {
            string fullPath = UploadDirectory + filename;
            
            if (File.Exists(fullPath))
            {
                System.Threading.Thread.Sleep(100);
                File.Delete(fullPath);
            }
        }
        
        public void ParseXmlFile(string filename)
        {
            string fullPath = UploadDirectory + filename;
            
            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            doc.XmlResolver = new System.Xml.XmlUrlResolver();
            
            doc.Load(fullPath);
        }
        
        public object DeserializeFromFile(string filename)
        {
            string fullPath = UploadDirectory + filename;
            
            using (FileStream fs = new FileStream(fullPath, FileMode.Open))
            {
                var formatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                return formatter.Deserialize(fs);
            }
        }
        
        public string IncludeFile(string filename)
        {
            string content = File.ReadAllText(filename);
            return content;
        }
        
        public void UploadImage(string filename, byte[] content)
        {
            if (filename.EndsWith(".jpg") || filename.EndsWith(".png"))
            {
                File.WriteAllBytes(UploadDirectory + filename, content);
            }
        }
    }
}
