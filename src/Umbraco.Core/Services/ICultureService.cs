using System.Globalization;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Provides services for retrieving culture information.
/// </summary>
public interface ICultureService
{
    /// <summary>
    ///     Gets all valid culture information objects that can be used in the application.
    /// </summary>
    /// <returns>An array of valid <see cref="CultureInfo" /> objects.</returns>
    CultureInfo[] GetValidCultureInfos();
}
