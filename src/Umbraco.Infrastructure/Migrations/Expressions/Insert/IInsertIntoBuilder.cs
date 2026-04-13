using Umbraco.Cms.Infrastructure.Migrations.Expressions.Common;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Insert;

/// <summary>
///     Builds an Insert Into expression.
/// </summary>
public interface IInsertIntoBuilder : IFluentBuilder, IExecutableBuilder
{
    /// <summary>
    ///     Enables identity insert.
    /// </summary>
    /// <returns>The current <see cref="IInsertIntoBuilder"/> instance.</returns>
    IInsertIntoBuilder EnableIdentityInsert();

    /// <summary>
    /// Specifies the data for a row to be inserted into the table.
    /// </summary>
    /// <param name="dataAsAnonymousType">An anonymous type representing the column values for the row to insert.</param>
    /// <returns>The current <see cref="Umbraco.Cms.Infrastructure.Migrations.Expressions.Insert.IInsertIntoBuilder"/> instance, allowing for method chaining.</returns>
    IInsertIntoBuilder Row(object dataAsAnonymousType);
}
