using Umbraco.Cms.Core.Models;

namespace Umbraco.Search;

internal static class DeliveryApiContentIndexUtilites
{
    public static string IndexId(IContent content, string? culture) => $"{content.Id}|{culture}";
}
