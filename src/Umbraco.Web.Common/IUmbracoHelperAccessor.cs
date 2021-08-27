namespace Umbraco.Cms.Web.Common
{
    public interface IUmbracoHelperAccessor
    {
        bool TryGetUmbracoHelper(out UmbracoHelper umbracoHelper);
    }
}
