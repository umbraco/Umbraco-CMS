using Umbraco.Core.Dynamics;
using Umbraco.Core.Models;
using Umbraco.Web.Models;

namespace Umbraco.Web.Mvc
{
    public abstract class UmbracoTemplatePage<TContent> : UmbracoViewPage<RenderModel<TContent>>
        where TContent : IPublishedContent
    {
        private object _currentPage;

        /// <summary>
        /// Returns the content as a dynamic object
        /// </summary>
        public dynamic CurrentPage
        {
            get
            {
                // it's invalid to create a DynamicPublishedContent around a null content anyway
                // fixme - should we return null or DynamicNull.Null?
                if (Model == null || Model.Content == null) return null;
                return _currentPage ?? (_currentPage = Model.Content.AsDynamic());
            }
        }
    }
}
