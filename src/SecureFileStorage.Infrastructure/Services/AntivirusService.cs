using SecureFileStorage.Core.Interfaces;

namespace SecureFileStorage.Infrastructure.Services;

public class AntivirusService : IAntiVirusService
{
    public async Task<ScanResult> ScanFileAsync(Stream fileStream)
    {
            
        await Task.Delay(100); 
        
        if (fileStream.Length > 1_000_000_000) 
        {
            return new ScanResult
            {
                IsClean = false,
                ThreatName = "File.TooLarge",
                Details = "File exceeds maximum allowed size"
            };
        }

        
        var buffer = new byte[1024];
        await fileStream.ReadAsync(buffer, 0, buffer.Length);
        fileStream.Position = 0;

        
        var eicarSignature = "X5O!P%@AP[4\\PZX54(P^)7CC)7}$EICAR-STANDARD-ANTIVIRUS-TEST-FILE!$H+H*";
        var content = System.Text.Encoding.ASCII.GetString(buffer);
        if (content.Contains(eicarSignature))
        {
            return new ScanResult
            {
                IsClean = false,
                ThreatName = "EICAR-Test-File",
                Details = "EICAR test file detected"
            };
        }

        return new ScanResult
        {
            IsClean = true,
            ThreatName = null,
            Details = "No threats detected"
        };
    }
}