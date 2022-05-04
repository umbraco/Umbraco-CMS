using Umbraco.Cms.Infrastructure.Migrations.Expressions.Common;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.Table;

public interface ICreateTableColumnOptionBuilder :
    IColumnOptionBuilder<ICreateTableColumnOptionBuilder, ICreateTableColumnOptionForeignKeyCascadeBuilder>,
    ICreateTableWithColumnBuilder
{
}
