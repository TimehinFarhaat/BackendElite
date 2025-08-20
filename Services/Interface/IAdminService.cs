public interface IAdminService
{
    Task<bool> LoginAsync(string username, string password);

    void Logout();
}