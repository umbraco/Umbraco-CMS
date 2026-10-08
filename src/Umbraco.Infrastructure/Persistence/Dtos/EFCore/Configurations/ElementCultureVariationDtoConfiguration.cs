using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

public class ElementCultureVariationDtoConfiguration : IEntityTypeConfiguration<ElementCultureVariationDto>
{
    public void Configure(EntityTypeBuilder<ElementCultureVariationDto> builder)
    {
        builder.ToTable(ElementCultureVariationDto.TableName);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName(ElementCultureVariationDto.PrimaryKeyColumnName)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.NodeId)
            .HasColumnName(ElementCultureVariationDto.NodeIdColumnName);

        builder.Property(x => x.LanguageId)
            .HasColumnName(ElementCultureVariationDto.LanguageIdColumnName);

        builder.Property(x => x.Edited)
            .HasColumnName(ElementCultureVariationDto.EditedColumnName);

        builder.Property(x => x.Available)
            .HasColumnName(ElementCultureVariationDto.AvailableColumnName);

        builder.Property(x => x.Published)
            .HasColumnName(ElementCultureVariationDto.PublishedColumnName);

        builder.Property(x => x.Name)
            .HasColumnName(ElementCultureVariationDto.NameColumnName);

        // FK: NodeId -> umbracoNode.id
        builder.HasOne<NodeDto>()
            .WithMany()
            .HasForeignKey(x => x.NodeId)
            .OnDelete(DeleteBehavior.NoAction);

        // FK: LanguageId -> umbracoLanguage.id
        builder.HasOne<LanguageDto>()
            .WithMany()
            .HasForeignKey(x => x.LanguageId)
            .OnDelete(DeleteBehavior.NoAction);

        // IX_umbracoElementCultureVariation_NodeId (unique, composite on NodeId+LanguageId)
        builder.HasIndex(x => new { x.NodeId, x.LanguageId })
            .IsUnique()
            .HasDatabaseName($"IX_{ElementCultureVariationDto.TableName}_NodeId");

        // IX_umbracoElementCultureVariation_LanguageId
        builder.HasIndex(x => x.LanguageId)
            .HasDatabaseName($"IX_{ElementCultureVariationDto.TableName}_LanguageId");
    }
}
