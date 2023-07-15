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
    internal class UmbracoUser2NodeNotifyConfiguration : IEntityTypeConfiguration<UmbracoUser2NodeNotify>
    {
        public void Configure(EntityTypeBuilder<UmbracoUser2NodeNotify> builder)
        {
            builder.HasKey(e => new { e.UserId, e.NodeId, e.Action });

            builder.ToTable("umbracoUser2NodeNotify");

            builder.Property(e => e.UserId).HasColumnName("userId");
            builder.Property(e => e.NodeId).HasColumnName("nodeId");
            builder.Property(e => e.Action)
                .HasMaxLength(1)
                .IsFixedLength()
                .HasColumnName("action");

            builder.HasOne(d => d.Node).WithMany(p => p.UmbracoUser2NodeNotifies)
                .HasForeignKey(d => d.NodeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_umbracoUser2NodeNotify_umbracoNode_id");

            builder.HasOne(d => d.User).WithMany(p => p.UmbracoUser2NodeNotifies)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_umbracoUser2NodeNotify_umbracoUser_id");
        }
    }
}
