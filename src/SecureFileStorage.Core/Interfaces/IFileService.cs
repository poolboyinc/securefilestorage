namespace SecureFileStorage.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SecureFileStorage.Core.DTOs;
using SecureFileStorage.Core.Entities;

public interface IFileService
{
    Task<SecureFile> SaveFileMetadataAsync(SecureFile file);
    Task<SecureFile> GetFileMetadataAsync(Guid fileId);
    Task<List<FileDto>> GetUserFilesAsync(Guid userId);
    Task SecureDeleteFileAsync(Guid fileId);
}