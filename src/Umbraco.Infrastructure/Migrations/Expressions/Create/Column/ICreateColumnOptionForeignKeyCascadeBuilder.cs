using Umbraco.Cms.Infrastructure.Migrations.Expressions.Common;

namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.Column;

/// <summary>
/// Provides a builder interface for configuring cascade options on a foreign key constraint for a column.
/// </summary>
public interface ICreateColumnOptionForeignKeyCascadeBuilder : ICreateColumnOptionBuilder,
    IForeignKeyCascadeBuilder<ICreateColumnOptionBuilder, ICreateColumnOptionForeignKeyCascadeBuilder>
{
}
