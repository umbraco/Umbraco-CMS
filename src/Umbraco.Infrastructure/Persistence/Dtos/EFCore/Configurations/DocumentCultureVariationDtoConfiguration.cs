using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

public class DocumentCultureVariationDtoConfiguration : IEntityTypeConfiguration<DocumentCultureVariationDto>
{
    public void Configure(EntityTypeBuilder<DocumentCultureVariationDto> builder)
    {
        builder.ToTable(DocumentCultureVariationDto.TableName);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName(DocumentCultureVariationDto.PrimaryKeyColumnName)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.NodeId)
            .HasColumnName(DocumentCultureVariationDto.NodeIdColumnName);

        builder.Property(x => x.LanguageId)
            .HasColumnName(DocumentCultureVariationDto.LanguageIdColumnName);

        builder.Property(x => x.Edited)
            .HasColumnName(DocumentCultureVariationDto.EditedColumnName);

        builder.Property(x => x.Available)
            .HasColumnName(DocumentCultureVariationDto.AvailableColumnName);

        builder.Property(x => x.Published)
            .HasColumnName(DocumentCultureVariationDto.PublishedColumnName);

        builder.Property(x => x.Name)
            .HasColumnName(DocumentCultureVariationDto.NameColumnName);

        // FK: NodeId -> umbracoNode.id
        builder.HasOne<NodeDto>()
            .WithMany()
            .HasForeignKey(x => x.NodeId)
            .OnDelete(DeleteBehavior.Cascade);

        // FK: LanguageId -> umbracoLanguage.id
        builder.HasOne<LanguageDto>()
            .WithMany()
            .HasForeignKey(x => x.LanguageId)
            .OnDelete(DeleteBehavior.Cascade);

        // IX_umbracoDocumentCultureVariation_NodeId (unique, composite on NodeId+LanguageId)
        builder.HasIndex(x => new { x.NodeId, x.LanguageId })
            .IsUnique()
            .HasDatabaseName($"IX_{DocumentCultureVariationDto.TableName}_NodeId");

        // IX_umbracoDocumentCultureVariation_LanguageId
        builder.HasIndex(x => x.LanguageId)
            .HasDatabaseName($"IX_{DocumentCultureVariationDto.TableName}_LanguageId");
    }
}
