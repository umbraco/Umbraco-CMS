using Umbraco.Cms.Infrastructure.Migrations.Expressions.Common;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Alter.Table;

/// <summary>
/// Represents a builder interface for configuring column options when altering a table during a database migration.
/// </summary>
public interface IAlterTableColumnOptionBuilder :
    IColumnOptionBuilder<IAlterTableColumnOptionBuilder, IAlterTableColumnOptionForeignKeyCascadeBuilder>,
    IAlterTableBuilder,
    IExecutableBuilder
{
}
