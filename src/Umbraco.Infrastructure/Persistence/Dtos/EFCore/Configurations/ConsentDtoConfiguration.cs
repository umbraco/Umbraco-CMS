using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

public class ConsentDtoConfiguration : IEntityTypeConfiguration<ConsentDto>
{
    public void Configure(EntityTypeBuilder<ConsentDto> builder)
    {
        builder.ToTable(ConsentDto.TableName);

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .HasColumnName(ConsentDto.PrimaryKeyColumnName)
            .ValueGeneratedOnAdd();

        builder.Property(x => x.Current)
            .HasColumnName("current");

        builder.Property(x => x.Source)
            .HasColumnName("source")
            .HasMaxLength(512);

        builder.Property(x => x.Context)
            .HasColumnName("context")
            .HasMaxLength(128);

        builder.Property(x => x.Action)
            .HasColumnName("action")
            .HasMaxLength(512);

        builder.Property(x => x.CreateDate)
            .HasColumnName("createDate");

        builder.Property(x => x.State)
            .HasColumnName("state");

        builder.Property(x => x.Comment)
            .HasColumnName("comment");
    }
}
