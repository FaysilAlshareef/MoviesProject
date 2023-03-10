using MoviesApi.Models.Identity;

namespace MoviesApi.Services
{
    public interface IAuthService
    {
        Task<AuthModel> RegisterAsync(RegisterModel model);
        Task<AuthModel> GetTokenAsync(TokenRequestModel model);
        Task<string> AddRoleAsync(RoleModel model);

        Task<AuthModel> RefreshTokenAsync(string token);

        Task<bool> RevokeTokenAsync(string token);
    }
}
