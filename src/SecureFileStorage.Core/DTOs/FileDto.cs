namespace SecureFileStorage.Core.DTOs;

public class FileDto
{
    public Guid Id { get; set; }
    public string OriginalFileName { get; set; }
    public long FileSize { get; set; }
    public string ContentType { get; set; }
    public DateTime UploadedAt { get; set; }
    public bool IsShared { get; set; }
    public List<string> SharedWith { get; set; }
}