namespace Hospital.UI.Models;

public sealed class UserRowModel
{
    public int Id { get; set; }
    public string Email { get; set; } = "";
    public string Role { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
}

public sealed class RegisterUserModel
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
}

public sealed class LoginRequestModel
{
    public string Email { get; set; } = "";
    public string Password { get; set; } = "";
}

public sealed class LoginResponseModel
{
    public string Token { get; set; } = "";
}

public sealed class DoctorModel
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Specialization { get; set; } = "";
    public int ExperienceYears { get; set; }

    public string FullName => $"{FirstName} {LastName}".Trim();
}

public sealed class PatientModel
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public DateTime DateOfBirth { get; set; }

    public string FullName => $"{FirstName} {LastName}".Trim();
}

public sealed class AppointmentModel
{
    public int Id { get; set; }
    public int DoctorId { get; set; }
    public string? DoctorFullName { get; set; }
    public int PatientId { get; set; }
    public string? PatientFullName { get; set; }
    public DateTime AppointmentDate { get; set; }
    public string? Notes { get; set; }
    public bool IsCompleted { get; set; }
}

public sealed class AppointmentCreateModel
{
    public int DoctorId { get; set; }
    public DateTime AppointmentDate { get; set; }
    public string? Notes { get; set; }
}
