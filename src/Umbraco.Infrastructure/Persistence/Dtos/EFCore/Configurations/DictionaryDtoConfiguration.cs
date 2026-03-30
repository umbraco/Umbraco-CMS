using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

public class DictionaryDtoConfiguration : IEntityTypeConfiguration<DictionaryDto>
{
    public void Configure(EntityTypeBuilder<DictionaryDto> builder)
    {
        builder.ToTable(DictionaryDto.TableName);

        builder.HasKey(x => x.PrimaryKey);

        builder.Property(x => x.PrimaryKey)
            .HasColumnName(DictionaryDto.PrimaryKeyColumnName)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.UniqueId)
            .HasColumnName(DictionaryDto.UniqueIdColumnName);

        builder.Property(x => x.Parent)
            .HasColumnName("parent");

        builder.Property(x => x.Key)
            .HasColumnName("key")
            .HasMaxLength(450);

        // Unique index on UniqueId (the Guid identifier)
        builder.HasIndex(x => x.UniqueId)
            .IsUnique()
            .HasDatabaseName($"IX_{DictionaryDto.TableName}_id");

        // Unique index on Key (the dictionary item key string)
        builder.HasIndex(x => x.Key)
            .IsUnique()
            .HasDatabaseName($"IX_{DictionaryDto.TableName}_key");

        // Index on Parent
        builder.HasIndex(x => x.Parent)
            .HasDatabaseName($"IX_{DictionaryDto.TableName}_Parent");

        // Self-referencing FK: Parent → UniqueId
        builder.HasOne(x => x.ParentEntry)
            .WithMany()
            .HasForeignKey(x => x.Parent)
            .HasPrincipalKey(x => x.UniqueId)
            .OnDelete(DeleteBehavior.NoAction);

        // One-to-many: DictionaryEntry → LanguageTexts
        builder.HasMany(x => x.LanguageTextDtos)
            .WithOne(x => x.DictionaryEntry)
            .HasForeignKey(x => x.UniqueId)
            .HasPrincipalKey(x => x.UniqueId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
