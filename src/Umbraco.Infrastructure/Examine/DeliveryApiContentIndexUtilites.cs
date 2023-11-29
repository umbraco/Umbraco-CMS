using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Infrastructure.Examine;

internal static class DeliveryApiContentIndexUtilites
{
    public static string IndexId(IContent content, string culture) => $"{content.Id}|{culture}";
}
