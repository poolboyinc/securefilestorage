using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SecureFileStorage.Core.Entities;
using SecureFileStorage.Core.Interfaces;
using SecureFileStorage.Core.Options;

namespace SecureFileStorage.Cryptography.Services;
using System;
using System.Security.Cryptography;
using Sodium;

public interface IKeyManagementService
{
    Task<byte[]> EncryptKeyAsync(byte[] key, Guid userId);
    Task<byte[]> DecryptKeyAsync(byte[] encryptedKey, Guid userId);
    Task RotateKeysAsync();
    Task<string> GenerateKeyIdAsync();
    Task ShareFileKeyAsync(Guid fileId, Guid fromUserId, Guid toUserId);
}

public class KeyManagementService : IKeyManagementService
{
    private readonly IUserRepository _userRepository;
    private readonly IKeyRepository _keyRepository;
    private readonly IHsmService _hsmService;
    private readonly KeyManagementOptions _options;
    private readonly ILogger<KeyManagementService> _logger;
    private readonly IFileRepository _fileRepository;
    
    public KeyManagementService(
        IUserRepository userRepository,
        IKeyRepository keyRepository,
        IHsmService hsmService,
        IOptions<KeyManagementOptions> options,
        ILogger<KeyManagementService> logger,
        IFileRepository fileRepository)
    {
        _userRepository = userRepository;
        _keyRepository = keyRepository;
        _hsmService = hsmService;
        _options = options.Value;
        _logger = logger;
        _fileRepository = fileRepository;
    }
    
    public async Task<byte[]> EncryptKeyAsync(byte[] key, Guid userId)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new InvalidOperationException("User not found");
            
            using var rsa = RSA.Create();
            rsa.ImportRSAPublicKey(user.PublicKey, out _);
            
            return rsa.Encrypt(key, RSAEncryptionPadding.OaepSHA256);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to encrypt key for user {userId}");
            throw;
        }
    }
    
    public async Task<byte[]> DecryptKeyAsync(byte[] encryptedKey, Guid userId)
    {
        try
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new InvalidOperationException("User not found");
            
            var privateKey = DecryptUserPrivateKey(user);
            
            using var rsa = RSA.Create();
            rsa.ImportRSAPrivateKey(privateKey, out _);
            
            return rsa.Decrypt(encryptedKey, RSAEncryptionPadding.OaepSHA256);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to decrypt key for user {userId}");
            throw;
        }
    }
    
    public async Task<string> GenerateKeyIdAsync()
    {
        return Guid.NewGuid().ToString("N").Substring(0, 16);
    }
    
    public async Task ShareFileKeyAsync(Guid fileId, Guid fromUserId, Guid toUserId)
    {
        try
        {
            var file = await _fileRepository.GetByIdAsync(fileId);
            if (file == null)
                throw new InvalidOperationException("File not found");
            
            var fileKey = await DecryptKeyAsync(file.EncryptedFileKey, fromUserId);
            var chachaKey = await DecryptKeyAsync(file.EncryptedChachaKey, fromUserId);
            
            var newEncryptedFileKey = await EncryptKeyAsync(fileKey, toUserId);
            var newEncryptedChachaKey = await EncryptKeyAsync(chachaKey, toUserId);
            
            _logger.LogInformation($"File keys re-encrypted for sharing from user {fromUserId} to {toUserId}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to share file keys for file {fileId}");
            throw;
        }
    }
    
    public async Task RotateKeysAsync()
    {
        _logger.LogInformation("Key rotation not implemented for this version");
        await Task.CompletedTask;
    }
    
    private byte[] DecryptUserPrivateKey(User user)
    {
        using var pbkdf2 = new Rfc2898DeriveBytes(
            user.PasswordHash, 
            user.PasswordSalt, 
            10000,
            HashAlgorithmName.SHA256);
        
        var key = pbkdf2.GetBytes(32);
        var iv = pbkdf2.GetBytes(16);
        
        using var aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;
        
        using var decryptor = aes.CreateDecryptor();
        using var ms = new MemoryStream(user.EncryptedPrivateKey);
        using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
        
        var decrypted = new byte[user.EncryptedPrivateKey.Length];
        var bytesRead = cs.Read(decrypted, 0, decrypted.Length);
        
        var result = new byte[bytesRead];
        Array.Copy(decrypted, result, bytesRead);
        return result;
    }
}