using Umbraco.Cms.Infrastructure.Migrations.Expressions.Insert.Expressions;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Insert;

/// <summary>
///     Implements <see cref="IInsertBuilder" />.
/// </summary>
public class InsertBuilder : IInsertBuilder
{
    private readonly IMigrationContext _context;

    public InsertBuilder(IMigrationContext context) => _context = context;

    /// <inheritdoc />
    public IInsertIntoBuilder IntoTable(string tableName)
    {
        var expression = new InsertDataExpression(_context) { TableName = tableName };
        return new InsertIntoBuilder(expression);
    }
}
