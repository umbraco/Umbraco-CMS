namespace Umbraco.Core.Migrations.Expressions.Delete.Constraint
{
    /// <summary>
    /// Builds a Delete Constraint expression.
    /// </summary>
    public interface IDeleteConstraintBuilder : IFluentBuilder
    {
        /// <summary>
        /// Specifies the table of the constraint to delete, and executes.
        /// </summary>
        void FromTable(string tableName);
    }
}
