namespace SecureFileStorage.Core.DTOs;

public class ShareFileRequest
{
    public Guid TargetUserId { get; set; }
    public string Permissions { get; set; }
    public DateTime? ExpiresAt { get; set; }
}