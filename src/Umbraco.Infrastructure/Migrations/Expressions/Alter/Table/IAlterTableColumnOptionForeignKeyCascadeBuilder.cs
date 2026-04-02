using Umbraco.Cms.Infrastructure.Migrations.Expressions.Common;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Alter.Table;

/// <summary>
/// Provides a builder for configuring foreign key cascade behaviors on a table column during an alter table migration.
/// </summary>
public interface IAlterTableColumnOptionForeignKeyCascadeBuilder :
    IAlterTableColumnOptionBuilder,
    IForeignKeyCascadeBuilder<IAlterTableColumnOptionBuilder, IAlterTableColumnOptionForeignKeyCascadeBuilder>
{
}
