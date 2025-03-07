using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_15_4_0;

public class IncludeUrlSegmentInDocumentUrlUniqueIndex : MigrationBase
{
    private readonly ILogger<IncludeUrlSegmentInDocumentUrlUniqueIndex> _logger;

    public IncludeUrlSegmentInDocumentUrlUniqueIndex(IMigrationContext context, ILogger<IncludeUrlSegmentInDocumentUrlUniqueIndex> logger)
        : base(context)
    {
        _logger = logger;
    }

    protected override void Migrate()
    {
        Logger.LogDebug("Extending the unique index on {TableName} to include the urlSegment column", Constants.DatabaseSchema.Tables.DocumentUrl);

        if (TableExists(Constants.DatabaseSchema.Tables.DocumentUrl))
        {
            var indexName = "IX_" + Constants.DatabaseSchema.Tables.DocumentUrl;
            if (IndexExists(indexName))
            {
                DeleteIndex<DocumentUrlDto>(indexName);
            }

            CreateIndex<DocumentUrlDto>(indexName);
        }
        else
        {
            Logger.LogWarning($"Table {Constants.DatabaseSchema.Tables.DocumentUrl} does not exist so the index extension in migration {nameof(IncludeUrlSegmentInDocumentUrlUniqueIndex)} cannot be completed.");
        }
    }
}
