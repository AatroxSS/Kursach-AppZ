using System.ComponentModel.DataAnnotations;

namespace Hospital.UI.Models
{
    public class LoginModel
    {
        [Required(ErrorMessage = "Введіть Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Введіть пароль")]
        public string Password { get; set; } = string.Empty;
    }
}