using Umbraco.Cms.Infrastructure.Migrations.Expressions.Common;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.Constraint;

public interface ICreateConstraintColumnsBuilder : IFluentBuilder
{
    /// <summary>
    ///     Specifies the constraint column.
    /// </summary>
    IExecutableBuilder Column(string columnName);

    /// <summary>
    ///     Specifies the constraint columns.
    /// </summary>
    IExecutableBuilder Columns(string[] columnNames);
}
