using Umbraco.Core.Migrations.Expressions.Common;

namespace Umbraco.Core.Migrations.Expressions.Alter.Table
{
    public interface IAlterTableColumnOptionForeignKeyCascadeBuilder :
        IAlterTableColumnOptionBuilder,
        IForeignKeyCascadeBuilder<IAlterTableColumnOptionBuilder, IAlterTableColumnOptionForeignKeyCascadeBuilder>
    { }
}
