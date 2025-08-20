using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

public class AdminService : IAdminService
{
    private readonly AdminSettings _adminSettings;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AdminService(IOptions<AdminSettings> options, IHttpContextAccessor httpContextAccessor)
    {
        _adminSettings = options.Value;
        _httpContextAccessor = httpContextAccessor;
    }

    public Task<bool> LoginAsync(string username, string password)
    {
        bool success = (username == _adminSettings.Username && password == _adminSettings.Password);

        if (success)
            _httpContextAccessor.HttpContext?.Session.SetString("IsAdmin", "true");

        return Task.FromResult(success);
    }

    public void Logout()
    {
        _httpContextAccessor.HttpContext?.Session.Remove("IsAdmin");
    }

    public bool IsAdmin()
    {
        var isAdmin = _httpContextAccessor.HttpContext?.Session.GetString("IsAdmin");
        return isAdmin == "true";
    }
}
