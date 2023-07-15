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
    internal class UmbracoDataTypeConfiguration : IEntityTypeConfiguration<UmbracoDataType>
    {
        public void Configure(EntityTypeBuilder<UmbracoDataType> builder)
        {
            builder.HasKey(e => e.NodeId);
            builder.ToTable("umbracoDataType");

            builder.Property(e => e.NodeId)
                .ValueGeneratedNever()
                .HasColumnName("nodeId");
            builder.Property(e => e.Config).HasColumnName("config");
            builder.Property(e => e.DbType)
                .HasMaxLength(50)
                .HasColumnName("dbType");
            builder.Property(e => e.PropertyEditorAlias)
                .HasMaxLength(255)
                .HasColumnName("propertyEditorAlias");

            builder.HasOne(d => d.Node).WithOne(p => p.UmbracoDataType)
                .HasForeignKey<UmbracoDataType>(d => d.NodeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_umbracoDataType_umbracoNode_id");
        }
    }
}
