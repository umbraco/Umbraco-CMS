using Umbraco.Cms.Infrastructure.Migrations.Expressions.Common;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.Column;

public interface ICreateColumnOptionForeignKeyCascadeBuilder : ICreateColumnOptionBuilder,
    IForeignKeyCascadeBuilder<ICreateColumnOptionBuilder, ICreateColumnOptionForeignKeyCascadeBuilder>
{
}
