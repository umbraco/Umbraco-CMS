using System.Data;
using Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

namespace Umbraco.Cms.Infrastructure.Persistence.DatabaseModelDefinitions;

public class ColumnDefinition
{
    public virtual string Name { get; set; } = null!;

    // This type is typically used as part of a migration
    public virtual DbType? Type { get; set; }

    // When DbType isn't set explicitly the Type will be used to find the right DbType in the SqlSyntaxProvider.
    // This type is typically used as part of an initial table creation
    public Type PropertyType { get; set; } = null!;

    /// <summary>
    ///     Used for column types that cannot be natively mapped.
    /// </summary>
    public SpecialDbType? CustomDbType { get; set; }

    public virtual int Seeding { get; set; }

    public virtual int Size { get; set; }

    public virtual int Precision { get; set; }

    public virtual string? CustomType { get; set; }

    public virtual object? DefaultValue { get; set; }

    public virtual string? ConstraintName { get; set; }

    public virtual bool IsForeignKey { get; set; }

    public virtual bool IsIdentity { get; set; }

    public virtual bool IsIndexed { get; set; } // Clustered?

    public virtual bool IsPrimaryKey { get; set; }

    public virtual string? PrimaryKeyName { get; set; }

    public virtual string? PrimaryKeyColumns { get; set; } // When the primary key spans multiple columns

    public virtual bool IsNullable { get; set; }

    public virtual bool IsUnique { get; set; }

    public virtual string? TableName { get; set; }

    public virtual ModificationType ModificationType { get; set; }
}
