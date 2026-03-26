namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.Constraint;

/// <summary>
/// Provides a builder interface for defining and creating constraints on a database table during a migration.
/// </summary>
public interface ICreateConstraintOnTableBuilder : IFluentBuilder
{
    /// <summary>
    ///     Specifies the table name.
    /// </summary>
    ICreateConstraintColumnsBuilder OnTable(string tableName);
}
