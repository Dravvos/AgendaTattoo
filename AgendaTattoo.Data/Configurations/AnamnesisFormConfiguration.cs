using AgendaTattoo.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgendaTattoo.Data.Configurations
{
    public class AnamnesisFormConfiguration : IEntityTypeConfiguration<AnamnesisForm>
    {
        public void Configure(EntityTypeBuilder<AnamnesisForm> builder)
        {
            builder.ToTable("AnamnesisForms");
            builder.HasKey(a => a.Id);

            builder.Property(a => a.AllergiesDescription).HasMaxLength(500);
            builder.Property(a => a.SkinConditionsDescription).HasMaxLength(500);
            builder.Property(a => a.PreviousTattooReactionDescription).HasMaxLength(500);
            builder.Property(a => a.CurrentMedications).HasMaxLength(500);
            builder.Property(a => a.AdditionalNotes).HasMaxLength(1000);
            builder.Property(a => a.ConsentText).IsRequired();
            builder.Property(a => a.SignatureFullName).IsRequired().HasMaxLength(150);
            builder.Property(a => a.SubmittedFromIp).HasMaxLength(64);

            builder.HasOne(a => a.Studio)
                .WithMany()
                .HasForeignKey(a => a.StudioId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(a => a.Client)
                .WithMany()
                .HasForeignKey(a => a.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            // Um agendamento tem no máximo uma ficha — reenvio atualiza a existente, não cria outra.
            builder.HasOne(a => a.Appointment)
                .WithOne()
                .HasForeignKey<AnamnesisForm>(a => a.AppointmentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(a => a.AppointmentId).IsUnique();
            builder.HasIndex(a => new { a.StudioId, a.ClientId });
        }
    }
}
