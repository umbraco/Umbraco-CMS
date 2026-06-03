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

        // FKs to cmsContentType.nodeId, umbracoNode and cmsTemplate are created by NPoco's schema.
        // No EF Core navigations are declared: cmsTemplate has no EF Core DTO yet and the content-type
        // FK references the alternate key (nodeId) rather than the primary key.
    }
}
