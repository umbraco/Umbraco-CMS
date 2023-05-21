namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Delete.DefaultConstraint;

/// <summary>
///     Builds a Delete expression.
/// </summary>
public interface IDeleteDefaultConstraintOnTableBuilder : IFluentBuilder
{
    /// <summary>
    ///     Specifies the table of the constraint to delete.
    /// </summary>
    IDeleteDefaultConstraintOnColumnBuilder OnTable(string tableName);
}
