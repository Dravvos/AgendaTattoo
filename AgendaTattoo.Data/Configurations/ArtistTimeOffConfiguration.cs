using AgendaTattoo.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace AgendaTattoo.Data.Configurations
{
    public class ArtistTimeOffConfiguration : IEntityTypeConfiguration<ArtistTimeOff>
    {
        public void Configure(EntityTypeBuilder<ArtistTimeOff> builder)
        {
            builder.ToTable("ArtistTimeOffs");
            builder.HasKey(t => t.Id);

            builder.Property(t => t.Reason).HasMaxLength(200);

            builder.HasOne(t => t.Artist)
                .WithMany(u => u.TimeOffs)
                .HasForeignKey(t => t.ArtistId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
