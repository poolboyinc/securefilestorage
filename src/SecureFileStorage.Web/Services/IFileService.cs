using SecureFileStorage.Core.DTOs;

namespace SecureFileStorage.Web.Services;

public interface IFileService
{
    Task<List<FileDto>> GetUserFilesAsync();
    Task<byte[]> DownloadFileAsync(Guid fileId);
    Task UploadFileAsync(Stream fileStream, string fileName, string contentType, Action<int>? progressCallback = null);
    Task ShareFileAsync(Guid fileId, string targetEmail, string permissions, DateTime expiresAt);
    Task DeleteFileAsync(Guid fileId);
}