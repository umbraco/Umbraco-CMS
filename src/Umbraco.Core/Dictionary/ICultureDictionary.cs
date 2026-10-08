using System.Globalization;

namespace Umbraco.Cms.Core.Dictionary;

/// <summary>
/// Represents a dictionary based on a specific culture.
/// </summary>
public interface ICultureDictionary
{
    /// <summary>
    /// Gets the culture for the dictionary.
    /// </summary>
    CultureInfo Culture { get; }

    /// <summary>
    /// Gets the value for a given key.
    /// </summary>
    /// <param name="key">The key for the dictionary item.</param>
    /// <returns>The value matching the provided key. If no value is found, an empty string is returned.</returns>
    string this[string key] { get; }

    /// <summary>
    /// Returns the child dictionary entries for a given key.
    /// </summary>
    /// <param name="key">The key for the dictionary item.</param>
    /// <returns>The value matching the provided key. If no value is found, an empty dictionary is returned.</returns>
    IDictionary<string, string> GetChildren(string key);
}
