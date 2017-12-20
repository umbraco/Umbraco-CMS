using Umbraco.Core.Migrations.Expressions.Common;

namespace Umbraco.Core.Migrations.Expressions.Create.Constraint
{
    public interface ICreateConstraintColumnsBuilder : IFluentBuilder
    {
        /// <summary>
        /// Specifies the constraint column.
        /// </summary>
        IExecutableBuilder Column(string columnName);

        /// <summary>
        /// Specifies the constraint columns.
        /// </summary>
        IExecutableBuilder Columns(string[] columnNames);
    }
}
