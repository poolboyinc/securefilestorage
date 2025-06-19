using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using SecureFileStorage.Core.Interfaces;
using SecureFileStorage.Core.Entities;
using System.Security.Cryptography;
using SecureFileStorage.Core.DTOs;
using System.Threading.Tasks; 

namespace SecureFileStorage.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IUserRepository _userRepository;
    private readonly IAuditService _auditService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IConfiguration configuration,
        IUserRepository userRepository,
        IAuditService auditService,
        ILogger<AuthController> logger)
    {
        _configuration = configuration;
        _userRepository = userRepository;
        _auditService = auditService;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
                return BadRequest("Username and password are required");

            if (string.IsNullOrEmpty(request.Email))
                return BadRequest("Email is required");

            if (request.Password.Length < 8)
                return BadRequest("Password must be at least 8 characters long");
            
            var existingUserByUsername = await _userRepository.GetByUsernameAsync(request.Username);
            if (existingUserByUsername != null)
                return BadRequest("Username already exists");

            var existingUserByEmail = await _userRepository.GetByEmailAsync(request.Email);
            if (existingUserByEmail != null)
                return BadRequest("Email already exists");
            
            var salt = GenerateSalt();
            var passwordHash = HashPassword(request.Password, salt);
            
            var (publicKey, privateKey) = GenerateKeyPair();
            var encryptedPrivateKey = EncryptPrivateKey(privateKey, request.Password);
            
            var userId = Guid.NewGuid();
            var newUser = new User
            {
                Id = userId,
                Username = request.Username,
                Email = request.Email,
                PasswordHash = passwordHash,
                PasswordSalt = salt,
                PublicKey = publicKey,
                EncryptedPrivateKey = encryptedPrivateKey,
                KeyDerivationAlgorithm = "PBKDF2",
                KeyDerivationIterations = 100000,
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                IsMfaEnabled = false,
                BackupCodes = new List<string>()
            };

            await _userRepository.CreateAsync(newUser);


            await _auditService.LogUserSecurityEventAsync(userId, "UserRegistered", $"User '{request.Username}' registered successfully.");
            
            var token = GenerateJwtToken(userId, request.Username, new[] { "User" });

            return Ok(new RegisterResponse
            {
                UserId = userId,
                Username = request.Username,
                Email = request.Email,
                Token = token,
                Message = "User registered successfully"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during registration");
            return StatusCode(500, "An error occurred during registration");
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
                return BadRequest("Username and password are required");
            
            var user = await _userRepository.GetByUsernameAsync(request.Username);
            if (user == null)
                return Unauthorized("Invalid credentials");
            
            if (!VerifyPassword(request.Password, user.PasswordHash, user.PasswordSalt))
            {

                await _auditService.LogUserSecurityEventAsync(user.Id, "LoginFailed", $"Failed login attempt for username: '{request.Username}'. Invalid credentials.");
                return Unauthorized("Invalid credentials");
            }
            
            user.LastLoginAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);
            
            var roles = user.Roles?.Select(ur => ur.Role.Name).ToArray() ?? new[] { "User" };
            var token = GenerateJwtToken(user.Id, user.Username, roles);


            await _auditService.LogUserSecurityEventAsync(user.Id, "LoginSuccess", $"User '{user.Username}' logged in successfully.");

            return Ok(new LoginResponse
            {
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddHours(24),
                Username = user.Username
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during login");
            return StatusCode(500, "An error occurred during login");
        }
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        try
        {
            return Ok(new { message = "Logged out successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            return StatusCode(500, "An error occurred during logout");
        }
    }

    private byte[] GenerateSalt()
    {
        var salt = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }
        return salt;
    }

    private byte[] HashPassword(string password, byte[] salt)
    {
        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 100000, HashAlgorithmName.SHA256);
        return pbkdf2.GetBytes(32);
    }

    private bool VerifyPassword(string password, byte[] hash, byte[] salt)
    {
        var testHash = HashPassword(password, salt);
        return testHash.SequenceEqual(hash);
    }

    private (byte[] publicKey, byte[] privateKey) GenerateKeyPair()
    {
        using var rsa = RSA.Create(2048);
        var privateKey = rsa.ExportRSAPrivateKey();
        var publicKey = rsa.ExportRSAPublicKey();
        return (publicKey, privateKey);
    }

    private byte[] EncryptPrivateKey(byte[] privateKey, string password)
    {
        var salt = GenerateSalt(); 

        using var pbkdf2 = new Rfc2898DeriveBytes(
            Encoding.UTF8.GetBytes(password),
            salt, 
            10000,
            HashAlgorithmName.SHA256);

        var key = pbkdf2.GetBytes(32); 
        var iv = pbkdf2.GetBytes(16);  

        using var aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;

        using var encryptor = aes.CreateEncryptor();
        using var ms = new MemoryStream();

        ms.Write(salt, 0, salt.Length);
        ms.Write(iv, 0, iv.Length);

        using var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write);

        cs.Write(privateKey, 0, privateKey.Length);
        cs.FlushFinalBlock();

        return ms.ToArray();
    }

    private string GenerateJwtToken(Guid userId, string username, string[] roles)
    {
        var jwtKey = _configuration["Jwt:Key"];
        if (string.IsNullOrEmpty(jwtKey))
        {
            _logger.LogError("JWT Key is not configured. Please add 'Jwt:Key' to your configuration.");
            throw new InvalidOperationException("JWT Key is missing from configuration.");
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Name, username),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}