using Umbraco.Cms.Infrastructure.Migrations.Expressions.Update.Expressions;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Update;

/// <summary>
/// Provides methods to construct and execute update expressions used in database schema migrations.
/// This builder is typically used to define how data should be updated during a migration process.
/// </summary>
public class UpdateBuilder : IUpdateBuilder
{
    private readonly IMigrationContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Infrastructure.Migrations.Expressions.Update.UpdateBuilder"/> class
    /// using the specified migration context.
    /// </summary>
    /// <param name="context">The <see cref="IMigrationContext"/> that provides context information for the migration operation.</param>
    public UpdateBuilder(IMigrationContext context) => _context = context;

    /// <inheritdoc />
    public IUpdateTableBuilder Table(string tableName)
    {
        var expression = new UpdateDataExpression(_context) { TableName = tableName };
        return new UpdateDataBuilder(expression);
    }
}
