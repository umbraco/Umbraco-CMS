using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Umbraco.Cms.Persistence.EFCore.Models;

namespace Umbraco.Cms.Persistence.EFCore.Configurations
{
    internal class CmsPropertyTypeConfiguration : IEntityTypeConfiguration<CmsPropertyType>
    {
        public void Configure(EntityTypeBuilder<CmsPropertyType> builder)
        {
            builder.ToTable("cmsPropertyType");

            builder.HasIndex(e => e.Alias, "IX_cmsPropertyTypeAlias");

            builder.HasIndex(e => e.UniqueId, "IX_cmsPropertyTypeUniqueID").IsUnique();

            builder.Property(e => e.Id).HasColumnName("id");
            builder.Property(e => e.Alias).HasMaxLength(255);
            builder.Property(e => e.ContentTypeId).HasColumnName("contentTypeId");
            builder.Property(e => e.DataTypeId).HasColumnName("dataTypeId");
            builder.Property(e => e.Description).HasMaxLength(2000);
            builder.Property(e => e.LabelOnTop)
                .IsRequired()
                .HasDefaultValueSql("('0')")
                .HasColumnName("labelOnTop");
            builder.Property(e => e.Mandatory)
                .IsRequired()
                .HasDefaultValueSql("('0')")
                .HasColumnName("mandatory");
            builder.Property(e => e.MandatoryMessage)
                .HasMaxLength(500)
                .HasColumnName("mandatoryMessage");
            builder.Property(e => e.Name).HasMaxLength(255);
            builder.Property(e => e.PropertyTypeGroupId).HasColumnName("propertyTypeGroupId");
            builder.Property(e => e.SortOrder)
                .HasDefaultValueSql("('0')")
                .HasColumnName("sortOrder");
            builder.Property(e => e.UniqueId)
                .HasDefaultValueSql("(newid())")
                .HasColumnName("UniqueID");
            builder.Property(e => e.ValidationRegExp)
                .HasMaxLength(255)
                .HasColumnName("validationRegExp");
            builder.Property(e => e.ValidationRegExpMessage)
                .HasMaxLength(500)
                .HasColumnName("validationRegExpMessage");
            builder.Property(e => e.Variations)
                .HasDefaultValueSql("('1')")
                .HasColumnName("variations");

            builder.HasOne(d => d.ContentType).WithMany(p => p.CmsPropertyTypes)
                .HasPrincipalKey(p => p.NodeId)
                .HasForeignKey(d => d.ContentTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_cmsPropertyType_cmsContentType_nodeId");

            builder.HasOne(d => d.DataType).WithMany(p => p.CmsPropertyTypes)
                .HasForeignKey(d => d.DataTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_cmsPropertyType_umbracoDataType_nodeId");

            builder.HasOne(d => d.PropertyTypeGroup).WithMany(p => p.CmsPropertyTypes)
                .HasForeignKey(d => d.PropertyTypeGroupId)
                .HasConstraintName("FK_cmsPropertyType_cmsPropertyTypeGroup_id");
        }
    }
}
