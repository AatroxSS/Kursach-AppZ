using Hospital.BLL.DTO;

namespace Hospital.BLL.Interfaces
{
    public interface IAppointmentService : IDisposable
    {
        void MakeAppointment(AppointmentDTO appointmentDto);
        void MakeAppointment(AppointmentDTO appointmentDto, int userId, string role);
        IEnumerable<AppointmentDTO> GetAllAppointments();
        IEnumerable<AppointmentDTO> GetAppointmentsForUser(int userId, string role);
        IEnumerable<AppointmentDTO> GetAppointmentsByDoctor(int doctorId);
        void CompleteAppointment(int appointmentId, string notes);
    }
}
