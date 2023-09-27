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
    [Reference(ReferenceType.Many, ReferenceMemberName = nameof(Event2WebhookDto.WebhookId))]
    public List<Event2WebhookDto> Event2WebhookDtos { get; set; } = new();

    [ResultColumn]
    [Reference(ReferenceType.Many, ReferenceMemberName = nameof(EntityKey2WebhookDto.WebhookId))]
    public List<EntityKey2WebhookDto> EntityKey2WebhookDtos { get; set; } = new();
}

