using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Umbraco.Cms.Infrastructure.Persistence.Models;

namespace Umbraco.Cms.Persistence.EFCore.Configurations
{
    internal class UmbracoAccessRuleConfiguration : IEntityTypeConfiguration<UmbracoAccessRule>
    {
        public void Configure(EntityTypeBuilder<UmbracoAccessRule> builder)
        {
            builder.ToTable("umbracoAccessRule");

            builder.HasIndex(e => new { e.RuleValue, e.RuleType, e.AccessId }, "IX_umbracoAccessRule").IsUnique();

            builder.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            builder.Property(e => e.AccessId).HasColumnName("accessId");
            builder.Property(e => e.CreateDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("createDate");
            builder.Property(e => e.RuleType)
                .HasMaxLength(255)
                .HasColumnName("ruleType");
            builder.Property(e => e.RuleValue)
                .HasMaxLength(255)
                .HasColumnName("ruleValue");
            builder.Property(e => e.UpdateDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updateDate");

            builder.HasOne(d => d.Access).WithMany(p => p.UmbracoAccessRules)
                .HasForeignKey(d => d.AccessId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_umbracoAccessRule_umbracoAccess_id");
        }
    }
}
