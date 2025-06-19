namespace SecureFileStorage.Core.Interfaces;
using System;
using System.Threading.Tasks;

public interface ISharingService
{
    Task<string> CreateShareLinkAsync(Guid fileId, Guid targetUserId, string permissions, DateTime? expiresAt);
    Task<bool> ValidateShareTokenAsync(string token);
}