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
    internal class UmbracoUserStartNodeConfiguration : IEntityTypeConfiguration<UmbracoUserStartNode>
    {
        public void Configure(EntityTypeBuilder<UmbracoUserStartNode> builder)
        {
            builder.HasKey(e => e.Id).HasName("PK_userStartNode");

            builder.ToTable("umbracoUserStartNode");

            builder.HasIndex(e => new { e.StartNodeType, e.StartNode, e.UserId }, "IX_umbracoUserStartNode_startNodeType").IsUnique();

            builder.Property(e => e.Id).HasColumnName("id");
            builder.Property(e => e.StartNode).HasColumnName("startNode");
            builder.Property(e => e.StartNodeType).HasColumnName("startNodeType");
            builder.Property(e => e.UserId).HasColumnName("userId");

            builder.HasOne(d => d.StartNodeNavigation).WithMany(p => p.UmbracoUserStartNodes)
                .HasForeignKey(d => d.StartNode)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_umbracoUserStartNode_umbracoNode_id");

            builder.HasOne(d => d.User).WithMany(p => p.UmbracoUserStartNodes)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_umbracoUserStartNode_umbracoUser_id");
        }
    }
}
