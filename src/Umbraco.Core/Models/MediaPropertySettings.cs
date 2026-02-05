namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents settings for media properties including reserved field names.
/// </summary>
public class MediaPropertySettings
{
    private readonly HashSet<string> _reservedFieldNames = new();

    /// <summary>
    ///     Gets a set of standard names for fields that cannot be used for custom properties.
    /// </summary>
    public ISet<string> ReservedFieldNames => _reservedFieldNames;

    /// <summary>
    ///     Adds a single reserved field name.
    /// </summary>
    /// <param name="name">The field name to reserve.</param>
    /// <returns><c>true</c> if the name was added; <c>false</c> if it already exists.</returns>
    public bool AddReservedFieldName(string name) => _reservedFieldNames.Add(name);

    /// <summary>
    ///     Adds multiple reserved field names.
    /// </summary>
    /// <param name="names">The set of field names to reserve.</param>
    public void AddReservedFieldNames(ISet<string> names) => _reservedFieldNames.UnionWith(names);
}
