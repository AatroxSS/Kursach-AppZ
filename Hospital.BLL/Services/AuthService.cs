using Hospital.BLL.DTO;
using Hospital.BLL.Interfaces;
using Hospital.DAL.Entities;
using Hospital.DAL.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace Hospital.BLL.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AuthService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public void RegisterUser(UserRegisterDTO dto, string roleName = "RegisteredUser")
        {
            var existingUser = _unitOfWork.Users.Find(u => u.Email == dto.Email).FirstOrDefault();
            if (existingUser != null)
            {
                throw new Exception("Користувач з таким Email вже існує.");
            }

            if (!Enum.TryParse<Role>(roleName, out var parsedRole))
            {
                parsedRole = Role.RegisteredUser;
            }

            if (dto.Email == "admin@admin.com")
            {
                parsedRole = Role.Administrator;
            }

            var user = new User
            {
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = parsedRole
            };

            _unitOfWork.Users.Create(user); // ВИПРАВЛЕНО Add -> Create

            if (parsedRole == Role.RegisteredUser)
            {
                _unitOfWork.Patients.Create(new Patient { FirstName = dto.FirstName, LastName = dto.LastName, DateOfBirth = DateTime.UtcNow.Date, User = user }); // ВИПРАВЛЕНО Add -> Create
            }
            else if (parsedRole == Role.Manager)
            {
                _unitOfWork.Doctors.Create(new Doctor { FirstName = dto.FirstName, LastName = dto.LastName, Specialization = "Лікар загальної практики", User = user }); // ВИПРАВЛЕНО Add -> Create
            }

            _unitOfWork.Save();
        }

        public string Login(string email, string password)
        {
            var user = _unitOfWork.Users.Find(u => u.Email == email).FirstOrDefault();
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                throw new Exception("Невірний Email або пароль.");
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes("ТВІЙ_ДУЖЕ_СЕКРЕТНИЙ_КЛЮЧ_ЯКИЙ_МАЄ_БУТИ_БІЛЬШЕ_16_СИМВОЛІВ");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            return tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
        }

        public IEnumerable<UserDTO> GetAllUsers()
        {
            return _unitOfWork.Users.GetAll().Select(u => new UserDTO
            {
                Id = u.Id,
                Email = u.Email,
                Role = u.Role.ToString()
            });
        }

        public void ChangeUserRole(int id, string newRole)
        {
            var user = _unitOfWork.Users.GetById(id); // ВИПРАВЛЕНО Get -> GetById
            if (user == null) throw new Exception("Користувача не знайдено.");
            if (Enum.TryParse<Role>(newRole, out var parsedRole))
            {
                user.Role = parsedRole;
                _unitOfWork.Users.Update(user);
                _unitOfWork.Save();
            }
        }

        public void DeleteUser(int id)
        {
            var user = _unitOfWork.Users.GetById(id); // ВИПРАВЛЕНО Get -> GetById
            if (user != null)
            {
                _unitOfWork.Users.Delete(id);
                _unitOfWork.Save();
            }
        }
    }
}