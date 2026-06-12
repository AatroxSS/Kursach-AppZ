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
    public class DoctorServiceTests
    {
        private readonly IMapper _mapper;
        private readonly Mock<IUnitOfWork> _mockUow;
        private readonly Mock<IRepository<Doctor>> _mockDoctorRepo;
        private readonly DoctorService _doctorService;

        public DoctorServiceTests()
        {
            var mapperConfig = new MapperConfiguration(
                cfg => cfg.AddProfile(new HospitalMapperProfile()),
                NullLoggerFactory.Instance);
            _mapper = mapperConfig.CreateMapper();

            _mockUow = new Mock<IUnitOfWork>();
            _mockDoctorRepo = new Mock<IRepository<Doctor>>();

            _mockUow.Setup(u => u.Doctors).Returns(_mockDoctorRepo.Object);

            _doctorService = new DoctorService(_mockUow.Object, _mapper);
        }

        [Fact]
        public void GetAllDoctors_ReturnsMappedDoctors()
        {
            // Arrange
            var doctors = new List<Doctor> { new Doctor { Id = 1, Specialization = "Хірург" } };
            _mockDoctorRepo.Setup(r => r.GetAll()).Returns(doctors);

            // Act
            var result = _doctorService.GetAllDoctors().ToList();

            // Assert
            Assert.Single(result);
        }

        [Fact]
        public void FindDoctorsBySpecialization_ReturnsFilteredDoctors()
        {
            // Arrange
            var doctors = new List<Doctor>
            {
                new Doctor { Id = 1, Specialization = "Хірург" },
                new Doctor { Id = 2, Specialization = "Терапевт" }
            };

            _mockDoctorRepo.Setup(r => r.Find(It.IsAny<Expression<Func<Doctor, bool>>>(), It.IsAny<Expression<Func<Doctor, object>>[]>()))
                .Returns((Expression<Func<Doctor, bool>> predicate, Expression<Func<Doctor, object>>[] includes) =>
                    doctors.Where(predicate.Compile()).ToList());

            // Act
            var result = _doctorService.FindDoctorsBySpecialization("Хірург").ToList();

            // Assert
            Assert.Single(result);
            Assert.Equal("Хірург", result.First().Specialization);
        }

        [Fact]
        public void GetDoctor_ValidId_ReturnsDoctor()
        {
            // Arrange
            _mockDoctorRepo.Setup(r => r.GetById(1)).Returns(new Doctor { Id = 1 });

            // Act
            var result = _doctorService.GetDoctor(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.Id);
        }

        [Fact]
        public void GetDoctor_InvalidId_ThrowsValidationException()
        {
            // Arrange
            _mockDoctorRepo.Setup(r => r.GetById(99)).Returns((Doctor)null);

            // Act & Assert
            Assert.Throws<ValidationException>(() => _doctorService.GetDoctor(99));
        }

        [Theory]
        [InlineData("", "Іванов")]
        [InlineData("Іван", "")]
        [InlineData(" ", "   ")]
        public void AddDoctor_InvalidName_ThrowsValidationException(string firstName, string lastName)
        {
            // Arrange
            var dto = new DoctorDTO { FirstName = firstName, LastName = lastName };

            // Act & Assert
            var ex = Assert.Throws<ValidationException>(() => _doctorService.AddDoctor(dto));
            Assert.Equal("Ім'я та прізвище лікаря є обов'язковими", ex.Message);
        }

        [Fact]
        public void AddDoctor_ValidData_CreatesAndSaves()
        {
            // Arrange
            var dto = new DoctorDTO { FirstName = "Іван", LastName = "Іванов", Specialization = "ЛОР" };

            // Act
            _doctorService.AddDoctor(dto);
            // Assert
            _mockDoctorRepo.Verify(r => r.Create(It.IsAny<Doctor>()), Times.Once);
            _mockUow.Verify(u => u.Save(), Times.Once);
        }
    }
}