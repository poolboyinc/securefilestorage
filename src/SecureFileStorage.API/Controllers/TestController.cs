using Microsoft.AspNetCore.Mvc;

namespace SecureFileStorage.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly ILogger<TestController> _logger;

    public TestController(ILogger<TestController> logger)
    {
        _logger = logger;
    }

    [HttpGet("ping")]
    public IActionResult Ping()
    {
        _logger.LogInformation("Ping endpoint called");
        return Ok(new 
        { 
            message = "API is running", 
            timestamp = DateTime.UtcNow,
            version = "1.0.0"
        });
    }
    
    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new
        {
            status = "Healthy",
            timestamp = DateTime.UtcNow,
            services = new
            {
                api = "Running",
                database = "Connected", 
                storage = "Available"
            }
        });
    }
}