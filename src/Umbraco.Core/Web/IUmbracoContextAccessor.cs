namespace Umbraco.Cms.Core.Web
{
    /// <summary>
    /// Provides access to UmbracoContext.
    /// </summary>
    public interface IUmbracoContextAccessor
    {
        bool TryGetUmbracoContext(out IUmbracoContext umbracoContext);
        void Clear();
        void Set(IUmbracoContext umbracoContext);
    }
}
