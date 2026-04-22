namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.ForeignKey;

/// <summary>
/// Represents a builder used to specify the foreign column(s) when defining a foreign key in a database migration expression.
/// </summary>
public interface ICreateForeignKeyForeignColumnBuilder : IFluentBuilder
{
    /// <summary>
    /// Specifies the foreign column for the foreign key constraint.
    /// </summary>
    /// <param name="column">The name of the foreign column.</param>
    /// <returns>The builder to specify the table the foreign key references.</returns>
    ICreateForeignKeyToTableBuilder ForeignColumn(string column);

    /// <summary>
    /// Specifies the foreign key columns for the foreign key constraint.
    /// </summary>
    /// <param name="columns">The names of the foreign key columns.</param>
    /// <returns>A builder to specify the referenced table for the foreign key.</returns>
    ICreateForeignKeyToTableBuilder ForeignColumns(params string[] columns);
}
