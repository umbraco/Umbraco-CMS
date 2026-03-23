using Umbraco.Cms.Infrastructure.Migrations.Expressions.Common;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Delete.ForeignKey;

/// <summary>
///     Builds a Delete expression.
/// </summary>
public interface IDeleteForeignKeyOnTableBuilder : IFluentBuilder
{
    /// <summary>
    ///     Specifies the table from which the foreign key will be deleted.
    /// </summary>
    /// <param name="foreignTableName">The name of the table containing the foreign key to delete.</param>
    /// <returns>An executable builder to continue the migration expression.</returns>
    IExecutableBuilder OnTable(string foreignTableName);
}
