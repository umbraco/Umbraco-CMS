using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

public class DocumentDtoConfiguration : IEntityTypeConfiguration<DocumentDto>
{
    public void Configure(EntityTypeBuilder<DocumentDto> builder)
    {
        builder.ToTable(DocumentDto.TableName);

        builder.HasKey(x => x.NodeId);

        builder.Property(x => x.NodeId)
            .HasColumnName(DocumentDto.PrimaryKeyColumnName)
            .ValueGeneratedNever();

        builder.Property(x => x.Published)
            .HasColumnName(DocumentDto.PublishedColumnName);

        builder.Property(x => x.Edited)
            .HasColumnName(DocumentDto.EditedColumnName);

        // FK: NodeId -> umbracoContent.nodeId
        builder.HasOne<ContentDto>()
            .WithMany()
            .HasForeignKey(x => x.NodeId)
            .OnDelete(DeleteBehavior.Cascade);

        // IX_umbracoDocument_Published
        builder.HasIndex(x => x.Published)
            .HasDatabaseName($"IX_{DocumentDto.TableName}_Published");
    }
}
