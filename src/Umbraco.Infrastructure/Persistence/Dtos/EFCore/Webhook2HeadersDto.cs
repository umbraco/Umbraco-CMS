using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;

[EntityTypeConfiguration(typeof(Webhook2HeadersDtoConfiguration))]
public class Webhook2HeadersDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.Webhook2Headers;

    internal const string ReferenceMemberName = "WebhookId"; // should be WebhookIdColumnName, but for database compatibility we keep it like this

    internal const string WebhookIdColumnName = "webhookId";
    internal const string KeyColumnName = "Key";
    internal const string ValueColumnName = "Value";

    public int WebhookId { get; set; }

    public string Key { get; set; } = string.Empty;

    public string Value { get; set; } = string.Empty;

    public WebhookDto? Webhook { get; set; }
}
