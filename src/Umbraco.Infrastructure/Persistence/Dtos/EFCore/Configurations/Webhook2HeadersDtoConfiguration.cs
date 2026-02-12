using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

public class Webhook2HeadersDtoConfiguration : IEntityTypeConfiguration<Webhook2HeadersDto>
{
    public void Configure(EntityTypeBuilder<Webhook2HeadersDto> builder)
    {
        builder.ToTable(Webhook2HeadersDto.TableName);

        builder.HasKey(x => new { x.WebhookId, x.Key })
            .HasName("PK_headers2WebhookDto");

        builder.Property(x => x.Key)
            .HasColumnName(Webhook2HeadersDto.KeyColumnName);

        builder.Property(x => x.WebhookId)
            .HasColumnName(Webhook2HeadersDto.WebhookIdColumnName)
            .IsRequired();
    }
}
