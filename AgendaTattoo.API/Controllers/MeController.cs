using AgendaTattoo.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AgendaTattoo.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MeController : ControllerBase
    {
        private readonly IPublicBookingService _publicBookingService;
        private readonly ICurrentUserService _currentUser;

        public MeController(ICurrentUserService currentUser, IPublicBookingService publicBookingService)
        {
            _currentUser = currentUser;
            _publicBookingService = publicBookingService;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var studio = _currentUser.StudioId.HasValue
                ? _publicBookingService.GetStudioAsync(_currentUser.StudioId.Value, CancellationToken.None).Result
                : null;

            return Ok(new
            {
                userId = _currentUser.UserId,
                studioSlug = studio?.Slug,
                roles = _currentUser.Roles,
            });
        }

        [HttpGet("getStudio")]
        public async Task<IActionResult> GetAsync(CancellationToken ct)
        {
            var studio = _currentUser.StudioId.HasValue
                ? _publicBookingService.GetStudioAsync(_currentUser.StudioId.Value, CancellationToken.None).Result
                : null;
            return Ok(studio);
        }

    }
}
