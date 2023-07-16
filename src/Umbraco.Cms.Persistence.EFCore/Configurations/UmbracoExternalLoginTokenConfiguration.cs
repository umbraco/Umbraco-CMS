using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Umbraco.Cms.Infrastructure.Persistence.Models;

namespace Umbraco.Cms.Persistence.EFCore.Configurations
{
    internal class UmbracoExternalLoginTokenConfiguration : IEntityTypeConfiguration<UmbracoExternalLoginToken>
    {
        public void Configure(EntityTypeBuilder<UmbracoExternalLoginToken> builder)
        {
            builder.ToTable("umbracoExternalLoginToken");

            builder.HasIndex(e => new { e.ExternalLoginId, e.Name }, "IX_umbracoExternalLoginToken_Name").IsUnique();

            builder.Property(e => e.Id).HasColumnName("id");
            builder.Property(e => e.CreateDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("createDate");
            builder.Property(e => e.ExternalLoginId).HasColumnName("externalLoginId");
            builder.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            builder.Property(e => e.Value).HasColumnName("value");

            builder.HasOne(d => d.ExternalLogin).WithMany(p => p.UmbracoExternalLoginTokens)
                .HasForeignKey(d => d.ExternalLoginId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_umbracoExternalLoginToken_umbracoExternalLogin_id");
        }
    }
}
