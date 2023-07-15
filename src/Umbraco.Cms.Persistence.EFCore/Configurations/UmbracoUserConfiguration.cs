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
    internal class UmbracoUserConfiguration : IEntityTypeConfiguration<UmbracoUser>
    {
        public void Configure(EntityTypeBuilder<UmbracoUser> builder)
        {
            builder.HasKey(e => e.Id).HasName("PK_user");
            builder.ToTable("umbracoUser");

            builder.HasIndex(e => e.UserLogin, "IX_umbracoUser_userLogin");

            builder.Property(e => e.Id).HasColumnName("id");
            builder.Property(e => e.Avatar)
                .HasMaxLength(500)
                .HasColumnName("avatar");
            builder.Property(e => e.CreateDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("createDate");
            builder.Property(e => e.EmailConfirmedDate)
                .HasColumnType("datetime")
                .HasColumnName("emailConfirmedDate");
            builder.Property(e => e.FailedLoginAttempts).HasColumnName("failedLoginAttempts");
            builder.Property(e => e.InvitedDate)
                .HasColumnType("datetime")
                .HasColumnName("invitedDate");
            builder.Property(e => e.LastLockoutDate)
                .HasColumnType("datetime")
                .HasColumnName("lastLockoutDate");
            builder.Property(e => e.LastLoginDate)
                .HasColumnType("datetime")
                .HasColumnName("lastLoginDate");
            builder.Property(e => e.LastPasswordChangeDate)
                .HasColumnType("datetime")
                .HasColumnName("lastPasswordChangeDate");
            builder.Property(e => e.PasswordConfig)
                .HasMaxLength(500)
                .HasColumnName("passwordConfig");
            builder.Property(e => e.SecurityStampToken)
                .HasMaxLength(255)
                .HasColumnName("securityStampToken");
            builder.Property(e => e.TourData).HasColumnName("tourData");
            builder.Property(e => e.UpdateDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updateDate");
            builder.Property(e => e.UserDisabled)
                .IsRequired()
                .HasDefaultValueSql("('0')")
                .HasColumnName("userDisabled");
            builder.Property(e => e.UserEmail)
                .HasMaxLength(255)
                .HasColumnName("userEmail");
            builder.Property(e => e.UserLanguage)
                .HasMaxLength(10)
                .HasColumnName("userLanguage");
            builder.Property(e => e.UserLogin)
                .HasMaxLength(125)
                .HasColumnName("userLogin");
            builder.Property(e => e.UserName)
                .HasMaxLength(255)
                .HasColumnName("userName");
            builder.Property(e => e.UserNoConsole)
                .IsRequired()
                .HasDefaultValueSql("('0')")
                .HasColumnName("userNoConsole");
            builder.Property(e => e.UserPassword)
                .HasMaxLength(500)
                .HasColumnName("userPassword");

            builder.HasMany(d => d.UserGroups).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "UmbracoUser2UserGroup",
                    r => r.HasOne<UmbracoUserGroup>().WithMany()
                        .HasForeignKey("UserGroupId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_umbracoUser2UserGroup_umbracoUserGroup_id"),
                    l => l.HasOne<UmbracoUser>().WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_umbracoUser2UserGroup_umbracoUser_id"),
                    j =>
                    {
                        j.HasKey("UserId", "UserGroupId").HasName("PK_user2userGroup");
                        j.ToTable("umbracoUser2UserGroup");
                        j.IndexerProperty<int>("UserId").HasColumnName("userId");
                        j.IndexerProperty<int>("UserGroupId").HasColumnName("userGroupId");
                    });
        }
    }
}
