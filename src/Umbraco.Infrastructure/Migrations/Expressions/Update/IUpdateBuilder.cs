namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Update;

/// <summary>
///     Builds an Update expression.
/// </summary>
public interface IUpdateBuilder : IFluentBuilder
{
    /// <summary>
    /// Specifies the table to update in the update expression.
    /// </summary>
    /// <param name="tableName">The name of the table to update.</param>
    /// <returns>An <see cref="IUpdateTableBuilder"/> instance for further configuration of the update operation.</returns>
    IUpdateTableBuilder Table(string tableName);
}
