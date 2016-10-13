using Umbraco.Core.Models.PublishedContent;

// ReSharper disable once CheckNamespace
namespace Umbraco.Web
{
    public static class UmbracoHelperExtensions
    {
        // fixme - missing many more

        public static IPublishedContent TypedMedia(this UmbracoHelper helper, int id)
            => helper.Media(id);
    }
}
