using Hospital.BLL.DTO;
using Hospital.BLL.Infrastructure;
using Hospital.BLL.Interfaces;
using Hospital.DAL.Entities;
using Hospital.DAL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Hospital.API.Controllers;

public sealed class AppointmentCreateDTO
{
    public int DoctorId { get; set; }
    public DateTime AppointmentDate { get; set; }
    public string? Notes { get; set; }
}

[ApiController]
[Route("api/[controller]")]
public class AppointmentsController : ControllerBase
{
    private readonly IAppointmentService _appointmentService;
    private readonly IUnitOfWork _unitOfWork;

    public AppointmentsController(IAppointmentService appointmentService, IUnitOfWork unitOfWork)
    {
        _appointmentService = appointmentService;
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    [Authorize(Roles = "RegisteredUser,Manager,Administrator")]
    public ActionResult<IEnumerable<AppointmentDTO>> GetMyAppointments()
    {
        var appointments = _unitOfWork.Appointments.GetAll().ToList();

        if (User.IsInRole("Administrator"))
        {
            return Ok(appointments.Select(ToDto));
        }

        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        if (User.IsInRole("Manager"))
        {
            var doctor = _unitOfWork.Doctors.Find(d => d.UserId == userId.Value).FirstOrDefault();
            if (doctor is null)
            {
                return Ok(Array.Empty<AppointmentDTO>());
            }

            return Ok(appointments.Where(a => a.DoctorId == doctor.Id).Select(ToDto));
        }

        var patient = _unitOfWork.Patients.Find(p => p.UserId == userId.Value).FirstOrDefault();
        if (patient is null)
        {
            return Ok(Array.Empty<AppointmentDTO>());
        }

        return Ok(appointments.Where(a => a.PatientId == patient.Id).Select(ToDto));
    }

    [HttpPost]
    [Authorize(Roles = "RegisteredUser,Manager,Administrator")]
    public ActionResult MakeAppointment([FromBody] AppointmentDTO appointmentDto)
    {
        try
        {
            _appointmentService.MakeAppointment(appointmentDto);
            return Ok("Запис на прийом успішно створено");
        }
        catch (ValidationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("my")]
    [Authorize(Roles = "RegisteredUser")]
    public ActionResult MakeMyAppointment([FromBody] AppointmentCreateDTO dto)
    {
        var userId = GetCurrentUserId();

        if (userId is null)
        {
            return Unauthorized();
        }

        var patient = _unitOfWork.Patients.Find(p => p.UserId == userId.Value).FirstOrDefault();

        if (patient is null)
        {
            return BadRequest("Для поточного користувача не знайдено профіль пацієнта.");
        }

        try
        {
            _appointmentService.MakeAppointment(new AppointmentDTO
            {
                DoctorId = dto.DoctorId,
                PatientId = patient.Id,
                AppointmentDate = dto.AppointmentDate,
                Notes = dto.Notes
            });

            return Ok("Запис на прийом успішно створено");
        }
        catch (ValidationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("doctor/{doctorId}")]
    public ActionResult<IEnumerable<AppointmentDTO>> GetDoctorAppointments(int doctorId)
    {
        var appointments = _unitOfWork.Appointments
            .Find(a => a.DoctorId == doctorId)
            .Select(ToDto);

        return Ok(appointments);
    }

    [HttpPatch("{id}/complete")]
    [Authorize(Roles = "Manager,Administrator")]
    public ActionResult CompleteAppointment(int id, [FromBody] string notes)
    {
        try
        {
            _appointmentService.CompleteAppointment(id, notes);
            return Ok("Прийом завершено");
        }
        catch (ValidationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    private AppointmentDTO ToDto(Appointment appointment)
    {
        var doctor = _unitOfWork.Doctors.GetById(appointment.DoctorId);
        var patient = _unitOfWork.Patients.GetById(appointment.PatientId);

        return new AppointmentDTO
        {
            Id = appointment.Id,
            DoctorId = appointment.DoctorId,
            PatientId = appointment.PatientId,
            AppointmentDate = appointment.AppointmentDate,
            Notes = appointment.Notes,
            IsCompleted = appointment.IsCompleted,
            DoctorFullName = doctor is null ? null : $"{doctor.FirstName} {doctor.LastName}".Trim(),
            PatientFullName = patient is null ? null : $"{patient.FirstName} {patient.LastName}".Trim()
        };
    }

    private int? GetCurrentUserId()
    {
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? User.FindFirstValue("nameid")
                  ?? User.FindFirstValue("sub");

        return int.TryParse(raw, out var id) ? id : null;
    }
}
