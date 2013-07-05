using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Umbraco.Web.Mvc
{
    /// <summary>
    /// Mvc handler class to intercept creation of controller and store it for later use.
    /// This means we create two instances of the same controller to support some features later on.
    /// 
    /// The alternate option for this is to completely rewrite all MvcHandler methods.
    /// 
    /// This is currently needed for the 'return CurrentUmbracoPage()' surface controller functionality 
    /// because it needs to send data back to the page controller.
    /// </summary>
    internal class UmbracoMvcHandler : MvcHandler
    {
        public UmbracoMvcHandler(RequestContext requestContext)
            : base(requestContext)
        {
        }

        private void StoreControllerInRouteDefinition()
        {
            var routeDef = (RouteDefinition)RequestContext.RouteData.DataTokens["umbraco-route-def"];

            if (routeDef == null) return;

            // Get the factory and controller and create a new instance of the controller
            var factory = ControllerBuilder.Current.GetControllerFactory();
            var controller = factory.CreateController(RequestContext, routeDef.ControllerName) as ControllerBase;

            // Store the controller
            routeDef.Controller = controller;
        }

        /// <summary>
        /// This is used internally purely to render an Umbraco MVC template to string and shouldn't be used for anything else.
        /// </summary>
        internal void ExecuteUmbracoRequest()
        {
            StoreControllerInRouteDefinition();
            base.ProcessRequest(RequestContext.HttpContext);
        }

        protected override void ProcessRequest(HttpContextBase httpContext)
        {
            StoreControllerInRouteDefinition();

            // Let MVC do its magic and continue the request
            base.ProcessRequest(httpContext);
        }

        protected override IAsyncResult BeginProcessRequest(HttpContextBase httpContext, AsyncCallback callback,
                                                            object state)
        {
            StoreControllerInRouteDefinition();

            return base.BeginProcessRequest(httpContext, callback, state);
        }
    }


}