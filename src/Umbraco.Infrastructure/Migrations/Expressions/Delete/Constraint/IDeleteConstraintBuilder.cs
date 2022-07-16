using Umbraco.Cms.Infrastructure.Migrations.Expressions.Common;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Delete.Constraint;

/// <summary>
///     Builds a Delete Constraint expression.
/// </summary>
public interface IDeleteConstraintBuilder : IFluentBuilder
{
    /// <summary>
    ///     Specifies the table of the constraint to delete.
    /// </summary>
    IExecutableBuilder FromTable(string tableName);
}
