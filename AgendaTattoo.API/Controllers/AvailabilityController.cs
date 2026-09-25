using AgendaTattoo.BLL.Interfaces;
using AgendaTattoo.DTO.Scheduling;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AgendaTattoo.API.Controllers
{
    [Route("api/artists/{artistId}/working-hours")]
    [ApiController]
    public class AvailabilityController : ControllerBase
    {
        private readonly IAvailabilityService _availability;

        public AvailabilityController(IAvailabilityService availability)
        {
            _availability = availability;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<WorkingHoursDto>>> Get(Guid artistId, CancellationToken ct)
            => Ok(await _availability.GetWorkingHoursAsync(artistId, ct));

        [HttpPut]
        public async Task<IActionResult> Set(Guid artistId, [FromBody] SetWorkingHoursRequest request, CancellationToken ct)
        {
            await _availability.SetWorkingHoursAsync(artistId, request, ct);
            return NoContent();
        }
    }
}
