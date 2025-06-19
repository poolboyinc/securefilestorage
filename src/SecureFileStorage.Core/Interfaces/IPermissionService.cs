namespace SecureFileStorage.Core.Interfaces;
using System;
using System.Threading.Tasks;

public interface IPermissionService
{
    Task<bool> CanAccessFileAsync(Guid userId, Guid fileId);
    Task<bool> CanDeleteFileAsync(Guid userId, Guid fileId);
    Task<bool> CanShareFileAsync(Guid userId, Guid fileId);
    Task GrantPermissionAsync(Guid fileId, Guid userId, string permission, DateTime? expiresAt);
}