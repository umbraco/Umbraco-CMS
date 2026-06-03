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
            .HasColumnName(ContentTypeDto.AliasColumnName);

        builder.Property(x => x.Icon)
            .HasColumnName(ContentTypeDto.IconColumnName);

        builder.Property(x => x.Thumbnail)
            .HasColumnName(ContentTypeDto.ThumbnailColumnName)
            .HasDefaultValue("folder.png");

        builder.Property(x => x.Description)
            .HasColumnName(ContentTypeDto.DescriptionColumnName)
            .HasMaxLength(1500);

        builder.Property(x => x.ListView)
            .HasColumnName(ContentTypeDto.ListViewColumnName);

        builder.Property(x => x.IsElement)
            .HasColumnName(ContentTypeDto.IsElementColumnName)
            .HasDefaultValue(false);

        builder.Property(x => x.AllowedInLibrary)
            .HasColumnName(ContentTypeDto.AllowedInLibraryColumnName)
            .HasDefaultValue(false);

        builder.Property(x => x.AllowAtRoot)
            .HasColumnName(ContentTypeDto.AllowAtRootColumnName)
            .HasDefaultValue(false);

        builder.Property(x => x.Variations)
            .HasColumnName(ContentTypeDto.VariationsColumnName)
            .HasDefaultValue((byte)1);

        // IX_cmsContentType (unique on nodeId)
        builder.HasIndex(x => x.NodeId)
            .IsUnique()
            .HasDatabaseName("IX_cmsContentType");

        // IX_cmsContentType_icon
        builder.HasIndex(x => x.Icon)
            .HasDatabaseName($"IX_{ContentTypeDto.TableName}_{ContentTypeDto.IconColumnName}");

        // FK to umbracoNode (NodeId -> NodeDto.NodeId)
        builder.HasOne(x => x.NodeDto)
            .WithMany()
            .HasForeignKey(x => x.NodeId)
            .OnDelete(DeleteBehavior.NoAction)
            .HasConstraintName($"FK_{ContentTypeDto.TableName}_{NodeDto.TableName}");
    }
}
