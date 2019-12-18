using System;
using Umbraco.Core.Migrations.Expressions.Common;

namespace Umbraco.Core.Migrations.Expressions.Delete.Index
{
    /// <summary>
    /// Builds a Delete expression.
    /// </summary>
    public interface IDeleteIndexOnColumnBuilder : IFluentBuilder, IExecutableBuilder
    {
        /// <summary>
        /// Specifies the column of the index.
        /// </summary>
        [Obsolete("I don't think this would ever be used when dropping an index, see DeleteIndexExpression.ToString")]
        IExecutableBuilder OnColumn(string columnName);

        /// <summary>
        /// Specifies the column of the index.
        /// </summary>
        [Obsolete("I don't think this would ever be used when dropping an index, see DeleteIndexExpression.ToString")]
        IExecutableBuilder OnColumns(params string[] columnNames);
    }
}
