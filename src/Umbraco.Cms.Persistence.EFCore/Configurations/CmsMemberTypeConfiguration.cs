using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Umbraco.Cms.Persistence.EFCore.Models;

namespace Umbraco.Cms.Persistence.EFCore.Configurations
{
    internal class CmsMemberTypeConfiguration : IEntityTypeConfiguration<CmsMemberType>
    {
        public void Configure(EntityTypeBuilder<CmsMemberType> builder)
        {
            builder.HasKey(e => e.Pk);

            builder.ToTable("cmsMemberType");

            builder.Property(e => e.Pk).HasColumnName("pk");
            builder.Property(e => e.IsSensitive)
                .IsRequired()
                .HasDefaultValueSql("('0')")
                .HasColumnName("isSensitive");
            builder.Property(e => e.MemberCanEdit)
                .IsRequired()
                .HasDefaultValueSql("('0')")
                .HasColumnName("memberCanEdit");
            builder.Property(e => e.PropertytypeId).HasColumnName("propertytypeId");
            builder.Property(e => e.ViewOnProfile)
                .IsRequired()
                .HasDefaultValueSql("('0')")
                .HasColumnName("viewOnProfile");

            builder.HasOne(d => d.Node).WithMany(p => p.CmsMemberTypes)
                .HasForeignKey(d => d.NodeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_cmsMemberType_umbracoNode_id");

            builder.HasOne(d => d.NodeNavigation).WithMany(p => p.CmsMemberTypes)
                .HasPrincipalKey(p => p.NodeId)
                .HasForeignKey(d => d.NodeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_cmsMemberType_cmsContentType_nodeId");
        }
    }
}
