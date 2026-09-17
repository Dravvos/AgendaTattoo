using AgendaTattoo.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgendaTattoo.Data.Configurations;

public class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.ToTable("Clients");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.FullName).IsRequired().HasMaxLength(150);
        builder.Property(c => c.PhoneNumber).IsRequired().HasMaxLength(30);
        builder.Property(c => c.Email).HasMaxLength(200);
        builder.Property(c => c.Notes).HasMaxLength(1000);

        builder.HasOne(c => c.Studio)
            .WithMany(s => s.Clients)
            .HasForeignKey(c => c.StudioId)
            .OnDelete(DeleteBehavior.Cascade);

        // Busca rápida de um cliente pelo telefone, dentro do mesmo estúdio.
        builder.HasIndex(c => new { c.StudioId, c.PhoneNumber });
    }
}
