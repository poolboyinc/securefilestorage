using SecureFileStorage.Core.DTOs; 

namespace SecureFileStorage.Web.Services;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(string username, string password); 
    Task LogoutAsync();
    Task<bool> IsAuthenticatedAsync();
    Task<string?> GetTokenAsync();
    void NotifyLogin(string token);
    void NotifyLogout();
}