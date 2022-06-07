using Umbraco.Cms.Infrastructure.Migrations.Expressions.Common;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Alter.Table;

public interface IAlterTableColumnOptionForeignKeyCascadeBuilder :
    IAlterTableColumnOptionBuilder,
    IForeignKeyCascadeBuilder<IAlterTableColumnOptionBuilder, IAlterTableColumnOptionForeignKeyCascadeBuilder>
{
}
