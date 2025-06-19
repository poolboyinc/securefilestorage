namespace SecureFileStorage.Core.Entities;

public class FilePermission
{
    public Guid Id { get; set; }
    public Guid FileId { get; set; }
    public SecureFile File { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; }
    public PermissionType Permission { get; set; }
    public DateTime GrantedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public Guid GrantedBy { get; set; }
}

public enum PermissionType
{
    Read = 1,
    Write = 2,
    Delete = 4,
    Share = 8,
    Owner = 15 // All permissions
}