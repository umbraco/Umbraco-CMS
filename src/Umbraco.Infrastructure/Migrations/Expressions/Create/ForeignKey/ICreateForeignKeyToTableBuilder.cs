namespace Umbraco.Cms.Infrastructure.Migrations.Expressions.Create.ForeignKey;

/// <summary>
/// Represents a builder for specifying the target table of a foreign key constraint.
/// </summary>
public interface ICreateForeignKeyToTableBuilder : IFluentBuilder
{
    /// <summary>
    /// Specifies the table to which the foreign key will reference.
    /// </summary>
    /// <param name="table">The name of the table to reference.</param>
    /// <returns>A builder to specify the primary column of the foreign key.</returns>
    ICreateForeignKeyPrimaryColumnBuilder ToTable(string table);
}
