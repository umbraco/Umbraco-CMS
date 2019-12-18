using Umbraco.Core.Migrations.Expressions.Common;

namespace Umbraco.Core.Migrations.Expressions.Create.Table
{
    public interface ICreateTableColumnOptionForeignKeyCascadeBuilder :
        ICreateTableColumnOptionBuilder,
        IForeignKeyCascadeBuilder<ICreateTableColumnOptionBuilder, ICreateTableColumnOptionForeignKeyCascadeBuilder>
    { }
}
