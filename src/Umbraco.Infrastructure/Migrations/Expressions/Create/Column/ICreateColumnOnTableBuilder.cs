using Umbraco.Cms.Infrastructure.Migrations.Expressions.Common;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.Column;

/// <summary>
/// Represents a builder interface used to define and configure a new column on a database table as part of a migration process.
/// </summary>
public interface ICreateColumnOnTableBuilder : IColumnTypeBuilder<ICreateColumnOptionBuilder>
{
    /// <summary>
    ///     Specifies the name of the table.
    /// </summary>
    ICreateColumnTypeBuilder OnTable(string name);
}
