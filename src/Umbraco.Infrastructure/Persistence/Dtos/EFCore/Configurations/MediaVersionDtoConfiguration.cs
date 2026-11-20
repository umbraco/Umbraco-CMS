using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

/// <summary>
/// Maps <see cref="MediaVersionDto" /> to the media version table.
/// </summary>
public class MediaVersionDtoConfiguration : IEntityTypeConfiguration<MediaVersionDto>
{
    public void Configure(EntityTypeBuilder<MediaVersionDto> builder)
    {
        builder.ToTable(MediaVersionDto.TableName);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName(MediaVersionDto.PrimaryKeyColumnName)
            .ValueGeneratedNever();

        builder.Property(x => x.Path)
            .HasColumnName(MediaVersionDto.PathColumnName)
            .HasMaxLength(255);

        // FK: Id -> umbracoContentVersion.id
        builder.HasOne<ContentVersionDto>()
            .WithMany()
            .HasForeignKey(x => x.Id)
            .OnDelete(DeleteBehavior.NoAction);

        // IX_umbracoMediaVersion (unique on Id+Path)
        builder.HasIndex(x => new { x.Id, x.Path })
            .IsUnique()
            .HasDatabaseName($"IX_{MediaVersionDto.TableName}");
    }
}
