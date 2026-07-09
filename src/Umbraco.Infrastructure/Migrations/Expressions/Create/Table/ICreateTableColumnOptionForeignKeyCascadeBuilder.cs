using Umbraco.Cms.Infrastructure.Migrations.Expressions.Common;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.Table;

/// <summary>
/// Provides a fluent builder for configuring cascade behaviors on a foreign key column when creating a table.
/// </summary>
public interface ICreateTableColumnOptionForeignKeyCascadeBuilder :
    ICreateTableColumnOptionBuilder,
    IForeignKeyCascadeBuilder<ICreateTableColumnOptionBuilder, ICreateTableColumnOptionForeignKeyCascadeBuilder>
{
}
