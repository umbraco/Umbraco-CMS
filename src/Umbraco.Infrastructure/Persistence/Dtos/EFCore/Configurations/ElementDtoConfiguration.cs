using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

public class ElementDtoConfiguration : IEntityTypeConfiguration<ElementDto>
{
    public void Configure(EntityTypeBuilder<ElementDto> builder)
    {
        builder.ToTable(ElementDto.TableName);

        builder.HasKey(x => x.NodeId);

        builder.Property(x => x.NodeId)
            .HasColumnName(ElementDto.PrimaryKeyColumnName)
            .ValueGeneratedNever();

        builder.Property(x => x.Published)
            .HasColumnName(ElementDto.PublishedColumnName);

        builder.Property(x => x.Edited)
            .HasColumnName(ElementDto.EditedColumnName);

        // FK: NodeId -> umbracoContent.nodeId
        builder.HasOne<ContentDto>()
            .WithMany()
            .HasForeignKey(x => x.NodeId)
            .OnDelete(DeleteBehavior.NoAction);

        // IX_umbracoElement_Published
        builder.HasIndex(x => x.Published)
            .HasDatabaseName($"IX_{ElementDto.TableName}_Published");

        builder.Ignore(x => x.ContentDto);
        builder.Ignore(x => x.CurrentVersion);
        builder.Ignore(x => x.PublishedVersion);
    }
}
