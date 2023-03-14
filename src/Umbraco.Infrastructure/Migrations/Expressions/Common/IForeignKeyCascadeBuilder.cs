using System.Data;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Common;

public interface IForeignKeyCascadeBuilder<out TNext, out TNextFk> : IFluentBuilder
    where TNext : IFluentBuilder
    where TNextFk : IFluentBuilder
{
    /// <summary>
    ///     Specifies a rule on deletes.
    /// </summary>
    TNextFk OnDelete(Rule rule);

    /// <summary>
    ///     Specifies a rule on updates.
    /// </summary>
    TNextFk OnUpdate(Rule rule);

    /// <summary>
    ///     Specifies a rule on deletes and updates.
    /// </summary>
    TNext OnDeleteOrUpdate(Rule rule);
}
