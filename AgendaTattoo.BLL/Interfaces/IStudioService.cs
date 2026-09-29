using AgendaTattoo.DTO.Studio;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgendaTattoo.BLL.Interfaces
{
    public interface IStudioService
    {
        Task<StudioDto> GetAsync(CancellationToken ct = default);

        /// <summary>Só o Owner pode editar.</summary>
        Task<StudioDto> UpdateAsync(UpdateStudioRequest request, CancellationToken ct = default);

    }
}
