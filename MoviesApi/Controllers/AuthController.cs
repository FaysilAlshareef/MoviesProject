using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MoviesApi.Models.Identity;
using MoviesApi.Services;
using System.Net.Http;

namespace MoviesApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync([FromBody] RegisterModel model)
        {
            if (!ModelState.IsValid) 
                return BadRequest(ModelState);

            var result=await _authService.RegisterAsync(model);
            if (!result.IsAuthenticated)
                return BadRequest(result.Massege);

            SetRefreshTokenInCookie(result.RefreshToken, result.RefreshTokenExpiration);

            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> GetTokenAsync([FromBody] TokenRequestModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _authService.GetTokenAsync(model);
            if (!result.IsAuthenticated)
                return BadRequest(result.Massege);

            if (!string.IsNullOrEmpty(result.RefreshToken))
                SetRefreshTokenInCookie(result.RefreshToken, result.RefreshTokenExpiration);


            return Ok(result);
        }

        //[Authorize("Admin")]
        [HttpPost("addRole")]
        public async Task<IActionResult> AddRoleAsync(RoleModel roleModel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result= await _authService.AddRoleAsync(roleModel);

            if(!string.IsNullOrEmpty(result))
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("refreshToken")]
        public async Task<IActionResult> RefreshToken()
        {
            var refreshToken = Request.Cookies["refreshToken"];
            var result= await _authService.RefreshTokenAsync(refreshToken);    

            if(!result.IsAuthenticated)
                return BadRequest(result);

            SetRefreshTokenInCookie(result.RefreshToken, result.RefreshTokenExpiration);

            return Ok(result);
        }


        [HttpPost("revokeToken")]
        public async Task<IActionResult> RevokeToken([FromBody]RevokeToken model )
        {
            var token = model.Token ?? Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(token))
                return BadRequest("Token is Required");

            var result=await _authService.RevokeTokenAsync(token);  
            
            if(!result)
                return BadRequest("Token is Invalid");

            return Ok("Token is Revoked");
        }

        private void SetRefreshTokenInCookie(string refreshToken, DateTime expires)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = expires.ToLocalTime()
            };

            Response.Cookies.Append("refreshToken", refreshToken, cookieOptions);

        }
    }
}
