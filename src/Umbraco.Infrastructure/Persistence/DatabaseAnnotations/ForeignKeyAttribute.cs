using System.Data;

namespace Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

/// <summary>
///     Attribute that represents a Foreign Key reference
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
public class ForeignKeyAttribute : ReferencesAttribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ForeignKeyAttribute"/> class with the specified type.
    /// </summary>
    /// <param name="type">The type that this foreign key references.</param>
    public ForeignKeyAttribute(Type type)
        : base(type)
    {
    }

    /// <summary>
    ///     Gets or sets the cascade rule for deletions.
    /// </summary>
    public Rule OnDelete { get; set; } = Rule.None;

    /// <summary>
    ///     Gets or sets the cascade rule for updates.
    /// </summary>
    public Rule OnUpdate { get; set; } = Rule.None;

    /// <summary>
    ///     Gets or sets the name of the foreign key reference
    /// </summary>
    /// <remarks>
    ///     Overrides the default naming of a foreign key reference:
    ///     FK_thisTableName_refTableName
    /// </remarks>
    public string? Name { get; set; }

    /// <summary>
    ///     Gets or sets the name of the Column that this foreign key should reference.
    /// </summary>
    /// <remarks>PrimaryKey column is used by default</remarks>
    public string? Column { get; set; }
}
