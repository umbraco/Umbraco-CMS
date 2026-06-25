using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

public class ContentTypeTemplateDtoConfiguration : IEntityTypeConfiguration<ContentTypeTemplateDto>
{
    public void Configure(EntityTypeBuilder<ContentTypeTemplateDto> builder)
    {
        builder.ToTable(ContentTypeTemplateDto.TableName);

        builder.HasKey(x => new { x.ContentTypeNodeId, x.TemplateNodeId })
            .HasName("PK_cmsDocumentType");

        builder.Property(x => x.ContentTypeNodeId)
            .HasColumnName(ContentTypeTemplateDto.ContentTypeNodeIdColumnName);

        builder.Property(x => x.TemplateNodeId)
            .HasColumnName(ContentTypeTemplateDto.TemplateNodeIdColumnName);

        builder.Property(x => x.IsDefault)
            .HasColumnName(ContentTypeTemplateDto.IsDefaultColumnName)
            .HasDefaultValue(false);

        // No EF Core navigation is declared for the content-type FK: it references the alternate key
        // (nodeId) rather than the primary key.
        // TODO (EF Core): the FKs to cmsContentType.nodeId, umbracoNode and cmsTemplate are currently created by
        // NPoco's schema, and cmsTemplate has no EF Core DTO yet; revisit this comment once NPoco is removed.
    }
}
