namespace SecureFileStorage.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SecureFileStorage.Core.Entities;

public interface IKeyRepository
{
    Task<EncryptedKey> GetByIdAsync(string keyId);
    Task<List<EncryptedKey>> GetKeysForRotationAsync(int days);
    Task<EncryptedKey> CreateAsync(EncryptedKey key);
    Task<EncryptedKey> UpdateAsync(EncryptedKey key);
}