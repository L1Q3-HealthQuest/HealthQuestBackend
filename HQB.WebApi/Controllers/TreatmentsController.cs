﻿using HQB.WebApi.Models;
using HQB.WebApi.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HQB.WebApi.Controllers
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class TreatmentsController : ControllerBase
    {
        private readonly ITreatmentRepository _treatmentRepository;
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly ILogger<TreatmentsController> _logger;

        public TreatmentsController
        (
            ITreatmentRepository treatmentRepository,
            IAppointmentRepository appointmentRepository,
            ILogger<TreatmentsController> logger
        )
        {
            _appointmentRepository = appointmentRepository ?? throw new ArgumentNullException(nameof(appointmentRepository));
            _treatmentRepository = treatmentRepository ?? throw new ArgumentNullException(nameof(treatmentRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [HttpGet(Name = "GetAllTreatments")]
        public async Task<ActionResult<IEnumerable<Treatment>>> GetTreatmentsAsync()
        {
            _logger.LogInformation("Getting all treatments");
            var treatments = await _treatmentRepository.GetAllTreatmentsAsync();
            if (treatments == null || !treatments.Any())
            {
                _logger.LogWarning("No treatments found");
                return NotFound();
            }
            return Ok(treatments);
        }

        [HttpGet("{id}", Name = "GetTreatmentById")]
        public async Task<ActionResult<Treatment>> GetTreatmentByIdAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                _logger.LogWarning("Invalid treatment ID");
                return BadRequest("Invalid treatment ID");
            }

            _logger.LogInformation("Getting treatment with ID: {Id}", id);
            var treatment = await _treatmentRepository.GetTreatmentByIdAsync(id);
            if (treatment == null)
            {
                _logger.LogWarning("Treatment with ID: {Id} not found", id);
                return NotFound();
            }
            return Ok(treatment);
        }

        [HttpGet("{treatmentId}/appointments", Name = "GetAppointmentsByTreatmentId")]
        public async Task<ActionResult<IEnumerable<Appointment>>> GetAppointmentByTreatmentId(Guid treatmentId)
        {
            if (treatmentId == Guid.Empty)
            {
                _logger.LogWarning("Invalid treatment ID");
                return BadRequest("Invalid treatment ID");
            }

            _logger.LogInformation("Fetching appointments for treatment ID: {TreatmentId} from the repository", treatmentId);

            var treatmentAppointment = await _appointmentRepository.GetAppointmentsByTreatmentIdAsync(treatmentId);
            if (treatmentAppointment == null || !treatmentAppointment.Any())
            {
                _logger.LogWarning("No appointments found for the provided treatment ID: {TreatmentId}", treatmentId);
                return NotFound($"No appointments found for the treatment ID: {treatmentId}");
            }

            var appointments = new List<(Appointment Appointment, int SequenceNr)>();
            foreach (var item in treatmentAppointment)
            {
                var appointmentDetails = await _appointmentRepository.GetAppointmentByIdAsync(item.AppointmentID);
                if (appointmentDetails != null)
                {
                    appointments.Add((appointmentDetails, item.Sequence));
                }
            }

            var sortedAppointments = appointments.OrderBy(a => a.SequenceNr).Select(a => a.Appointment).ToList();

            return Ok(sortedAppointments);
        }

        [HttpPost(Name = "CreateTreatment")]
        public async Task<ActionResult<Treatment>> CreateTreatmentAsync([FromBody] Treatment treatment)
        {
            if (treatment == null)
            {
                _logger.LogWarning("Treatment object is null.");
                return BadRequest("Treatment object is null.");
            }

            if (string.IsNullOrWhiteSpace(treatment.Name))
            {
                _logger.LogWarning("Treatment name is null or empty");
                return BadRequest("Treatment name is required");
            }

            treatment.ID = Guid.NewGuid();

            _logger.LogInformation("Creating new treatment with ID: {Id}", treatment.ID);
            var result = await _treatmentRepository.AddTreatmentAsync(treatment);
            if (result == 0)
            {
                _logger.LogError("Error creating treatment");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error creating treatment");
            }
            return CreatedAtRoute("GetTreatmentById", new { id = treatment.ID }, treatment);
        }

        [HttpPut("{id}", Name = "UpdateTreatment")]
        public async Task<ActionResult<Treatment>> UpdateTreatmentAsync(Guid id, [FromBody] Treatment treatment)
        {
            if (id == Guid.Empty)
            {
                _logger.LogWarning("Invalid treatment ID");
                return BadRequest("Invalid treatment ID");
            }

            if (treatment == null)
            {
                _logger.LogWarning("Treatment object is null");
                return BadRequest("Treatment object is null");
            }

            if (string.IsNullOrWhiteSpace(treatment.Name))
            {
                _logger.LogWarning("Treatment name is null or empty");
                return BadRequest("Treatment name is required");
            }

            var existingTreatment = await _treatmentRepository.GetTreatmentByIdAsync(id);
            if (existingTreatment == null)
            {
                _logger.LogWarning("Treatment with ID: {Id} not found", id);
                return NotFound();
            }

            treatment.ID = id;

            _logger.LogInformation("Updating treatment with ID: {Id}", id);
            var result = await _treatmentRepository.UpdateTreatmentAsync(treatment);
            if (result == 0)
            {
                _logger.LogError("Error updating treatment");
                return StatusCode(StatusCodes.Status500InternalServerError, "Error updating treatment");
            }
            return Ok(treatment);
        }

        [HttpDelete("{id}", Name = "DeleteTreatment")]
        public async Task<IActionResult> DeleteTreatmentAsync(Guid id)
        {
            if (id == Guid.Empty)
            {
                _logger.LogWarning("Invalid treatment ID");
                return BadRequest("Invalid treatment ID");
            }

            _logger.LogInformation("Deleting treatment with ID: {Id}", id);
            var existingTreatment = await _treatmentRepository.GetTreatmentByIdAsync(id);
            if (existingTreatment == null)
            {
                _logger.LogWarning("Treatment with ID: {Id} not found", id);
                return NotFound();
            }
            await _treatmentRepository.DeleteTreatmentAsync(id);
            return NoContent();
        }
    }
}
