using System;

namespace Umbraco.Core.Migrations.Expressions.Delete.Index
{
    /// <summary>
    /// Builds a Delete Index expression.
    /// </summary>
    public interface IDeleteIndexOnColumnBuilder : IFluentBuilder
    {
        /// <summary>
        /// Specifies the column of the index, and executes.
        /// </summary>
        [Obsolete("I don't think this would ever be used when dropping an index, see DeleteIndexExpression.ToString")]
        void OnColumn(string columnName);

        /// <summary>
        /// Specifies the column of the index, and executes.
        /// </summary>
        [Obsolete("I don't think this would ever be used when dropping an index, see DeleteIndexExpression.ToString")]
        void OnColumns(params string[] columnNames);
    }
}
