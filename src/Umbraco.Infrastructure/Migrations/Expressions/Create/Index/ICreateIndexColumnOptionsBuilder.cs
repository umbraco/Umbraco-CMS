namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.Index;

/// <summary>
/// Provides options for configuring a column when creating an index in a migration expression.
/// </summary>
public interface ICreateIndexColumnOptionsBuilder : IFluentBuilder
{
    /// <summary>Marks the index column to be sorted in ascending order.</summary>
    /// <returns>The builder to continue configuring the index column.</returns>
    ICreateIndexOnColumnBuilder Ascending();

    /// <summary>
    /// Specifies that the index column should be sorted in descending order.
    /// </summary>
    /// <returns>The builder for configuring the index column.</returns>
    ICreateIndexOnColumnBuilder Descending();

    /// <summary>
    /// Specifies that the index on this column should enforce uniqueness, ensuring that no duplicate values are allowed.
    /// </summary>
    /// <returns>The builder instance for further index configuration.</returns>
    ICreateIndexOnColumnBuilder Unique();
}
