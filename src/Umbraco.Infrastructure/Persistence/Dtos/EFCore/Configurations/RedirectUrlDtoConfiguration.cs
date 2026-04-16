using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

public class RedirectUrlDtoConfiguration : IEntityTypeConfiguration<RedirectUrlDto>
{
    public void Configure(EntityTypeBuilder<RedirectUrlDto> builder)
    {
        builder.ToTable(RedirectUrlDto.TableName);

        builder.HasKey(x => x.Id)
            .HasName("PK_umbracoRedirectUrl");

        builder.Property(x => x.Id)
            .HasColumnName(RedirectUrlDto.PrimaryKeyColumnName)
            .ValueGeneratedNever();

        builder.Property(x => x.ContentKey)
            .HasColumnName(RedirectUrlDto.ContentKeyColumnName)
            .IsRequired();

        builder.Property(x => x.CreateDateUtc)
            .HasColumnName(RedirectUrlDto.CreateDateUtcColumnName)
            .IsRequired();

        builder.Property(x => x.Url)
            .HasColumnName(RedirectUrlDto.UrlColumnName)
            .IsRequired();

        builder.Property(x => x.Culture)
            .HasColumnName(RedirectUrlDto.CultureColumnName);

        builder.Property(x => x.UrlHash)
            .HasColumnName(RedirectUrlDto.UrlHashColumnName)
            .HasMaxLength(40)
            .IsRequired();

        // FK: contentKey -> umbracoNode.uniqueId
        // Uses HasPrincipalKey because the target is UniqueId (a unique column), not the NodeDto primary key (NodeId).
        builder.HasOne<NodeDto>()
            .WithMany()
            .HasForeignKey(x => x.ContentKey)
            .HasPrincipalKey(x => x.UniqueId)
            .OnDelete(DeleteBehavior.NoAction);

        // IX_umbracoRedirectUrl_culture_hash — non-clustered on CreateDateUtc
        // Note: SQL Server included columns are added by SqlServerRedirectUrlDtoModelCustomizer.
        builder.HasIndex(x => x.CreateDateUtc)
            .HasDatabaseName($"IX_{RedirectUrlDto.TableName}_culture_hash");

        // IX_umbracoRedirectUrl — unique non-clustered on (UrlHash, ContentKey, Culture, CreateDateUtc)
        builder.HasIndex(x => new { x.UrlHash, x.ContentKey, x.Culture, x.CreateDateUtc })
            .IsUnique()
            .HasDatabaseName($"IX_{RedirectUrlDto.TableName}");
    }
}
