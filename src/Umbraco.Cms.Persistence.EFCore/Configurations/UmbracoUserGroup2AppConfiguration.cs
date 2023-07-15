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
    internal class UmbracoUserGroup2AppConfiguration : IEntityTypeConfiguration<UmbracoUserGroup2App>
    {
        public void Configure(EntityTypeBuilder<UmbracoUserGroup2App> builder)
        {
            builder.HasKey(e => new { e.UserGroupId, e.App }).HasName("PK_userGroup2App");

            builder.ToTable("umbracoUserGroup2App");

            builder.Property(e => e.UserGroupId).HasColumnName("userGroupId");
            builder.Property(e => e.App)
                .HasMaxLength(50)
                .HasColumnName("app");

            builder.HasOne(d => d.UserGroup).WithMany(p => p.UmbracoUserGroup2Apps)
                .HasForeignKey(d => d.UserGroupId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_umbracoUserGroup2App_umbracoUserGroup_id");
        }
    }
}
