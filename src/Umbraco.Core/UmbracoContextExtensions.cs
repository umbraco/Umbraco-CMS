using Umbraco.Web;

namespace Umbraco.Core
{
    public static class UmbracoContextExtensions
    {
        /// <summary>
        /// Boolean value indicating whether the current request is a front-end umbraco request
        /// </summary>
        public static bool IsFrontEndUmbracoRequest(this IUmbracoContext umbracoContext) => umbracoContext.PublishedRequest != null;
    }
}
