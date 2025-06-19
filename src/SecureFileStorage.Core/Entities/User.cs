namespace SecureFileStorage.Core.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public byte[] PasswordHash { get; set; }
    public byte[] PasswordSalt { get; set; }
    public bool IsMfaEnabled { get; set; }
    public string? MfaSecret { get; set; } 
    public List<string>? BackupCodes { get; set; } 
    public byte[] PublicKey { get; set; }
    public byte[] EncryptedPrivateKey { get; set; }
    public string KeyDerivationAlgorithm { get; set; }
    public int KeyDerivationIterations { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public bool IsActive { get; set; }
    
    public ICollection<SecureFile> Files { get; set; } = new List<SecureFile>();
    public ICollection<UserRole> Roles { get; set; } = new List<UserRole>();
}