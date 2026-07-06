namespace Umbraco.Cms.Search.Provider.Examine.Helpers;

internal static class DocumentIdHelper
{
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
