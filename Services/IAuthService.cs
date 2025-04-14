using System;

namespace DelCorp.Services
{
    public interface IAuthService
    {
        Task<bool> LoginAsync(string email, string password);
        Task<bool> RegisterAsync(string email, string password, string name);
        Task<bool> SendPasswordResetAsync(string email);
        Task<bool> LogoutAsync();
        Task<bool> IsAuthenticatedAsync();
        Task<string?> GetCurrentUserEmail();
        Task<UserProfile> GetCurrentUserProfileAsync();
        Task<bool> CheckAuthenticationAsync();
    }
}
