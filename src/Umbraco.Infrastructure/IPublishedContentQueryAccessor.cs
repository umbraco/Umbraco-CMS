namespace Umbraco.Cms.Core
{
    /// <remarks>
    /// Not intended for use in background threads where you should instead resolve IPublishedContentQuery directly
    /// and make use of <see cref="Umbraco.Cms.Core.Web.IUmbracoContextFactory.EnsureUmbracoContext"/>
    /// </remarks>
    public interface IPublishedContentQueryAccessor
    {
        bool TryGetValue(out IPublishedContentQuery publishedContentQuery);
    }
}
