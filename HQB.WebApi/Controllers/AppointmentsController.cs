using HQB.WebApi.Models;
using HQB.WebApi.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HQB.WebApi.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class AppointmentsController : ControllerBase
    {
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IAuthenticationService _authenticationService;
        private readonly ILogger<AppointmentsController> _logger;

        public AppointmentsController(ILogger<AppointmentsController> logger, IAppointmentRepository appointmentRepository, IAuthenticationService authenticationService)
        {
            _appointmentRepository = appointmentRepository;
            _authenticationService = authenticationService;
            _logger = logger;
        }

        [HttpGet(Name = "GetAppointmentsByTreatmentId")]
        public async Task<ActionResult<IEnumerable<Appointment>>> GetAppointmentsByTreatmentId([FromQuery] Guid treatmentId)
        {
            var loggedInUserId = _authenticationService.GetCurrentAuthenticatedUserId();
            if (string.IsNullOrWhiteSpace(loggedInUserId))
            {
                _logger.LogWarning("No authenticated user found");
                return Unauthorized("No authenticated user found");
            }

            try
            {
                if (treatmentId == Guid.Empty)
                {
                    _logger.LogWarning("Invalid treatment ID provided: {TreatmentId}", treatmentId);
                    return BadRequest("The provided treatment ID is invalid. Please provide a valid ID.");
                }

                _logger.LogInformation("Fetching appointments for treatment ID: {TreatmentId} from the repository", treatmentId);
                var appointments = await _appointmentRepository.GetAppointmentsByTreatmentIdAsync(treatmentId);
                if (appointments == null || !appointments.Any())
                {
                    _logger.LogWarning("No appointments found for the provided treatment ID: {TreatmentId}", treatmentId);
                    return NotFound($"No appointments found for the treatment ID: {treatmentId}");
                }

                _logger.LogInformation("Successfully fetched appointments for treatment ID: {TreatmentId}", treatmentId);

                var sortedList = appointments.OrderBy(x => x.Sequence).ToList();
                return Ok(sortedList);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching appointments");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request");
            }
        }

        [HttpGet("{id}", Name = "GetAppointmentById")]
        public async Task<ActionResult<Appointment>> GetAppointmentById(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    _logger.LogWarning("Invalid appointment ID provided: {AppointmentId}", id);
                    return BadRequest("The provided appointment ID is invalid. Please provide a valid ID.");
                }

                _logger.LogInformation("Fetching appointment with ID: {AppointmentId} from the repository", id);
                var appointment = await _appointmentRepository.GetAppointmentByIdAsync(id);
                if (appointment == null)
                {
                    _logger.LogWarning("No appointment found with the provided ID: {AppointmentId}", id);
                    return NotFound($"No appointment found with the ID: {id}");
                }
                return Ok(appointment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching the appointment");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request");
            }
        }

        [HttpPost(Name = "CreateAppointment")]
        public async Task<ActionResult<Appointment>> CreateAppointment([FromBody] Appointment appointment)
        {
            try
            {
                if (appointment == null)
                {
                    _logger.LogWarning("Attempted to create a null appointment");
                    return BadRequest("Appointment cannot be null");
                }

                if (string.IsNullOrWhiteSpace(appointment.Name))
                {
                    _logger.LogWarning("Attempted to create an appointment with an empty name");
                    return BadRequest("Appointment name cannot be empty");
                }

                if (string.IsNullOrWhiteSpace(appointment.Description))
                {
                    _logger.LogWarning("Attempted to create an appointment with an empty description");
                    return BadRequest("Appointment description cannot be empty");
                }

                appointment.ID = Guid.NewGuid();

                var result = await _appointmentRepository.AddAppointmentAsync(appointment);
                if (result == 0)
                {
                    _logger.LogError("Failed to create appointment");
                    return StatusCode(StatusCodes.Status500InternalServerError, "Failed to create appointment");
                }

                _logger.LogInformation("Created new appointment with ID: {AppointmentId}", appointment.ID);
                return CreatedAtAction(nameof(GetAppointmentById), new { id = appointment.ID }, appointment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating the appointment");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request");
            }
        }

        [HttpPut("{id}", Name = "UpdateAppointment")]
        public async Task<IActionResult> UpdateAppointment(Guid id, [FromBody] Appointment appointment)
        {
            try
            {
                if (id == Guid.Empty || appointment == null || appointment.ID != id)
                {
                    _logger.LogWarning("Invalid update attempt for appointment with ID: {AppointmentId}", id);
                    return BadRequest("Invalid appointment data");
                }

                if (string.IsNullOrWhiteSpace(appointment.Name))
                {
                    _logger.LogWarning("Attempted to update an appointment with an empty title");
                    return BadRequest("Appointment title cannot be empty");
                }

                if (string.IsNullOrWhiteSpace(appointment.Description))
                {
                    _logger.LogWarning("Attempted to update an appointment with an empty description");
                    return BadRequest("Appointment description cannot be empty");
                }

                var existingAppointment = await _appointmentRepository.GetAppointmentByIdAsync(id);
                if (existingAppointment == null)
                {
                    _logger.LogWarning("Appointment with ID: {AppointmentId} not found for update", id);
                    return NotFound();
                }

                var result = await _appointmentRepository.UpdateAppointmentAsync(appointment);
                if (result == 0)
                {
                    _logger.LogError("Failed to update appointment with ID: {AppointmentId}", id);
                    return StatusCode(StatusCodes.Status500InternalServerError, "Failed to update appointment");
                }

                _logger.LogInformation("Updated appointment with ID: {AppointmentId}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while updating the appointment");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request");
            }
        }

        [HttpDelete("{id}", Name = "DeleteAppointment")]
        public async Task<IActionResult> DeleteAppointment(Guid id)
        {
            try
            {
                if (id == Guid.Empty)
                {
                    _logger.LogWarning("Invalid appointment ID: {AppointmentId}", id);
                    return BadRequest("Invalid appointment ID");
                }

                _logger.LogInformation("Deleting appointment with ID: {AppointmentId}", id);
                var appointment = await _appointmentRepository.GetAppointmentByIdAsync(id);
                if (appointment == null)
                {
                    _logger.LogWarning("Appointment with ID: {AppointmentId} not found for deletion", id);
                    return NotFound();
                }

                var result = await _appointmentRepository.DeleteAppointmentAsync(id);
                if (result == 0)
                {
                    _logger.LogError("Failed to delete appointment with ID: {AppointmentId}", id);
                    return StatusCode(StatusCodes.Status500InternalServerError, "Failed to delete appointment");
                }

                _logger.LogInformation("Deleted appointment with ID: {AppointmentId}", id);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting the appointment");
                return StatusCode(StatusCodes.Status500InternalServerError, "An error occurred while processing your request");
            }
        }
    }
}