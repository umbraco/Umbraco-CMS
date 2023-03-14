namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Update;

/// <summary>
///     Builds an Update expression.
/// </summary>
public interface IUpdateBuilder : IFluentBuilder
{
    /// <summary>
    ///     Specifies the table to update.
    /// </summary>
    IUpdateTableBuilder Table(string tableName);
}
