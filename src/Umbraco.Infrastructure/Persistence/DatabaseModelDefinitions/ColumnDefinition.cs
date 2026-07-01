using System.Data;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

/// <summary>
/// Represents the definition of a database column, including its data type, properties, and constraints.
/// </summary>
public class ColumnDefinition
{
    /// <summary>
    /// Gets or sets the name of the column.
    /// </summary>
    public virtual string Name { get; set; } = null!;

    /// <summary>
    /// Gets or sets the data type of the column in the database.
    /// </summary>
    /// <remarks>This type is typically used as part of a migration</remarks>
    public virtual DbType? Type { get; set; }

    /// <summary>
    /// Gets or sets the CLR <see cref="Type"/> that represents the property associated with this column.
    /// This type is used to infer the database type when <c>DbType</c> is not explicitly set, typically during initial table creation.
    /// </summary>
    /// <remarks>
    /// When DbType isn't set explicitly the Type will be used to find the right DbType in the SqlSyntaxProvider.
    /// This type is typically used as part of an initial table creation
    /// </remarks>
    public Type PropertyType { get; set; } = null!;

    /// <summary>
    ///     Used for column types that cannot be natively mapped.
    /// </summary>
    public SpecialDbType? CustomDbType { get; set; }

    /// <summary>
    /// Gets or sets the initial seed value used for auto-incrementing the column, if applicable.
    /// </summary>
    public virtual int Seeding { get; set; }

    /// <summary>
    /// Gets or sets the maximum size or length of the column, typically used for string or binary data types.
    /// </summary>
    public virtual int Size { get; set; }

    /// <summary>
    /// Gets or sets the precision of the column, which defines the total number of digits that can be stored for numeric data types.
    /// </summary>
    public virtual int Precision { get; set; }

    /// <summary>
    /// Gets or sets a custom database type for the column, overriding the default type mapping if specified.
    /// </summary>
    public virtual string? CustomType { get; set; }

    /// <summary>
    /// Gets or sets the default value for the column.
    /// </summary>
    public virtual object? DefaultValue { get; set; }

    /// <summary>
    /// Gets or sets the name of the constraint associated with the column.
    /// </summary>
    public virtual string? ConstraintName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this column is a foreign key.
    /// </summary>
    public virtual bool IsForeignKey { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this column is an identity column.
    /// </summary>
    public virtual bool IsIdentity { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the column is indexed.
    /// </summary>
    /// <remarks>Clustered?</remarks>
    public virtual bool IsIndexed { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether this column is part of the primary key.
    /// </summary>
    public virtual bool IsPrimaryKey { get; set; }

    /// <summary>Gets or sets the name of the primary key associated with this column, if any.</summary>
    public virtual string? PrimaryKeyName { get; set; }

    /// <summary>
    /// Gets or sets the names of the columns that compose a composite primary key.
    /// </summary>
    /// <remarks>When the primary key spans multiple columns</remarks>
    public virtual string? PrimaryKeyColumns { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the column allows null values.
    /// </summary>
    public virtual bool IsNullable { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the column has a unique constraint.
    /// </summary>
    public virtual bool IsUnique { get; set; }

    /// <summary>
    /// Gets or sets the name of the table that this column belongs to.
    /// </summary>
    public virtual string? TableName { get; set; }

    /// <summary>
    /// Gets or sets a value indicating the type of modification (such as addition, update, or deletion) applied to the column definition.
    /// </summary>
    public virtual ModificationType ModificationType { get; set; }
}
