using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;

[EntityTypeConfiguration(typeof(Webhook2EventsDtoConfiguration))]
public class Webhook2EventsDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.Webhook2Events;

    internal const string ReferenceMemberName = "WebhookId"; // should be WebhookIdColumnName, but for database compatibility we keep it like this

    internal const string WebhookIdColumnName = "webhookId";
    internal const string EventColumnName = "event";

    public int WebhookId { get; set; }

    public string Event { get; set; } = string.Empty;

    public WebhookDto? Webhook { get; set; }
}
