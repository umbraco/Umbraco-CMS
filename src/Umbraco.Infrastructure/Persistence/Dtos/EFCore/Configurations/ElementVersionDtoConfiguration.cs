using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

public class ElementVersionDtoConfiguration : IEntityTypeConfiguration<ElementVersionDto>
{
    public void Configure(EntityTypeBuilder<ElementVersionDto> builder)
    {
        builder.ToTable(ElementVersionDto.TableName);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName(ElementVersionDto.PrimaryKeyColumnName)
            .ValueGeneratedNever();

        builder.Property(x => x.Published)
            .HasColumnName(ElementVersionDto.PublishedColumnName);

        // FK: Id -> umbracoContentVersion.id
        builder.HasOne<ContentVersionDto>()
            .WithMany()
            .HasForeignKey(x => x.Id)
            .OnDelete(DeleteBehavior.NoAction);

        // IX_umbracoElementVersion_id_published (composite on Id+Published)
        builder.HasIndex(x => new { x.Id, x.Published })
            .HasDatabaseName($"IX_{ElementVersionDto.TableName}_id_published");

        // IX_umbracoElementVersion_published (on Published)
        // Note: SQL Server included columns (Id) are added by SqlServerElementVersionDtoModelCustomizer.
        builder.HasIndex(x => x.Published)
            .HasDatabaseName($"IX_{ElementVersionDto.TableName}_published");

        builder.Ignore(x => x.ContentVersionDto);
    }
}
