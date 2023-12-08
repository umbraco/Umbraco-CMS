using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.Dtos;

[TableName(Constants.DatabaseSchema.Tables.WebhookLog)]
[PrimaryKey("id")]
[ExplicitColumns]
internal class WebhookLogDto
{
    [Column("id")]
    [PrimaryKeyColumn(AutoIncrement = true)]
    public int Id { get; set; }

    [Column("webhookKey")]
    public Guid WebhookKey { get; set; }

    [Column(Name = "key")]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public Guid Key { get; set; }

    [Column(Name = "statusCode")]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public string StatusCode { get; set; } = string.Empty;

    [Column(Name = "date")]
    [Index(IndexTypes.NonClustered, Name = "IX_" + Constants.DatabaseSchema.Tables.WebhookLog + "_date")]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public DateTime Date { get; set; }

    [Column(Name = "url")]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public string Url { get; set; } = string.Empty;

    [Column(Name = "eventAlias")]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public string EventAlias { get; set; } = string.Empty;

    [Column(Name = "retryCount")]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public int RetryCount { get; set; }

    [Column(Name = "requestHeaders")]
    [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public string RequestHeaders { get; set; } = string.Empty;

    [Column(Name = "requestBody")]
    [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public string RequestBody { get; set; } = string.Empty;

    [Column(Name = "responseHeaders")]
    [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public string ResponseHeaders { get; set; } = string.Empty;

    [Column(Name = "responseBody")]
    [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public string ResponseBody { get; set; } = string.Empty;
}
