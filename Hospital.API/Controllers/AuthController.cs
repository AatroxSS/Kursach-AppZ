using Hospital.BLL.DTO;
using Hospital.BLL.Infrastructure;
using Hospital.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.API.Controllers
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
        [AllowAnonymous]
        [HttpPost("register")]
        public IActionResult Register([FromBody] UserRegisterDTO dto)
        {
            try
            {
                _authService.RegisterUser(dto);
                return Ok("Реєстрація успішна");
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
        [AllowAnonymous]
        [HttpPost("login")]
        public IActionResult Login([FromBody] UserLoginDTO dto)
        {
            try
            {
                var token = _authService.Login(dto.Email, dto.Password);
                return Ok(new { Token = token });
            }
            catch (ValidationException ex)
            {
                return Unauthorized(ex.Message);
            }
        }
    }
}