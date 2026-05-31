namespace Umbraco.Cms.Infrastructure.Persistence.DatabaseAnnotations;

/// <summary>
///     Attribute that represents the usage of a special type
/// </summary>
/// <remarks>
///     Should only be used when the .NET type can't be directly translated to a DbType.
/// </remarks>
[AttributeUsage(AttributeTargets.Property)]
public class SpecialDbTypeAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="SpecialDbTypeAttribute"/> class with the specified database type.
    /// </summary>
    /// <param name="databaseType">The special database type to use.</param>
    public SpecialDbTypeAttribute(SpecialDbTypes databaseType)
        => DatabaseType = new SpecialDbType(databaseType);

    /// <summary>
    /// Initializes a new instance of the <see cref="SpecialDbTypeAttribute"/> class, associating it with the specified database type.
    /// </summary>
    /// <param name="databaseType">The name of the database type to associate with this attribute.</param>
    public SpecialDbTypeAttribute(string databaseType)
        => DatabaseType = new SpecialDbType(databaseType);

    /// <summary>
    ///     Gets or sets the <see cref="SpecialDbType" /> for this column
    /// </summary>
    public SpecialDbType DatabaseType { get; }
}
