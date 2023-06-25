using Umbraco.Cms.Core.Models;

namespace Umbraco.Search.Examine.ValueSetBuilders;

internal static class DeliveryApiContentIndexUtilites
{
    public static string IndexId(IContent content, string culture) => $"{content.Id}|{culture}";
}
