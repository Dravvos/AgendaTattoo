using AgendaTattoo.BLL.Interfaces;
using AgendaTattoo.DTO.Scheduling;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AgendaTattoo.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientsController : ControllerBase
    {
        private readonly IClientService _clients;

        public ClientsController(IClientService clients)
        {
            _clients = clients;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<ClientDto>>> List([FromQuery] string? search, CancellationToken ct)
            => Ok(await _clients.ListAsync(search, ct));

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<ClientDto>> Get(Guid id, CancellationToken ct)
            => Ok(await _clients.GetAsync(id, ct));

        [HttpPost]
        public async Task<ActionResult<ClientDto>> Create([FromBody] CreateClientRequest request, CancellationToken ct)
        {
            var created = await _clients.CreateAsync(request, ct);
            return CreatedAtAction(nameof(Get), new { id = created.Id }, created);
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<ClientDto>> Update(Guid id, [FromBody] UpdateClientRequest request, CancellationToken ct)
            => Ok(await _clients.UpdateAsync(id, request, ct));
    }
}
