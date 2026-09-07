namespace Umbraco.Cms.Search.Core.Extensions;

/// <summary>
/// Provides extension methods for converting a <see cref="Guid"/> to its index keyword representation.
/// </summary>
public static class GuidExtensions
{
    /// <summary>
    /// Converts the GUID to its index keyword representation.
    /// </summary>
    /// <param name="guid">The GUID.</param>
    /// <returns>The keyword representation.</returns>
    public static string AsKeyword(this Guid guid) => guid.ToString("D");
}
