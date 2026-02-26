namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents settings for content properties, including reserved field names.
/// </summary>
/// <remarks>
///     Reserved field names are standard names that cannot be used for custom properties
///     to prevent conflicts with built-in Umbraco functionality.
/// </remarks>
public class ContentPropertySettings
{
    private readonly HashSet<string> _reservedFieldNames = new();

    /// <summary>
    /// Gets a set of standard names for fields that cannot be used for custom properties.
    /// </summary>
    public ISet<string> ReservedFieldNames => _reservedFieldNames;

    /// <summary>
    ///     Adds a reserved field name to the set.
    /// </summary>
    /// <param name="name">The field name to reserve.</param>
    /// <returns><c>true</c> if the name was added; <c>false</c> if it already exists.</returns>
    public bool AddReservedFieldName(string name) => _reservedFieldNames.Add(name);

    /// <summary>
    ///     Adds multiple reserved field names to the set.
    /// </summary>
    /// <param name="names">The set of field names to reserve.</param>
    public void AddReservedFieldNames(ISet<string> names) => _reservedFieldNames.UnionWith(names);
}
