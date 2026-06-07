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
public class DoctorsController : ControllerBase
{
    private readonly IDoctorService _doctorService;
    private readonly IUnitOfWork _unitOfWork;

    public DoctorsController(IDoctorService doctorService, IUnitOfWork unitOfWork)
    {
        _doctorService = doctorService;
        _unitOfWork = unitOfWork;
    }

    [HttpGet]
    public ActionResult<IEnumerable<DoctorDTO>> GetAll()
    {
        return Ok(_doctorService.GetAllDoctors());
    }

    [HttpGet("me")]
    [Authorize(Roles = "Manager,Administrator")]
    public ActionResult<DoctorDTO> GetMe()
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var doctor = _unitOfWork.Doctors.Find(d => d.UserId == userId.Value).FirstOrDefault();

        if (doctor is null)
        {
            return NotFound("Для цього акаунта не знайдено профіль лікаря.");
        }

        return Ok(new DoctorDTO
        {
            Id = doctor.Id,
            FirstName = doctor.FirstName,
            LastName = doctor.LastName,
            Specialization = doctor.Specialization,
            ExperienceYears = doctor.ExperienceYears
        });
    }

    [HttpGet("{id}")]
    public ActionResult<DoctorDTO> GetById(int id)
    {
        try
        {
            return Ok(_doctorService.GetDoctor(id));
        }
        catch (ValidationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("specialization/{spec}")]
    public ActionResult<IEnumerable<DoctorDTO>> GetBySpecialization(string spec)
    {
        return Ok(_doctorService.FindDoctorsBySpecialization(spec));
    }

    [HttpPost]
    [Authorize(Roles = "Manager,Administrator")]
    public ActionResult AddDoctor([FromBody] DoctorDTO doctorDto)
    {
        try
        {
            _doctorService.AddDoctor(doctorDto);
            return Ok("Лікаря успішно додано");
        }
        catch (ValidationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut]
    [Authorize(Roles = "Manager,Administrator")]
    public ActionResult UpdateDoctor([FromBody] DoctorDTO doctorDto)
    {
        _doctorService.UpdateDoctor(doctorDto);
        return Ok("Дані лікаря оновлено");
    }

    private int? GetCurrentUserId()
    {
        var raw = User.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? User.FindFirstValue("nameid")
                  ?? User.FindFirstValue("sub");

        return int.TryParse(raw, out var id) ? id : null;
    }
}
