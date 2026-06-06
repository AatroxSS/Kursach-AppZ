using System;

namespace Hospital.BLL.DTO
{
    public class AppointmentDTO
    {
        public int Id { get; set; }
        public int DoctorId { get; set; }

        // Додали знаки питання, щоб поля не були обов'язковими при POST-запитах
        public string? DoctorFullName { get; set; }

        public int PatientId { get; set; }

        // Додали знак питання
        public string? PatientFullName { get; set; }

        public DateTime AppointmentDate { get; set; }

        // Нотатки теж можуть бути порожніми
        public string? Notes { get; set; }

        public bool IsCompleted { get; set; }
    }
}