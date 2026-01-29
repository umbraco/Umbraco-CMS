using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;

[EntityTypeConfiguration(typeof(Webhook2ContentTypeKeysDtoConfiguration))]
public class Webhook2ContentTypeKeysDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.Webhook2ContentTypeKeys;

    internal const string ReferenceMemberName = "WebhookId"; // should be WebhookIdColumnName, but for database compatibility we keep it like this

    internal const string WebhookIdColumnName = "webhookId";
    internal const string ContentTypeKeyColumnName = "entityKey";

    public int WebhookId { get; set; }

    public Guid ContentTypeKey { get; set; }

    public WebhookDto? Webhook { get; set; }
}
