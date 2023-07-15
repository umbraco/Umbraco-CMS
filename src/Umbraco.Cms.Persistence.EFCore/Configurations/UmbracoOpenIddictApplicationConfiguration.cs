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
    internal class UmbracoOpenIddictApplicationConfiguration : IEntityTypeConfiguration<UmbracoOpenIddictApplication>
    {
        public void Configure(EntityTypeBuilder<UmbracoOpenIddictApplication> builder)
        {
            builder.ToTable("umbracoOpenIddictApplications");

            builder.Property(e => e.ClientId).HasMaxLength(100);
            builder.Property(e => e.ConcurrencyToken).HasMaxLength(50);
            builder.Property(e => e.ConsentType).HasMaxLength(50);
            builder.Property(e => e.Type).HasMaxLength(50);
        }
    }
}
