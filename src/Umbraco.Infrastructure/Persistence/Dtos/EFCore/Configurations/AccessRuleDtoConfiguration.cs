using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

public class AccessRuleDtoConfiguration : IEntityTypeConfiguration<AccessRuleDto>
{
    public void Configure(EntityTypeBuilder<AccessRuleDto> builder)
    {
        builder.ToTable(AccessRuleDto.TableName);

        builder.HasKey(x => x.Id)
            .HasName($"PK_{AccessRuleDto.TableName}");

        builder.Property(x => x.Id)
            .HasColumnName(AccessRuleDto.PrimaryKeyColumnName)
            .ValueGeneratedNever();

        builder.Property(x => x.AccessId)
            .HasColumnName("accessId");

        builder.Property(x => x.RuleValue)
            .HasColumnName("ruleValue");

        builder.Property(x => x.RuleType)
            .HasColumnName("ruleType");

        builder.Property(x => x.CreateDate)
            .HasColumnName("createDate");

        builder.Property(x => x.UpdateDate)
            .HasColumnName("updateDate");

        // IX_umbracoAccessRule — unique composite index on (ruleValue, ruleType, accessId).
        builder.HasIndex(x => new { x.RuleValue, x.RuleType, x.AccessId })
            .IsUnique()
            .HasDatabaseName($"IX_{AccessRuleDto.TableName}");
    }
}
