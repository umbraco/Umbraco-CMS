using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

public class DocumentVersionDtoConfiguration : IEntityTypeConfiguration<DocumentVersionDto>
{
    public void Configure(EntityTypeBuilder<DocumentVersionDto> builder)
    {
        builder.ToTable(DocumentVersionDto.TableName);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName(DocumentVersionDto.PrimaryKeyColumnName)
            .ValueGeneratedNever();

        builder.Property(x => x.TemplateId)
            .HasColumnName(DocumentVersionDto.TemplateIdColumnName);

        builder.Property(x => x.Published)
            .HasColumnName(DocumentVersionDto.PublishedColumnName);

        // FK: Id -> umbracoContentVersion.id
        builder.HasOne<ContentVersionDto>()
            .WithMany()
            .HasForeignKey(x => x.Id)
            .OnDelete(DeleteBehavior.Cascade);

        // IX_umbracoDocumentVersion_id_published (composite on Id+Published)
        // Note: SQL Server included columns (TemplateId) are added by SqlServerDocumentVersionDtoModelCustomizer.
        builder.HasIndex(x => new { x.Id, x.Published })
            .HasDatabaseName($"IX_{DocumentVersionDto.TableName}_id_published");

        // IX_umbracoDocumentVersion_published (on Published)
        // Note: SQL Server included columns (Id, TemplateId) are added by SqlServerDocumentVersionDtoModelCustomizer.
        builder.HasIndex(x => x.Published)
            .HasDatabaseName($"IX_{DocumentVersionDto.TableName}_published");
    }
}
