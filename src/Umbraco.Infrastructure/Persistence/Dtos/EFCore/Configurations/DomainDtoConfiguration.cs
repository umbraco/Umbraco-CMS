using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

public class DomainDtoConfiguration : IEntityTypeConfiguration<DomainDto>
{
    public void Configure(EntityTypeBuilder<DomainDto> builder)
    {
        builder.ToTable(DomainDto.TableName);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName(DomainDto.PrimaryKeyColumnName)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Key)
            .HasColumnName("key");

        builder.HasIndex(x => x.Key)
            .IsUnique();

        builder.Property(x => x.DefaultLanguage)
            .HasColumnName("domainDefaultLanguage");

        builder.Property(x => x.RootStructureId)
            .HasColumnName("domainRootStructureID");

        builder.Property(x => x.DomainName)
            .HasColumnName("domainName");

        builder.Property(x => x.SortOrder)
            .HasColumnName("sortOrder");
    }
}
