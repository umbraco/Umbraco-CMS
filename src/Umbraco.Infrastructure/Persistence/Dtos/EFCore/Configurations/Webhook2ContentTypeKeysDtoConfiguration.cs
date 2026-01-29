using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

public class Webhook2ContentTypeKeysDtoConfiguration : IEntityTypeConfiguration<Webhook2ContentTypeKeysDto>
{
    public void Configure(EntityTypeBuilder<Webhook2ContentTypeKeysDto> builder)
    {
        builder.ToTable(Webhook2ContentTypeKeysDto.TableName);

        builder
            .HasKey(x => new {x.WebhookId, x.ContentTypeKey})
            .HasName("PK_webhookEntityKey2Webhook");

        builder.Property(x => x.WebhookId)
            .HasColumnName(Webhook2ContentTypeKeysDto.WebhookIdColumnName)
            .IsRequired();

        builder.Property(x => x.ContentTypeKey)
            .HasColumnName(Webhook2ContentTypeKeysDto.ContentTypeKeyColumnName)
            .IsRequired();
    }
}
