namespace Umbraco.Core.Migrations.Expressions.Delete.Data
{
    /// <summary>
    /// Builds a Delete Data expression.
    /// </summary>
    public interface IDeleteDataBuilder : IFluentBuilder
    {
        /// <summary>
        /// Specifies a row to be deleted.
        /// </summary>
        IDeleteDataBuilder Row(object dataAsAnonymousType);

        /// <summary>
        /// Specifies that all rows must be deleted, and executes.
        /// </summary>
        void AllRows();

        /// <summary>
        /// Specifies that rows with a specified column being null must be deleted, and executes.
        /// </summary>
        void IsNull(string columnName);

        /// <summary>
        /// Executes.
        /// </summary>
        void Execute();
    }
}
