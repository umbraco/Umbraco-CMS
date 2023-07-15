using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Umbraco.Cms.Persistence.EFCore.Models;

namespace Umbraco.Cms.Persistence.EFCore.Configurations
{
    internal class UmbracoLockConfiguration : IEntityTypeConfiguration<UmbracoLock>
    {
        public void Configure(EntityTypeBuilder<UmbracoLock> builder)
        {
            builder.ToTable("umbracoLock");

            builder.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            builder.Property(e => e.Name)
                .HasMaxLength(64)
                .HasColumnName("name");
            builder.Property(e => e.Value).HasColumnName("value");
        }
    }
}
