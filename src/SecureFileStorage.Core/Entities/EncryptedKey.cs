namespace SecureFileStorage.Core.Entities;

public class EncryptedKey
{
    public Guid Id { get; set; }
    public string KeyId { get; set; }
    public byte[] EncryptedKeyData { get; set; }
    public string Algorithm { get; set; }
    public KeyStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? RotatedAt { get; set; }
    public string? ReplacementKeyId { get; set; } 
}

public enum KeyStatus
{
    Active,
    Rotated,
    Revoked,
    Expired
}