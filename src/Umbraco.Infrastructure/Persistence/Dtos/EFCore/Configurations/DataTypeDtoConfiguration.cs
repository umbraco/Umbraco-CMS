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
            .HasColumnName(DataTypeDto.EditorAliasColumnName)
            .IsRequired();

        builder.Property(x => x.EditorUiAlias)
            .HasColumnName("propertyEditorUiAlias");

        builder.Property(x => x.DbType)
            .HasColumnName(DataTypeDto.DbTypeColumnName)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Configuration)
            .HasColumnName("config");

        // FK: NodeId -> umbracoNode.id
        builder.HasOne<NodeDto>()
            .WithMany()
            .HasForeignKey(x => x.NodeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
