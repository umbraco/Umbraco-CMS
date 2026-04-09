using Umbraco.Cms.Infrastructure.Migrations.Expressions.Common;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Delete.Constraint;

/// <summary>
///     Builds a Delete Constraint expression.
/// </summary>
public interface IDeleteConstraintBuilder : IFluentBuilder
{
    /// <summary>
    /// Specifies the table from which to delete the constraint.
    /// </summary>
    /// <param name="tableName">The name of the table from which the constraint will be deleted.</param>
    /// <returns>An <see cref="IExecutableBuilder"/> to execute the delete operation.</returns>
    IExecutableBuilder FromTable(string tableName);
}
