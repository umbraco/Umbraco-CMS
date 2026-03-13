using System.Data;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Common;

/// <summary>
/// Provides a builder interface for configuring cascade behaviors (such as ON DELETE or ON UPDATE actions) on a foreign key constraint within a migration expression.
/// </summary>
public interface IForeignKeyCascadeBuilder<out TNext, out TNextFk> : IFluentBuilder
    where TNext : IFluentBuilder
    where TNextFk : IFluentBuilder
{
    /// <summary>
    ///     Specifies a rule on deletes.
    /// </summary>
    TNextFk OnDelete(Rule rule);

    /// <summary>
    /// Specifies the action to take when the referenced primary key is updated.
    /// </summary>
    /// <param name="rule">The rule to apply on update (e.g., cascade, set null, restrict).</param>
    /// <returns>The next foreign key cascade builder.</returns>
    TNextFk OnUpdate(Rule rule);

    /// <summary>
    ///     Specifies a rule on deletes and updates.
    /// </summary>
    TNext OnDeleteOrUpdate(Rule rule);
}
