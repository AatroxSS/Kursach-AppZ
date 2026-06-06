using AutoMapper;
using Hospital.BLL.DTO;
using Hospital.BLL.Infrastructure;
using Hospital.BLL.Services;
using Hospital.DAL.Entities;
using Hospital.DAL.Interfaces;
using Moq;
using System.Numerics;
using System.Timers;
using Xunit;

namespace Hospital.Tests
{
    public class DoctorServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUoW;
        private readonly Mock<IMapper> _mockMapper;
        private readonly DoctorService _service;

        public DoctorServiceTests()
        {
            _mockUoW = new Mock<IUnitOfWork>();
            _mockMapper = new Mock<IMapper>();
            _service = new DoctorService(_mockUoW.Object, _mockMapper.Object);
        }

        [Theory]
        [InlineData("", "Петренко")]
        [InlineData("Олександр", "")]
        [InlineData(null, null)]
        public void AddDoctor_EmptyNameOrSurname_ThrowsValidationException(string firstName, string lastName)
        {
            var doctorDto = new DoctorDTO
            {
                FirstName = firstName,
                LastName = lastName,
                Specialization = "Хірург"
            };

            var exception = Assert.Throws<ValidationException>(() => _service.AddDoctor(doctorDto));
            Assert.Equal("Ім'я та прізвище лікаря є обов'язковими", exception.Message);
        }

        [Fact]
        public void AddDoctor_ValidData_CallsCreateAndSave()
        {
            var doctorDto = new DoctorDTO
            {
                FirstName = "Олександр",
                LastName = "Петренко",
                Specialization = "Терапевт"
            };

            var doctorEntity = new Doctor { FirstName = "Олександр", LastName = "Петренко" };

            _mockMapper.Setup(m => m.Map<DoctorDTO, Doctor>(doctorDto)).Returns(doctorEntity);

            _service.AddDoctor(doctorDto);

            _mockUoW.Verify(u => u.Doctors.Create(doctorEntity), Times.Once);
            _mockUoW.Verify(u => u.Save(), Times.Once);
        }
    }
}