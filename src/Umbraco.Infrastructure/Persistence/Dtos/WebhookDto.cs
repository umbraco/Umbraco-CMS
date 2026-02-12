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

    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn(AutoIncrement = true)]
    public int Id { get; set; }

    [Column(Name = "key")]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public Guid Key { get; set; }

    [Column(Name = "name")]
    [NullSetting(NullSetting = NullSettings.Null)]
    public string? Name { get; set; }

    [Column(Name = "description")]
    [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
    [NullSetting(NullSetting = NullSettings.Null)]
    public string? Description { get; set; }

    [Column(Name = "url")]
    [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public string Url { get; set; } = string.Empty;

    [Column(Name = "enabled")]
    public bool Enabled { get; set; }

    [ResultColumn]
    [Reference(ReferenceType.Many, ReferenceMemberName = Webhook2EventsDto.ReferenceMemberName)]
    public List<Webhook2EventsDto> Webhook2Events { get; set; } = new();

    [ResultColumn]
    [Reference(ReferenceType.Many, ReferenceMemberName = Webhook2ContentTypeKeysDto.ReferenceMemberName)]
    public List<Webhook2ContentTypeKeysDto> Webhook2ContentTypeKeys { get; set; } = new();

    [ResultColumn]
    [Reference(ReferenceType.Many, ReferenceMemberName = Webhook2HeadersDto.ReferenceMemberName)]
    public List<Webhook2ContentTypeKeysDto> Webhook2Headers { get; set; } = new();
}

