using System.Text;
using System.Text.Json;
using Blazored.LocalStorage; 
using Microsoft.AspNetCore.Components.Authorization;
using SecureFileStorage.Core.DTOs;


namespace SecureFileStorage.Web.Services;

public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;
    private readonly AuthStateProvider _authStateProvider;
    private readonly JsonSerializerOptions _jsonOptions;
    
    public AuthService(IHttpClientFactory httpClientFactory, ILocalStorageService localStorage, AuthenticationStateProvider authStateProvider)
    {
        _httpClient = httpClientFactory.CreateClient("BackendApi");
        _localStorage = localStorage;
        _authStateProvider = (AuthStateProvider)authStateProvider; 
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public async Task<LoginResponse?> LoginAsync(string username, string password)
    {
        try
        {
            var loginRequest = new LoginRequest
            {
                Username = username,
                Password = password
            };

            var json = JsonSerializer.Serialize(loginRequest, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("api/auth/login", content);

            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var loginResponse = JsonSerializer.Deserialize<LoginResponse>(responseJson, _jsonOptions);

                if (loginResponse?.Token != null)
                {
                    await _localStorage.SetItemAsStringAsync("authToken", loginResponse.Token);
                    _authStateProvider.NotifyUserLoggedIn(loginResponse.Token);
                    return loginResponse;
                }
            }

            await _localStorage.RemoveItemAsync("authToken");
            _authStateProvider.NotifyUserLoggedOut();
            return null;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Login failed: {ex.Message}");
            await _localStorage.RemoveItemAsync("authToken");
            _authStateProvider.NotifyUserLoggedOut();
            return null;
        }
    }

    public async Task LogoutAsync()
    {
        await _localStorage.RemoveItemAsync("authToken");
        _authStateProvider.NotifyUserLoggedOut();
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var token = await _localStorage.GetItemAsStringAsync("authToken");
        return !string.IsNullOrEmpty(token);
    }

    public async Task<string?> GetTokenAsync()
    {
        return await _localStorage.GetItemAsStringAsync("authToken");
    }

    public void NotifyLogin(string token)
    {
        _authStateProvider.NotifyUserLoggedIn(token);
    }

    public void NotifyLogout()
    {
        _authStateProvider.NotifyUserLoggedOut();
    }
}