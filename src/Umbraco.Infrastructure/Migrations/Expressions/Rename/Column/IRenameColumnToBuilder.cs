using Umbraco.Cms.Infrastructure.Migrations.Expressions.Common;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Rename.Column;

/// <summary>
///     Builds a Rename Column expression.
/// </summary>
public interface IRenameColumnToBuilder : IFluentBuilder
{
    /// <summary>
    ///     Specifies the new name of the column.
    /// </summary>
    IExecutableBuilder To(string name);
}
