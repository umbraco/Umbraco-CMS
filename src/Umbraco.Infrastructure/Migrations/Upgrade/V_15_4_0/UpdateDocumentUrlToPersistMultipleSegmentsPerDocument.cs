using System.Data.Common;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_15_4_0;

public class UpdateDocumentUrlToPersistMultipleSegmentsPerDocument : MigrationBase
{
    private readonly ILogger<UpdateDocumentUrlToPersistMultipleSegmentsPerDocument> _logger;

    public UpdateDocumentUrlToPersistMultipleSegmentsPerDocument(IMigrationContext context, ILogger<UpdateDocumentUrlToPersistMultipleSegmentsPerDocument> logger)
        : base(context)
    {
        _logger = logger;
    }

    protected override void Migrate()
    {
        Logger.LogDebug("Schema updates to {TableName} for support of multiple segments per document", Constants.DatabaseSchema.Tables.DocumentUrl);

        if (TableExists(Constants.DatabaseSchema.Tables.DocumentUrl))
        {
            ExtendUniqueIndexAcrossSegmentField();
            AddAndPopulateIsPrimaryColumn();
        }
        else
        {
            Logger.LogWarning($"Table {Constants.DatabaseSchema.Tables.DocumentUrl} does not exist so the migration {nameof(UpdateDocumentUrlToPersistMultipleSegmentsPerDocument)} could not be completed.");
        }
    }

    private void ExtendUniqueIndexAcrossSegmentField()
    {
        Logger.LogDebug("Extending the unique index on {TableName} to include the urlSegment column", Constants.DatabaseSchema.Tables.DocumentUrl);

        var indexName = "IX_" + Constants.DatabaseSchema.Tables.DocumentUrl;
        if (IndexExists(indexName))
        {
            DeleteIndex<DocumentUrlDto>(indexName);
        }

        CreateIndex<DocumentUrlDto>(indexName);
    }

    private void AddAndPopulateIsPrimaryColumn()
    {
        const string IsPrimaryColumnName = "isPrimary";

        Logger.LogDebug("Adding the {Column} column {TableName} to include the urlSegment column", IsPrimaryColumnName, Constants.DatabaseSchema.Tables.DocumentUrl);

        var columns = Context.SqlContext.SqlSyntax.GetColumnsInSchema(Context.Database).ToList();
        if (columns
            .SingleOrDefault(x => x.TableName == Constants.DatabaseSchema.Tables.DocumentUrl && x.ColumnName == IsPrimaryColumnName) is null)
        {
            AddColumn<DocumentUrlDto>(Constants.DatabaseSchema.Tables.DocumentUrl, IsPrimaryColumnName);
            Execute.Sql($"UPDATE {Constants.DatabaseSchema.Tables.DocumentUrl} SET {IsPrimaryColumnName} = 1").Do();
        }
    }
}
