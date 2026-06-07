using AutoMapper;
using Hospital.BLL.DTO;
using Hospital.BLL.Infrastructure;
using Hospital.BLL.Interfaces;
using Hospital.DAL.Entities;
using Hospital.DAL.Interfaces;

namespace Hospital.BLL.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public AppointmentService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }

        public void MakeAppointment(AppointmentDTO appointmentDto)
        {
            if (appointmentDto == null)
                throw new ValidationException("Дані запису не передано", nameof(appointmentDto));

            if (appointmentDto.AppointmentDate <= DateTime.Now)
                throw new ValidationException("Неможливо записатися на дату в минулому", nameof(appointmentDto.AppointmentDate));

            var doctor = _uow.Doctors.GetById(appointmentDto.DoctorId);
            if (doctor == null)
                throw new ValidationException("Обраного лікаря не знайдено", nameof(appointmentDto.DoctorId));

            var patient = _uow.Patients.GetById(appointmentDto.PatientId);
            if (patient == null)
                throw new ValidationException("Пацієнта не знайдено", nameof(appointmentDto.PatientId));

            var alreadyBooked = _uow.Appointments
                .Find(a => a.DoctorId == appointmentDto.DoctorId
                           && a.AppointmentDate == appointmentDto.AppointmentDate
                           && !a.IsCompleted)
                .Any();

            if (alreadyBooked)
                throw new ValidationException("На цей час лікар вже має запис", nameof(appointmentDto.AppointmentDate));

            var appointment = new Appointment
            {
                DoctorId = appointmentDto.DoctorId,
                PatientId = appointmentDto.PatientId,
                AppointmentDate = appointmentDto.AppointmentDate,
                Notes = appointmentDto.Notes ?? string.Empty,
                IsCompleted = false
            };

            _uow.Appointments.Create(appointment);
            _uow.Save();
        }


        public void MakeAppointment(AppointmentDTO appointmentDto, int userId, string role)
        {
            if (appointmentDto == null)
                throw new ValidationException("Дані запису не передано", nameof(appointmentDto));

            if (role == "RegisteredUser")
            {
                var patient = _uow.Patients.Find(p => p.UserId == userId).FirstOrDefault();
                if (patient == null)
                    throw new ValidationException("Для цього користувача не знайдено профіль пацієнта", nameof(userId));

                appointmentDto.PatientId = patient.Id;
            }

            MakeAppointment(appointmentDto);
        }

        public IEnumerable<AppointmentDTO> GetAllAppointments()
        {
            var appointments = _uow.Appointments.GetAll().ToList();
            return _mapper.Map<List<AppointmentDTO>>(appointments);
        }

        public IEnumerable<AppointmentDTO> GetAppointmentsForUser(int userId, string role)
        {
            if (role == "Administrator" || role == "Manager")
                return GetAllAppointments();

            var patient = _uow.Patients.Find(p => p.UserId == userId).FirstOrDefault();
            if (patient == null)
                return Enumerable.Empty<AppointmentDTO>();

            var appointments = _uow.Appointments.Find(a => a.PatientId == patient.Id).ToList();
            return _mapper.Map<List<AppointmentDTO>>(appointments);
        }

        public IEnumerable<AppointmentDTO> GetAppointmentsByDoctor(int doctorId)
        {
            var appointments = _uow.Appointments.Find(a => a.DoctorId == doctorId).ToList();
            return _mapper.Map<List<AppointmentDTO>>(appointments);
        }

        public void CompleteAppointment(int appointmentId, string notes)
        {
            var appointment = _uow.Appointments.GetById(appointmentId);
            if (appointment == null)
                throw new ValidationException("Прийом не знайдено", nameof(appointmentId));

            appointment.IsCompleted = true;
            appointment.Notes = notes ?? string.Empty;

            _uow.Appointments.Update(appointment);
            _uow.Save();
        }

        public void Dispose()
        {
            _uow.Dispose();
        }
    }
}
