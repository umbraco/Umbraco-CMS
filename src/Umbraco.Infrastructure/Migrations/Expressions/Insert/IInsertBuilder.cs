namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Insert;

/// <summary>
///     Builds an Insert expression.
/// </summary>
public interface IInsertBuilder : IFluentBuilder
{
    /// <summary>
    ///     Specifies the table to insert into.
    /// </summary>
    IInsertIntoBuilder IntoTable(string tableName);
}
