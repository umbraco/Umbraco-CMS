using Umbraco.Core.Migrations.Expressions.Common;

namespace Umbraco.Core.Migrations.Expressions.Delete.DefaultConstraint
{
    /// <summary>
    /// Builds a Delete expression.
    /// </summary>
    public interface IDeleteDefaultConstraintOnColumnBuilder : IFluentBuilder
    {
        /// <summary>
        /// Specifies the column of the constraint to delete.
        /// </summary>
        IExecutableBuilder OnColumn(string columnName);
    }
}
