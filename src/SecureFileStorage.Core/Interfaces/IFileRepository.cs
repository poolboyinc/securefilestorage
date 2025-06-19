namespace SecureFileStorage.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SecureFileStorage.Core.Entities;

public interface IFileRepository
{
    Task<SecureFile> GetByIdAsync(Guid id);
    Task<List<SecureFile>> GetUserFilesAsync(Guid userId);
    Task<List<SecureFile>> GetFilesByKeyIdAsync(string keyId);
    Task<SecureFile> CreateAsync(SecureFile file);
    Task<SecureFile> UpdateAsync(SecureFile file);
    Task<bool> DeleteAsync(Guid id);
}