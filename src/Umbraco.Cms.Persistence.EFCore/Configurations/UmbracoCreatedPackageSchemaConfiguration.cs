using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Persistence.EFCore.Models;

namespace Umbraco.Cms.Persistence.EFCore.Configurations
{
    internal class UmbracoCreatedPackageSchemaConfiguration : IEntityTypeConfiguration<UmbracoCreatedPackageSchema>
    {
        public void Configure(EntityTypeBuilder<UmbracoCreatedPackageSchema> builder)
        {
            builder.ToTable("umbracoCreatedPackageSchema");

            builder.HasIndex(e => e.Name, "IX_umbracoCreatedPackageSchema_Name").IsUnique();

            builder.Property(e => e.Id).HasColumnName("id");
            builder.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            builder.Property(e => e.PackageId).HasColumnName("packageId");
            builder.Property(e => e.UpdateDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updateDate");
            builder.Property(e => e.Value).HasColumnName("value");
        }
    }
}
