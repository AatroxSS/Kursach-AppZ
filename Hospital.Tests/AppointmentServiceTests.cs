using AutoMapper;
using Hospital.BLL.DTO;
using Hospital.BLL.Infrastructure;
using Hospital.BLL.Services;
using Hospital.DAL.Entities;
using Hospital.DAL.Interfaces;
using Moq;
using System.Linq.Expressions;
using Xunit;

namespace Hospital.Tests
{
    public class AppointmentServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUoW = new();
        private readonly Mock<IMapper> _mockMapper = new();
        private readonly Mock<IRepository<Doctor>> _doctorRepo = new();
        private readonly Mock<IRepository<Patient>> _patientRepo = new();
        private readonly Mock<IRepository<Appointment>> _appointmentRepo = new();
        private readonly AppointmentService _service;

        public AppointmentServiceTests()
        {
            _mockUoW.SetupGet(u => u.Doctors).Returns(_doctorRepo.Object);
            _mockUoW.SetupGet(u => u.Patients).Returns(_patientRepo.Object);
            _mockUoW.SetupGet(u => u.Appointments).Returns(_appointmentRepo.Object);

            _appointmentRepo
                .Setup(r => r.Find(It.IsAny<Expression<Func<Appointment, bool>>>()))
                .Returns(new List<Appointment>());

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

            _doctorRepo.Setup(r => r.GetById(99)).Returns((Doctor)null!);

            var exception = Assert.Throws<ValidationException>(() => _service.MakeAppointment(appointmentDto));
            Assert.Equal("Обраного лікаря не знайдено", exception.Message);
        }

        [Fact]
        public void MakeAppointment_PatientDoesNotExist_ThrowsValidationException()
        {
            var appointmentDto = new AppointmentDTO
            {
                DoctorId = 1,
                PatientId = 99,
                AppointmentDate = DateTime.Now.AddDays(1)
            };

            _doctorRepo.Setup(r => r.GetById(1)).Returns(new Doctor { Id = 1, FirstName = "Іван", LastName = "Іванов" });
            _patientRepo.Setup(r => r.GetById(99)).Returns((Patient)null!);

            var exception = Assert.Throws<ValidationException>(() => _service.MakeAppointment(appointmentDto));
            Assert.Equal("Пацієнта не знайдено", exception.Message);
        }

        [Fact]
        public void MakeAppointment_DoctorAlreadyBooked_ThrowsValidationException()
        {
            var appointmentDate = DateTime.Now.AddDays(2);
            var appointmentDto = new AppointmentDTO
            {
                DoctorId = 1,
                PatientId = 1,
                AppointmentDate = appointmentDate
            };

            _doctorRepo.Setup(r => r.GetById(1)).Returns(new Doctor { Id = 1, FirstName = "Іван", LastName = "Іванов" });
            _patientRepo.Setup(r => r.GetById(1)).Returns(new Patient { Id = 1, FirstName = "Петро", LastName = "Петров" });
            _appointmentRepo
                .Setup(r => r.Find(It.IsAny<Expression<Func<Appointment, bool>>>()))
                .Returns(new List<Appointment> { new Appointment { Id = 1, DoctorId = 1, PatientId = 1, AppointmentDate = appointmentDate } });

            var exception = Assert.Throws<ValidationException>(() => _service.MakeAppointment(appointmentDto));
            Assert.Equal("На цей час лікар вже має запис", exception.Message);
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

            _doctorRepo.Setup(r => r.GetById(1)).Returns(new Doctor { Id = 1, FirstName = "Іван", LastName = "Іванов" });
            _patientRepo.Setup(r => r.GetById(1)).Returns(new Patient { Id = 1, FirstName = "Петро", LastName = "Петров" });

            _service.MakeAppointment(appointmentDto);

            _appointmentRepo.Verify(r => r.Create(It.IsAny<Appointment>()), Times.Once);
            _mockUoW.Verify(u => u.Save(), Times.Once);
        }
    }
}
