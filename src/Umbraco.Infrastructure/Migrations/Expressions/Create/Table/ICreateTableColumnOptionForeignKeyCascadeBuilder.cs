using Umbraco.Cms.Infrastructure.Migrations.Expressions.Common;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.Table;

public interface ICreateTableColumnOptionForeignKeyCascadeBuilder :
    ICreateTableColumnOptionBuilder,
    IForeignKeyCascadeBuilder<ICreateTableColumnOptionBuilder, ICreateTableColumnOptionForeignKeyCascadeBuilder>
{
}
