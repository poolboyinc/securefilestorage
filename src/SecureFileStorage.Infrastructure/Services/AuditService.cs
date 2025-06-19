using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;
using SecureFileStorage.Core.Entities;
using SecureFileStorage.Core.Interfaces;
using SecureFileStorage.Infrastructure.Data;

namespace SecureFileStorage.Infrastructure.Services;

public class AuditService : IAuditService
{
    private readonly SecureStorageDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditService(SecureStorageDbContext context, IHttpContextAccessor httpContextAccessor)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task LogFileOperationAsync(Guid userId, Guid fileId, string operation)
    {
        var log = new FileAuditLog
        {
            Id = Guid.NewGuid(),
            FileId = fileId,
            UserId = userId,
            Action = operation,
            Details = $"User performed {operation} on file",
            IpAddress = GetIpAddress(),
            UserAgent = GetUserAgent(),
            Timestamp = DateTime.UtcNow
        };

        _context.FileAuditLogs.Add(log);
        await _context.SaveChangesAsync();
    }

    public async Task LogFileAccessAsync(Guid userId, Guid fileId, string operation)
    {
        await LogFileOperationAsync(userId, fileId, operation);
    }


    public async Task LogSecurityEventAsync(Guid userId, string eventType, string details)
    {
        _context.FileAuditLogs.Add(new FileAuditLog 
        {
            Id = Guid.NewGuid(),
            FileId = Guid.Empty, 
            UserId = userId,
            Action = eventType,
            Details = details,
            IpAddress = GetIpAddress(),
            UserAgent = GetUserAgent(),
            Timestamp = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();
    }
    
    public async Task LogUserSecurityEventAsync(Guid userId, string eventType, string details)
    {
        var log = new UserAuditLog 
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            EventType = eventType, 
            Details = details,
            IpAddress = GetIpAddress(),
            UserAgent = GetUserAgent(),
            Timestamp = DateTime.UtcNow
        };

        _context.UserAuditLogs.Add(log); 
        await _context.SaveChangesAsync();
    }

    private string GetIpAddress()
    {
        return _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString() ?? "Unknown";
    }

    private string GetUserAgent()
    {
        return _httpContextAccessor.HttpContext?.Request?.Headers["User-Agent"].ToString() ?? "Unknown";
    }
}