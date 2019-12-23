namespace Umbraco.Core.Migrations.Expressions.Delete.Index
{
    /// <summary>
    /// Builds a Delete expression.
    /// </summary>
    public interface IDeleteIndexForTableBuilder : IFluentBuilder
    {
        /// <summary>
        /// Specifies the table of the index to delete.
        /// </summary>
        IDeleteIndexOnColumnBuilder OnTable(string tableName);
    }
}
