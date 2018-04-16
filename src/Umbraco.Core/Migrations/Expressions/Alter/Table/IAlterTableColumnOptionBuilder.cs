using Umbraco.Core.Migrations.Expressions.Common;

namespace Umbraco.Core.Migrations.Expressions.Alter.Table
{
    public interface IAlterTableColumnOptionBuilder : IColumnOptionBuilder<IAlterTableColumnOptionBuilder, IAlterTableColumnOptionForeignKeyCascadeBuilder>,
        IAlterTableBuilder, IExecutableBuilder
    { }
}
