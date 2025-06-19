namespace SecureFileStorage.Infrastructure.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SecureFileStorage.Core.Entities;
using SecureFileStorage.Core.Interfaces;
using SecureFileStorage.Infrastructure.Data;

public class PermissionService : IPermissionService
{
    private readonly SecureStorageDbContext _context;

    public PermissionService(SecureStorageDbContext context)
    {
        _context = context;
    }

    public async Task<bool> CanAccessFileAsync(Guid userId, Guid fileId)
    {
        var file = await _context.SecureFiles
            .Include(f => f.Permissions)
            .FirstOrDefaultAsync(f => f.Id == fileId);

        if (file == null || file.IsDeleted) return false;
        if (file.OwnerId == userId) return true;

        var permission = file.Permissions?.FirstOrDefault(p => p.UserId == userId);
        if (permission == null) return false;

        if (permission.ExpiresAt.HasValue && permission.ExpiresAt < DateTime.UtcNow)
            return false;

        return permission.Permission.HasFlag(PermissionType.Read);
    }

    public async Task<bool> CanDeleteFileAsync(Guid userId, Guid fileId)
    {
        var file = await _context.SecureFiles.FindAsync(fileId);
        return file != null && file.OwnerId == userId;
    }

    public async Task<bool> CanShareFileAsync(Guid userId, Guid fileId)
    {
        var file = await _context.SecureFiles
            .Include(f => f.Permissions)
            .FirstOrDefaultAsync(f => f.Id == fileId);

        if (file == null) return false;
        if (file.OwnerId == userId) return true;

        var permission = file.Permissions?.FirstOrDefault(p => p.UserId == userId);
        return permission != null && permission.Permission.HasFlag(PermissionType.Share);
    }

    public async Task GrantPermissionAsync(Guid fileId, Guid userId, string permission, DateTime? expiresAt)
    {
        var permissionType = Enum.Parse<PermissionType>(permission);

        var filePermission = new FilePermission
        {
            Id = Guid.NewGuid(),
            FileId = fileId,
            UserId = userId,
            Permission = permissionType,
            GrantedAt = DateTime.UtcNow,
            ExpiresAt = expiresAt,
            GrantedBy = userId
        };

        _context.FilePermissions.Add(filePermission);
        await _context.SaveChangesAsync();
    }
}