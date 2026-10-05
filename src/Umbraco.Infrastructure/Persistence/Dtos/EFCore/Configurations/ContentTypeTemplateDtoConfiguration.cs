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

        // No EF Core navigations are declared for the content-type and template FKs: they reference the alternate
        // key (nodeId) of cmsContentType and cmsTemplate rather than their primary keys.
        // TODO (EF Core): the FKs to cmsContentType.nodeId, umbracoNode and cmsTemplate.nodeId are currently created
        // by NPoco's schema; revisit this comment once NPoco is removed.
    }
}
