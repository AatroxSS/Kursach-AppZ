using System;

namespace Hospital.BLL.DTO
{
    public class AppointmentDTO
    {
        public int Id { get; set; }
        public int DoctorId { get; set; }

        public string? DoctorFullName { get; set; }

        public int PatientId { get; set; }

        public string? PatientFullName { get; set; }

        public DateTime AppointmentDate { get; set; }

        public string? Notes { get; set; }

        public bool IsCompleted { get; set; }
    }
}