using Hospital.BLL.DTO;
using Hospital.BLL.DTOs;
using Hospital.BLL.Interfaces;
using Hospital.DAL.EF;
using Hospital.DAL.Entities;

namespace Hospital.BLL.Services
{
    public class AuthService : IAuthService
    {
        private readonly HospitalDbContext _context;

        public AuthService(HospitalDbContext context)
        {
            _context = context;
        }

        public string Login(string email, string password)
        {
            var user = _context.Users.FirstOrDefault(u => u.Email == email);
            if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
            {
                throw new Exception("Невірний email або пароль");
            }
            return "jwt_token_stub";
        }

        public void RegisterUser(UserRegisterDTO dto)
        {
            var hash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

         
            System.Diagnostics.Debug.WriteLine($"РЕЄСТРАЦІЯ: Email={dto.Email}, Hash={hash}");

            var user = new User
            {
                Email = dto.Email,
                PasswordHash = hash,
                Role = Role.RegisteredUser
            };

            _context.Users.Add(user);
            _context.SaveChanges();
        }
        public IEnumerable<UserDTO> GetAllUsers()
        {
            return _context.Users.Select(u => new UserDTO
            {
                Id = u.Id,
                Email = u.Email,
                Role = u.Role.ToString()
            }).ToList();
        }

        public void ChangeUserRole(int userId, string newRole)
        {
            var user = _context.Users.Find(userId);
            if (user != null)
            {
                // Перетворюємо рядок "Doctor"/"Patient" в Enum
                user.Role = Enum.Parse<Role>(newRole);
                _context.SaveChanges();
            }
        }

        public void DeleteUser(int userId)
        {
            var user = _context.Users.Find(userId);
            if (user != null)
            {
                _context.Users.Remove(user);
                _context.SaveChanges();
            }
        }
        }
    }

