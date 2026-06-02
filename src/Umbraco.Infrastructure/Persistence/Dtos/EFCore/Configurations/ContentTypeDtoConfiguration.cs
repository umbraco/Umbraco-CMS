using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

public class ContentTypeDtoConfiguration : IEntityTypeConfiguration<ContentTypeDto>
{
    public void Configure(EntityTypeBuilder<ContentTypeDto> builder)
    {
        builder.ToTable(ContentTypeDto.TableName);

        builder.HasKey(x => x.PrimaryKey);

        builder.Property(x => x.PrimaryKey)
            .HasColumnName(ContentTypeDto.PrimaryKeyColumnName)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.NodeId)
            .HasColumnName(ContentTypeDto.NodeIdColumnName);

        builder.Property(x => x.Alias)
            .HasColumnName("alias");

        builder.Property(x => x.Icon)
            .HasColumnName("icon");

        builder.Property(x => x.Thumbnail)
            .HasColumnName("thumbnail");

        builder.Property(x => x.Description)
            .HasColumnName("description")
            .HasMaxLength(1500);

        builder.Property(x => x.ListView)
            .HasColumnName("listView");

        builder.Property(x => x.IsElement)
            .HasColumnName("isElement")
            .HasDefaultValue(false);

        builder.Property(x => x.AllowedInLibrary)
            .HasColumnName("allowedInLibrary")
            .HasDefaultValue(false);

        builder.Property(x => x.AllowAtRoot)
            .HasColumnName("allowAtRoot")
            .HasDefaultValue(false);

        builder.Property(x => x.Variations)
            .HasColumnName("variations")
            .HasDefaultValue((byte)1);

        // IX_cmsContentType (unique on NodeId)
        builder.HasIndex(x => x.NodeId)
            .IsUnique()
            .HasDatabaseName($"IX_{ContentTypeDto.TableName}");

        // FK: NodeId -> umbracoNode.id
        builder.HasOne<NodeDto>()
            .WithMany()
            .HasForeignKey(x => x.NodeId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
