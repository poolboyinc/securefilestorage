namespace SecureFileStorage.Infrastructure.Services;
using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using SecureFileStorage.Core.Interfaces;

public class HsmService : IHsmService
{
    public async Task<byte[]> WrapKeyAsync(byte[] key, byte[] publicKey)
    {
        await Task.Delay(10);
            
        using var rsa = RSA.Create();
        rsa.ImportSubjectPublicKeyInfo(publicKey, out _);
        
        return rsa.Encrypt(key, RSAEncryptionPadding.OaepSHA256);
    }

    public async Task<byte[]> UnwrapKeyAsync(byte[] wrappedKey, byte[] privateKey)
    {
        await Task.Delay(10);
        
        using var rsa = RSA.Create();
        rsa.ImportRSAPrivateKey(privateKey, out _);
        
        return rsa.Decrypt(wrappedKey, RSAEncryptionPadding.OaepSHA256);
    }

    public async Task<byte[]> GenerateKeyAsync(int keySize)
    {
        await Task.Delay(10);
        
        using var rng = RandomNumberGenerator.Create();
        var key = new byte[keySize];
        rng.GetBytes(key);
        
        return key;
    }
}