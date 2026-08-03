using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

public class DataTypeDtoConfiguration : IEntityTypeConfiguration<DataTypeDto>
{
    public void Configure(EntityTypeBuilder<DataTypeDto> builder)
    {
        builder.ToTable(DataTypeDto.TableName);

        builder.HasKey(x => x.NodeId);

        builder.Property(x => x.NodeId)
            .HasColumnName(DataTypeDto.PrimaryKeyColumnName)
            .ValueGeneratedNever();

        builder.Property(x => x.EditorAlias)
            .HasColumnName(DataTypeDto.EditorAliasColumnName);

        builder.Property(x => x.EditorUiAlias)
            .HasColumnName(DataTypeDto.EditorUiAliasColumnName);

        builder.Property(x => x.DbType)
            .HasColumnName(DataTypeDto.DbTypeColumnName)
            .HasMaxLength(50);

        builder.Property(x => x.Configuration)
            .HasColumnName(DataTypeDto.ConfigurationColumnName);

        // FK to umbracoNode (NodeId -> NodeDto.NodeId)
        builder.HasOne(x => x.NodeDto)
            .WithMany()
            .HasForeignKey(x => x.NodeId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName($"FK_{DataTypeDto.TableName}_{NodeDto.TableName}");
    }
}
