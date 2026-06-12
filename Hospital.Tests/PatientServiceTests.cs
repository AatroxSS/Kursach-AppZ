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
using Xunit;

namespace Hospital.Tests
{
    public class PatientServiceTests
    {
        private readonly IMapper _mapper;
        private readonly Mock<IUnitOfWork> _mockUow;
        private readonly Mock<IRepository<Patient>> _mockPatientRepo;
        private readonly Mock<IRepository<User>> _mockUserRepo;
        private readonly PatientService _patientService;

        public PatientServiceTests()
        {
            var mapperConfig = new MapperConfiguration(
                cfg => cfg.AddProfile(new HospitalMapperProfile()),
                NullLoggerFactory.Instance
            );
            _mapper = mapperConfig.CreateMapper();

            _mockUow = new Mock<IUnitOfWork>();
            _mockPatientRepo = new Mock<IRepository<Patient>>();
            _mockUserRepo = new Mock<IRepository<User>>();

            _mockUow.Setup(u => u.Patients).Returns(_mockPatientRepo.Object);
            _mockUow.Setup(u => u.Users).Returns(_mockUserRepo.Object);

            _patientService = new PatientService(_mockUow.Object, _mapper);
        }

        [Fact]
        public void AddPatient_ValidPatient_SavesPatient()
        {
            // Arrange
            var patientDto = new PatientDTO { FirstName = "Іван", LastName = "Іванов", UserId = 1 };
            _mockUserRepo.Setup(repo => repo.GetById(1)).Returns(new User { Id = 1 });

            // Act
            _patientService.AddPatient(patientDto);

            // Assert
            _mockPatientRepo.Verify(repo => repo.Create(It.Is<Patient>(p => p.FirstName == "Іван")), Times.Once);
            _mockUow.Verify(u => u.Save(), Times.Once);
        }

        [Fact]
        public void AddPatient_EmptyFirstName_ThrowsValidationException()
        {
            // Arrange
            var patientDto = new PatientDTO { FirstName = "", LastName = "Іванов", UserId = 1 };

            // Act & Assert
            var ex = Assert.Throws<ValidationException>(() => _patientService.AddPatient(patientDto));
            Assert.Equal("Ім'я обов'язкове", ex.Message);
        }

        [Fact]
        public void AddPatient_InvalidUserId_ThrowsValidationException()
        {
            // Arrange
            var patientDto = new PatientDTO { FirstName = "Іван", LastName = "Іванов", UserId = 0 };

            // Act & Assert
            var ex = Assert.Throws<ValidationException>(() => _patientService.AddPatient(patientDto));
            Assert.Equal("Необхідно вказати дійсний UserId для зв'язку з акаунтом", ex.Message);
        }

        [Fact]
        public void AddPatient_UserDoesNotExist_ThrowsValidationException()
        {
            // Arrange
            var patientDto = new PatientDTO { FirstName = "Іван", LastName = "Іванов", UserId = 99 };
            _mockUserRepo.Setup(repo => repo.GetById(99)).Returns((User)null);

            // Act & Assert
            var ex = Assert.Throws<ValidationException>(() => _patientService.AddPatient(patientDto));
            Assert.Equal("Користувача з вказаним UserId не існує", ex.Message);
        }

        [Fact]
        public void GetAllPatients_ReturnsMappedPatients()
        {
            // Arrange
            var patients = new List<Patient>
            {
                new Patient { Id = 1, FirstName = "Олег", LastName = "Петров" },
                new Patient { Id = 2, FirstName = "Анна", LastName = "Коваленко" }
            };
            _mockPatientRepo.Setup(repo => repo.GetAll()).Returns(patients);

            // Act
            var result = _patientService.GetAllPatients().ToList();

            // Assert
            Assert.Equal(2, result.Count);
            Assert.Equal("Олег", result[0].FirstName);
        }
    }
}