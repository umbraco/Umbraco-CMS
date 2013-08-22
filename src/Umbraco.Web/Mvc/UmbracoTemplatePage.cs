using System;
using StackExchange.Profiling;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Dictionary;
using Umbraco.Core.Dynamics;
using Umbraco.Core.IO;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Profiling;
using Umbraco.Web.Models;

namespace Umbraco.Web.Mvc
{
    /// <summary>
    /// The View that front-end templates inherit from
    /// </summary>
    public abstract class UmbracoTemplatePage : UmbracoViewPage<RenderModel>
    {
        protected UmbracoTemplatePage()
        {

        }

        protected override void InitializePage()
        {
            base.InitializePage();
            //set the model to the current node if it is not set, this is generally not the case
            if (Model != null)
            {
                ////this.ViewData.Model = Model;
                //var backingItem = new DynamicBackingItem(Model.CurrentNode);
                var dynamicNode = new DynamicPublishedContent(Model.Content);
                CurrentPage = dynamicNode.AsDynamic();
            }
        }

        protected override void SetViewData(System.Web.Mvc.ViewDataDictionary viewData)
        {
            //Here we're going to check if the viewData's model is of IPublishedContent, this is basically just a helper for
            //syntax on the front-end so we can just pass in an IPublishedContent object to partial views that inherit from
            //UmbracoTemplatePage. Then we're going to manually contruct a RenderViewModel to pass back in to SetViewData			
            if (viewData.Model is IPublishedContent)
            {
                //change the model to a RenderModel and auto set the culture
                viewData.Model = new RenderModel((IPublishedContent)viewData.Model, UmbracoContext.PublishedContentRequest.Culture);
            }

            base.SetViewData(viewData);
        }

        /// <summary>
        /// Returns the a DynamicPublishedContent object
        /// </summary>
        public dynamic CurrentPage { get; private set; }

        private UmbracoHelper _helper;

        /// <summary>
        /// Gets an UmbracoHelper
        /// </summary>
        /// <remarks>
        /// This ensures that the UmbracoHelper is constructed with the content model of this view
        /// </remarks>
        public override UmbracoHelper Umbraco
        {
            get
            {
                return _helper ?? (_helper = Model == null
                                                 ? new UmbracoHelper(UmbracoContext)
                                                 : new UmbracoHelper(UmbracoContext, Model.Content));
            }
        }
    }
}