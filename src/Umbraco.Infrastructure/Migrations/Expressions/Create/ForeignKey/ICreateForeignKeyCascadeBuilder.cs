using System.Data;
using Umbraco.Cms.Infrastructure.Migrations.Expressions.Common;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.ForeignKey;

/// <summary>
/// Provides a builder interface for configuring cascade behaviors (such as ON DELETE or ON UPDATE) on a foreign key constraint during migration creation.
/// </summary>
public interface ICreateForeignKeyCascadeBuilder : IFluentBuilder, IExecutableBuilder
{
    /// <summary>
    /// Sets the action to take when a referenced row is deleted by specifying the delete rule for the foreign key constraint.
    /// </summary>
    /// <param name="rule">The <see cref="Rule"/> that defines the delete behavior (e.g., Cascade, SetNull, Restrict).</param>
    /// <returns>The current <see cref="ICreateForeignKeyCascadeBuilder"/> instance for method chaining.</returns>
    ICreateForeignKeyCascadeBuilder OnDelete(Rule rule);

    /// <summary>
    /// Sets the action to take when the referenced primary key is updated for this foreign key constraint.
    /// </summary>
    /// <param name="rule">The update rule to apply (e.g., Cascade, SetNull, Restrict).</param>
    /// <returns>The current <see cref="Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.ForeignKey.ICreateForeignKeyCascadeBuilder"/> instance for method chaining.</returns>
    ICreateForeignKeyCascadeBuilder OnUpdate(Rule rule);

    /// <summary>
    /// Configures the action to take when the referenced row is deleted or updated in the foreign key relationship.
    /// </summary>
    /// <param name="rule">The referential action (such as Cascade, SetNull, or Restrict) to apply on delete or update.</param>
    /// <returns>An <see cref="IExecutableBuilder"/> to continue building the migration expression.</returns>
    IExecutableBuilder OnDeleteOrUpdate(Rule rule);
}
