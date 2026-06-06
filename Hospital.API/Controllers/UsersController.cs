using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Hospital.BLL.Interfaces;


[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")] // Тільки адмін може бачити всіх користувачів
public class UsersController : ControllerBase
{
    private readonly IUserService _userService; // Припустимо, у тебе є сервіс для користувачів

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public IActionResult GetAllUsers()
    {
        return Ok(_userService.GetAllUsers());
    }

    [HttpPut("{id}/role")]
    public IActionResult UpdateRole(int id, [FromBody] string newRole)
    {
        _userService.ChangeUserRole(id, newRole);
        return Ok("Роль змінено");
    }

    [HttpDelete("{id}")]
    public IActionResult DeleteUser(int id)
    {
        _userService.DeleteUser(id);
        return Ok("Користувача видалено");
    }
}