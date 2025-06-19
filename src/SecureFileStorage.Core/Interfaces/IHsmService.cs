namespace SecureFileStorage.Core.Interfaces;
using System.Threading.Tasks;

public interface IHsmService
{
    Task<byte[]> WrapKeyAsync(byte[] key, byte[] publicKey);
    Task<byte[]> UnwrapKeyAsync(byte[] wrappedKey, byte[] privateKey);
    Task<byte[]> GenerateKeyAsync(int keySize);
}