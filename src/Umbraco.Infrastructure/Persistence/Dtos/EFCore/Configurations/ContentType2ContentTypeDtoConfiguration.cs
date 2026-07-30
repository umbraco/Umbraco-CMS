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

        builder.HasOne(x => x.ParentNode)
            .WithMany()
            .HasForeignKey(x => x.ParentId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_cmsContentType2ContentType_umbracoNode_parent");

        builder.HasOne(x => x.ChildNode)
            .WithMany()
            .HasForeignKey(x => x.ChildId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName("FK_cmsContentType2ContentType_umbracoNode_child");
    }
}
