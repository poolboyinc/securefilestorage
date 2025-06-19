namespace SecureFileStorage.Core.Entities;

public class UserAuditLog
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; } 
    public string EventType { get; set; } 
    public string Details { get; set; } 
    public string IpAddress { get; set; }
    public string UserAgent { get; set; }
    public DateTime Timestamp { get; set; }
    
    public User User { get; set; }
}