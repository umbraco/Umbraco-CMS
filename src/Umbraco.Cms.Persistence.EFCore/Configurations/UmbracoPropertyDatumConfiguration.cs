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
    internal class UmbracoPropertyDatumConfiguration : IEntityTypeConfiguration<UmbracoPropertyDatum>
    {
        public void Configure(EntityTypeBuilder<UmbracoPropertyDatum> builder)
        {
            builder.ToTable("umbracoPropertyData");

            builder.HasIndex(e => e.LanguageId, "IX_umbracoPropertyData_LanguageId");

            builder.HasIndex(e => e.PropertyTypeId, "IX_umbracoPropertyData_PropertyTypeId");

            builder.HasIndex(e => e.Segment, "IX_umbracoPropertyData_Segment");

            builder.HasIndex(e => new { e.VersionId, e.PropertyTypeId, e.LanguageId, e.Segment }, "IX_umbracoPropertyData_VersionId").IsUnique();

            builder.Property(e => e.Id).HasColumnName("id");
            builder.Property(e => e.DateValue)
                .HasColumnType("datetime")
                .HasColumnName("dateValue");
            builder.Property(e => e.DecimalValue)
                .HasColumnType("decimal(38, 6)")
                .HasColumnName("decimalValue");
            builder.Property(e => e.IntValue).HasColumnName("intValue");
            builder.Property(e => e.LanguageId).HasColumnName("languageId");
            builder.Property(e => e.PropertyTypeId).HasColumnName("propertyTypeId");
            builder.Property(e => e.Segment)
                .HasMaxLength(256)
                .HasColumnName("segment");
            builder.Property(e => e.TextValue).HasColumnName("textValue");
            builder.Property(e => e.VarcharValue)
                .HasMaxLength(512)
                .HasColumnName("varcharValue");
            builder.Property(e => e.VersionId).HasColumnName("versionId");

            builder.HasOne(d => d.Language).WithMany(p => p.UmbracoPropertyData)
                .HasForeignKey(d => d.LanguageId)
                .HasConstraintName("FK_umbracoPropertyData_umbracoLanguage_id");

            builder.HasOne(d => d.PropertyType).WithMany(p => p.UmbracoPropertyData)
                .HasForeignKey(d => d.PropertyTypeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_umbracoPropertyData_cmsPropertyType_id");

            builder.HasOne(d => d.Version).WithMany(p => p.UmbracoPropertyData)
                .HasForeignKey(d => d.VersionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_umbracoPropertyData_umbracoContentVersion_id");
        }
    }
}
