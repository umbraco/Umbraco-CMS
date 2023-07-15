using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Umbraco.Cms.Persistence.EFCore.Models;

namespace Umbraco.Cms.Persistence.EFCore.Configurations
{
    internal class UmbracoExternalLoginConfiguration : IEntityTypeConfiguration<UmbracoExternalLogin>
    {
        public void Configure(EntityTypeBuilder<UmbracoExternalLogin> builder)
        {
            builder.ToTable("umbracoExternalLogin");

            builder.HasIndex(e => new { e.LoginProvider, e.UserOrMemberKey }, "IX_umbracoExternalLogin_LoginProvider").IsUnique();

            builder.HasIndex(e => new { e.LoginProvider, e.ProviderKey }, "IX_umbracoExternalLogin_ProviderKey");

            builder.HasIndex(e => e.UserOrMemberKey, "IX_umbracoExternalLogin_userOrMemberKey");

            builder.Property(e => e.Id).HasColumnName("id");
            builder.Property(e => e.CreateDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("createDate");
            builder.Property(e => e.LoginProvider)
                .HasMaxLength(400)
                .HasColumnName("loginProvider");
            builder.Property(e => e.ProviderKey)
                .HasMaxLength(4000)
                .HasColumnName("providerKey");
            builder.Property(e => e.UserData).HasColumnName("userData");
            builder.Property(e => e.UserOrMemberKey).HasColumnName("userOrMemberKey");
        }
    }
}
