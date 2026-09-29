using AgendaTattoo.BLL.Interfaces;
using AgendaTattoo.Data.Entities;
using AgendaTattoo.DTO.Studio;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AgendaTattoo.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudioController : ControllerBase
    {
        private readonly IStudioService _studio;

        public StudioController(IStudioService service)
        {
            _studio = service;
        }

        [HttpGet]
        public async Task<ActionResult<StudioDto>> Get(CancellationToken ct)
            => Ok(await _studio.GetAsync(ct));

        [HttpPut]
        [Authorize(Policy = "OwnerOnly")]
        public async Task<ActionResult<StudioDto>> Update([FromBody] UpdateStudioRequest request, CancellationToken ct)
            => Ok(await _studio.UpdateAsync(request, ct));

    }
}
