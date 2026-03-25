using NPoco;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Common;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Execute.Expressions;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;
using Umbraco.Cms.Infrastructure.Persistence.SqlSyntax;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.Table;

/// <summary>
/// Provides a builder for creating a database table schema based on a specified DTO (Data Transfer Object) type.
/// Typically used in migration expressions to define table structure from a DTO.
/// </summary>
public class CreateTableOfDtoBuilder : IExecutableBuilder
{
    private readonly IMigrationContext _context;

    // TODO: This doesn't do anything.
    private readonly DatabaseType[] _supportedDatabaseTypes;

    /// <summary>
    /// Initializes a new instance of the <see cref="CreateTableOfDtoBuilder"/> class.
    /// </summary>
    /// <param name="context">The migration context to use for the table creation.</param>
    /// <param name="supportedDatabaseTypes">A params array of <see cref="DatabaseType"/> values specifying the database types supported by this table.</param>
    public CreateTableOfDtoBuilder(IMigrationContext context, params DatabaseType[] supportedDatabaseTypes)
    {
        _context = context;
        _supportedDatabaseTypes = supportedDatabaseTypes;
    }

    /// <summary>
    /// Gets or sets the type of the Data Transfer Object (DTO) for the table being created.
    /// </summary>
    public Type? TypeOfDto { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to create the table without any keys or indexes.
    /// </summary>
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
