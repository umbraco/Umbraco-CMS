using Umbraco.Cms.Infrastructure.Migrations.Expressions.Common;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.Constraint;

/// <summary>
/// Provides a builder interface for specifying columns when creating a database constraint in a migration expression.
/// </summary>
public interface ICreateConstraintColumnsBuilder : IFluentBuilder
{
    /// <summary>
    ///     Specifies the name of the column to which the constraint will be applied in the migration.
    /// </summary>
    /// <param name="columnName">The name of the column to specify for the constraint.</param>
    /// <returns>An <see cref="IExecutableBuilder"/> to execute the next step in the migration.</returns>
    IExecutableBuilder Column(string columnName);

    /// <summary>
    ///     Specifies the constraint columns.
    /// </summary>
    IExecutableBuilder Columns(string[] columnNames);
}
