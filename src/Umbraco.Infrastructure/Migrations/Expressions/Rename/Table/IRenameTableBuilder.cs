using Umbraco.Cms.Infrastructure.Migrations.Expressions.Common;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Rename.Table;

/// <summary>
///     Builds a Rename Table expression.
/// </summary>
public interface IRenameTableBuilder : IFluentBuilder
{
    /// <summary>
    /// Sets the new name for the table to be renamed.
    /// </summary>
    /// <param name="name">The new name of the table.</param>
    /// <returns>An <see cref="IExecutableBuilder"/> that can execute the rename operation.</returns>
    IExecutableBuilder To(string name);
}
