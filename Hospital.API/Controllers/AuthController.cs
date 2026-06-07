using Hospital.BLL.DTO;
using System;
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
            catch (Exception ex) // Змінено на загальний Exception, щоб ловити помилки бази
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
            catch (Exception ex)
            {
                return Unauthorized(ex.Message);
            }
        }

        // --- МЕТОДИ ДЛЯ АДМІНІСТРАТОРА ---

        [Authorize(Roles = "Administrator")]
        [HttpGet("/api/Users")]
        public IActionResult GetAllUsers()
        {
            return Ok(_authService.GetAllUsers());
        }

        [Authorize(Roles = "Administrator")]
        [HttpPost("/api/Users/{id}/role")]
        public IActionResult ChangeRole(int id, [FromBody] string newRole)
        {
            try
            {
                _authService.ChangeUserRole(id, newRole);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "Administrator")]
        [HttpDelete("/api/Users/{id}")]
        public IActionResult DeleteUser(int id)
        {
            try
            {
                _authService.DeleteUser(id);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}