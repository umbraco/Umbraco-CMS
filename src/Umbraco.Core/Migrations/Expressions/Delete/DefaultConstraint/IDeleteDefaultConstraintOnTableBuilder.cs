namespace Umbraco.Core.Migrations.Expressions.Delete.DefaultConstraint
{
    /// <summary>
    /// Builds a Delete Default Constraint On Table expression.
    /// </summary>
    public interface IDeleteDefaultConstraintOnTableBuilder : IFluentBuilder
    {
        /// <summary>
        /// Specifies the table of the constraint to delete.
        /// </summary>
        IDeleteDefaultConstraintOnColumnBuilder OnTable(string tableName);
    }
}
