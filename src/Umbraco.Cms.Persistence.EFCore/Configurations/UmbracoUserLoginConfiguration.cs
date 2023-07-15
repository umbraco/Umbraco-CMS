using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Umbraco.Cms.Persistence.EFCore.Models;

namespace Umbraco.Cms.Persistence.EFCore.Configurations
{
    internal class UmbracoUserLoginConfiguration : IEntityTypeConfiguration<UmbracoUserLogin>
    {
        public void Configure(EntityTypeBuilder<UmbracoUserLogin> builder)
        {
            builder.HasKey(e => e.SessionId);

            builder.ToTable("umbracoUserLogin");

            builder.HasIndex(e => e.LastValidatedUtc, "IX_umbracoUserLogin_lastValidatedUtc");

            builder.Property(e => e.SessionId)
                .ValueGeneratedNever()
                .HasColumnName("sessionId");
            builder.Property(e => e.IpAddress)
                .HasMaxLength(255)
                .HasColumnName("ipAddress");
            builder.Property(e => e.LastValidatedUtc)
                .HasColumnType("datetime")
                .HasColumnName("lastValidatedUtc");
            builder.Property(e => e.LoggedInUtc)
                .HasColumnType("datetime")
                .HasColumnName("loggedInUtc");
            builder.Property(e => e.LoggedOutUtc)
                .HasColumnType("datetime")
                .HasColumnName("loggedOutUtc");
            builder.Property(e => e.UserId).HasColumnName("userId");

            builder.HasOne(d => d.User).WithMany(p => p.UmbracoUserLogins)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_umbracoUserLogin_umbracoUser_id");
        }
    }
}
