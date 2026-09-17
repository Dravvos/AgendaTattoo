using AgendaTattoo.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AgendaTattoo.Data.Configurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.Property(u => u.FullName).IsRequired().HasMaxLength(150);

        builder.HasOne(u => u.Studio)
            .WithMany(s => s.Members)
            .HasForeignKey(u => u.StudioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(u => u.StudioId);
    }
}
