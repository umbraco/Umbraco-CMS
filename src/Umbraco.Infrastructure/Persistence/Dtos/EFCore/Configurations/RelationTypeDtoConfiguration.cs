using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

public class RelationTypeDtoConfiguration : IEntityTypeConfiguration<RelationTypeDto>
{
    public void Configure(EntityTypeBuilder<RelationTypeDto> builder)
    {
        builder.ToTable(RelationTypeDto.TableName);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName(RelationTypeDto.IdColumnName)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.UniqueId)
            .HasColumnName(RelationTypeDto.UniqueIdColumnName);

        builder.Property(x => x.Dual)
            .HasColumnName(RelationTypeDto.DualColumnName);

        builder.Property(x => x.ParentObjectType)
            .HasColumnName(RelationTypeDto.ParentObjectTypeColumnName);

        builder.Property(x => x.ChildObjectType)
            .HasColumnName(RelationTypeDto.ChildObjectTypeColumnName);

        builder.Property(x => x.Name)
            .HasColumnName(RelationTypeDto.NameColumnName)
            .IsRequired();

        builder.Property(x => x.Alias)
            .HasColumnName(RelationTypeDto.AliasColumnName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.IsDependency)
            .HasColumnName(RelationTypeDto.IsDependencyColumnName)
            .HasDefaultValue(false);

        // IX_umbracoRelationType_UniqueId
        builder.HasIndex(x => x.UniqueId)
            .IsUnique()
            .HasDatabaseName($"IX_{RelationTypeDto.TableName}_UniqueId");

        // IX_umbracoRelationType_name
        builder.HasIndex(x => x.Name)
            .IsUnique()
            .HasDatabaseName($"IX_{RelationTypeDto.TableName}_name");

        // IX_umbracoRelationType_alias
        builder.HasIndex(x => x.Alias)
            .IsUnique()
            .HasDatabaseName($"IX_{RelationTypeDto.TableName}_alias");
    }
}
