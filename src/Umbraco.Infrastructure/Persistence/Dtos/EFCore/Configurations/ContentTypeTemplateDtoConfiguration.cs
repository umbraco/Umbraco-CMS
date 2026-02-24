using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

internal class ContentTypeTemplateDtoConfiguration : IEntityTypeConfiguration<ContentTypeTemplateDto>
{
    public void Configure(EntityTypeBuilder<ContentTypeTemplateDto> builder)
    {
        builder.ToTable(ContentTypeTemplateDto.TableName);

        builder.HasKey(x => new { x.ContentTypeNodeId, x.TemplateNodeId })
            .HasName("PK_cmsDocumentType");

        builder.Property(x => x.ContentTypeNodeId)
            .HasColumnName(ContentTypeTemplateDto.ContentTypeNodeIdColumnName)
            .IsRequired();

        builder.Property(x => x.TemplateNodeId)
            .HasColumnName(ContentTypeTemplateDto.TemplateNodeIdColumnName)
            .IsRequired();

        builder.Property(x => x.IsDefault)
            .HasColumnName(ContentTypeTemplateDto.IsDefaultColumnName)
            .HasDefaultValue(false)
            .IsRequired();

        // FK to cmsTemplate.nodeId — use HasPrincipalKey to reference NodeId (alternate key)
        builder.HasOne(x => x.TemplateDto)
            .WithMany()
            .HasForeignKey(x => x.TemplateNodeId)
            .HasPrincipalKey(x => x.NodeId)
            .OnDelete(DeleteBehavior.NoAction);

        // TODO: Configure FK to cmsContentType.nodeId when ContentTypeDto is migrated to EFCore
        // TODO: Configure FK to umbracoNode.id when the content type node FK is modelled in EFCore
    }
}
