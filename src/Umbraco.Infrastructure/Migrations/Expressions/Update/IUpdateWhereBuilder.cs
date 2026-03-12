using Umbraco.Cms.Infrastructure.Migrations.Expressions.Common;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Update;

/// <summary>
///     Builds an Update expression.
/// </summary>
public interface IUpdateWhereBuilder
{
    /// <summary>
    ///     Specifies the filter conditions to determine which rows are affected by the update.
    /// </summary>
    /// <param name="dataAsAnonymousType">An anonymous type object containing the column names and values to match against.</param>
    /// <returns>An <see cref="IExecutableBuilder"/> that can be used to execute the update operation.</returns>
    IExecutableBuilder Where(object dataAsAnonymousType);

    /// <summary>
    /// Specifies that the update operation should affect all rows in the target table.
    /// </summary>
    /// <returns>An <see cref="IExecutableBuilder"/> that can be used to execute the update operation.</returns>
    IExecutableBuilder AllRows();
}
