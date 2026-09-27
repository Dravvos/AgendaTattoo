using AgendaTattoo.DTO.Studio;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgendaTattoo.BLL.Interfaces
{
    public interface IStaffService
    { 
        Task<IReadOnlyList<StaffMemberDto>> ListAsync(CancellationToken ct = default);
        Task<StaffMemberDto> InviteArtistAsync(InviteArtistRequest request, CancellationToken ct = default);
        Task<StaffMemberDto> UpdateAsync(Guid id, UpdateStaffMemberRequest request, CancellationToken ct = default);
        Task DeactivateAsync(Guid id, CancellationToken ct = default);
        Task ReactivateAsync(Guid id, CancellationToken ct = default);

    }
}
