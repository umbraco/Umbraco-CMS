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
    public SpecialDbTypeAttribute(SpecialDbTypes databaseType)
        => DatabaseType = new SpecialDbType(databaseType);

    public SpecialDbTypeAttribute(string databaseType)
        => DatabaseType = new SpecialDbType(databaseType);

    /// <summary>
    ///     Gets or sets the <see cref="SpecialDbType" /> for this column
    /// </summary>
    public SpecialDbType DatabaseType { get; }
}
