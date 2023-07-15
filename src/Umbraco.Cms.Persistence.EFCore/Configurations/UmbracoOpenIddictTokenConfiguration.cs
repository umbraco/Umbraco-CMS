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
    internal class UmbracoOpenIddictTokenConfiguration : IEntityTypeConfiguration<UmbracoOpenIddictToken>
    {
        public void Configure(EntityTypeBuilder<UmbracoOpenIddictToken> builder)
        {
            builder.ToTable("umbracoOpenIddictTokens");

            builder.HasIndex(e => new { e.ApplicationId, e.Status, e.Subject, e.Type }, "IX_umbracoOpenIddictTokens_ApplicationId_Status_Subject_Type");

            builder.HasIndex(e => e.AuthorizationId, "IX_umbracoOpenIddictTokens_AuthorizationId");

            builder.Property(e => e.ConcurrencyToken).HasMaxLength(50);
            builder.Property(e => e.ReferenceId).HasMaxLength(100);
            builder.Property(e => e.Status).HasMaxLength(50);
            builder.Property(e => e.Subject).HasMaxLength(400);
            builder.Property(e => e.Type).HasMaxLength(50);

            builder.HasOne(d => d.Application).WithMany(p => p.Tokens).HasForeignKey(d => d.ApplicationId);

            builder.HasOne(d => d.Authorization).WithMany(p => p.Tokens).HasForeignKey(d => d.AuthorizationId);
        }
    }
}
