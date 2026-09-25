using AgendaTattoo.BLL.Interfaces;
using AgendaTattoo.DTO.Scheduling;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AgendaTattoo.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppointmentsController : ControllerBase
    {
        private readonly IAppointmentService _appointments;

        public AppointmentsController(IAppointmentService appointments)
        {
            _appointments = appointments;
        }

        /// <summary>Lista agendamentos que se sobrepõem ao período [from, to). Artist: sempre a própria agenda.</summary>
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<AppointmentDto>>> List(
            [FromQuery] DateTimeOffset from, [FromQuery] DateTimeOffset to, [FromQuery] Guid? artistId, CancellationToken ct)
            => Ok(await _appointments.ListAsync(from, to, artistId, ct));

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<AppointmentDto>> Get(Guid id, CancellationToken ct)
            => Ok(await _appointments.GetAsync(id, ct));

        /// <summary>Cria um agendamento pela equipe do estúdio (ex.: cliente ligou/veio pessoalmente).</summary>
        [HttpPost]
        public async Task<ActionResult<AppointmentDto>> Create([FromBody] CreateAppointmentRequest request, CancellationToken ct)
        {
            var created = await _appointments.CreateAsync(request, ct);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }

        [HttpPut("{id:guid}/reschedule")]
        public async Task<ActionResult<AppointmentDto>> Reschedule(Guid id, [FromBody] RescheduleAppointmentRequest request, CancellationToken ct)
            => Ok(await _appointments.RescheduleAsync(id, request, ct));

        [HttpPut("{id:guid}/status")]
        public async Task<ActionResult<AppointmentDto>> UpdateStatus(Guid id, [FromBody] UpdateAppointmentStatusRequest request, CancellationToken ct)
            => Ok(await _appointments.UpdateStatusAsync(id, request, ct));
    }
}
