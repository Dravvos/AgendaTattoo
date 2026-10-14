using AgendaTattoo.BLL.Interfaces;
using AgendaTattoo.DTO.Scheduling;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AgendaTattoo.API.Controllers
{
    /// <summary>
    /// Visão interna (autenticada) da ficha de anamnese de um agendamento. O envio em si é feito
    /// pelo cliente, sem login, pelas rotas públicas em PublicBookingController.
    /// </summary>
    [Route("api/appointments/{appointmentId:guid}/anamnesis")]
    [ApiController]
    public class AnamnesisController : ControllerBase
    {
        private readonly IAnamnesisService _anamnesis;

        public AnamnesisController(IAnamnesisService anamnesis)
        {
            _anamnesis = anamnesis;
        }

        /// <summary>Retorna 204 (sem corpo) se o cliente ainda não enviou a ficha.</summary>
        [HttpGet]
        public async Task<ActionResult<AnamnesisFormDto>> Get(Guid appointmentId, CancellationToken ct)
        {
            var form = await _anamnesis.GetForStudioAsync(appointmentId, ct);
            return form is null ? NoContent() : Ok(form);
        }
    }
}
