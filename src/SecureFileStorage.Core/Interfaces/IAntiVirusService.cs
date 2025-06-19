namespace SecureFileStorage.Core.Interfaces;
using System.IO;
using System.Threading.Tasks;

public interface IAntiVirusService
{
    Task<ScanResult> ScanFileAsync(Stream fileStream);
}

public class ScanResult
{
    public bool IsClean { get; set; }
    public string ThreatName { get; set; }
    public string Details { get; set; }
}