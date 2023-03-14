using Umbraco.Cms.Infrastructure.Migrations.Expressions.Rename.Column;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Rename.Expressions;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Rename.Table;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Rename;

public class RenameBuilder : IRenameBuilder
{
    private readonly IMigrationContext _context;

    public RenameBuilder(IMigrationContext context) => _context = context;

    /// <inheritdoc />
    public IRenameTableBuilder Table(string oldName)
    {
        var expression = new RenameTableExpression(_context) { OldName = oldName };
        return new RenameTableBuilder(expression);
    }

    /// <inheritdoc />
    public IRenameColumnBuilder Column(string oldName)
    {
        var expression = new RenameColumnExpression(_context) { OldName = oldName };
        return new RenameColumnBuilder(expression);
    }
}
