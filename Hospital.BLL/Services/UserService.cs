using Hospital.BLL.DTO;
using Hospital.BLL.Interfaces;
using Hospital.DAL.EF;
using Hospital.DAL.Entities;

namespace Hospital.BLL.Services
{
    public class UserService : IUserService
    {
        private readonly HospitalDbContext _context;

        public UserService(HospitalDbContext context)
        {
            _context = context;
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
                user.Role = Enum.Parse<Role>(newRole);

                if (user.Role == Role.Manager)
                {
                    // Створюємо лікаря, якщо його ще немає
                    if (!_context.Doctors.Any(d => d.UserId == userId))
                    {
                        _context.Doctors.Add(new Doctor { UserId = userId, FirstName = "Новий", LastName = "Лікар", Specialization = "Не вказано" });
                    }

                    // ВИДАЛЯЄМО користувача з таблиці пацієнтів (щоб не було дублювання)
                    var patient = _context.Patients.FirstOrDefault(p => p.UserId == userId);
                    if (patient != null) _context.Patients.Remove(patient);
                }
                else if (user.Role == Role.RegisteredUser)
                {
                    // Створюємо пацієнта, якщо його ще немає
                    if (!_context.Patients.Any(p => p.UserId == userId))
                    {
                        _context.Patients.Add(new Patient { UserId = userId, FirstName = "Новий", LastName = "Пацієнт", DateOfBirth = DateTime.UtcNow });
                    }

                    // ВИДАЛЯЄМО з таблиці лікарів
                    var doctor = _context.Doctors.FirstOrDefault(d => d.UserId == userId);
                    if (doctor != null) _context.Doctors.Remove(doctor);
                }

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