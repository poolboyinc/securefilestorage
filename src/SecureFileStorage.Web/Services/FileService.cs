using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using SecureFileStorage.Core.DTOs; 

namespace SecureFileStorage.Web.Services;

public class FileService : IFileService
{
    private readonly HttpClient _httpClient;

    private readonly JsonSerializerOptions _jsonOptions;

    public FileService(HttpClient httpClient ) 
    {
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task<List<FileDto>> GetUserFilesAsync()
    {
        
        var response = await _httpClient.GetAsync("api/securefiles");
        response.EnsureSuccessStatusCode();
        
        var json = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<FileDto>>(json, _jsonOptions) ?? new List<FileDto>();
    }

    public async Task<byte[]> DownloadFileAsync(Guid fileId)
    {
        
        var response = await _httpClient.GetAsync($"api/securefiles/{fileId}/download");
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadAsByteArrayAsync();
    }

    public async Task UploadFileAsync(Stream fileStream, string fileName, string contentType, Action<int>? progressCallback = null)
    {
        
        using var content = new MultipartFormDataContent();
        using var streamContent = new StreamContent(fileStream);
        
        streamContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        content.Add(streamContent, "file", fileName);
        
        progressCallback?.Invoke(50); // Simulate progress
        
        var response = await _httpClient.PostAsync("api/securefiles/upload", content);
        response.EnsureSuccessStatusCode();
        
        progressCallback?.Invoke(100);
    }

    public async Task ShareFileAsync(Guid fileId, string targetEmail, string permissions, DateTime expiresAt)
    {
        var shareRequest = new
        {
            TargetEmail = targetEmail, // Change to use email directly
            Permissions = permissions,
            ExpiresAt = expiresAt
        };
        
        var json = JsonSerializer.Serialize(shareRequest, _jsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync($"api/securefiles/{fileId}/share", content);
        response.EnsureSuccessStatusCode();
    }

    public async Task DeleteFileAsync(Guid fileId)
    {
        var response = await _httpClient.DeleteAsync($"api/securefiles/{fileId}");
        response.EnsureSuccessStatusCode();
    }
    
}