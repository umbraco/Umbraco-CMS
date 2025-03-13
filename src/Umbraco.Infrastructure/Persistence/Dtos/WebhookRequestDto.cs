using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(Constants.DatabaseSchema.Tables.WebhookRequest)]
[PrimaryKey("id")]
[ExplicitColumns]
public class WebhookRequestDto
{
    [Column("id")]
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
