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
                // Використовуємо твій Enum Role
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