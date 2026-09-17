using AgendaTattoo.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgendaTattoo.Data.Configurations;

public class AppointmentConfiguration : IEntityTypeConfiguration<Appointment>
{
    public void Configure(EntityTypeBuilder<Appointment> builder)
    {
        builder.ToTable("Appointments");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Notes).HasMaxLength(1000);

        builder.HasOne(a => a.Studio)
            .WithMany(s => s.Appointments)
            .HasForeignKey(a => a.StudioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Artist)
            .WithMany(u => u.Appointments)
            .HasForeignKey(a => a.ArtistId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Client)
            .WithMany(c => c.Appointments)
            .HasForeignKey(a => a.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.Service)
            .WithMany(sv => sv.Appointments)
            .HasForeignKey(a => a.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);

        // Consulta mais comum: agenda de um artista, de um estúdio, em um período.
        builder.HasIndex(a => new { a.StudioId, a.ArtistId, a.StartsAt });
    }
}
