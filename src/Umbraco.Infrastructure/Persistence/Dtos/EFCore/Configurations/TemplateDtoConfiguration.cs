using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

internal class TemplateDtoConfiguration : IEntityTypeConfiguration<TemplateDto>
{
    public void Configure(EntityTypeBuilder<TemplateDto> builder)
    {
        builder.ToTable(TemplateDto.TableName);

        builder.HasKey(x => x.PrimaryKey);

        builder.Property(x => x.PrimaryKey)
            .HasColumnName(TemplateDto.PrimaryKeyColumnName)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.NodeId)
            .HasColumnName(TemplateDto.NodeIdColumnName)
            .IsRequired();

        builder.HasIndex(x => x.NodeId)
            .IsUnique();

        builder.Property(x => x.Alias)
            .HasColumnName("alias")
            .HasMaxLength(100);

        builder.HasOne(x => x.NodeDto)
            .WithOne()
            .HasForeignKey<TemplateDto>(x => x.NodeId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
