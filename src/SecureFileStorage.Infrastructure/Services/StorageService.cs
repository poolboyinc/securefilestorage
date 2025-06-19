namespace SecureFileStorage.Infrastructure.Services;
using System;
using System.IO;
using System.Threading.Tasks;
using SecureFileStorage.Core.Interfaces;

public class StorageService : IStorageService
{
    private readonly string _basePath;

    public StorageService()
    {
        _basePath = Path.Combine(Directory.GetCurrentDirectory(), "storage");
        Directory.CreateDirectory(_basePath);
    }

    public async Task<string> SaveEncryptedFileAsync(byte[] data, string fileName)
    {
        var path = Path.Combine(_basePath, fileName);
        await File.WriteAllBytesAsync(path, data);
        return fileName;
    }

    public async Task<byte[]> ReadFileAsync(string path)
    {
        var fullPath = Path.Combine(_basePath, path);
        return await File.ReadAllBytesAsync(fullPath);
    }

    public async Task<bool> DeleteFileAsync(string path)
    {
        var fullPath = Path.Combine(_basePath, path);
        if (File.Exists(fullPath))
        {
            var random = new Random();
            var fileInfo = new FileInfo(fullPath);
            var dummy = new byte[fileInfo.Length];

            for (int i = 0; i < 3; i++)
            {
                random.NextBytes(dummy);
                await File.WriteAllBytesAsync(fullPath, dummy);
            }

            File.Delete(fullPath);
            return true;
        }
        return false;
    }
}