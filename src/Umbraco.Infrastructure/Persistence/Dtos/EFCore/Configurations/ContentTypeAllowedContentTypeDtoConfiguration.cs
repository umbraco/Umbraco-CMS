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

        // FKs to cmsContentType.nodeId are created by NPoco's schema; no EF Core navigations are
        // declared because they reference the content-type alternate key (nodeId) rather than its
        // primary key, and the repository matches allowed types in memory rather than via navigation.
    }
}
