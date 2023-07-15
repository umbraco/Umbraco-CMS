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
    internal class UmbracoKeyValueConfiguration : IEntityTypeConfiguration<UmbracoKeyValue>
    {
        public void Configure(EntityTypeBuilder<UmbracoKeyValue> builder)
        {
            builder.HasKey(e => e.Key);
            builder.ToTable("umbracoKeyValue");

            builder.Property(e => e.Key)
                .HasMaxLength(256)
                .HasColumnName("key");
            builder.Property(e => e.Updated)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated");
            builder.Property(e => e.Value).HasColumnName("value");
        }
    }
}
