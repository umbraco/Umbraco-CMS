using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Umbraco.Cms.Persistence.EFCore.Models;

namespace Umbraco.Cms.Persistence.EFCore.Configurations
{
    internal class UmbracoOpenIddictAuthorizationConfiguration : IEntityTypeConfiguration<UmbracoOpenIddictAuthorization>
    {
        public void Configure(EntityTypeBuilder<UmbracoOpenIddictAuthorization> builder)
        {
            builder.ToTable("umbracoOpenIddictAuthorizations");

            builder.HasIndex(e => new { e.ApplicationId, e.Status, e.Subject, e.Type }, "IX_umbracoOpenIddictAuthorizations_ApplicationId_Status_Subject_Type");

            builder.Property(e => e.ConcurrencyToken).HasMaxLength(50);
            builder.Property(e => e.Status).HasMaxLength(50);
            builder.Property(e => e.Subject).HasMaxLength(400);
            builder.Property(e => e.Type).HasMaxLength(50);

            builder.HasOne(d => d.Application).WithMany(p => p.Authorizations).HasForeignKey(d => d.ApplicationId);
        }
    }
}
