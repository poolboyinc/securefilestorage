namespace SecureFileStorage.Core.Entities;
using System;
using System.Collections.Generic;

public class SecureFile
{
    public Guid Id { get; set; }
    public string FileName { get; set; }
    public string OriginalFileName { get; set; }
    public long FileSize { get; set; }
    public string ContentType { get; set; }
    public DateTime UploadedAt { get; set; }
    public Guid OwnerId { get; set; }
    public User Owner { get; set; }
        
    public string EncryptionAlgorithm { get; set; }
    public string KeyId { get; set; }
    public byte[] IV { get; set; }
    public byte[] Salt { get; set; }
    public byte[] Tag { get; set; }
    public byte[] EncryptedMetadata { get; set; }
    public byte[] EncryptedFileKey { get; set; } 
    public byte[] EncryptedChachaKey { get; set; } 
    
    public string StoragePath { get; set; }
        
    // Integrity
    public byte[] FileHash { get; set; }
    public string HashAlgorithm { get; set; }
        
    // Access control
    public List<FilePermission> Permissions { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
        
    // Audit
    public List<FileAuditLog> AuditLogs { get; set; }
}