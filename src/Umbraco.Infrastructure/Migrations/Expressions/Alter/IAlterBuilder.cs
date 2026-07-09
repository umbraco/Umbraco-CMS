using Umbraco.Cms.Infrastructure.Migrations.Expressions.Alter.Table;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Alter;

/// <summary>
///     Builds an Alter expression.
/// </summary>
public interface IAlterBuilder : IFluentBuilder
{
    /// <summary>
    /// Specifies which table to alter in the database schema.
    /// </summary>
    /// <param name="tableName">The name of the table to be altered.</param>
    /// <returns>An <see cref="IAlterTableBuilder"/> instance to continue building the alteration expression.</returns>
    IAlterTableBuilder Table(string tableName);
}
