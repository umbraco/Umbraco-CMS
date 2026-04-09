using NPoco;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Common;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Execute.Expressions;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.KeysAndIndexes;

/// <summary>
/// Provides a fluent builder for defining keys and indexes as part of database migration expressions in Umbraco.
/// </summary>
public class CreateKeysAndIndexesBuilder : IExecutableBuilder
{
    private readonly IMigrationContext _context;
    private readonly DatabaseType[] _supportedDatabaseTypes;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateKeysAndIndexesBuilder"/> class for creating keys and indexes during a migration.
    /// </summary>
    /// <param name="context">The migration context in which the builder operates.</param>
    /// <param name="supportedDatabaseTypes">An array of <see cref="DatabaseType"/> values specifying the database types supported by this builder.</param>
    public CreateKeysAndIndexesBuilder(IMigrationContext context, params DatabaseType[] supportedDatabaseTypes)
    {
        _context = context;
        _supportedDatabaseTypes = supportedDatabaseTypes;
    }

    /// <summary>
    /// Gets or sets the type of the data transfer object (DTO) used by this builder.
    /// </summary>
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
