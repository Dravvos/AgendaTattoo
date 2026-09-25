using AgendaTattoo.BLL.Interfaces;
using AgendaTattoo.DTO.Public;
using AgendaTattoo.DTO.Scheduling;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace AgendaTattoo.API.Controllers
{
    [Route("api/public/studios/{slug}")]
    [ApiController]
    [AllowAnonymous]
    [EnableRateLimiting("public")]
    public class PublicBookingController : ControllerBase
    {
        private readonly IPublicBookingService _publicBooking;

        public PublicBookingController(IPublicBookingService publicBooking)
        {
            _publicBooking = publicBooking;
        }

        [HttpGet]
        public async Task<ActionResult<PublicStudioDto>> GetStudio(string slug, CancellationToken ct)
            => Ok(await _publicBooking.GetStudioAsync(slug, ct));

        [HttpGet("services")]
        public async Task<ActionResult<IReadOnlyList<PublicServiceDto>>> ListServices(string slug, CancellationToken ct)
            => Ok(await _publicBooking.ListServicesAsync(slug, ct));

        [HttpGet("artists")]
        public async Task<ActionResult<IReadOnlyList<PublicArtistDto>>> ListArtists(string slug, CancellationToken ct)
            => Ok(await _publicBooking.ListArtistsAsync(slug, ct));

        [HttpGet("availability")]
        public async Task<ActionResult<IReadOnlyList<AvailableSlotDto>>> GetAvailability(
            string slug, [FromQuery] Guid artistId, [FromQuery] Guid serviceId, [FromQuery] DateOnly date, CancellationToken ct)
            => Ok(await _publicBooking.GetAvailableSlotsAsync(slug, artistId, serviceId, date, ct));

        /// <summary>Ponto mais sensível a abuso do controller público — limite mais apertado que os GETs.</summary>
        [HttpPost("bookings")]
        [EnableRateLimiting("public-booking")]
        public async Task<ActionResult<PublicBookingResponse>> CreateBooking(
            string slug, [FromBody] CreatePublicBookingRequest request, CancellationToken ct)
        {
            var result = await _publicBooking.CreateBookingAsync(slug, request, ct);
            return Ok(result);
        }

    }
}
