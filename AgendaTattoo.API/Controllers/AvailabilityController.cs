using AgendaTattoo.BLL.Interfaces;
using AgendaTattoo.DTO.Scheduling;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AgendaTattoo.API.Controllers
{
    [Route("api/artists/{artistId}")]
    [ApiController]
    public class AvailabilityController : ControllerBase
    {
        private readonly IAvailabilityService _availability;

        public AvailabilityController(IAvailabilityService availability)
        {
            _availability = availability;
        }

        [HttpGet("working-hours")]
        public async Task<ActionResult<IReadOnlyList<WorkingHoursDto>>> GetWorkingHours(Guid artistId, CancellationToken ct)
    => Ok(await _availability.GetWorkingHoursAsync(artistId, ct));

        /// <summary>Substitui a grade semanal inteira. Pode enviar mais de uma faixa por dia (ex.: intervalo de almoço).</summary>
        [HttpPut("working-hours")]
        public async Task<IActionResult> SetWorkingHours(Guid artistId, [FromBody] SetWorkingHoursRequest request, CancellationToken ct)
        {
            await _availability.SetWorkingHoursAsync(artistId, request, ct);
            return NoContent();
        }

        /// <summary>Lista as folgas pontuais do artista no período [from, to].</summary>
        [HttpGet("time-off")]
        public async Task<ActionResult<IReadOnlyList<ArtistTimeOffDto>>> ListTimeOff(
            Guid artistId, [FromQuery] DateOnly from, [FromQuery] DateOnly to, CancellationToken ct)
            => Ok(await _availability.ListTimeOffAsync(artistId, from, to, ct));

        /// <summary>Registra uma folga pontual (dia inteiro ou intervalo específico numa data).</summary>
        [HttpPost("time-off")]
        public async Task<ActionResult<ArtistTimeOffDto>> AddTimeOff(
            Guid artistId, [FromBody] CreateArtistTimeOffRequest request, CancellationToken ct)
        {
            var created = await _availability.AddTimeOffAsync(artistId, request, ct);
            return CreatedAtAction(nameof(ListTimeOff), new { artistId }, created);
        }

        [HttpDelete("time-off/{timeOffId:guid}")]
        public async Task<IActionResult> RemoveTimeOff(Guid artistId, Guid timeOffId, CancellationToken ct)
        {
            await _availability.RemoveTimeOffAsync(artistId, timeOffId, ct);
            return NoContent();
        }

    }
}
