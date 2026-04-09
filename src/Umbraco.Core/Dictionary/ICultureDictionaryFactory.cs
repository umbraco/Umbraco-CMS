using System.Globalization;

namespace Umbraco.Cms.Core.Dictionary;

/// <summary>
/// Factory for creating <see cref="ICultureDictionary"/> instances.
/// </summary>
/// <remarks>
/// This factory allows for creating dictionaries using either the current thread's culture
/// or a specific culture. It enables flexibility in dictionary item retrieval across different
/// cultures and can be extended to support alternative storage mechanisms.
/// </remarks>
public interface ICultureDictionaryFactory
{
    /// <summary>
    /// Creates a dictionary using the current thread's culture.
    /// </summary>
    /// <returns>A culture dictionary for the current thread's culture.</returns>
    ICultureDictionary CreateDictionary();

    /// <summary>
    /// Creates a dictionary for a specific culture.
    /// </summary>
    /// <param name="specificCulture">The culture to create the dictionary for.</param>
    /// <returns>A culture dictionary for the specified culture.</returns>
    ICultureDictionary CreateDictionary(CultureInfo specificCulture) => throw new NotImplementedException();
}
