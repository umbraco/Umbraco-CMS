using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

internal class DocumentVersionDtoConfiguration : IEntityTypeConfiguration<DocumentVersionDto>
{
    public void Configure(EntityTypeBuilder<DocumentVersionDto> builder)
    {
        builder.ToTable(DocumentVersionDto.TableName);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName(DocumentVersionDto.IdColumnName)
            .ValueGeneratedNever(); // Non-auto-increment: FK PK

        builder.Property(x => x.TemplateId)
            .HasColumnName(DocumentVersionDto.TemplateIdColumnName);

        builder.Property(x => x.Published)
            .HasColumnName(DocumentVersionDto.PublishedColumnName)
            .IsRequired();

        // Base indexes (without include columns - SQL Server adds those via customizer)
        builder.HasIndex(x => new { x.Id, x.Published })
            .HasDatabaseName($"IX_{DocumentVersionDto.TableName}_id_published");

        builder.HasIndex(x => x.Published)
            .HasDatabaseName($"IX_{DocumentVersionDto.TableName}_published");

        // FK to cmsTemplate.nodeId — use HasPrincipalKey to reference NodeId (alternate key)
        builder.HasOne(x => x.TemplateDto)
            .WithMany()
            .HasForeignKey(x => x.TemplateId)
            .HasPrincipalKey(x => x.NodeId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.NoAction);

        // TODO: Configure FK to cmsContentVersion when ContentVersionDto is migrated to EFCore
    }
}
