using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromForm] string username, [FromForm] string password)
    {
        bool success = await _adminService.LoginAsync(username, password);
        if (!success)
            return Unauthorized(new { message = "Invalid credentials" });

        return Ok(new { message = "Login successful" });
    }

    [HttpPost("logout")]
    public IActionResult Logout()
    {
        _adminService.Logout();
        return Ok(new { message = "Logged out" });
    }

    // Example admin-only endpoint
    [HttpGet("secret")]
    [AdminOnly]
    public IActionResult Secret()
    {
        return Ok(new { message = "You are an admin!" });
    }
}
