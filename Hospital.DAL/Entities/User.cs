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

        public Role Role { get; set; }

        public virtual Patient Patient { get; set; }
    }
}