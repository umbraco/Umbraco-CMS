using NPoco;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Common;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Execute.Expressions;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.Table;

public class CreateTableOfDtoBuilder : IExecutableBuilder
{
    private readonly IMigrationContext _context;

    // TODO: This doesn't do anything.
    private readonly DatabaseType[] _supportedDatabaseTypes;

    public CreateTableOfDtoBuilder(IMigrationContext context, params DatabaseType[] supportedDatabaseTypes)
    {
        _context = context;
        _supportedDatabaseTypes = supportedDatabaseTypes;
    }

    public Type? TypeOfDto { get; set; }

    public bool WithoutKeysAndIndexes { get; set; }

    /// <inheritdoc />
    public void Do()
    {
        ISqlSyntaxProvider syntax = _context.SqlContext.SqlSyntax;
        if (TypeOfDto is null)
        {
            return;
        }

        TableDefinition tableDefinition = DefinitionFactory.GetTableDefinition(TypeOfDto, syntax);

        syntax.HandleCreateTable(_context.Database, tableDefinition, WithoutKeysAndIndexes);
        _context.BuildingExpression = false;
    }

    private void ExecuteSql(string sql) =>
        new ExecuteSqlStatementExpression(_context) { SqlStatement = sql }
            .Execute();
}
