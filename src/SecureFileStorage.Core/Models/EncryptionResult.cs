namespace SecureFileStorage.Core.Models;

public class EncryptionResult
{
    public byte[] EncryptedData { get; set; }
    public byte[] EncryptedFileKey { get; set; }
    public byte[] EncryptedChachaKey { get; set; }
    public byte[] IV { get; set; }
    public byte[] Tag { get; set; }
    public byte[] FileHash { get; set; }
    public string Algorithm { get; set; }
    public string KeyId { get; set; }
    public string StorageFileName { get; set; }
}