using Umbraco.Cms.Infrastructure.Migrations.Expressions.Rename.Column;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Rename.Expressions;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Rename.Table;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Rename;

/// <summary>
/// Provides methods to construct rename expressions used in database migrations.
/// This builder is typically used to specify how database objects should be renamed during a migration.
/// </summary>
public class RenameBuilder : IRenameBuilder
{
    private readonly IMigrationContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="Umbraco.Cms.Infrastructure.Migrations.Expressions.Rename.RenameBuilder"/> class using the specified migration context.
    /// </summary>
    /// <param name="context">The <see cref="IMigrationContext"/> to use for the rename operation.</param>
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
