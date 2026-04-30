namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Delete.DefaultConstraint;

/// <summary>
///     Builds a Delete expression.
/// </summary>
public interface IDeleteDefaultConstraintOnTableBuilder : IFluentBuilder
{
    /// <summary>
    ///     Specifies the table containing the default constraint to delete.
    /// </summary>
    /// <param name="tableName">The name of the table containing the constraint to delete.</param>
    /// <returns>An <see cref="Umbraco.Cms.Infrastructure.Migrations.Expressions.Delete.DefaultConstraint.IDeleteDefaultConstraintOnColumnBuilder"/> that can be used to specify the column.</returns>
    IDeleteDefaultConstraintOnColumnBuilder OnTable(string tableName);
}
