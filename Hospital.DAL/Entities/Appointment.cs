using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hospital.DAL.Entities
{
    public class Appointment
    {
        [Key]
        public int Id { get; set; }

        public int DoctorId { get; set; }
        [ForeignKey("DoctorId")]
        public virtual Doctor Doctor { get; set; }

        public int PatientId { get; set; }
        [ForeignKey("PatientId")]
        public virtual Patient Patient { get; set; }

        public DateTime AppointmentDate { get; set; }

        [MaxLength(500)]
        public string Notes { get; set; }

        public bool IsCompleted { get; set; }
    }
}