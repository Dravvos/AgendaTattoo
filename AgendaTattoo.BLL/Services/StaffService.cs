using AgendaTattoo.BLL.Exceptions;
using AgendaTattoo.BLL.Interfaces;
using AgendaTattoo.Data.Context;
using AgendaTattoo.Data.Entities;
using AgendaTattoo.DTO.Studio;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.EntityFrameworkCore;


namespace AgendaTattoo.BLL.Services
{
    public class StaffService : IStaffService
    {
        private readonly AgendaTattooDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICurrentUserService _currentUser;

        public StaffService(AgendaTattooDbContext db, UserManager<ApplicationUser> userManager, ICurrentUserService currentUser)
        {
            _db = db;
            _userManager = userManager;
            _currentUser = currentUser;
        }

        private Guid StudioId => _currentUser.StudioId ?? throw new ForbiddenException("Usuário sem estúdio associado.");
        private bool IsOwner => _currentUser.Roles.Contains(Roles.Owner);


        public async Task DeactivateAsync(Guid id, CancellationToken ct = default)
        {
            EnsureIsOwner();

            if (id == _currentUser.UserId)
                throw new ValidationAppException("Você não pode desativar a própria conta.");

            var member = await FindOwnedAsync(id, ct);

            var roles = await _userManager.GetRolesAsync(member);
            if (roles.Contains(Roles.Owner))
                throw new ValidationAppException("Não é possível desativar o dono do estúdio.");

            member.IsActive = false;
            await _db.SaveChangesAsync(ct);

            // Usuário desativado não deve continuar autenticado com tokens já emitidos.
            var activeTokens = await _db.RefreshTokens
                .Where(rt => rt.UserId == member.Id && rt.RevokedAt == null)
                .ToListAsync(ct);

            foreach (var token in activeTokens)
                token.RevokedAt = DateTimeOffset.UtcNow;

            await _db.SaveChangesAsync(ct);

        }

        public async Task<StaffMemberDto> InviteArtistAsync(InviteArtistRequest request, CancellationToken ct = default)
        {
            EnsureIsOwner();

            var email = request.Email.Trim();
            var existing = await _userManager.FindByEmailAsync(email);
            if (existing is not null)
                throw new ConflictException("Já existe um usuário com esse e-mail.");

            var user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = email,
                Email = email,
                FullName = request.FullName.Trim(),
                StudioId = StudioId,
                // TODO: quando houver envio de e-mail, trocar para false + fluxo de convite/confirmação.
                EmailConfirmed = true,
            };

            var createResult = await _userManager.CreateAsync(user, request.Password);
            if (!createResult.Succeeded)
                throw new ValidationAppException(string.Join(' ', createResult.Errors.Select(e => e.Description)));

            await _userManager.AddToRoleAsync(user, Roles.Artist);

            return ToDto(user, [Roles.Artist]);

        }

        public async Task<IReadOnlyList<StaffMemberDto>> ListAsync(CancellationToken ct = default)
        {
            var members = await _db.Users.Where(u=>u.StudioId == StudioId).ToListAsync(ct);
            var result = new List<StaffMemberDto>();
            foreach (var member in members)
            {
                var roles = await _userManager.GetRolesAsync(member);
                result.Add(ToDto(member, roles));
            }

            return result;

        }

        public async Task ReactivateAsync(Guid id, CancellationToken ct = default)
        {
            EnsureIsOwner();

            var member = await FindOwnedAsync(id, ct);
            member.IsActive = true;
            await _db.SaveChangesAsync(ct);

        }

        public async Task<StaffMemberDto> UpdateAsync(Guid id, UpdateStaffMemberRequest request, CancellationToken ct = default)
        {
            EnsureIsOwner();

            var member = await FindOwnedAsync(id, ct);
            member.FullName = request.FullName.Trim();
            await _db.SaveChangesAsync(ct);

            var roles = await _userManager.GetRolesAsync(member);
            return ToDto(member, roles);

        }

        private void EnsureIsOwner()
        {
            if (!IsOwner)
                throw new ForbiddenException("Só o dono do estúdio pode gerenciar a equipe.");
        }

        private async Task<ApplicationUser> FindOwnedAsync(Guid id, CancellationToken ct)
        {
            var member = await _db.Users.FirstOrDefaultAsync(u => u.Id == id && u.StudioId == StudioId, ct);
            return member ?? throw new NotFoundException("Membro não encontrado.");
        }

        private static StaffMemberDto ToDto(ApplicationUser user, IList<string> roles) => new()
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            Role = roles.Contains(Roles.Owner) ? Roles.Owner : Roles.Artist,
            IsActive = user.IsActive,
        };

    }
}
