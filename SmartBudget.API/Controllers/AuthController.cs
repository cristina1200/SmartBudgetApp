using Microsoft.AspNetCore.Mvc;
using SmartBudget.Server.DTOs.Auth;
using SmartBudget.Server.Services.Interfaces;

namespace SmartBudget.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            try
            {
                var result =
                    await _authService.RegisterAsync(dto);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var result =
                await _authService.LoginAsync(dto);

            if (result == null)
            {
                return Unauthorized("Invalid credentials.");
            }

            return Ok(result);
        }
    }
}