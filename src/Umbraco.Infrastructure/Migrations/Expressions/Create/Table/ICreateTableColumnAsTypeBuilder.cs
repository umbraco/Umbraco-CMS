using Umbraco.Cms.Infrastructure.Migrations.Expressions.Common;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.Table;

/// <summary>
/// Represents a builder interface for specifying the data type of a column when defining a table schema in a create table migration expression.
/// </summary>
public interface ICreateTableColumnAsTypeBuilder : IColumnTypeBuilder<ICreateTableColumnOptionBuilder>
{
}
