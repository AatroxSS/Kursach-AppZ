using Hospital.BLL.DTO; 
using Hospital.BLL.Services;
using Hospital.DAL.Entities;
using Hospital.DAL.Interfaces;
using Microsoft.Extensions.Configuration;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xunit;

namespace Hospital.Tests
{
    public class AuthServiceTests
    {
        private readonly Mock<IUnitOfWork> _mockUow;
        private readonly Mock<IRepository<User>> _mockUserRepo;
        private readonly Mock<IConfiguration> _mockConfig;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _mockUow = new Mock<IUnitOfWork>();
            _mockUserRepo = new Mock<IRepository<User>>();

            _mockConfig = new Mock<IConfiguration>();
            _mockConfig.Setup(c => c["Jwt:Key"]).Returns("SuperSecretKey1234567890qwertyuiop");

            _mockUow.Setup(u => u.Users).Returns(_mockUserRepo.Object);

            _authService = new AuthService(_mockUow.Object, _mockConfig.Object);
        }

        [Fact]
        public void RegisterUser_ValidData_CreatesUser()
        {
            // Arrange
            var dto = new UserRegisterDTO { Email = "new@test.com", Password = "123" };

            _mockUserRepo.Setup(r => r.Find(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<Expression<Func<User, object>>[]>()))
                .Returns(new List<User>());

            // Act
            _authService.RegisterUser(dto);

            // Assert
            _mockUserRepo.Verify(r => r.Create(It.Is<User>(u => u.Email == "new@test.com")), Times.Once);
            _mockUow.Verify(u => u.Save(), Times.Once);
        }

        [Fact]
        public void RegisterUser_ExistingEmail_ThrowsException()
        {
            // Arrange
            var dto = new UserRegisterDTO { Email = "test@test.com", Password = "123" };

            _mockUserRepo.Setup(r => r.Find(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<Expression<Func<User, object>>[]>()))
                .Returns(new List<User> { new User { Email = "test@test.com" } });

            // Act & Assert
            var ex = Assert.Throws<Exception>(() => _authService.RegisterUser(dto));
            Assert.Contains("вже існує", ex.Message);
        }

        [Fact]
        public void Login_InvalidPassword_ThrowsException()
        {
            // Arrange
            string hash = BCrypt.Net.BCrypt.HashPassword("RealPassword");

            _mockUserRepo.Setup(r => r.Find(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<Expression<Func<User, object>>[]>()))
                .Returns(new List<User> { new User { Email = "test@test.com", PasswordHash = hash } });

            // Act & Assert
            var ex = Assert.Throws<Exception>(() => _authService.Login("test@test.com", "WrongPassword"));
            Assert.Contains("Невірний", ex.Message); 
        }

        [Fact]
        public void Login_ValidCredentials_ReturnsJwtToken()
        {
            // Arrange
            string password = "SecretPassword123";
            string hash = BCrypt.Net.BCrypt.HashPassword(password);

            _mockUserRepo.Setup(r => r.Find(It.IsAny<Expression<Func<User, bool>>>(), It.IsAny<Expression<Func<User, object>>[]>()))
                .Returns(new List<User> { new User { Id = 1, Email = "test@test.com", PasswordHash = hash, Role = Role.Administrator } });

            // Act
            var token = _authService.Login("test@test.com", password);
            // Assert
            Assert.False(string.IsNullOrEmpty(token));
            Assert.Equal(2, token.Count(c => c == '.')); 
        }
    }
}