using AutoMapper;
using Hospital.BLL.DTO;
using Hospital.BLL.Infrastructure;
using Hospital.BLL.Services;
using Hospital.DAL.Entities;
using Hospital.DAL.Interfaces;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xunit;

namespace Hospital.Tests
{
    public class AppointmentServiceTests
    {
        private readonly IMapper _mapper;
        private readonly Mock<IUnitOfWork> _mockUow;
        private readonly Mock<IRepository<Appointment>> _mockAppointmentRepo;
        private readonly Mock<IRepository<Doctor>> _mockDoctorRepo;
        private readonly Mock<IRepository<Patient>> _mockPatientRepo;
        private readonly AppointmentService _appointmentService;

        public AppointmentServiceTests()
        {
            var mapperConfig = new MapperConfiguration(
                cfg => cfg.AddProfile(new HospitalMapperProfile()),
                NullLoggerFactory.Instance
            );
            _mapper = mapperConfig.CreateMapper();

            _mockUow = new Mock<IUnitOfWork>();
            _mockAppointmentRepo = new Mock<IRepository<Appointment>>();
            _mockDoctorRepo = new Mock<IRepository<Doctor>>();
            _mockPatientRepo = new Mock<IRepository<Patient>>();

            _mockUow.Setup(u => u.Appointments).Returns(_mockAppointmentRepo.Object);
            _mockUow.Setup(u => u.Doctors).Returns(_mockDoctorRepo.Object);
            _mockUow.Setup(u => u.Patients).Returns(_mockPatientRepo.Object);

            _appointmentService = new AppointmentService(_mockUow.Object, _mapper);
        }

        [Fact]
        public void MakeAppointment_ValidData_CreatesAppointment()
        {
            // Arrange
            var validDate = DateTime.Now.AddDays(1);
            var dto = new AppointmentDTO { DoctorId = 1, PatientId = 1, AppointmentDate = validDate };

            _mockDoctorRepo.Setup(r => r.GetById(1)).Returns(new Doctor { Id = 1 });
            _mockPatientRepo.Setup(r => r.GetById(1)).Returns(new Patient { Id = 1 });

            _mockAppointmentRepo.Setup(r => r.Find(It.IsAny<Expression<Func<Appointment, bool>>>(), It.IsAny<Expression<Func<Appointment, object>>[]>()))
                .Returns(new List<Appointment>());

            // Act
            _appointmentService.MakeAppointment(dto);

            // Assert
            _mockAppointmentRepo.Verify(r => r.Create(It.Is<Appointment>(a => a.DoctorId == 1 && a.PatientId == 1)), Times.Once);
            _mockUow.Verify(u => u.Save(), Times.Once);
        }

        [Fact]
        public void MakeAppointment_PastDate_ThrowsValidationException()
        {
            // Arrange
            var pastDate = DateTime.Now.AddDays(-1);
            var dto = new AppointmentDTO { AppointmentDate = pastDate };

            // Act & Assert
            var ex = Assert.Throws<ValidationException>(() => _appointmentService.MakeAppointment(dto));
            Assert.Equal("Неможливо записатися на дату в минулому", ex.Message);
        }

        [Fact]
        public void MakeAppointment_TimeAlreadyTaken_ThrowsValidationException()
        {
            // Arrange
            var validDate = DateTime.Now.AddDays(1);
            var dto = new AppointmentDTO { DoctorId = 1, PatientId = 1, AppointmentDate = validDate };

            _mockDoctorRepo.Setup(r => r.GetById(1)).Returns(new Doctor { Id = 1 });
            _mockPatientRepo.Setup(r => r.GetById(1)).Returns(new Patient { Id = 1 });

            _mockAppointmentRepo.Setup(r => r.Find(It.IsAny<Expression<Func<Appointment, bool>>>(), It.IsAny<Expression<Func<Appointment, object>>[]>()))
                .Returns(new List<Appointment> { new Appointment { Id = 10 } });

            // Act & Assert
            var ex = Assert.Throws<ValidationException>(() => _appointmentService.MakeAppointment(dto));
            Assert.Equal("Цей час у лікаря вже зайнятий", ex.Message);
        }

        [Fact]
        public void CompleteAppointment_ValidId_UpdatesAppointment()
        {
            // Arrange
            var appointment = new Appointment { Id = 1, IsCompleted = false };
            _mockAppointmentRepo.Setup(r => r.GetById(1)).Returns(appointment);

            // Act
            _appointmentService.CompleteAppointment(1, "Пацієнт здоровий");

            // Assert
            Assert.True(appointment.IsCompleted);
            Assert.Equal("Пацієнт здоровий", appointment.Notes);
            _mockAppointmentRepo.Verify(r => r.Update(appointment), Times.Once);
            _mockUow.Verify(u => u.Save(), Times.Once);
        }
    }
}
