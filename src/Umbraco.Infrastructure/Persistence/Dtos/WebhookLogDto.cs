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

    [Column(Name = "key")]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public Guid Key { get; set; }

    [Column(Name = "statusCode")]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public string StatusCode { get; set; } = string.Empty;

    [Column(Name = "date")]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public DateTime Date { get; set; }

    [Column(Name = "url")]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public string Url { get; set; } = string.Empty;

    [Column(Name = "eventName")]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public string EventName { get; set; } = string.Empty;

    [Column(Name = "retryCount")]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public int RetryCount { get; set; }

    [Column(Name = "requestHeaders")]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public string RequestHeaders { get; set; } = string.Empty;

    [Column(Name = "requestBody")]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public string RequestBody { get; set; } = string.Empty;

    [Column(Name = "responseHeaders")]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public string ResponseHeaders { get; set; } = string.Empty;

    [Column(Name = "responseBody")]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public string ResponseBody { get; set; } = string.Empty;
}
