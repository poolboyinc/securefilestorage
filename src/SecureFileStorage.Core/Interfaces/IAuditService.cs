namespace SecureFileStorage.Core.Interfaces;
using System;
using System.Threading.Tasks;

public interface IAuditService
{
    Task LogFileOperationAsync(Guid userId, Guid fileId, string operation);
    Task LogFileAccessAsync(Guid userId, Guid fileId, string operation);
    Task LogSecurityEventAsync(Guid userId, string eventType, string details);

    Task LogUserSecurityEventAsync(Guid userId, string eventType, string details);
}