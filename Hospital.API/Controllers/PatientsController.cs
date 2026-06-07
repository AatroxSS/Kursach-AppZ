using Hospital.BLL.DTO;
using Hospital.BLL.Infrastructure;
using Hospital.BLL.Interfaces;
using Hospital.DAL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Hospital.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PatientsController : ControllerBase
{
    private readonly IPatientService _patientService;
    private readonly IUnitOfWork _unitOfWork;

    public PatientsController(IPatientService patientService, IUnitOfWork unitOfWork)
    {
        _patientService = patientService;
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    [Authorize(Roles = "Manager,Administrator")]
    public ActionResult<IEnumerable<PatientDTO>> GetAll()
    {
        return Ok(_patientService.GetAllPatients());
    }

    [HttpGet("me")]
    [Authorize(Roles = "RegisteredUser,Manager,Administrator")]
    public ActionResult<PatientDTO> GetMe()
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var patient = _unitOfWork.Patients.Find(p => p.UserId == userId.Value).FirstOrDefault();

        if (patient is null)
        {
            return NotFound("Для цього акаунта не знайдено профіль пацієнта.");
        }

        return Ok(new PatientDTO
        {
            Id = patient.Id,
            UserId = patient.UserId,
            FirstName = patient.FirstName,
            LastName = patient.LastName,
            DateOfBirth = patient.DateOfBirth
        });
    }

    [HttpPost]
    [Authorize(Roles = "Manager,Administrator")]
    public ActionResult AddPatient([FromBody] PatientDTO patientDto)
    {
        try
        {
            _patientService.AddPatient(patientDto);
            return Ok("Пацієнта успішно додано");
        }
        catch (ValidationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut]
    [Authorize(Roles = "Manager,Administrator")]
    public ActionResult UpdatePatient([FromBody] PatientDTO patientDto)
    {
        _patientService.UpdatePatient(patientDto);
        return Ok("Дані пацієнта оновлено");
    }

    private int? GetCurrentUserId()
    {
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? User.FindFirstValue("nameid")
                  ?? User.FindFirstValue("sub");

        return int.TryParse(raw, out var id) ? id : null;
    }
}
