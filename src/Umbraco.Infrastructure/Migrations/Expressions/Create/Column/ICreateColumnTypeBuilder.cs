using Umbraco.Cms.Infrastructure.Migrations.Expressions.Common;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.Column;

/// <summary>
/// Defines the builder for specifying the type of a column during a create migration.
/// </summary>
public interface ICreateColumnTypeBuilder : IColumnTypeBuilder<ICreateColumnOptionBuilder>
{
}
