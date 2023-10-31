using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;


[TableName(Constants.DatabaseSchema.Tables.Webhook)]
[PrimaryKey("id")]
[ExplicitColumns]
internal class WebhookDto
{
    [Column("id")]
    [PrimaryKeyColumn(AutoIncrement = true)]
    public int Id { get; set; }

    [Column(Name = "key")]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public Guid Key { get; set; }

    [Column(Name = "url")]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public string Url { get; set; } = string.Empty;

    [Column(Name = "enabled")]
    public bool Enabled { get; set; }

    [ResultColumn]
    [Reference(ReferenceType.Many, ReferenceMemberName = nameof(Webhook2EventsDto.WebhookId))]
    public List<Webhook2EventsDto> Webhook2Events { get; set; } = new();

    [ResultColumn]
    [Reference(ReferenceType.Many, ReferenceMemberName = nameof(Webhook2ContentTypeKeysDto.WebhookId))]
    public List<Webhook2ContentTypeKeysDto> Webhook2ContentTypeKeys { get; set; } = new();

    [ResultColumn]
    [Reference(ReferenceType.Many, ReferenceMemberName = nameof(Webhook2HeadersDto.WebhookId))]
    public List<Webhook2ContentTypeKeysDto> Webhook2Headers { get; set; } = new();
}

