using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Umbraco.Cms.Persistence.EFCore.Models;

namespace Umbraco.Cms.Persistence.EFCore.Configurations
{
    internal class UmbracoConsentConfiguration : IEntityTypeConfiguration<UmbracoConsent>
    {
        public void Configure(EntityTypeBuilder<UmbracoConsent> builder)
        {
            builder.ToTable("umbracoConsent");

            builder.Property(e => e.Id).HasColumnName("id");
            builder.Property(e => e.Action)
                .HasMaxLength(512)
                .HasColumnName("action");
            builder.Property(e => e.Comment)
                .HasMaxLength(255)
                .HasColumnName("comment");
            builder.Property(e => e.Context)
                .HasMaxLength(128)
                .HasColumnName("context");
            builder.Property(e => e.CreateDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("createDate");
            builder.Property(e => e.Current).HasColumnName("current");
            builder.Property(e => e.Source)
                .HasMaxLength(512)
                .HasColumnName("source");
            builder.Property(e => e.State).HasColumnName("state");
        }
    }
}
