using Umbraco.Cms.Infrastructure.Migrations.Expressions.Common;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Delete.Data;

/// <summary>
///     Builds a Delete expression.
/// </summary>
public interface IDeleteDataBuilder : IFluentBuilder, IExecutableBuilder
{
    /// <summary>
    ///     Specifies a row to be deleted.
    /// </summary>
    IDeleteDataBuilder Row(object dataAsAnonymousType);

    /// <summary>
    ///     Specifies that all rows must be deleted.
    /// </summary>
    IExecutableBuilder AllRows();

    /// <summary>
    ///     Specifies that rows with a specified column being null must be deleted.
    /// </summary>
    IExecutableBuilder IsNull(string columnName);
}
