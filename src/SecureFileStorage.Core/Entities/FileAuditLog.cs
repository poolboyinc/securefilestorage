namespace SecureFileStorage.Core.Entities;

public class FileAuditLog
{
    public Guid Id { get; set; }
    public Guid FileId { get; set; }
    public SecureFile File { get; set; }
    public Guid UserId { get; set; }
    public User User { get; set; }
    public string Action { get; set; }
    public string Details { get; set; }
    public string IpAddress { get; set; }
    public string UserAgent { get; set; }
    public DateTime Timestamp { get; set; }
}