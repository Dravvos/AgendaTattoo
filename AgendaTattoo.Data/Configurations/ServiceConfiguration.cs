using AgendaTattoo.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgendaTattoo.Data.Configurations;

public class ServiceConfiguration : IEntityTypeConfiguration<Service>
{
    public void Configure(EntityTypeBuilder<Service> builder)
    {
        builder.ToTable("Services");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name).IsRequired().HasMaxLength(150);
        builder.Property(s => s.Description).HasMaxLength(1000);
        builder.Property(s => s.Price).HasColumnType("decimal(10,2)");

        builder.HasOne(s => s.Studio)
            .WithMany(st => st.Services)
            .HasForeignKey(s => s.StudioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(s => s.StudioId);
    }
}
