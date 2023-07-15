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
    internal class UmbracoDocumentCultureVariationConfiguration : IEntityTypeConfiguration<UmbracoDocumentCultureVariation>
    {
        public void Configure(EntityTypeBuilder<UmbracoDocumentCultureVariation> builder)
        {
            builder.ToTable("umbracoDocumentCultureVariation");

            builder.HasIndex(e => e.LanguageId, "IX_umbracoDocumentCultureVariation_LanguageId");

            builder.HasIndex(e => new { e.NodeId, e.LanguageId }, "IX_umbracoDocumentCultureVariation_NodeId").IsUnique();

            builder.Property(e => e.Id).HasColumnName("id");
            builder.Property(e => e.Available).HasColumnName("available");
            builder.Property(e => e.Edited).HasColumnName("edited");
            builder.Property(e => e.LanguageId).HasColumnName("languageId");
            builder.Property(e => e.Name)
                .HasMaxLength(255)
                .HasColumnName("name");
            builder.Property(e => e.NodeId).HasColumnName("nodeId");
            builder.Property(e => e.Published).HasColumnName("published");

            builder.HasOne(d => d.Language).WithMany(p => p.UmbracoDocumentCultureVariations)
                .HasForeignKey(d => d.LanguageId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_umbracoDocumentCultureVariation_umbracoLanguage_id");

            builder.HasOne(d => d.Node).WithMany(p => p.UmbracoDocumentCultureVariations)
                .HasForeignKey(d => d.NodeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_umbracoDocumentCultureVariation_umbracoNode_id");
        }
    }
}
