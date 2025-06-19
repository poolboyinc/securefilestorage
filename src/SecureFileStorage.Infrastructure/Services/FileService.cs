namespace SecureFileStorage.Infrastructure.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SecureFileStorage.Core.DTOs;
using SecureFileStorage.Core.Entities;
using SecureFileStorage.Core.Interfaces;

public class FileService : IFileService
{
    private readonly IFileRepository _fileRepository;
    private readonly IStorageService _storageService;

    public FileService(IFileRepository fileRepository, IStorageService storageService)
    {
        _fileRepository = fileRepository;
        _storageService = storageService;
    }

    public async Task<SecureFile> SaveFileMetadataAsync(SecureFile file)
    {
        return await _fileRepository.CreateAsync(file);
    }

    public async Task<SecureFile> GetFileMetadataAsync(Guid fileId)
    {
        return await _fileRepository.GetByIdAsync(fileId);
    }

    public async Task<List<FileDto>> GetUserFilesAsync(Guid userId)
    {
        var files = await _fileRepository.GetUserFilesAsync(userId);
        return files.Select(f => new FileDto
        {
            Id = f.Id,
            OriginalFileName = f.OriginalFileName,
            FileSize = f.FileSize,
            ContentType = f.ContentType,
            UploadedAt = f.UploadedAt,
            IsShared = f.Permissions?.Any(p => p.UserId != userId) ?? false,
            SharedWith = new List<string>()
        }).ToList();
    }

    public async Task SecureDeleteFileAsync(Guid fileId)
    {
        var file = await _fileRepository.GetByIdAsync(fileId);
        if (file != null)
        {
            await _storageService.DeleteFileAsync(file.StoragePath);
            await _fileRepository.DeleteAsync(fileId);
        }
    }
}