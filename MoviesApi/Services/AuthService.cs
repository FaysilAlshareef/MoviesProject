using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MoviesApi.Helpers;
using MoviesApi.Models.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;

namespace MoviesApi.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        private readonly JWT _jWT;

        public AuthService(
            UserManager<ApplicationUser> userManager,
            IOptions<JWT> jWT,
            RoleManager<IdentityRole> roleManager

            )
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _jWT = jWT.Value;
        }

        public async Task<AuthModel> GetTokenAsync(TokenRequestModel model)
        {
            var authModel = new AuthModel();

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || await _userManager.CheckPasswordAsync(user, model.Password))
            {
                authModel.Massege = "Email or Password is incorrect";
                return authModel;
            }

            authModel = await CreateAuthModel(user);

            if (user.RefreshTokens.Any(t => t.IsActive))
            {
                var activeRefreshToken = user.RefreshTokens.FirstOrDefault(t => t.IsActive);
                authModel.RefreshToken = activeRefreshToken.Token;
                authModel.RefreshTokenExpiration = activeRefreshToken.ExpireOn;
            }
            else
            {
                var refreshToken = GenerateRefreshToken();
                authModel.RefreshToken = refreshToken.Token;
                authModel.RefreshTokenExpiration = refreshToken.ExpireOn;
                user.RefreshTokens.Add(refreshToken);
                await _userManager.UpdateAsync(user);
            }
            return authModel;
        }

        public async Task<AuthModel> RegisterAsync(RegisterModel model)
        {
            if (await _userManager.FindByEmailAsync(model.Email) != null)
                return new AuthModel { Massege = "Email is already registerd!" };

            if (await _userManager.FindByNameAsync(model.UserName) != null)
                return new AuthModel { Massege = "UserName is already registerd!" };

            var user = new ApplicationUser
            {
                UserName = model.UserName,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
            {
                var errors = string.Empty;
                foreach (var error in result.Errors)
                    errors += $"{error.Description},";

                return new AuthModel { Massege = errors };
            }

            await _userManager.AddToRoleAsync(user, "User");



            return await CreateAuthModel(user);
        }

        public async Task<string> AddRoleAsync(RoleModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null || !await _roleManager.RoleExistsAsync(model.Role))
                return "Invalid User ID or Role";

            if (await _userManager.IsInRoleAsync(user, model.Role))
                return "User is Already assigned to this role";

            var result = await _userManager.AddToRoleAsync(user, model.Role);

            return result.Succeeded ? string.Empty : "Something went wrong";

        }

        public async Task<AuthModel> RefreshTokenAsync(string token)
        {
            var authModel = new AuthModel();

            var user = await _userManager.Users.SingleOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == token));
            var refreshToken = user.RefreshTokens.Single(t => t.Token == token);

            if (user == null || !refreshToken.IsActive)
            {
                authModel.Massege = "Invalid or Inactive Token";
                return authModel;
            }

            refreshToken.RevokedOn = DateTime.UtcNow;

            var newRefreshToken = GenerateRefreshToken();
            user.RefreshTokens.Add(newRefreshToken);
            await _userManager.UpdateAsync(user);

            authModel = await CreateAuthModel(user);

            authModel.RefreshToken = newRefreshToken.Token;
            authModel.RefreshTokenExpiration = newRefreshToken.ExpireOn;

            return authModel;
        }

        public async Task<bool> RevokeTokenAsync(string token)
        {
            var authModel = new AuthModel();

            var user = await _userManager.Users.SingleOrDefaultAsync(u => u.RefreshTokens.Any(t => t.Token == token));
            var refreshToken = user.RefreshTokens.Single(t => t.Token == token);

            if (user == null || !refreshToken.IsActive)
                return false;


            refreshToken.RevokedOn = DateTime.UtcNow;

            await _userManager.UpdateAsync(user);

            return false;
        }
        private async Task<JwtSecurityToken> CreateJwtToken(ApplicationUser user)
        {
            var userClaims = await _userManager.GetClaimsAsync(user);
            var roles = await _userManager.GetRolesAsync(user);
            var roleClaims = new List<Claim>();

            foreach (var role in roles)
                roleClaims.Add(new Claim(ClaimTypes.Role, role));

            var Claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub,user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email,user.Email),
                new Claim("uid",user.Id)
            }
             .Union(userClaims)
             .Union(roleClaims);

            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jWT.Key));

            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

            var jwtSecurityToken = new JwtSecurityToken(
                issuer: _jWT.Issure,
                audience: _jWT.Audience,
                claims: Claims,
                expires: DateTime.Now.AddMinutes(_jWT.DurationInMinutes),
                signingCredentials: signingCredentials);

            return jwtSecurityToken;
        }

        private RefreshToken GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var generator = new RNGCryptoServiceProvider();

            generator.GetBytes(randomNumber);

            return new RefreshToken
            {
                Token = Convert.ToBase64String(randomNumber),
                ExpireOn = DateTime.UtcNow.AddMinutes(_jWT.DurationInMinutes),
                CreatedOn = DateTime.UtcNow,
            };
            //HttpContext.Response.Cookies.Append
        }

        private async Task<AuthModel> CreateAuthModel(ApplicationUser user)
        {
            var authModel = new AuthModel();

            var roleList = await _userManager.GetRolesAsync(user);
            var JwtSecurityToken = await CreateJwtToken(user);


            authModel.IsAuthenticated = true;
            authModel.Token = new JwtSecurityTokenHandler().WriteToken(JwtSecurityToken);
            authModel.Email = user.Email;
            authModel.Username = user.UserName;

            authModel.Roles = roleList.ToList();

            return authModel;
        }


    }
}
