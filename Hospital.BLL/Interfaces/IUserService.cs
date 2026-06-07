using Hospital.BLL.DTO;
using System.Collections.Generic;

namespace Hospital.BLL.Interfaces
{
    public interface IUserService
    {
        IEnumerable<UserDTO> GetAllUsers();
        void ChangeUserRole(int userId, string newRole);
        void DeleteUser(int userId);
    }
}