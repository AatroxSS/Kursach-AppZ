using Hospital.BLL.DTO;
using Hospital.BLL.DTOs;

namespace Hospital.BLL.Interfaces
{
    public interface IAuthService
    {
       
        string Login(string email, string password);

        void RegisterUser(UserRegisterDTO dto, string role = "RegisteredUser");
        IEnumerable<UserDTO> GetAllUsers();
        void ChangeUserRole(int userId, string newRole);
        void DeleteUser(int userId);
    }
}