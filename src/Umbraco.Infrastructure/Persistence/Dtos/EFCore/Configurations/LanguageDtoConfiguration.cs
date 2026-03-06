using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

public class LanguageDtoConfiguration : IEntityTypeConfiguration<LanguageDto>
{
    public void Configure(EntityTypeBuilder<LanguageDto> builder)
    {
        builder.ToTable(LanguageDto.TableName);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName(LanguageDto.PrimaryKeyColumnName)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.IsoCode)
            .HasColumnName(LanguageDto.IsoCodeColumnName)
            .HasMaxLength(14);

        builder.Property(x => x.CultureName)
            .HasColumnName("languageCultureName")
            .HasMaxLength(100);

        builder.Property(x => x.IsDefault)
            .HasColumnName("isDefaultVariantLang")
            .HasDefaultValue(false);

        builder.Property(x => x.IsMandatory)
            .HasColumnName("mandatory")
            .HasDefaultValue(false);

        builder.Property(x => x.FallbackLanguageId)
            .HasColumnName("fallbackLanguageId");

        builder.HasIndex(x => x.IsoCode)
            .IsUnique();

        builder.HasIndex(x => x.FallbackLanguageId);

        builder.HasOne(x => x.FallbackLanguage)
            .WithMany()
            .HasForeignKey(x => x.FallbackLanguageId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
