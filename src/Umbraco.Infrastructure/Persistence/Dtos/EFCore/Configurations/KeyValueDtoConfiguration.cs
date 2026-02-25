using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

public class KeyValueDtoConfiguration : IEntityTypeConfiguration<KeyValueDto>
{
    public void Configure(EntityTypeBuilder<KeyValueDto> builder)
    {
        builder.ToTable(KeyValueDto.TableName);

        builder.HasKey(x => x.Key);

        builder.Property(x => x.Key)
            .HasColumnName(KeyValueDto.PrimaryKeyColumnName)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Value)
            .HasColumnName("value");

        builder.Property(x => x.UpdateDate)
            .HasColumnName("updated");
    }
}
