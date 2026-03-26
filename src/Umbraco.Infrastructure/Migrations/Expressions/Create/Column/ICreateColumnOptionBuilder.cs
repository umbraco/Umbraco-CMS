using Umbraco.Cms.Infrastructure.Migrations.Expressions.Common;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.Column;

/// <summary>
/// Provides methods for configuring options of a column when creating a table in a migration.
/// </summary>
public interface ICreateColumnOptionBuilder :
    IColumnOptionBuilder<ICreateColumnOptionBuilder, ICreateColumnOptionForeignKeyCascadeBuilder>,
    IExecutableBuilder
{
}
