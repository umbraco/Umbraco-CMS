using Umbraco.Cms.Infrastructure.Migrations.Expressions.Common;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Rename.Column;

/// <summary>
///     Builds a Rename Column expression.
/// </summary>
public interface IRenameColumnToBuilder : IFluentBuilder
{
    /// <summary>
    /// Specifies the new name of the column.
    /// </summary>
    /// <param name="name">The new name to assign to the column.</param>
    /// <returns>An <see cref="Umbraco.Cms.Infrastructure.Migrations.Expressions.IExecutableBuilder" /> that can be used to execute the rename operation.</returns>
    IExecutableBuilder To(string name);
}
