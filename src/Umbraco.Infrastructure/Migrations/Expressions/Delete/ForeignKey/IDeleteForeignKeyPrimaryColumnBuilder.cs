using Umbraco.Cms.Infrastructure.Migrations.Expressions.Common;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Delete.ForeignKey;

/// <summary>
///     Builds a Delete expression.
/// </summary>
public interface IDeleteForeignKeyPrimaryColumnBuilder : IFluentBuilder
{
    /// <summary>
    ///     Specifies the target primary column.
    /// </summary>
    IExecutableBuilder PrimaryColumn(string column);

    /// <summary>
    ///     Specifies the target primary columns.
    /// </summary>
    IExecutableBuilder PrimaryColumns(params string[] columns);
}
