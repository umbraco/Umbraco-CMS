using Umbraco.Core.Migrations.Expressions.Common;

namespace Umbraco.Core.Migrations.Expressions.Delete.Column
{
    /// <summary>
    /// Builds a Delete Column expression.
    /// </summary>
    public interface IDeleteColumnBuilder : IFluentBuilder
    {
        /// <summary>
        /// Specifies the table of the column to delete.
        /// </summary>
        IExecutableBuilder FromTable(string tableName);

        /// <summary>
        /// Specifies the column to delete.
        /// </summary>
        IDeleteColumnBuilder Column(string columnName);
    }
}
