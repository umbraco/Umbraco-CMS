using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

public class TemplateDtoConfiguration : IEntityTypeConfiguration<TemplateDto>
{
    public void Configure(EntityTypeBuilder<TemplateDto> builder)
    {
        builder.ToTable(TemplateDto.TableName);

        builder.HasKey(x => x.PrimaryKey);

        builder.Property(x => x.PrimaryKey)
            .HasColumnName(TemplateDto.PrimaryKeyColumnName)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.NodeId)
            .HasColumnName(TemplateDto.NodeIdColumnName);

        builder.Property(x => x.Alias)
            .HasColumnName(TemplateDto.AliasColumnName)
            .HasMaxLength(100);

        builder.HasIndex(x => x.NodeId)
            .IsUnique()
            .HasDatabaseName($"IX_{TemplateDto.TableName}_{TemplateDto.NodeIdColumnName}");

        builder.HasOne(x => x.NodeDto)
            .WithMany()
            .HasForeignKey(x => x.NodeId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName($"FK_{TemplateDto.TableName}_{NodeDto.TableName}");
    }
}
