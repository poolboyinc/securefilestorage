namespace SecureFileStorage.Core.Interfaces;
using System.IO;
using System.Threading.Tasks;

public interface IStorageService
{
    Task<string> SaveEncryptedFileAsync(byte[] data, string fileName);
    Task<byte[]> ReadFileAsync(string path);
    Task<bool> DeleteFileAsync(string path);
}