using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core.Models;
using System.Web.Mvc;
using Umbraco.Web.Templates;
using System.IO;
using System.Web.Routing;
using Umbraco.Web.Mvc;

namespace Umbraco.Web
{
    public static class CanvasTemplateExtensions
    {
        public static MvcHtmlString RenderCanvas(this IPublishedProperty property, string framework = "bootstrap3")
        {
            var view = "Canvas/" + framework;
            return new MvcHtmlString(renderPartialViewToString(view, property.Value));
        }

        public static MvcHtmlString RenderCanvas(this IPublishedContent contentItem)
        {
            return RenderCanvas(contentItem, "bodyText", "bootstrap3");
        }

        public static MvcHtmlString RenderCanvas(this IPublishedContent contentItem, string propertyAlias)
        {
            return RenderCanvas(contentItem, propertyAlias, "bootstrap3");    
        }

        public static MvcHtmlString RenderCanvas(this IPublishedContent contentItem, string propertyAlias, string framework)
        {
            var view = "Canvas/" + framework;
            var model =  contentItem.GetProperty(propertyAlias).Value;

            return  new MvcHtmlString(renderPartialViewToString(view, model));
        }

        private static string renderPartialViewToString(string viewName, object model)
        {
            using (var sw = new StringWriter())
            {
                var cc = new ControllerContext();
                cc.RequestContext = new RequestContext(UmbracoContext.Current.HttpContext, new RouteData()
                {
                    Route = RouteTable.Routes["Umbraco_default"]
                });

                var routeHandler = new RenderRouteHandler(ControllerBuilder.Current.GetControllerFactory(), UmbracoContext.Current);
                var routeDef = routeHandler.GetUmbracoRouteDefinition(cc.RequestContext, UmbracoContext.Current.PublishedContentRequest);
                cc.RequestContext.RouteData.Values.Add("action", routeDef.ActionName);
                cc.RequestContext.RouteData.Values.Add("controller", routeDef.ControllerName);

                var partialView = ViewEngines.Engines.FindPartialView(cc, viewName);
                var viewData = new ViewDataDictionary();
                var tempData = new TempDataDictionary();
                
                viewData.Model = model;

                var viewContext = new ViewContext(cc, partialView.View, viewData, tempData, sw);
                partialView.View.Render(viewContext, sw);
                partialView.ViewEngine.ReleaseView(cc, partialView.View);
                
                return sw.GetStringBuilder().ToString();
            }
        }

    }
}
