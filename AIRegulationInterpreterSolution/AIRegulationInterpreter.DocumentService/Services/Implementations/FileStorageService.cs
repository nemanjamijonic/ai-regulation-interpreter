using AIRegulationInterpreter.DocumentService.Services.Interfaces;

namespace AIRegulationInterpreter.DocumentService.Services.Implementations
{
    public class FileStorageService : IFileStorageService
    {
        private readonly string _baseDirectory;
        private const string DocumentsFolder = "Documents";

        public FileStorageService()
        {
            // PERZISTENTNA PUTANJA - ne nestaje pri restartu Service Fabric-a
            // Opcija 1: Fiksna putanja na C:\";
            _baseDirectory = @"C:\AIRegulationInterpreter\Documents";
            
            // Opcija 2: U project folderu (development)
            // var projectRoot = FindProjectRoot();
            // _baseDirectory = Path.Combine(projectRoot, DocumentsFolder);
            
            // Log the path for debugging
            Console.WriteLine($"?? FileStorageService initialized. PERSISTENT Base directory: {_baseDirectory}");
            
            // Create Documents folder if it doesn't exist
            if (!Directory.Exists(_baseDirectory))
            {
                Directory.CreateDirectory(_baseDirectory);
                Console.WriteLine($"? Created PERSISTENT Documents directory: {_baseDirectory}");
            }
            else
            {
                Console.WriteLine($"? PERSISTENT Documents directory already exists: {_baseDirectory}");
            }
        }

        public async Task<(string FilePath, long FileSize)> SaveFileAsync(string documentTitle, string version, byte[] fileData, string fileName)
        {
            try
            {
                Console.WriteLine($"?? SaveFileAsync called - Title: {documentTitle}, Version: {version}, FileName: {fileName}, FileSize: {fileData.Length} bytes");
                
                // Sanitize document title for folder name
                var sanitizedTitle = SanitizeFolderName(documentTitle);
                Console.WriteLine($"?? Sanitized title: {sanitizedTitle}");
                
                // Create folder structure: C:\AIRegulationInterpreter\Documents\DocumentTitle\
                var documentFolder = Path.Combine(_baseDirectory, sanitizedTitle);
                Console.WriteLine($"?? Document folder path: {documentFolder}");
                
                if (!Directory.Exists(documentFolder))
                {
                    Directory.CreateDirectory(documentFolder);
                    Console.WriteLine($"? Created document folder: {documentFolder}");
                }

                // Get file extension from original filename
                var extension = Path.GetExtension(fileName);
                
                // File name: version + extension (e.g., "1.0.pdf")
                var newFileName = $"{version}{extension}";
                var fullPath = Path.Combine(documentFolder, newFileName);
                Console.WriteLine($"?? PERSISTENT Full file path: {fullPath}");

                // Relative path for database storage
                var relativePath = Path.Combine(sanitizedTitle, newFileName);

                // Save file to disk
                await File.WriteAllBytesAsync(fullPath, fileData);
                Console.WriteLine($"? File saved successfully to PERSISTENT location: {fullPath}");

                // Verify file exists
                if (File.Exists(fullPath))
                {
                    var savedFileSize = new FileInfo(fullPath).Length;
                    Console.WriteLine($"? File verified - Size: {savedFileSize} bytes");
                    Console.WriteLine($"?? File will PERSIST after Service Fabric restart!");
                }
                else
                {
                    Console.WriteLine($"? ERROR: File not found after save: {fullPath}");
                }

                // Get file size
                var fileSize = fileData.Length;

                return (relativePath, fileSize);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? ERROR in SaveFileAsync: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw;
            }
        }

        public async Task<byte[]?> ReadFileAsync(string filePath)
        {
            var fullPath = GetFullPath(filePath);
            
            Console.WriteLine($"?? Reading file from PERSISTENT location: {fullPath}");
            
            if (!File.Exists(fullPath))
            {
                Console.WriteLine($"? File not found: {fullPath}");
                return null;
            }

            var data = await File.ReadAllBytesAsync(fullPath);
            Console.WriteLine($"? File read successfully - Size: {data.Length} bytes");
            return data;
        }

        public Task<bool> DeleteFileAsync(string filePath)
        {
            try
            {
                var fullPath = GetFullPath(filePath);
                
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                    Console.WriteLine($"??? File deleted from PERSISTENT storage: {fullPath}");
                    return Task.FromResult(true);
                }
                
                Console.WriteLine($"? File not found for deletion: {fullPath}");
                return Task.FromResult(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"? Error deleting file: {ex.Message}");
                return Task.FromResult(false);
            }
        }

        public Task<bool> FileExistsAsync(string filePath)
        {
            var fullPath = GetFullPath(filePath);
            var exists = File.Exists(fullPath);
            Console.WriteLine($"?? File exists check: {fullPath} = {exists}");
            return Task.FromResult(exists);
        }

        public string GetFullPath(string relativePath)
        {
            return Path.Combine(_baseDirectory, relativePath);
        }

        private string SanitizeFolderName(string folderName)
        {
            // Remove invalid characters for folder names
            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitized = string.Join("_", folderName.Split(invalidChars, StringSplitOptions.RemoveEmptyEntries));
            
            // Trim and limit length
            sanitized = sanitized.Trim();
            if (sanitized.Length > 100)
            {
                sanitized = sanitized.Substring(0, 100);
            }

            return sanitized;
        }

        // OPCIONO: Helper za development - pronalazi project root
        private string FindProjectRoot()
        {
            var currentDir = AppDomain.CurrentDomain.BaseDirectory;
            
            // Trazi folder koji sadrzi .csproj fajl
            while (!string.IsNullOrEmpty(currentDir))
            {
                if (Directory.GetFiles(currentDir, "*.csproj").Any())
                {
                    Console.WriteLine($"?? Found project root: {currentDir}");
                    return currentDir;
                }
                
                currentDir = Directory.GetParent(currentDir)?.FullName;
            }
            
            // Fallback
            return @"C:\AIRegulationInterpreter";
        }
    }
}
