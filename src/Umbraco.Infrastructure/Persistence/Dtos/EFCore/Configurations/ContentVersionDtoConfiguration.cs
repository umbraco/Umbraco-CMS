using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

public class ContentVersionDtoConfiguration : IEntityTypeConfiguration<ContentVersionDto>
{
    public void Configure(EntityTypeBuilder<ContentVersionDto> builder)
    {
        builder.ToTable(ContentVersionDto.TableName);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName(ContentVersionDto.PrimaryKeyColumnName)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.NodeId)
            .HasColumnName(ContentVersionDto.NodeIdColumnName);

        builder.Property(x => x.VersionDate)
            .HasColumnName(ContentVersionDto.VersionDateColumnName);

        builder.Property(x => x.UserId)
            .HasColumnName(ContentVersionDto.UserIdColumnName);

        builder.Property(x => x.Current)
            .HasColumnName(ContentVersionDto.CurrentColumnName);

        builder.Property(x => x.Text)
            .HasColumnName(ContentVersionDto.TextColumnName);

        builder.Property(x => x.PreventCleanup)
            .HasColumnName(ContentVersionDto.PreventCleanupColumnName)
            .HasDefaultValue(false);

        // FK: NodeId -> umbracoContent.nodeId
        builder.HasOne<ContentDto>()
            .WithMany()
            .HasForeignKey(x => x.NodeId)
            .OnDelete(DeleteBehavior.Cascade);

        // IX_umbracoContentVersion_NodeId (composite on NodeId+Current)
        // Note: SQL Server included columns are added by SqlServerContentVersionDtoModelCustomizer.
        builder.HasIndex(x => new { x.NodeId, x.Current })
            .HasDatabaseName($"IX_{ContentVersionDto.TableName}_NodeId");

        // IX_umbracoContentVersion_VersionDate
        builder.HasIndex(x => x.VersionDate)
            .HasDatabaseName($"IX_{ContentVersionDto.TableName}_VersionDate");

        // IX_umbracoContentVersion_Current
        // Note: SQL Server included columns (NodeId) are added by SqlServerContentVersionDtoModelCustomizer.
        builder.HasIndex(x => x.Current)
            .HasDatabaseName($"IX_{ContentVersionDto.TableName}_Current");
    }
}
