using System.Net;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_13_0_0;

/// <summary>
/// Defines status codes used to represent the state of the change log during the upgrade process to version 13.0.0.
/// </summary>
public class ChangeLogStatusCode : MigrationBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ChangeLogStatusCode"/> class, using the specified migration context.
    /// </summary>
    /// <param name="context">The <see cref="IMigrationContext"/> that provides information and services for the migration process.</param>
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
        /// <summary>
        /// Gets or sets the unique identifier of the webhook log entry.
        /// </summary>
        [Column("id")]
        [PrimaryKeyColumn(AutoIncrement = true)]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier for the webhook.
        /// </summary>
        [Column("webhookKey")]
        public Guid WebhookKey { get; set; }

        /// <summary>
        /// Gets or sets the unique identifier key for the webhook log entry.
        /// </summary>
        [Column(Name = "key")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public Guid Key { get; set; }

        /// <summary>
        /// Gets or sets the HTTP status code returned by the webhook operation for this log entry.
        /// </summary>
        [Column(Name = "statusCode")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public string StatusCode { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the date and time when the webhook log entry was created.
        /// </summary>
        [Column(Name = "date")]
        [Index(IndexTypes.NonClustered, Name = "IX_" + Constants.DatabaseSchema.Tables.WebhookLog + "_date")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public DateTime Date { get; set; }

        /// <summary>
        /// Gets or sets the URL related to this webhook log entry.
        /// </summary>
        [Column(Name = "url")]
        [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public string Url { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the alias of the event associated with this webhook log entry.
        /// </summary>
        [Column(Name = "eventAlias")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public string EventAlias { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the number of times the webhook log entry has been retried.
        /// </summary>
        [Column(Name = "retryCount")]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public int RetryCount { get; set; }

        /// <summary>
        /// Gets or sets the serialized HTTP request headers associated with the webhook log entry.
        /// </summary>
        [Column(Name = "requestHeaders")]
        [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public string RequestHeaders { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the HTTP request body content associated with the webhook log entry.
        /// </summary>
        [Column(Name = "requestBody")]
        [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public string RequestBody { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the serialized response headers returned by the webhook request.
        /// </summary>
        [Column(Name = "responseHeaders")]
        [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public string ResponseHeaders { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the HTTP response body returned by the webhook, as recorded in the log.
        /// </summary>
        [Column(Name = "responseBody")]
        [SpecialDbType(SpecialDbTypes.NVARCHARMAX)]
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public string ResponseBody { get; set; } = string.Empty;
    }
}
