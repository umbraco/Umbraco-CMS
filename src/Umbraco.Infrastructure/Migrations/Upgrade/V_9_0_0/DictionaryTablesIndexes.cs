using Microsoft.Extensions.Logging;
using NPoco;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Execute.Expressions;
using Umbraco.Cms.Infrastructure.Persistence;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;
using Umbraco.Cms.Infrastructure.Persistence.Dtos;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Migrations.Upgrade.V_9_0_0;

public class DictionaryTablesIndexes : MigrationBase
{
    private const string IndexedDictionaryColumn = "key";
    private const string IndexedLanguageTextColumn = "languageId";

    public DictionaryTablesIndexes(IMigrationContext context)
        : base(context)
    {
    }

    protected override void Migrate()
    {
        var indexDictionaryDto = $"IX_{DictionaryDto.TableName}_{IndexedDictionaryColumn}";
        var indexLanguageTextDto = $"IX_{LanguageTextDto.TableName}_{IndexedLanguageTextColumn}";
        var dictionaryColumnsToBeIndexed = new[] { IndexedDictionaryColumn };
        var langTextColumnsToBeIndexed = new[] { IndexedLanguageTextColumn, "UniqueId" };

        var dictionaryTableHasDuplicates = ContainsDuplicates<DictionaryDto>(dictionaryColumnsToBeIndexed);
        var langTextTableHasDuplicates = ContainsDuplicates<LanguageTextDto>(langTextColumnsToBeIndexed);

        // Check if there are any duplicates before we delete and re-create the indexes since
        // if there are duplicates we won't be able to create the new unique indexes
        if (!dictionaryTableHasDuplicates)
        {
            // Delete existing
            DeleteIndex<DictionaryDto>(indexDictionaryDto);
        }

        if (!langTextTableHasDuplicates)
        {
            // Delete existing
            DeleteIndex<LanguageTextDto>(indexLanguageTextDto);
        }

        // Try to re-create/add
        TryAddUniqueConstraint<DictionaryDto>(dictionaryColumnsToBeIndexed, indexDictionaryDto,
            dictionaryTableHasDuplicates);
        TryAddUniqueConstraint<LanguageTextDto>(langTextColumnsToBeIndexed, indexLanguageTextDto,
            langTextTableHasDuplicates);
    }

    private void DeleteIndex<TDto>(string indexName)
    {
        TableDefinition tableDef = DefinitionFactory.GetTableDefinition(typeof(TDto), Context.SqlContext.SqlSyntax);

        if (IndexExists(indexName))
        {
            Delete.Index(indexName).OnTable(tableDef.Name).Do();
        }
    }

    private void CreateIndex<TDto>(string indexName)
    {
        TableDefinition tableDef = DefinitionFactory.GetTableDefinition(typeof(TDto), Context.SqlContext.SqlSyntax);

        // get the definition by name
        IndexDefinition index = tableDef.Indexes.First(x => x.Name == indexName);
        new ExecuteSqlStatementExpression(Context) { SqlStatement = Context.SqlContext.SqlSyntax.Format(index) }
            .Execute();
    }

    private void TryAddUniqueConstraint<TDto>(string[] columns, string index, bool containsDuplicates)
    {
        TableDefinition tableDef = DefinitionFactory.GetTableDefinition(typeof(TDto), Context.SqlContext.SqlSyntax);

        // Check the existing data to ensure the constraint can be successfully applied.
        // This seems to be better than relying on catching an exception as this leads to
        // transaction errors: "This SqlTransaction has completed; it is no longer usable".
        var columnsDescription = string.Join("], [", columns);
        if (containsDuplicates)
        {
            var message = $"Could not create unique constraint on [{tableDef.Name}] due to existing " +
                          $"duplicate records across the column{(columns.Length > 1 ? "s" : string.Empty)}: [{columnsDescription}].";

            LogIncompleteMigrationStep(message);
            return;
        }

        CreateIndex<TDto>(index);
    }

    private bool ContainsDuplicates<TDto>(string[] columns)
    {
        // Check for duplicates by comparing the total count of all records with the count of records distinct by the
        // provided column. If the former is greater than the latter, there's at least one duplicate record.
        var recordCount = GetRecordCount<TDto>();
        var distinctRecordCount = GetDistinctRecordCount<TDto>(columns);

        return recordCount > distinctRecordCount;
    }

    private int GetRecordCount<TDto>()
    {
        Sql<ISqlContext> countQuery = Database.SqlContext.Sql()
            .SelectCount()
            .From<TDto>();

        return Database.ExecuteScalar<int>(countQuery);
    }

    private int GetDistinctRecordCount<TDto>(string[] columns)
    {
        string columnSpecification;

        columnSpecification = columns.Length == 1
            ? QuoteColumnName(columns[0])
            : $"CONCAT({string.Join(",", columns.Select(QuoteColumnName))})";

        Sql<ISqlContext> distinctCountQuery = Database.SqlContext.Sql()
            .Select($"COUNT(DISTINCT({columnSpecification}))")
            .From<TDto>();

        return Database.ExecuteScalar<int>(distinctCountQuery);
    }

    private void LogIncompleteMigrationStep(string message) =>
        Logger.LogError($"Database migration step failed: {message}");

    private string StringConvertedAndQuotedColumnName(string column) =>
        $"CONVERT(nvarchar(1000),{QuoteColumnName(column)})";

    private string QuoteColumnName(string column) => $"[{column}]";
}
