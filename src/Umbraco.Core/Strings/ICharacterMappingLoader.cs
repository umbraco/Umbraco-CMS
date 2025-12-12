using System.Collections.Frozen;

namespace Umbraco.Cms.Core.Strings;

/// <summary>
/// Loads character mappings from JSON files.
/// </summary>
public interface ICharacterMappingLoader
{
    /// <summary>
    /// Loads all mapping files and returns combined FrozenDictionary.
    /// Higher priority mappings override lower priority.
    /// </summary>
    /// <returns>Frozen dictionary of character to string mappings.</returns>
    FrozenDictionary<char, string> LoadMappings();
}
