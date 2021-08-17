namespace Umbraco.Cms.Core.Web
{
    /// <summary>
    /// Provides access to a TryGetUmbracoContext bool method that will return true if the "current" <see cref="IUmbracoContext"/>. is not null.
    /// Provides a Clear() method that will clear the current UmbracoContext.
    /// Provides a Set() method that til set the current UmbracoContext.
    /// </summary>
    public interface IUmbracoContextAccessor
    {
        bool TryGetUmbracoContext(out IUmbracoContext umbracoContext);
        void Clear();
        void Set(IUmbracoContext umbracoContext);
    }
}
