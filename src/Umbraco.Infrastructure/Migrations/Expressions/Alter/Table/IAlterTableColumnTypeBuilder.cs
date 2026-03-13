using Umbraco.Cms.Infrastructure.Migrations.Expressions.Common;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Alter.Table;

/// <summary>
/// Represents a builder for altering the data type of a table column during a database migration.
/// </summary>
public interface IAlterTableColumnTypeBuilder : IColumnTypeBuilder<IAlterTableColumnOptionBuilder>
{
}
