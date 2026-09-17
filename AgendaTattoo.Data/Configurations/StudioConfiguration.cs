using AgendaTattoo.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgendaTattoo.Data.Configurations;

public class StudioConfiguration : IEntityTypeConfiguration<Studio>
{
    public void Configure(EntityTypeBuilder<Studio> builder)
    {
        builder.ToTable("Studios");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name).IsRequired().HasMaxLength(150);
        builder.Property(s => s.Slug).IsRequired().HasMaxLength(80);
        builder.Property(s => s.Description).HasMaxLength(1000);
        builder.Property(s => s.PhoneNumber).HasMaxLength(30);
        builder.Property(s => s.Address).HasMaxLength(300);

        // O slug é usado na URL pública de agendamento — precisa ser único no sistema.
        builder.HasIndex(s => s.Slug).IsUnique();
    }
}
