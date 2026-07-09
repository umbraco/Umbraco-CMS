using Umbraco.Cms.Infrastructure.Migrations.Expressions.Insert.Expressions;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Insert;

/// <summary>
///     Implements <see cref="IInsertBuilder" />.
/// </summary>
public class InsertBuilder : IInsertBuilder
{
    private readonly IMigrationContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Infrastructure.Migrations.Expressions.Insert.InsertBuilder"/> class.
    /// </summary>
    /// <param name="context">The migration context used to manage the state and execution of the migration.</param>
    public InsertBuilder(IMigrationContext context) => _context = context;

    /// <inheritdoc />
    public IInsertIntoBuilder IntoTable(string tableName)
    {
        var expression = new InsertDataExpression(_context) { TableName = tableName };
        return new InsertIntoBuilder(expression);
    }
}
