namespace Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

/// <summary>
///     Attribute that represents a Primary Key
/// </summary>
/// <remarks>
///     By default, Clustered and AutoIncrement is set to true.
/// </remarks>
[AttributeUsage(AttributeTargets.Property)]
public class PrimaryKeyColumnAttribute : Attribute
{
    public PrimaryKeyColumnAttribute()
    {
        Clustered = true;
        AutoIncrement = true;
    }

    /// <summary>
    ///     Gets or sets a boolean indicating whether the primary key is clustered.
    /// </summary>
    /// <remarks>Defaults to true</remarks>
    public bool Clustered { get; set; }

    /// <summary>
    ///     Gets or sets a boolean indicating whether the primary key is auto incremented.
    /// </summary>
    /// <remarks>Defaults to true</remarks>
    public bool AutoIncrement { get; set; }

    /// <summary>
    ///     Gets or sets the name of the PrimaryKey.
    /// </summary>
    /// <remarks>
    ///     Overrides the default naming of a PrimaryKey constraint:
    ///     PK_tableName
    /// </remarks>
    public string? Name { get; set; }

    /// <summary>
    ///     Gets or sets the names of the columns for this PrimaryKey.
    /// </summary>
    /// <remarks>
    ///     Should only be used if the PrimaryKey spans over multiple columns.
    ///     Usage: [nodeId], [otherColumn]
    /// </remarks>
    public string? OnColumns { get; set; }

    /// <summary>
    ///     Gets or sets the Identity Seed, which is used for Sql Ce databases.
    /// </summary>
    /// <remarks>
    ///     We'll only look for changes to seeding and apply them if the configured database
    ///     is an Sql Ce database.
    /// </remarks>
    public int IdentitySeed { get; set; }
}
