using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

public class ContentType2ContentTypeDtoConfiguration : IEntityTypeConfiguration<ContentType2ContentTypeDto>
{
    public void Configure(EntityTypeBuilder<ContentType2ContentTypeDto> builder)
    {
        builder.ToTable(ContentType2ContentTypeDto.TableName);

        builder.HasKey(x => new { x.ParentId, x.ChildId })
            .HasName("PK_cmsContentType2ContentType");

        builder.Property(x => x.ParentId)
            .HasColumnName(ContentType2ContentTypeDto.ParentIdColumnName);

        builder.Property(x => x.ChildId)
            .HasColumnName(ContentType2ContentTypeDto.ChildIdColumnName);

        // FKs to umbracoNode (parent and child). The DB constraints are created by NPoco's schema;
        // no EF Core navigations are declared because they reference umbracoNode rather than the
        // content-type primary key and are not traversed by the repository.
    }
}
