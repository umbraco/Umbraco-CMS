using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Umbraco.Cms.Persistence.EFCore.Models;

namespace Umbraco.Cms.Persistence.EFCore.Configurations
{
    internal class CmsMemberConfiguration : IEntityTypeConfiguration<CmsMember>
    {
        public void Configure(EntityTypeBuilder<CmsMember> builder)
        {
            builder.HasKey(e => e.NodeId);

            builder.ToTable("cmsMember");

            builder.HasIndex(e => e.LoginName, "IX_cmsMember_LoginName");

            builder.Property(e => e.NodeId)
                .ValueGeneratedNever()
                .HasColumnName("nodeId");
            builder.Property(e => e.Email)
                .HasMaxLength(1000)
                .HasDefaultValueSql("('''')");
            builder.Property(e => e.EmailConfirmedDate)
                .HasColumnType("datetime")
                .HasColumnName("emailConfirmedDate");
            builder.Property(e => e.FailedPasswordAttempts).HasColumnName("failedPasswordAttempts");
            builder.Property(e => e.IsApproved)
                .IsRequired()
                .HasDefaultValueSql("('1')")
                .HasColumnName("isApproved");
            builder.Property(e => e.IsLockedOut)
                .HasDefaultValueSql("('0')")
                .HasColumnName("isLockedOut");
            builder.Property(e => e.LastLockoutDate)
                .HasColumnType("datetime")
                .HasColumnName("lastLockoutDate");
            builder.Property(e => e.LastLoginDate)
                .HasColumnType("datetime")
                .HasColumnName("lastLoginDate");
            builder.Property(e => e.LastPasswordChangeDate)
                .HasColumnType("datetime")
                .HasColumnName("lastPasswordChangeDate");
            builder.Property(e => e.LoginName)
                .HasMaxLength(1000)
                .HasDefaultValueSql("('''')");
            builder.Property(e => e.Password)
                .HasMaxLength(1000)
                .HasDefaultValueSql("('''')");
            builder.Property(e => e.PasswordConfig)
                .HasMaxLength(500)
                .HasColumnName("passwordConfig");
            builder.Property(e => e.SecurityStampToken)
                .HasMaxLength(255)
                .HasColumnName("securityStampToken");

            builder.HasOne(d => d.Node).WithOne(p => p.CmsMember)
                .HasForeignKey<CmsMember>(d => d.NodeId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            builder.HasMany(d => d.MemberGroups).WithMany(p => p.Members)
                .UsingEntity<Dictionary<string, object>>(
                    "CmsMember2MemberGroup",
                    r => r.HasOne<UmbracoNode>().WithMany()
                        .HasForeignKey("MemberGroup")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_cmsMember2MemberGroup_umbracoNode_id"),
                    l => l.HasOne<CmsMember>().WithMany()
                        .HasForeignKey("Member")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_cmsMember2MemberGroup_cmsMember_nodeId"),
                    j =>
                    {
                        j.HasKey("Member", "MemberGroup");
                        j.ToTable("cmsMember2MemberGroup");
                    });
        }
    }
}
