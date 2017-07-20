using Umbraco.Web.Models;
//using Umbraco.Core.Dynamics;

// ReSharper disable once CheckNamespace
namespace Umbraco.Web.Mvc
{
    /// <summary>
    /// The View that front-end templates inherit from
    /// </summary>
    public abstract class UmbracoTemplatePage : UmbracoViewPage<RenderModel>
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
                //return _currentPage ?? (_currentPage = Model.Content.AsDynamic());
                return _currentPage ?? (_currentPage = Model.Content);
            }
        }
    }
}
