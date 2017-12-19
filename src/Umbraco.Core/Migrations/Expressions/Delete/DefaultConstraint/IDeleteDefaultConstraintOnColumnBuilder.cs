namespace Umbraco.Core.Migrations.Expressions.Delete.DefaultConstraint
{
    /// <summary>
    /// Builds a Delete Default Constraint On Column expression.
    /// </summary>
    public interface IDeleteDefaultConstraintOnColumnBuilder : IFluentBuilder
    {
        /// <summary>
        /// Specifies the column of the constraint to delete, and executes.
        /// </summary>
        void OnColumn(string columnName);
    }
}
