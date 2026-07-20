using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;


[TableName(TableName)]
[PrimaryKey(PrimaryKeyColumnName, AutoIncrement = true)]
[ExplicitColumns]
internal sealed class WebhookDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.Webhook;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameId;

    /// <summary>
    /// Gets or sets the unique identifier for the webhook.
    /// </summary>
    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn(AutoIncrement = true)]
    public int Id { get; set; }

    /// <summary>
    /// Gets or sets the unique identifier key for the webhook.
    /// </summary>
    [Column(Name = "key")]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public Guid Key { get; set; }

    /// <summary>
    /// Gets or sets the name of the webhook.
    /// </summary>
    [Column(Name = "name")]
    [NullSetting(NullSetting = NullSettings.Null)]
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the description of the webhook.
    /// </summary>
    [Column(Name = "description")]
    [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
    [NullSetting(NullSetting = NullSettings.Null)]
    public string? Description { get; set; }

    /// <summary>
    /// Gets or sets the endpoint URL that the webhook will call.
    /// </summary>
    [Column(Name = "url")]
    [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public string Url { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the webhook is enabled.
    /// </summary>
    [Column(Name = "enabled")]
    public bool Enabled { get; set; }

    /// <summary>
    /// Gets or sets the collection of Webhook2EventsDto instances associated with this webhook.
    /// Represents the events linked to the current webhook.
    /// </summary>
    [ResultColumn]
    [Reference(ReferenceType.Many, ReferenceMemberName = nameof(Webhook2EventsDto.WebhookId))]
    public List<Webhook2EventsDto> Webhook2Events { get; set; } = new();

    /// <summary>
    /// Gets or sets the collection of <see cref="Webhook2ContentTypeKeysDto"/> objects that represent the associations between this webhook and related content type keys.
    /// </summary>
    [ResultColumn]
    [Reference(ReferenceType.Many, ReferenceMemberName = nameof(Webhook2ContentTypeKeysDto.WebhookId))]
    public List<Webhook2ContentTypeKeysDto> Webhook2ContentTypeKeys { get; set; } = new();

    /// <summary>
    /// Gets or sets the collection of headers associated with the webhook.
    /// </summary>
    [ResultColumn]
    [Reference(ReferenceType.Many, ReferenceMemberName = nameof(Webhook2HeadersDto.WebhookId))]
    public List<Webhook2HeadersDto> Webhook2Headers { get; set; } = new();
}

