using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Umbraco.Cms.Persistence.EFCore.Models;

namespace Umbraco.Cms.Persistence.EFCore.Configurations
{
    internal class UmbracoRelationTypeConfiguration : IEntityTypeConfiguration<UmbracoRelationType>
    {
        public void Configure(EntityTypeBuilder<UmbracoRelationType> builder)
        {
            builder.ToTable("umbracoRelationType");

            builder.HasIndex(e => e.TypeUniqueId, "IX_umbracoRelationType_UniqueId").IsUnique();

            builder.HasIndex(e => e.Alias, "IX_umbracoRelationType_alias").IsUnique();

            builder.HasIndex(e => e.Name, "IX_umbracoRelationType_name").IsUnique();

            builder.Property(e => e.Id).HasColumnName("id");
            builder.Property(e => e.Alias)
                .HasMaxLength(100)
                .HasColumnName("alias");
            builder.Property(e => e.ChildObjectType).HasColumnName("childObjectType");
            builder.Property(e => e.Dual).HasColumnName("dual");
            builder.Property(e => e.IsDependency)
                .IsRequired()
                .HasDefaultValueSql("('0')")
                .HasColumnName("isDependency");
            builder.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            builder.Property(e => e.ParentObjectType).HasColumnName("parentObjectType");
            builder.Property(e => e.TypeUniqueId).HasColumnName("typeUniqueId");
        }
    }
}
