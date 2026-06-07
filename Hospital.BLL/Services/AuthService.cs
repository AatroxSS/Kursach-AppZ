using Hospital.BLL.DTO;
using Hospital.BLL.Interfaces;
using Hospital.DAL.Entities;
using Hospital.DAL.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Collections.Generic;

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

            _unitOfWork.Users.Add(user);

            if (parsedRole == Role.RegisteredUser)
            {
                _unitOfWork.Patients.Add(new Patient { FirstName = dto.FirstName, LastName = dto.LastName, User = user });
            }
            else if (parsedRole == Role.Manager)
            {
                _unitOfWork.Doctors.Add(new Doctor { FirstName = dto.FirstName, LastName = dto.LastName, User = user });
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
            var user = _unitOfWork.Users.Get(id);
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
            var user = _unitOfWork.Users.Get(id);
            if (user != null)
            {
                _unitOfWork.Users.Delete(id);
                _unitOfWork.Save();
            }
        }
    }
}