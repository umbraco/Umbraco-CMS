using Umbraco.Cms.Infrastructure.Migrations.Expressions.Alter.Expressions;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Alter.Table;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Alter;

/// <summary>
///     Implements <see cref="IAlterBuilder" />.
/// </summary>
public class AlterBuilder : IAlterBuilder
{
    private readonly IMigrationContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Infrastructure.Migrations.Expressions.Alter.AlterBuilder"/> class.
    /// </summary>
    /// <param name="context">The migration context.</param>
    public AlterBuilder(IMigrationContext context) => _context = context;

    /// <inheritdoc />
    public IAlterTableBuilder Table(string tableName)
    {
        var expression = new AlterTableExpression(_context) { TableName = tableName };
        return new AlterTableBuilder(_context, expression);
    }
}
