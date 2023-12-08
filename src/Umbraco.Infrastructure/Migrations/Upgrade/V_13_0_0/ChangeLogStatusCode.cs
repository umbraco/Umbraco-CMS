using System.Net;
using NPoco;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
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
            .Select<WebhookLogDto>()
            .From<WebhookLogDto>();

        // Use a custom SQL query to prevent selecting explicit columns (sortOrder doesn't exist yet)
        List<WebhookLogDto> webhookLogDtos = Database.Fetch<WebhookLogDto>(fetchQuery);

        Sql<ISqlContext> deleteQuery = Database.SqlContext.Sql()
            .Delete<WebhookLogDto>();

        Database.Execute(deleteQuery);

        foreach (WebhookLogDto webhookLogDto in webhookLogDtos)
        {
            if (Enum.TryParse(webhookLogDto.StatusCode, out HttpStatusCode statusCode))
            {
                webhookLogDto.StatusCode = $"{statusCode.ToString()} ({(int)statusCode})";
            }
        }

        Database.InsertBatch(webhookLogDtos);
    }
}
