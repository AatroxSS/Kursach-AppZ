using Hospital.BLL.DTO;
using Hospital.BLL.DTOs;
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
            // 1. Перевірка, чи існує такий email
            var existingUser = _unitOfWork.Users.Find(u => u.Email == dto.Email).FirstOrDefault();
            if (existingUser != null)
            {
                throw new Exception("Користувач з таким Email вже існує.");
            }

            // 2. Безпечне перетворення рядка ролі у тип Enum (Role)
            if (!Enum.TryParse<Role>(roleName, out var parsedRole))
            {
                parsedRole = Role.RegisteredUser; // Якщо передали якусь дурницю, ставимо пацієнта за замовчуванням
            }

            // Тимчасовий бекдор для тебе: якщо реєструєш цей email, стаєш адміном автоматично
            if (dto.Email == "admin@admin.com")
            {
                parsedRole = Role.Administrator;
            }

            // 3. СТВОРЕННЯ КОРИСТУВАЧА (Тепер змінна user існує)
            var user = new User
            {
                Email = dto.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Role = parsedRole
            };

            // 4. СТВОРЕННЯ ПАЦІЄНТА (Використовуємо змінну user ПІСЛЯ її створення)
            var patient = new Patient
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                DateOfBirth = DateTime.UtcNow.Date, // Заглушка для дати, щоб БД не сварилася
                User = user // Зв'язуємо пацієнта з його акаунтом
            };

            // 5. Збереження в базу даних
            _unitOfWork.Users.Add(user);
            _unitOfWork.Patients.Add(patient);
            _unitOfWork.Save();
        }

        public string Login(string email, string password)
        {
            var user = _unitOfWork.Users.Find(u => u.Email == email).FirstOrDefault();

            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                throw new Exception("Невірний Email або пароль.");
            }

            // ГЕНЕРАЦІЯ ТОКЕНА (Тут ми додаємо роль у токен!)
            var tokenHandler = new JwtSecurityTokenHandler();

            // ВАЖЛИВО: Цей ключ має збігатися з тим, що у Program.cs
            var key = Encoding.UTF8.GetBytes("ТВІЙ_ДУЖЕ_СЕКРЕТНИЙ_КЛЮЧ_ЯКИЙ_МАЄ_БУТИ_БІЛЬШЕ_16_СИМВОЛІВ");

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Email),
                // НАЙГОЛОВНІШИЙ РЯДОК ДЛЯ АДМІН-ПАНЕЛІ:
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        // --- МЕТОДИ ДЛЯ АДМІНІСТРАТОРА ---

        public IEnumerable<UserDTO> GetAllUsers()
        {
            var users = _unitOfWork.Users.GetAll();
            return users.Select(u => new UserDTO
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
            else
            {
                throw new Exception("Невідома роль.");
            }
        }

        public void DeleteUser(int id)
        {
            var user = _unitOfWork.Users.Get(id);
            if (user == null) throw new Exception("Користувача не знайдено.");

            _unitOfWork.Users.Delete(id);
            _unitOfWork.Save();
        }
    }
}