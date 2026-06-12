using Hospital.BLL.Services;
using Hospital.DAL.EF;
using Hospital.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using Xunit;

namespace Hospital.Tests
{
    public class UserServiceTests : IDisposable
    {
        private readonly HospitalDbContext _context;
        private readonly UserService _userService;

        public UserServiceTests()
        {
            var options = new DbContextOptionsBuilder<HospitalDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new HospitalDbContext(options);
            _userService = new UserService(_context);
        }

        [Fact]
        public void ChangeUserRole_ValidRole_UpdatesRoleInDatabase()
        {
            // Arrange
            var user = new User { Id = 1, Email = "test@test.com", PasswordHash = "hash123", Role = Role.RegisteredUser };
            _context.Users.Add(user);
            _context.SaveChanges();

            // Act
            _userService.ChangeUserRole(1, "Manager");

            // Assert
            var updatedUser = _context.Users.Find(1);
            Assert.Equal(Role.Manager, updatedUser.Role);
        }

        [Fact]
        public void GetAllUsers_ReturnsMappedUserDTOs()
        {
            // Arrange
            _context.Users.Add(new User { Id = 1, Email = "user@test.com", PasswordHash = "hash123", Role = Role.RegisteredUser });
            _context.SaveChanges();

            // Act
            var result = _userService.GetAllUsers().ToList();

            // Assert
            Assert.Single(result);
            Assert.Equal("user@test.com", result[0].Email);
        }

        [Fact]
        public void DeleteUser_ExistingUser_RemovesFromDatabase()
        {
            // Arrange
            _context.Users.Add(new User { Id = 1, Email = "delete@test.com", PasswordHash = "hash123", Role = Role.RegisteredUser });
            _context.SaveChanges();

            // Act
            _userService.DeleteUser(1);

            // Assert
            Assert.Null(_context.Users.Find(1));
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }
    }
}