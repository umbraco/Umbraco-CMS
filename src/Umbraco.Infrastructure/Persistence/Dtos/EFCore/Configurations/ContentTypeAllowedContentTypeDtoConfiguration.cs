using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

public class ContentTypeAllowedContentTypeDtoConfiguration : IEntityTypeConfiguration<ContentTypeAllowedContentTypeDto>
{
    public void Configure(EntityTypeBuilder<ContentTypeAllowedContentTypeDto> builder)
    {
        builder.ToTable(ContentTypeAllowedContentTypeDto.TableName);

        builder.HasKey(x => new { x.Id, x.AllowedId })
            .HasName("PK_cmsContentTypeAllowedContentType");

        builder.Property(x => x.Id)
            .HasColumnName(ContentTypeAllowedContentTypeDto.IdKeyColumnName);

        builder.Property(x => x.AllowedId)
            .HasColumnName(ContentTypeAllowedContentTypeDto.AllowedIdColumnName);

        builder.Property(x => x.SortOrder)
            .HasColumnName(ContentTypeAllowedContentTypeDto.SortOrderColumnName)
            .HasDefaultValue(0);

        // No EF Core navigations are declared: the FKs reference the content-type alternate key (nodeId)
        // rather than its primary key, and the repository matches allowed types in memory rather than via navigation.
        // TODO (EF Core): the FKs to cmsContentType.nodeId are currently created by NPoco's schema; revisit this comment once NPoco is removed.
    }
}
