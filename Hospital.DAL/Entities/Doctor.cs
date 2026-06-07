using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // ДОДАНО для ForeignKey

namespace Hospital.DAL.Entities
{
    public class Doctor
    {
        [Key]
        public int Id { get; set; }

        // ДОДАНО: Зв'язок з акаунтом, щоб AuthService міг зберігати лікаря
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [Required]
        [MaxLength(100)]
        public string FirstName { get; set; }

        [Required]
        [MaxLength(100)]
        public string LastName { get; set; }

        [Required]
        [MaxLength(150)]
        public string Specialization { get; set; }

        public int ExperienceYears { get; set; }

        public virtual ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}