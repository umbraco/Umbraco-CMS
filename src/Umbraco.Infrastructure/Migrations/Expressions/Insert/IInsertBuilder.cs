namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Insert;

/// <summary>
///     Builds an Insert expression.
/// </summary>
public interface IInsertBuilder : IFluentBuilder
{
    /// <summary>
    /// Specifies the table into which data will be inserted.
    /// </summary>
    /// <param name="tableName">The name of the table to insert into.</param>
    /// <returns>An <see cref="IInsertIntoBuilder"/> to continue building the insert expression.</returns>
    IInsertIntoBuilder IntoTable(string tableName);
}
