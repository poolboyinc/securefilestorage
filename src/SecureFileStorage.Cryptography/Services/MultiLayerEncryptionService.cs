using Microsoft.Extensions.Logging;

namespace SecureFileStorage.Cryptography.Services;
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using Sodium;
using SecureFileStorage.Core.Interfaces;
using SecureFileStorage.Core.Models;

public interface IMultiLayerEncryptionService
{
    Task<EncryptionResult> EncryptFileAsync(Stream inputStream, string fileName, Guid userId);
    Task<Stream> DecryptFileAsync(Guid fileId, Guid userId);
    byte[] GenerateKey(int keySize = 32);
    (byte[] publicKey, byte[] privateKey) GenerateKeyPair();
}

public class MultiLayerEncryptionService : IMultiLayerEncryptionService
{
    private readonly IKeyManagementService _keyManagement;
    private readonly ILogger<MultiLayerEncryptionService> _logger;
    private readonly IFileRepository _fileRepository;
    private readonly IPermissionService _permissionService;
    private readonly IStorageService _storageService;
    private readonly IAuditService _auditService;
    
    public MultiLayerEncryptionService(
        IKeyManagementService keyManagement,
        ILogger<MultiLayerEncryptionService> logger,
        IFileRepository fileRepository,
        IPermissionService permissionService,
        IStorageService storageService,
        IAuditService auditService)
    {
        _keyManagement = keyManagement;
        _logger = logger;
        _fileRepository = fileRepository;
        _permissionService = permissionService;
        _storageService = storageService;
        _auditService = auditService;
    }
    
    public async Task<EncryptionResult> EncryptFileAsync(Stream inputStream, string fileName, Guid userId)
    {
        try
        {
            _logger.LogInformation($"Starting encryption for file {fileName}");
            
            var fileKey = GenerateKey(32); 
            var chachaKey = GenerateKey(32); 
            var keyId = await _keyManagement.GenerateKeyIdAsync();
            
            using var ms = new MemoryStream();
            await inputStream.CopyToAsync(ms);
            var inputData = ms.ToArray();
            
            byte[] aesEncrypted;
            byte[] aesIv;
            using (var aes = Aes.Create())
            {
                aes.Key = fileKey;
                aes.GenerateIV();
                aesIv = aes.IV;
                
                using var encryptor = aes.CreateEncryptor();
                aesEncrypted = encryptor.TransformFinalBlock(inputData, 0, inputData.Length);
            }
            
            var finalEncrypted = new byte[aesEncrypted.Length];
            for (int i = 0; i < aesEncrypted.Length; i++)
            {
                finalEncrypted[i] = (byte)(aesEncrypted[i] ^ chachaKey[i % chachaKey.Length]);
            }
            
            var encryptedFileKey = await _keyManagement.EncryptKeyAsync(fileKey, userId);
            var encryptedChachaKey = await _keyManagement.EncryptKeyAsync(chachaKey, userId);
            
            var fileHash = ComputeHash(finalEncrypted);
            
            return new EncryptionResult
            {
                EncryptedData = finalEncrypted,
                EncryptedFileKey = encryptedFileKey,
                EncryptedChachaKey = encryptedChachaKey,
                IV = aesIv,
                Tag = new byte[0], 
                FileHash = fileHash,
                Algorithm = "AES-256-CBC+XOR",
                KeyId = keyId
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error encrypting file");
            throw new CryptographicException("File encryption failed", ex);
        }
    }
    
    public async Task<Stream> DecryptFileAsync(Guid fileId, Guid userId)
    {
        try
        {
            _logger.LogInformation($"Starting decryption for file {fileId}");
            
            var file = await _fileRepository.GetByIdAsync(fileId);
            
            if (file == null || file.IsDeleted)
                throw new FileNotFoundException("File not found");
            
            if (!await _permissionService.CanAccessFileAsync(userId, fileId))
                throw new UnauthorizedAccessException("Access denied");
            
            var fileKey = await _keyManagement.DecryptKeyAsync(file.EncryptedFileKey, userId);
            var chachaKey = await _keyManagement.DecryptKeyAsync(file.EncryptedChachaKey, userId);
            
            var encryptedData = await _storageService.ReadFileAsync(file.StoragePath);
            
            var computedHash = ComputeHash(encryptedData);
            if (!computedHash.SequenceEqual(file.FileHash))
            {
                _logger.LogError("File integrity check failed");
                throw new CryptographicException("File integrity check failed");
            }
            
            var xorDecrypted = new byte[encryptedData.Length];
            for (int i = 0; i < encryptedData.Length; i++)
            {
                xorDecrypted[i] = (byte)(encryptedData[i] ^ chachaKey[i % chachaKey.Length]);
            }
            
            byte[] decryptedData;
            using (var aes = Aes.Create())
            {
                aes.Key = fileKey;
                aes.IV = file.IV;
                
                using var decryptor = aes.CreateDecryptor();
                decryptedData = decryptor.TransformFinalBlock(xorDecrypted, 0, xorDecrypted.Length);
            }
            
            await _auditService.LogFileAccessAsync(userId, fileId, "Decrypt");
            
            return new MemoryStream(decryptedData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error decrypting file {fileId}");
            throw;
        }
    }
    
    public byte[] GenerateKey(int keySize = 32)
    {
        using var rng = RandomNumberGenerator.Create();
        var key = new byte[keySize];
        rng.GetBytes(key);
        return key;
    }
    
    public (byte[] publicKey, byte[] privateKey) GenerateKeyPair()
    {
        using var rsa = RSA.Create(2048);
        return (rsa.ExportRSAPublicKey(), rsa.ExportRSAPrivateKey());
    }
    
    private byte[] ComputeHash(byte[] data)
    {
        using var sha512 = SHA512.Create();
        return sha512.ComputeHash(data);
    }
}