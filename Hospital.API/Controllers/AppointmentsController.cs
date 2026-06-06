using Hospital.BLL.DTO;
using Hospital.BLL.Infrastructure;
using Hospital.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Hospital.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentsController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;

        public AppointmentsController(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        // Записатись можуть тільки Зареєстровані користувачі (або вище)
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

        // Усі можуть дивитися графік (зайнятість лікаря)
        [HttpGet("doctor/{doctorId}")]
        public ActionResult<IEnumerable<AppointmentDTO>> GetDoctorAppointments(int doctorId)
        {
            return Ok(_appointmentService.GetAppointmentsByDoctor(doctorId));
        }

        // Завершити прийом може тільки Менеджер або Адмін
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
    }
}