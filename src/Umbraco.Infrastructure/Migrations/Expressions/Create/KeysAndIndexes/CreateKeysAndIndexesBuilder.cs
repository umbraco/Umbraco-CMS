using NPoco;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Common;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Execute.Expressions;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.KeysAndIndexes;

public class CreateKeysAndIndexesBuilder : IExecutableBuilder
{
    private readonly IMigrationContext _context;
    private readonly DatabaseType[] _supportedDatabaseTypes;

    public CreateKeysAndIndexesBuilder(IMigrationContext context, params DatabaseType[] supportedDatabaseTypes)
    {
        _context = context;
        _supportedDatabaseTypes = supportedDatabaseTypes;
    }

    public Type? TypeOfDto { get; set; }

    /// <inheritdoc />
    public void Do()
    {
        ISqlSyntaxProvider syntax = _context.SqlContext.SqlSyntax;
        if (TypeOfDto is null)
        {
            return;
        }

        TableDefinition tableDefinition = DefinitionFactory.GetTableDefinition(TypeOfDto, syntax);

        // note: of course we are creating the keys and indexes as per the DTO, so
        // changing the DTO may break old migrations - or, better, these migrations
        // should capture a copy of the DTO class that will not change
        ExecuteSql(syntax.FormatPrimaryKey(tableDefinition));
        foreach (var sql in syntax.Format(tableDefinition.Indexes))
        {
            ExecuteSql(sql);
        }

        foreach (var sql in syntax.Format(tableDefinition.ForeignKeys))
        {
            ExecuteSql(sql);
        }

        // note: we do *not* create the DF_ default constraints
        /*
        foreach (var column in tableDefinition.Columns)
        {
            var sql = syntax.FormatDefaultConstraint(column);
            if (!sql.IsNullOrWhiteSpace())
                ExecuteSql(sql);
        }
        */
    }

    private void ExecuteSql(string sql) =>
        new ExecuteSqlStatementExpression(_context) { SqlStatement = sql }
            .Execute();
}
