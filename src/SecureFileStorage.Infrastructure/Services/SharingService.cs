using System.Security.Cryptography;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using SecureFileStorage.Core.Interfaces;

namespace SecureFileStorage.Infrastructure.Services;

public class SharingService : ISharingService
{
    private readonly IDistributedCache _cache;
    private readonly IPermissionService _permissionService;

    public SharingService(IDistributedCache cache, IPermissionService permissionService)
    {
        _cache = cache;
        _permissionService = permissionService;
    }

    public async Task<string> CreateShareLinkAsync(Guid fileId, Guid targetUserId, string permissions, DateTime? expiresAt)
    {
        var token = GenerateSecureToken();
        
        var shareData = new ShareData
        {
            FileId = fileId,
            TargetUserId = targetUserId,
            Permissions = permissions,
            ExpiresAt = expiresAt ?? DateTime.UtcNow.AddDays(7)
        };

        await _cache.SetStringAsync(
            $"share:{token}", 
            JsonConvert.SerializeObject(shareData),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpiration = shareData.ExpiresAt
            });

        await _permissionService.GrantPermissionAsync(fileId, targetUserId, permissions, expiresAt);

        return token;
    }

    public async Task<bool> ValidateShareTokenAsync(string token)
    {
        var data = await _cache.GetStringAsync($"share:{token}");
        if (string.IsNullOrEmpty(data)) return false;

        var shareData = JsonConvert.DeserializeObject<ShareData>(data);
        return shareData.ExpiresAt > DateTime.UtcNow;
    }

    private string GenerateSecureToken()
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[32];
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");
    }

    private class ShareData
    {
        public Guid FileId { get; set; }
        public Guid TargetUserId { get; set; }
        public string Permissions { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}