namespace AIRegulationInterpreter.DocumentService.Services.Interfaces
{
    public interface IFileStorageService
    {
        /// <summary>
        /// Saves file to disk and returns the relative file path
        /// </summary>
        Task<(string FilePath, long FileSize)> SaveFileAsync(string documentTitle, string version, byte[] fileData, string fileName);
        
        /// <summary>
        /// Reads file from disk
        /// </summary>
        Task<byte[]?> ReadFileAsync(string filePath);
        
        /// <summary>
        /// Deletes file from disk
        /// </summary>
        Task<bool> DeleteFileAsync(string filePath);
        
        /// <summary>
        /// Checks if file exists
        /// </summary>
        Task<bool> FileExistsAsync(string filePath);
        
        /// <summary>
        /// Gets the full physical path for a relative path
        /// </summary>
        string GetFullPath(string relativePath);
    }
}
