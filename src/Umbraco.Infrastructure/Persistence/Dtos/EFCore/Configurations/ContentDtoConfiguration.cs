using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

public class ContentDtoConfiguration : IEntityTypeConfiguration<ContentDto>
{
    public void Configure(EntityTypeBuilder<ContentDto> builder)
    {
        builder.ToTable(ContentDto.TableName);

        builder.HasKey(x => x.NodeId);

        builder.Property(x => x.NodeId)
            .HasColumnName(ContentDto.PrimaryKeyColumnName)
            .ValueGeneratedNever();

        builder.Property(x => x.ContentTypeId)
            .HasColumnName(ContentDto.ContentTypeIdColumnName);

        // IX_umbracoContent_contentTypeId
        builder.HasIndex(x => x.ContentTypeId)
            .HasDatabaseName($"IX_{ContentDto.TableName}_{ContentDto.ContentTypeIdColumnName}");

        // FK: NodeId -> umbracoNode.id
        builder.HasOne<NodeDto>()
            .WithMany()
            .HasForeignKey(x => x.NodeId)
            .OnDelete(DeleteBehavior.NoAction);

        // FK: ContentTypeId -> cmsContentType.nodeId
        builder.HasOne<ContentTypeDto>()
            .WithMany()
            .HasForeignKey(x => x.ContentTypeId)
            .HasPrincipalKey(x => x.NodeId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.Ignore(x => x.NodeDto);
    }
}
