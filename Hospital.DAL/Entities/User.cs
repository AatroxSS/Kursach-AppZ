using System.ComponentModel.DataAnnotations;

namespace Hospital.DAL.Entities
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        // ДОДАЙ ЦІ ПОЛЯ ДЛЯ ІМЕНІ ТА ПРІЗВИЩА:
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public Role Role { get; set; }

        public virtual Patient Patient { get; set; }

        // Додай зв'язок з лікарем:
        public virtual Doctor Doctor { get; set; }
    }
}