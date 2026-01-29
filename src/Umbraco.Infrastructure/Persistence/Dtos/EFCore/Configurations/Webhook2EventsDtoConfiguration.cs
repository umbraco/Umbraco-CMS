using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

public class Webhook2EventsDtoConfiguration : IEntityTypeConfiguration<Webhook2EventsDto>
{
    public void Configure(EntityTypeBuilder<Webhook2EventsDto> builder)
    {
        builder.ToTable(Webhook2EventsDto.TableName);

        builder.HasKey(x => new { x.WebhookId, x.Event })
            .HasName("PK_webhookEvent2WebhookDto");

        builder.Property(x => x.WebhookId)
            .HasColumnName(Webhook2EventsDto.WebhookIdColumnName)
            .IsRequired();

        // TODO: Does this currently have a max length of 255?
        builder.Property(x => x.Event)
            .HasColumnName(Webhook2EventsDto.EventColumnName)
            .HasMaxLength(255)
            .IsRequired();
    }
}
