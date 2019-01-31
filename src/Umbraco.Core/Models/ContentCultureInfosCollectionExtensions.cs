namespace Umbraco.Core.Models
{
    public static class ContentCultureInfosCollectionExtensions
    {
        public static bool IsCultureUpdated(this ContentCultureInfosCollection to, ContentCultureInfosCollection from, string culture)
            => to != null && to.ContainsKey(culture) &&
               (from == null || !from.ContainsKey(culture) || from[culture].Date != to[culture].Date);

        public static bool IsCultureRemoved(this ContentCultureInfosCollection to, ContentCultureInfosCollection from, string culture)
            => (to == null || !to.ContainsKey(culture)) && from != null && from.ContainsKey(culture);
    }
}