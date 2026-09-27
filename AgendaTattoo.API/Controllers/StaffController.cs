using AgendaTattoo.BLL.Interfaces;
using AgendaTattoo.DTO.Studio;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AgendaTattoo.API.Controllers
{
    [Route("api/studio/members")]
    [ApiController]
    public class StaffController : ControllerBase
    {
        private readonly IStaffService _staff;

        public StaffController(IStaffService staff)
        {
            _staff = staff;
        }

        /// <summary>Lista a equipe (dono + artistas) do estúdio do usuário autenticado.</summary>
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<StaffMemberDto>>> List(CancellationToken ct)
            => Ok(await _staff.ListAsync(ct));

        /// <summary>Cria um novo artista vinculado ao estúdio. Só o dono pode chamar.</summary>
        [HttpPost]
        [Authorize(Policy = "OwnerOnly")]
        public async Task<ActionResult<StaffMemberDto>> Invite([FromBody] InviteArtistRequest request, CancellationToken ct)
        {
            var created = await _staff.InviteArtistAsync(request, ct);
            return CreatedAtAction(nameof(List), created);
        }

        [HttpPut("{id:guid}")]
        [Authorize(Policy = "OwnerOnly")]
        public async Task<ActionResult<StaffMemberDto>> Update(Guid id, [FromBody] UpdateStaffMemberRequest request, CancellationToken ct)
            => Ok(await _staff.UpdateAsync(id, request, ct));

        /// <summary>Desativação (soft-delete) — o membro deixa de poder logar ou receber novos agendamentos.</summary>
        [HttpDelete("{id:guid}")]
        [Authorize(Policy = "OwnerOnly")]
        public async Task<IActionResult> Deactivate(Guid id, CancellationToken ct)
        {
            await _staff.DeactivateAsync(id, ct);
            return NoContent();
        }

        [HttpPost("{id:guid}/reactivate")]
        [Authorize(Policy = "OwnerOnly")]
        public async Task<IActionResult> Reactivate(Guid id, CancellationToken ct)
        {
            await _staff.ReactivateAsync(id, ct);
            return NoContent();
        }

    }
}
