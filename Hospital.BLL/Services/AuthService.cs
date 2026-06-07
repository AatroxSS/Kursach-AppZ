using Hospital.BLL.DTO;
using Hospital.BLL.Interfaces;
using Hospital.DAL.Entities;
using Hospital.DAL.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Hospital.BLL.Services;

public class AuthService : IAuthService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;

    public AuthService(IUnitOfWork unitOfWork, IConfiguration configuration)
    {
        _unitOfWork = unitOfWork;
        _configuration = configuration;
    }

    public void RegisterUser(UserRegisterDTO dto, string roleName = "RegisteredUser")
    {
        var email = dto.Email?.Trim() ?? "";

        if (string.IsNullOrWhiteSpace(email))
        {
            throw new Exception("Email не може бути порожнім.");
        }

        if (string.IsNullOrWhiteSpace(dto.Password))
        {
            throw new Exception("Пароль не може бути порожнім.");
        }

        var existingUser = _unitOfWork.Users
            .Find(u => u.Email.ToLower() == email.ToLower())
            .FirstOrDefault();

        if (existingUser is not null)
        {
            throw new Exception("Користувач з таким Email вже існує.");
        }

        if (!Enum.TryParse<Role>(roleName, out var parsedRole))
        {
            parsedRole = Role.RegisteredUser;
        }

        if (email.Equals("admin@admin.com", StringComparison.OrdinalIgnoreCase))
        {
            parsedRole = Role.Administrator;
        }

        var firstName = string.IsNullOrWhiteSpace(dto.FirstName) ? "Новий" : dto.FirstName.Trim();
        var lastName = string.IsNullOrWhiteSpace(dto.LastName) ? "Користувач" : dto.LastName.Trim();

        var user = new User
        {
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            FirstName = firstName,
            LastName = lastName,
            Role = parsedRole
        };

        _unitOfWork.Users.Create(user);
        _unitOfWork.Save();

        if (parsedRole == Role.RegisteredUser)
        {
            _unitOfWork.Patients.Create(new Patient
            {
                UserId = user.Id,
                FirstName = firstName,
                LastName = lastName,
                DateOfBirth = DateTime.UtcNow.Date
            });
        }
        else if (parsedRole == Role.Manager)
        {
            _unitOfWork.Doctors.Create(new Doctor
            {
                UserId = user.Id,
                FirstName = firstName,
                LastName = lastName,
                Specialization = "Лікар загальної практики",
                ExperienceYears = 0
            });
        }

        _unitOfWork.Save();
    }

    public string Login(string email, string password)
    {
        var normalizedEmail = email?.Trim() ?? "";

        var user = _unitOfWork.Users
            .Find(u => u.Email.ToLower() == normalizedEmail.ToLower())
            .FirstOrDefault();

        if (user is null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            throw new Exception("Невірний Email або пароль.");
        }

        var jwtKey = _configuration["Jwt:Key"]
            ?? throw new InvalidOperationException("Jwt:Key is missing in appsettings.json");

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Name, user.Email),
            new(ClaimTypes.Role, user.Role.ToString()),
            new("firstName", user.FirstName ?? ""),
            new("lastName", user.LastName ?? "")
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddDays(7),
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"],
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        return tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
    }

    public IEnumerable<UserDTO> GetAllUsers()
    {
        return _unitOfWork.Users.GetAll()
            .Select(u => new UserDTO
            {
                Id = u.Id,
                Email = u.Email,
                Role = u.Role.ToString(),
                FirstName = u.FirstName ?? "",
                LastName = u.LastName ?? ""
            })
            .OrderBy(u => u.Id);
    }

    public void ChangeUserRole(int id, string newRole)
    {
        var user = _unitOfWork.Users.GetById(id);

        if (user is null)
        {
            throw new Exception("Користувача не знайдено.");
        }

        if (!Enum.TryParse<Role>(newRole, out var parsedRole))
        {
            throw new Exception("Невідома роль користувача.");
        }

        user.Role = parsedRole;
        _unitOfWork.Users.Update(user);

        SyncLinkedProfile(user, parsedRole);
        _unitOfWork.Save();
    }

    public void DeleteUser(int id)
    {
        var user = _unitOfWork.Users.GetById(id);

        if (user is null)
        {
            return;
        }

        var patientIds = _unitOfWork.Patients.Find(p => p.UserId == id).Select(p => p.Id).ToList();
        var doctorIds = _unitOfWork.Doctors.Find(d => d.UserId == id).Select(d => d.Id).ToList();

        foreach (var appointment in _unitOfWork.Appointments
                     .Find(a => patientIds.Contains(a.PatientId) || doctorIds.Contains(a.DoctorId))
                     .ToList())
        {
            _unitOfWork.Appointments.Delete(appointment.Id);
        }

        foreach (var patientId in patientIds)
        {
            _unitOfWork.Patients.Delete(patientId);
        }

        foreach (var doctorId in doctorIds)
        {
            _unitOfWork.Doctors.Delete(doctorId);
        }

        _unitOfWork.Users.Delete(id);
        _unitOfWork.Save();
    }

    private void SyncLinkedProfile(User user, Role role)
    {
        var doctors = _unitOfWork.Doctors.Find(d => d.UserId == user.Id).ToList();
        var patients = _unitOfWork.Patients.Find(p => p.UserId == user.Id).ToList();

        if (role == Role.Manager)
        {
            foreach (var patient in patients)
            {
                _unitOfWork.Patients.Delete(patient.Id);
            }

            if (!doctors.Any())
            {
                _unitOfWork.Doctors.Create(new Doctor
                {
                    UserId = user.Id,
                    FirstName = string.IsNullOrWhiteSpace(user.FirstName) ? "Новий" : user.FirstName,
                    LastName = string.IsNullOrWhiteSpace(user.LastName) ? "Лікар" : user.LastName,
                    Specialization = "Лікар загальної практики",
                    ExperienceYears = 0
                });
            }
        }
        else if (role == Role.RegisteredUser)
        {
            foreach (var doctor in doctors)
            {
                _unitOfWork.Doctors.Delete(doctor.Id);
            }

            if (!patients.Any())
            {
                _unitOfWork.Patients.Create(new Patient
                {
                    UserId = user.Id,
                    FirstName = string.IsNullOrWhiteSpace(user.FirstName) ? "Новий" : user.FirstName,
                    LastName = string.IsNullOrWhiteSpace(user.LastName) ? "Пацієнт" : user.LastName,
                    DateOfBirth = DateTime.UtcNow.Date
                });
            }
        }
        else if (role == Role.Administrator)
        {
            foreach (var patient in patients)
            {
                _unitOfWork.Patients.Delete(patient.Id);
            }

            foreach (var doctor in doctors)
            {
                _unitOfWork.Doctors.Delete(doctor.Id);
            }
        }
    }
}
