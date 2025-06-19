namespace SecureFileStorage.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SecureFileStorage.Core.Entities;
using SecureFileStorage.Core.Interfaces;
using SecureFileStorage.Infrastructure.Data;

public class KeyRepository : IKeyRepository
{
    private readonly SecureStorageDbContext _context;

    public KeyRepository(SecureStorageDbContext context)
    {
        _context = context;
    }

    public async Task<EncryptedKey> GetByIdAsync(string keyId)
    {
        return await _context.EncryptedKeys
            .FirstOrDefaultAsync(k => k.KeyId == keyId);
    }

    public async Task<List<EncryptedKey>> GetKeysForRotationAsync(int days)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-days);
        return await _context.EncryptedKeys
            .Where(k => k.Status == KeyStatus.Active && k.CreatedAt < cutoffDate)
            .ToListAsync();
    }

    public async Task<EncryptedKey> CreateAsync(EncryptedKey key)
    {
        _context.EncryptedKeys.Add(key);
        await _context.SaveChangesAsync();
        return key;
    }

    public async Task<EncryptedKey> UpdateAsync(EncryptedKey key)
    {
        _context.EncryptedKeys.Update(key);
        await _context.SaveChangesAsync();
        return key;
    }
}