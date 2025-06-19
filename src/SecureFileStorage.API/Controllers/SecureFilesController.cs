using SecureFileStorage.Core.DTOs;
using SecureFileStorage.Core.Entities;
using SecureFileStorage.Core.Interfaces;
using SecureFileStorage.Cryptography.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace SecureFileStorage.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] 
public class SecureFilesController : ControllerBase
{
    private readonly IMultiLayerEncryptionService _encryptionService;
    private readonly IFileService _fileService;
    private readonly IAuditService _auditService;
    private readonly IAntiVirusService _antivirusService;
    private readonly ILogger<SecureFilesController> _logger;
    private readonly IStorageService _storageService;
    private readonly IPermissionService _permissionService;
    private readonly ISharingService _sharingService;
    private readonly IKeyManagementService _keyManagementService;
    
    public SecureFilesController(
        IMultiLayerEncryptionService encryptionService,
        IFileService fileService,
        IAuditService auditService,
        IAntiVirusService antivirusService,
        ILogger<SecureFilesController> logger,
        IStorageService storageService,
        IPermissionService permissionService,
        ISharingService sharingService,
        IKeyManagementService keyManagementService)
    {
        _encryptionService = encryptionService;
        _fileService = fileService;
        _auditService = auditService;
        _antivirusService = antivirusService;
        _logger = logger;
        _storageService = storageService;
        _permissionService = permissionService;
        _sharingService = sharingService;
        _keyManagementService = keyManagementService;
    }
    
    [HttpPost("upload")]
    [RequestSizeLimit(100_000_000)] // 100MB limit
    public async Task<IActionResult> UploadFile(IFormFile file)
    {
        try
        {
            var userId = GetUserId();
            _logger.LogInformation($"User {userId} attempting to upload file: {file?.FileName}");
            
            if (file == null || file.Length == 0)
                return BadRequest(new { error = "No file provided" });
                
            if (file.Length > 100_000_000)
                return BadRequest(new { error = "File size exceeds 100MB limit" });
            
            using var scanStream = file.OpenReadStream();
            var scanResult = await _antivirusService.ScanFileAsync(scanStream);
            if (!scanResult.IsClean)
            {
                await _auditService.LogSecurityEventAsync(userId, "MalwareDetected", file.FileName);
                return BadRequest(new { error = $"File contains malware: {scanResult.ThreatName}" });
            }
            
            var storageFileName = $"{Guid.NewGuid():N}{Path.GetExtension(file.FileName)}";
            
            file.OpenReadStream().Position = 0;
            
            using var encryptStream = file.OpenReadStream();
            var result = await _encryptionService.EncryptFileAsync(encryptStream, file.FileName, userId);
            result.StorageFileName = storageFileName;
            
            var secureFile = new SecureFile
            {
                Id = Guid.NewGuid(),
                FileName = storageFileName,
                OriginalFileName = file.FileName,
                FileSize = file.Length,
                ContentType = file.ContentType ?? "application/octet-stream",
                UploadedAt = DateTime.UtcNow,
                OwnerId = userId,
                EncryptionAlgorithm = result.Algorithm,
                KeyId = result.KeyId,
                IV = result.IV ?? new byte[0],
                Salt = new byte[0],
                Tag = result.Tag ?? new byte[0],
                EncryptedMetadata = new byte[0],
                FileHash = result.FileHash,
                HashAlgorithm = "SHA-512",
                EncryptedFileKey = result.EncryptedFileKey,
                EncryptedChachaKey = result.EncryptedChachaKey,
                StoragePath = storageFileName,
                IsDeleted = false
            };
            
            await _fileService.SaveFileMetadataAsync(secureFile);
            await _storageService.SaveEncryptedFileAsync(result.EncryptedData, storageFileName);
            
            await _auditService.LogFileOperationAsync(userId, secureFile.Id, "Upload");
            
            return Ok(new
            {
                fileId = secureFile.Id,
                fileName = secureFile.OriginalFileName,
                uploadedAt = secureFile.UploadedAt,
                size = secureFile.FileSize
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file");
            return StatusCode(500, new { error = "An error occurred while uploading the file", details = ex.Message });
        }
    }
    
    [HttpGet("{fileId}/download")]
    public async Task<IActionResult> DownloadFile(Guid fileId)
    {
        try
        {
            var userId = GetUserId();
            _logger.LogInformation($"User {userId} attempting to download file: {fileId}");
            
            if (!await _permissionService.CanAccessFileAsync(userId, fileId))
            {
                _logger.LogWarning($"User {userId} denied access to file {fileId}");
                return Forbid("Access denied");
            }
                
            var fileMetadata = await _fileService.GetFileMetadataAsync(fileId);
            if (fileMetadata == null)
            {
                _logger.LogWarning($"File {fileId} not found");
                return NotFound("File not found");
            }
                
            var decryptedStream = await _encryptionService.DecryptFileAsync(fileId, userId);
            await _auditService.LogFileOperationAsync(userId, fileId, "Download");
            
            return File(
                decryptedStream, 
                fileMetadata.ContentType ?? "application/octet-stream", 
                fileMetadata.OriginalFileName
            );
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (FileNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error downloading file {fileId}");
            return StatusCode(500, new { error = "An error occurred while downloading the file" });
        }
    }
    
    [HttpGet]
    public async Task<IActionResult> GetUserFiles()
    {
        try
        {
            var userId = GetUserId();
            _logger.LogInformation($"User {userId} retrieving files");
            
            var files = await _fileService.GetUserFilesAsync(userId);
            return Ok(files);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving user files");
            return StatusCode(500, new { error = "An error occurred while retrieving files" });
        }
    }
    
    [HttpDelete("{fileId}")]
    public async Task<IActionResult> DeleteFile(Guid fileId)
    {
        try
        {
            var userId = GetUserId();
            
            var file = await _fileService.GetFileMetadataAsync(fileId);
            if (file == null)
                return NotFound();
                
            if (file.OwnerId != userId)
                return Forbid("Only file owner can delete");
                
            await _fileService.SecureDeleteFileAsync(fileId);
            await _auditService.LogFileOperationAsync(userId, fileId, "Delete");
            
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting file {fileId}");
            return StatusCode(500, new { error = "An error occurred while deleting the file" });
        }
    }
    
    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            _logger.LogError($"Invalid user ID claim: {userIdClaim}");
            throw new InvalidOperationException("Invalid user ID");
        }
            
        return userId;
    }
}