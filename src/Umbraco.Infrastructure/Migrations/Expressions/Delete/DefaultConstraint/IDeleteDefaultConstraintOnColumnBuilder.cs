using Umbraco.Cms.Infrastructure.Migrations.Expressions.Common;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Delete.DefaultConstraint;

/// <summary>
///     Builds a Delete expression.
/// </summary>
public interface IDeleteDefaultConstraintOnColumnBuilder : IFluentBuilder
{
    /// <summary>
    /// Specifies the column from which to delete the default constraint.
    /// </summary>
    /// <param name="columnName">The name of the column that has the default constraint to be deleted.</param>
    /// <returns>An <see cref="Umbraco.Cms.Infrastructure.Migrations.Expressions.IExecutableBuilder" /> to execute the delete operation.</returns>
    IExecutableBuilder OnColumn(string columnName);
}
