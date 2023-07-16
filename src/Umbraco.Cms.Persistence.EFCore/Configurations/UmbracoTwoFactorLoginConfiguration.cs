using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Umbraco.Cms.Infrastructure.Persistence.Models;

namespace Umbraco.Cms.Persistence.EFCore.Configurations
{
    internal class UmbracoTwoFactorLoginConfiguration : IEntityTypeConfiguration<UmbracoTwoFactorLogin>
    {
        public void Configure(EntityTypeBuilder<UmbracoTwoFactorLogin> builder)
        {
            builder.ToTable("umbracoTwoFactorLogin");

            builder.HasIndex(e => new { e.ProviderName, e.UserOrMemberKey }, "IX_umbracoTwoFactorLogin_ProviderName").IsUnique();

            builder.HasIndex(e => e.UserOrMemberKey, "IX_umbracoTwoFactorLogin_userOrMemberKey");

            builder.Property(e => e.Id).HasColumnName("id");
            builder.Property(e => e.ProviderName)
                .HasMaxLength(400)
                .HasColumnName("providerName");
            builder.Property(e => e.Secret)
                .HasMaxLength(400)
                .HasColumnName("secret");
            builder.Property(e => e.UserOrMemberKey).HasColumnName("userOrMemberKey");
        }
    }
}
