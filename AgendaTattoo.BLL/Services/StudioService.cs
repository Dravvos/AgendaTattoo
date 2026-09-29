using AgendaTattoo.BLL.Exceptions;
using AgendaTattoo.BLL.Interfaces;
using AgendaTattoo.Data.Context;
using AgendaTattoo.Data.Entities;
using AgendaTattoo.DTO.Studio;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgendaTattoo.BLL.Services
{
    public class StudioService : IStudioService
    {
        private readonly AgendaTattooDbContext _db;
        private readonly ICurrentUserService _currentUser;

        public StudioService(AgendaTattooDbContext db, ICurrentUserService currentUser)
        {
            _db = db;
            _currentUser = currentUser;
        }

        private Guid StudioId => _currentUser.StudioId ?? throw new ForbiddenException("Usuário sem estúdio associado");

        public async Task<StudioDto> GetAsync(CancellationToken ct = default)
        {
            var studio = await FindAsync(ct);
            return ToDto(studio);
        }

        public async Task<StudioDto> UpdateAsync(UpdateStudioRequest request, CancellationToken ct = default)
        {
            if (!_currentUser.Roles.Contains(Roles.Owner))
                throw new ForbiddenException("Só o dono do estúdio pode editar esses dados.");

            var studio = await FindAsync(ct);

            var slug = request.Slug.Trim().ToLowerInvariant();
            if (slug != studio.Slug)
            {
                var slugTaken = await _db.Studios.AsNoTracking().AnyAsync(s => s.Slug == slug && s.Id != studio.Id, ct);
                if (slugTaken)
                    throw new ConflictException("Esse identificador de estúdio já está em uso.");
            }

            try
            {
                TimeZoneInfo.FindSystemTimeZoneById(request.TimeZoneId);
            }
            catch (TimeZoneNotFoundException)
            {
                throw new ValidationAppException("Fuso horário inválido.");
            }

            studio.Name = request.Name.Trim();
            studio.Slug = slug;
            studio.Description = request.Description?.Trim();
            studio.PhoneNumber = request.PhoneNumber?.Trim();
            studio.Address = request.Address?.Trim();
            studio.TimeZoneId = request.TimeZoneId;

            await _db.SaveChangesAsync(ct);

            return ToDto(studio);

        }

        private async Task<Studio> FindAsync(CancellationToken ct)
        {
            var studio = await _db.Studios.FirstOrDefaultAsync(x => x.Id == StudioId, ct);
            return studio ?? throw new NotFoundException("Estúdio não encontrado");
        }

        private static StudioDto ToDto(Studio studio) => new()
        {
            Id = studio.Id,
            Name = studio.Name,
            Slug = studio.Slug,
            Description = studio.Description,
            PhoneNumber = studio.PhoneNumber,
            Address = studio.Address,
            TimeZoneId = studio.TimeZoneId,
        };

    }
}
