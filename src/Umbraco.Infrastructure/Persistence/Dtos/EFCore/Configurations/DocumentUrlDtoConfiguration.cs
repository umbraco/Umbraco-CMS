using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

public class DocumentUrlDtoConfiguration : IEntityTypeConfiguration<DocumentUrlDto>
{
    public void Configure(EntityTypeBuilder<DocumentUrlDto> builder)
    {
        builder.ToTable(DocumentUrlDto.TableName);

        builder.HasKey(x => x.NodeId);

        builder.Property(x => x.NodeId)
            .HasColumnName(DocumentUrlDto.PrimaryKeyColumnName)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.UniqueId)
            .HasColumnName(DocumentUrlDto.UniqueIdColumnName);

        builder.Property(x => x.IsDraft)
            .HasColumnName(DocumentUrlDto.IsDraftColumnName);

        builder.Property(x => x.LanguageId)
            .HasColumnName(DocumentUrlDto.LanguageIdColumnName);

        builder.Property(x => x.UrlSegment)
            .HasColumnName(DocumentUrlDto.UrlSegmentColumnName)
            .IsRequired();

        builder.Property(x => x.IsPrimary)
            .HasColumnName(DocumentUrlDto.IsPrimaryColumnName)
            .HasDefaultValue(true);

        // FK: UniqueId -> umbracoNode.uniqueId (references the unique key, not the PK)
        builder.HasOne<NodeDto>()
            .WithMany()
            .HasForeignKey(x => x.UniqueId)
            .HasPrincipalKey(x => x.UniqueId)
            .OnDelete(DeleteBehavior.Cascade);

        // FK: LanguageId -> umbracoLanguage.id
        builder.HasOne<LanguageDto>()
            .WithMany()
            .HasForeignKey(x => x.LanguageId)
            .OnDelete(DeleteBehavior.NoAction);

        // IX_umbracoDocumentUrl (unique on UniqueId+LanguageId+IsDraft+UrlSegment)
        // Note: SQL Server clustered behavior is added by SqlServerDocumentUrlDtoModelCustomizer.
        builder.HasIndex(x => new { x.UniqueId, x.LanguageId, x.IsDraft, x.UrlSegment })
            .IsUnique()
            .HasDatabaseName($"IX_{DocumentUrlDto.TableName}");
    }
}
