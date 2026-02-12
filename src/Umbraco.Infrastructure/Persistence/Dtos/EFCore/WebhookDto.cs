using Microsoft.EntityFrameworkCore;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore.Configurations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos.EFCore;

[EntityTypeConfiguration(typeof(WebhookDtoConfiguration))]
public sealed class WebhookDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.Webhook;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameId;

    public int Id { get; set; }

    public Guid Key { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public string Url { get; set; } = string.Empty;

    public bool Enabled { get; set; }

    public List<Webhook2EventsDto> Webhook2Events { get; set; } = new();

    public List<Webhook2ContentTypeKeysDto> Webhook2ContentTypeKeys { get; set; } = new();

    public List<Webhook2HeadersDto> Webhook2Headers { get; set; } = new();
}
