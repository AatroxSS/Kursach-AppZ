using AutoMapper;
using Hospital.BLL.DTO;
using Hospital.BLL.Infrastructure;
using Hospital.BLL.Interfaces;
using Hospital.DAL.Entities;
using Hospital.DAL.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

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
            if (appointmentDto.AppointmentDate < DateTime.Now)
                throw new ValidationException("Неможливо записатися на дату в минулому", "AppointmentDate");

            var doctor = _uow.Doctors.GetById(appointmentDto.DoctorId);
            if (doctor == null)
                throw new ValidationException("Обраного лікаря не знайдено", "DoctorId");

            var appointment = new Appointment
            {
                DoctorId = appointmentDto.DoctorId,
                PatientId = appointmentDto.PatientId,
                AppointmentDate = appointmentDto.AppointmentDate,
                Notes = appointmentDto.Notes,
                IsCompleted = false
            };

            _uow.Appointments.Create(appointment);
            _uow.Save();
        }

        public IEnumerable<AppointmentDTO> GetAppointmentsByDoctor(int doctorId)
        {
            var appointments = _uow.Appointments.Find(a => a.DoctorId == doctorId).ToList();
            return _mapper.Map<IEnumerable<Appointment>, List<AppointmentDTO>>(appointments);
        }

        public void CompleteAppointment(int appointmentId, string notes)
        {
            var appointment = _uow.Appointments.GetById(appointmentId);
            if (appointment == null)
                throw new ValidationException("Прийом не знайдено", "");

            appointment.IsCompleted = true;
            appointment.Notes = notes;

            _uow.Appointments.Update(appointment);
            _uow.Save();
        }

        public void Dispose()
        {
            _uow.Dispose();
        }
    }
}