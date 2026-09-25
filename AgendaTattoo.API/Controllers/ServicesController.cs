using AgendaTattoo.BLL.Interfaces;
using AgendaTattoo.DTO.Scheduling;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AgendaTattoo.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServicesController : ControllerBase
    {
        private readonly IServiceCatalogService _services;

        public ServicesController(IServiceCatalogService services)
        {
            _services = services;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<ServiceDto>>> List(CancellationToken ct)
            => Ok(await _services.ListAsync(ct));

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ServiceDto>> Get(Guid id, CancellationToken ct)
            => Ok(await _services.GetAsync(id, ct));

        [HttpPost]
        [Authorize(Policy = "OwnerOnly")]
        public async Task<ActionResult<ServiceDto>> Create([FromBody] CreateServiceRequest request, CancellationToken ct)
        {
            var created = await _services.CreateAsync(request, ct);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }

        [HttpPut("{id:guid}")]
        [Authorize(Policy = "OwnerOnly")]
        public async Task<ActionResult<ServiceDto>> Update(Guid id, [FromBody] UpdateServiceRequest request, CancellationToken ct)
            => Ok(await _services.UpdateAsync(id, request, ct));

        [HttpDelete("{id:guid}")]
        [Authorize(Policy = "OwnerOnly")]
        public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
        {
            await _services.DeactivateAsync(id, ct);
            return NoContent();
        }
    }
}
