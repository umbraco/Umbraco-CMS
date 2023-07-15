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
    internal class UmbracoLogViewerQueryConfiguration : IEntityTypeConfiguration<UmbracoLogViewerQuery>
    {
        public void Configure(EntityTypeBuilder<UmbracoLogViewerQuery> builder)
        {
            builder.ToTable("umbracoLogViewerQuery");

            builder.HasIndex(e => e.Name, "IX_LogViewerQuery_name").IsUnique();

            builder.Property(e => e.Id).HasColumnName("id");
            builder.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            builder.Property(e => e.Query)
                .HasMaxLength(255)
                .HasColumnName("query");
        }
    }
}
