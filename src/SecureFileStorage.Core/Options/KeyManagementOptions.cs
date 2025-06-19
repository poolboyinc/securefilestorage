namespace SecureFileStorage.Core.Options;

public class KeyManagementOptions
{
    public bool UseHsm { get; set; }
    public int KeyRotationDays { get; set; } = 90;
    public string HsmConnectionString { get; set; }
    public int KeyDerivationIterations { get; set; } = 100000;
}