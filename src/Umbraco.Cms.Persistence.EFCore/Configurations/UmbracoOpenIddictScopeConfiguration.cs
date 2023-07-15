using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Umbraco.Cms.Persistence.EFCore.Models;

namespace Umbraco.Cms.Persistence.EFCore.Configurations
{
    internal class UmbracoOpenIddictScopeConfiguration : IEntityTypeConfiguration<UmbracoOpenIddictScope>
    {
        public void Configure(EntityTypeBuilder<UmbracoOpenIddictScope> builder)
        {
            builder.ToTable("umbracoOpenIddictScopes");

            builder.Property(e => e.ConcurrencyToken).HasMaxLength(50);
            builder.Property(e => e.Name).HasMaxLength(200);
        }
    }
}
