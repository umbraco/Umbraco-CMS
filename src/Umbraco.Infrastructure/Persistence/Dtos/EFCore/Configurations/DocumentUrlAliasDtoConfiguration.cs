using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

public class DocumentUrlAliasDtoConfiguration : IEntityTypeConfiguration<DocumentUrlAliasDto>
{
    public void Configure(EntityTypeBuilder<DocumentUrlAliasDto> builder)
    {
        builder.ToTable(DocumentUrlAliasDto.TableName);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName(DocumentUrlAliasDto.PrimaryKeyColumnName)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.UniqueId)
            .HasColumnName(DocumentUrlAliasDto.UniqueIdColumnName);

        builder.Property(x => x.LanguageId)
            .HasColumnName(DocumentUrlAliasDto.LanguageIdColumnName);

        builder.Property(x => x.Alias)
            .HasColumnName(DocumentUrlAliasDto.AliasColumnName)
            .IsRequired();

        // FK: UniqueId -> umbracoNode.uniqueId (references the unique key, not the PK)
        builder.HasOne<NodeDto>()
            .WithMany()
            .HasForeignKey(x => x.UniqueId)
            .HasPrincipalKey(x => x.UniqueId)
            .OnDelete(DeleteBehavior.Cascade);

        // IX_umbracoDocumentUrlAlias_Unique (unique on UniqueId+LanguageId+Alias)
        builder.HasIndex(x => new { x.UniqueId, x.LanguageId, x.Alias })
            .IsUnique()
            .HasDatabaseName($"IX_{DocumentUrlAliasDto.TableName}_Unique");

        // IX_umbracoDocumentUrlAlias_Lookup (on Alias+LanguageId)
        builder.HasIndex(x => new { x.Alias, x.LanguageId })
            .HasDatabaseName($"IX_{DocumentUrlAliasDto.TableName}_Lookup");
    }
}
