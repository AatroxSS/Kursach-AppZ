using System.ComponentModel.DataAnnotations;

namespace Hospital.DAL.Entities
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        [MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        public Role Role { get; set; }

        public virtual Patient? Patient { get; set; }

        public virtual Doctor? Doctor { get; set; }
    }
}
