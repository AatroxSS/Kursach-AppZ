using AutoMapper;
using Hospital.BLL.DTO;
using Hospital.BLL.Infrastructure;
using Hospital.BLL.Services;
using Hospital.DAL.Entities;
using Hospital.DAL.Interfaces;
using Moq;
using System;
using System.Numerics;
using System.Timers;
using Xunit;

namespace Hospital.Tests
{
    public class AppointmentServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUoW;
        private readonly Mock<IMapper> _mockMapper;
        private readonly AppointmentService _service;

        public AppointmentServiceTests()
        {
            _mockUoW = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _service = new AppointmentService(_mockUoW.Object, _mockMapper.Object);
        }

        [Fact]
        public void MakeAppointment_DateInPast_ThrowsValidationException()
        {
            var appointmentDto = new AppointmentDTO
            {
                DoctorId = 1,
                PatientId = 1,
                AppointmentDate = DateTime.Now.AddDays(-1) 
            };

            var exception = Assert.Throws<ValidationException>(() => _service.MakeAppointment(appointmentDto));
            Assert.Equal("Неможливо записатися на дату в минулому", exception.Message);
        }

        [Fact]
        public void MakeAppointment_DoctorDoesNotExist_ThrowsValidationException()
        {
            var appointmentDto = new AppointmentDTO
            {
                DoctorId = 99, 
                PatientId = 1,
                AppointmentDate = DateTime.Now.AddDays(1)
            };

            _mockUoW.Setup(u => u.Doctors.GetById(99)).Returns((Doctor)null);

            var exception = Assert.Throws<ValidationException>(() => _service.MakeAppointment(appointmentDto));
            Assert.Equal("Обраного лікаря не знайдено", exception.Message);
        }

        [Fact]
        public void MakeAppointment_ValidData_CreatesAppointmentAndSaves()
        {
            var appointmentDto = new AppointmentDTO
            {
                DoctorId = 1,
                PatientId = 1,
                AppointmentDate = DateTime.Now.AddDays(2),
                Notes = "Первинний огляд"
            };

            _mockUoW.Setup(u => u.Doctors.GetById(1)).Returns(new Doctor { Id = 1, FirstName = "Іван", LastName = "Іванов" });

            _mockUoW.Setup(u => u.Appointments.Create(It.IsAny<Appointment>()));

            _service.MakeAppointment(appointmentDto);

            _mockUoW.Verify(u => u.Appointments.Create(It.IsAny<Appointment>()), Times.Once);
            _mockUoW.Verify(u => u.Save(), Times.Once);
        }
    }
}