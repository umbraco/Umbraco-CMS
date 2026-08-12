namespace Umbraco.Cms.Search.Provider.Examine.Helpers;

/// <summary>
/// Builds the Examine document ID for a content item's culture variant.
/// </summary>
internal static class DocumentIdHelper
{
    /// <summary>
    /// Calculates the Examine document ID for a content item, optionally scoped to a culture.
    /// </summary>
    /// <param name="key">The content item's key.</param>
    /// <param name="culture">The culture the document represents, or null for the invariant document.</param>
    /// <returns>The document ID.</returns>
    public static string CalculateDocumentId(Guid key, string? culture)
    {
        var result = key.ToString().ToLowerInvariant();

        if (culture is not null)
        {
            result += $"_{culture}";
        }

        return result;
    }
}
