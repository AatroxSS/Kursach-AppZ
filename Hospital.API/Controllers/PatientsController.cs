using Hospital.BLL.DTO;
using Hospital.BLL.Infrastructure;
using Hospital.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PatientsController : ControllerBase
    {
        private readonly IPatientService _patientService;

        public PatientsController(IPatientService patientService)
        {
            _patientService = patientService;
        }

        // Тільки Менеджер або Адміністратор
        [HttpGet]
        [Authorize(Roles = "Manager,Administrator")]
        public ActionResult<IEnumerable<PatientDTO>> GetAll()
        {
            return Ok(_patientService.GetAllPatients());
        }

        // Тільки Менеджер або Адміністратор
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

        // Тільки Менеджер або Адміністратор
        [HttpPut]
        [Authorize(Roles = "Manager,Administrator")]
        public ActionResult UpdatePatient([FromBody] PatientDTO patientDto)
        {
            _patientService.UpdatePatient(patientDto);
            return Ok("Дані пацієнта оновлено");
        }
    }
}