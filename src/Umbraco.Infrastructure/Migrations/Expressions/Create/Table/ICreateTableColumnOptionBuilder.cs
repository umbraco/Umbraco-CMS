using Umbraco.Cms.Infrastructure.Migrations.Expressions.Common;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.Table;

/// <summary>
/// Provides a builder for configuring column options when creating a table.
/// </summary>
public interface ICreateTableColumnOptionBuilder :
    IColumnOptionBuilder<ICreateTableColumnOptionBuilder, ICreateTableColumnOptionForeignKeyCascadeBuilder>,
    ICreateTableWithColumnBuilder
{
}
