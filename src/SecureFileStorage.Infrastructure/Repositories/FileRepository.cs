namespace SecureFileStorage.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SecureFileStorage.Core.Entities;
using SecureFileStorage.Core.Interfaces;
using SecureFileStorage.Infrastructure.Data;

public class FileRepository : IFileRepository
{
    private readonly SecureStorageDbContext _context;

    public FileRepository(SecureStorageDbContext context)
    {
        _context = context;
    }

    public async Task<SecureFile> GetByIdAsync(Guid id)
    {
        return await _context.SecureFiles
            .Include(f => f.Owner)
            .Include(f => f.Permissions)
            .Include(f => f.AuditLogs)
            .FirstOrDefaultAsync(f => f.Id == id && !f.IsDeleted);
    }

    public async Task<List<SecureFile>> GetUserFilesAsync(Guid userId)
    {
        return await _context.SecureFiles
            .Where(f => f.OwnerId == userId && !f.IsDeleted)
            .OrderByDescending(f => f.UploadedAt)
            .ToListAsync();
    }

    public async Task<List<SecureFile>> GetFilesByKeyIdAsync(string keyId)
    {
        return await _context.SecureFiles
            .Where(f => f.KeyId == keyId && !f.IsDeleted)
            .ToListAsync();
    }

    public async Task<SecureFile> CreateAsync(SecureFile file)
    {
        _context.SecureFiles.Add(file);
        await _context.SaveChangesAsync();
        return file;
    }

    public async Task<SecureFile> UpdateAsync(SecureFile file)
    {
        _context.SecureFiles.Update(file);
        await _context.SaveChangesAsync();
        return file;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var file = await _context.SecureFiles.FindAsync(id);
        if (file == null) return false;
        
        file.IsDeleted = true;
        file.DeletedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return true;
    }
}