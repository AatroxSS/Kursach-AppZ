using Hospital.BLL.DTO;
using System.Collections.Generic;

namespace Hospital.BLL.Interfaces
{
    public interface IAppointmentService
    {
        void MakeAppointment(AppointmentDTO appointmentDto);
        IEnumerable<AppointmentDTO> GetAppointmentsByDoctor(int doctorId);
        void CompleteAppointment(int appointmentId, string notes);
        void Dispose();
    }
}