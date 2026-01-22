using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(TableName)]
[PrimaryKey(PrimaryKeyColumnName, AutoIncrement = true)]
[ExplicitColumns]
public class WebhookRequestDto
{
    public const string TableName = Constants.DatabaseSchema.Tables.WebhookRequest;
    public const string PrimaryKeyColumnName = Constants.DatabaseSchema.Columns.PrimaryKeyNameId;

    [Column(PrimaryKeyColumnName)]
    [PrimaryKeyColumn(AutoIncrement = true)]
    public int Id { get; set; }

    [Column("webhookKey")]
    public Guid WebhookKey { get; set; }

    [Column("eventName")]
    public string Alias { get; set; } = string.Empty;

    [Column(Name = "requestObject")]
    [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
    [NullSetting(NullSetting = NullSettings.Null)]
    public string? RequestObject { get; set; }

    [Column("retryCount")]
    public int RetryCount { get; set; }
}
