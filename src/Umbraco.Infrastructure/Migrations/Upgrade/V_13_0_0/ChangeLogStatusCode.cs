using System.Net;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_13_0_0;

public class ChangeLogStatusCode : MigrationBase
{
    public ChangeLogStatusCode(IMigrationContext context) : base(context)
    {
    }

    protected override void Migrate()
    {
        if (!TableExists(Constants.DatabaseSchema.Tables.WebhookLog))
        {
            return;
        }

        Sql<ISqlContext> fetchQuery = Database.SqlContext.Sql()
            .Select<WebhookLogDtoOld>()
            .From<WebhookLogDtoOld>();

        // Use a custom SQL query to prevent selecting explicit columns (sortOrder doesn't exist yet)
        List<WebhookLogDtoOld> webhookLogDtos = Database.Fetch<WebhookLogDtoOld>(fetchQuery);

        Sql<ISqlContext> deleteQuery = Database.SqlContext.Sql()
            .Delete<WebhookLogDtoOld>();

        Database.Execute(deleteQuery);

        foreach (WebhookLogDtoOld webhookLogDto in webhookLogDtos)
        {
            if (Enum.TryParse(webhookLogDto.StatusCode, out HttpStatusCode statusCode))
            {
                webhookLogDto.StatusCode = $"{statusCode.ToString()} ({(int)statusCode})";
            }
        }

        Database.InsertBatch(webhookLogDtos);
    }

    [TableName(Constants.DatabaseSchema.Tables.WebhookLog)]
    [PrimaryKey("id")]
    [ExplicitColumns]
    private class WebhookLogDtoOld
    {
        [Column("id")]
        [PrimaryKeyColumn(AutoIncrement = true)]
        public int Id { get; set; }

        [Column("webhookKey")] public Guid WebhookKey { get; set; }

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
        [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
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
}
