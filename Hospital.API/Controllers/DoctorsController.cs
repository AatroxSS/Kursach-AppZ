using Hospital.BLL.DTO;
using Hospital.BLL.Infrastructure;
using Hospital.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DoctorsController : ControllerBase
    {
        private readonly IDoctorService _doctorService;

        public DoctorsController(IDoctorService doctorService)
        {
            _doctorService = doctorService;
        }

        // Доступно всім (навіть незареєстрованим)
        [HttpGet]
        public ActionResult<IEnumerable<DoctorDTO>> GetAll()
        {
            return Ok(_doctorService.GetAllDoctors());
        }

        // Доступно всім
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

        // Доступно всім
        [HttpGet("specialization/{spec}")]
        public ActionResult<IEnumerable<DoctorDTO>> GetBySpecialization(string spec)
        {
            return Ok(_doctorService.FindDoctorsBySpecialization(spec));
        }

        // Тільки Менеджер або Адміністратор
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

        // Тільки Менеджер або Адміністратор
        [HttpPut]
        [Authorize(Roles = "Manager,Administrator")]
        public ActionResult UpdateDoctor([FromBody] DoctorDTO doctorDto)
        {
            _doctorService.UpdateDoctor(doctorDto);
            return Ok("Дані лікаря оновлено");
        }
    }
}