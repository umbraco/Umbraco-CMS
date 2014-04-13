using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.Remoting.Contexts;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Umbraco.Web.WebApi.Filters;

namespace Umbraco.Web.WebApi
{
    internal static class HttpControllerContextExtensions
    {
        /// <summary>
        /// This method will go an execute the authorization filters for the controller action, if any fail
        /// it will return their response, otherwise we'll return null.
        /// </summary>
        /// <param name="controllerContext"></param>
        internal static async Task<HttpResponseMessage> InvokeAuthorizationFiltersForRequest(this HttpControllerContext controllerContext)
        {
            var controllerDescriptor = controllerContext.ControllerDescriptor;
            var controllerServices = controllerDescriptor.Configuration.Services;
            var actionDescriptor = controllerServices.GetActionSelector().SelectAction(controllerContext);
            var actionContext = new HttpActionContext(controllerContext, actionDescriptor);
            var filters = actionDescriptor.GetFilterPipeline();
            var filterGrouping = new FilterGrouping(filters);
            var actionFilters = filterGrouping.ActionFilters;
            // Because the continuation gets built from the inside out we need to reverse the filter list
            // so that least specific filters (Global) get run first and the most specific filters (Action) get run last.
            var authorizationFilters = filterGrouping.AuthorizationFilters.Reverse().ToArray();

            if (authorizationFilters.Any())
            {
                var cancelToken = new CancellationToken();           
                var filterResult = await FilterContinuation(actionContext, cancelToken, authorizationFilters, 0);
                if (filterResult != null)
                {
                    //this means that the authorization filter has returned a result - unauthorized so we cannot continue
                    return filterResult;
                }                    
            }
            return null;
        }

        /// <summary>
        /// This method is how you execute a chain of filters, it needs to recursively call in to itself as the continuation for the next filter in the chain
        /// </summary>
        /// <param name="actionContext"></param>
        /// <param name="token"></param>
        /// <param name="filters"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        private static async Task<HttpResponseMessage> FilterContinuation(HttpActionContext actionContext, CancellationToken token, IList<IAuthorizationFilter> filters, int index)
        {
            return await filters[index].ExecuteAuthorizationFilterAsync(actionContext, token, () =>
            {
                Func<HttpResponseMessage> nullResponse = () => null;
                return (index + 1) == filters.Count
                    ? Task.Run(nullResponse)
                    : FilterContinuation(actionContext, token, filters, ++index);
            });
        }

        /// <summary>
        /// Sets the JSON GUID format to not have hyphens
        /// </summary>
        /// <param name="controllerContext"></param>
        internal static void SetOutgoingNoHyphenGuidFormat(this HttpControllerContext controllerContext)
        {
            var jsonFormatter = controllerContext.Configuration.Formatters.JsonFormatter;
            jsonFormatter.SerializerSettings.Converters.Add(new GuidNoHyphenConverter());
        }


        /// <summary>
        /// Sets the JSON datetime format to be a custom one
        /// </summary>
        /// <param name="controllerContext"></param>
        /// <param name="format"></param>
        internal static void SetOutgoingDateTimeFormat(this HttpControllerContext controllerContext, string format)
        {
            var jsonFormatter = controllerContext.Configuration.Formatters.JsonFormatter;
            jsonFormatter.SerializerSettings.Converters.Add(new CustomDateTimeConvertor(format));
        }

        /// <summary>
        /// Sets the JSON datetime format to be our regular iso standard
        /// </summary>
        internal static void SetOutgoingDateTimeFormat(this HttpControllerContext controllerContext)
        {
            var jsonFormatter = controllerContext.Configuration.Formatters.JsonFormatter;
            jsonFormatter.SerializerSettings.Converters.Add(new CustomDateTimeConvertor("yyyy-MM-dd HH:mm:ss"));
        }

        ///// <summary>
        ///// Removes the xml formatter so it only outputs angularized json (with the json vulnerability prefix added)
        ///// </summary>
        ///// <param name="controllerContext"></param>
        //internal static void EnsureJsonOutputOnly(this HttpControllerContext controllerContext)
        //{
        //    controllerContext.Configuration.Formatters = new MediaTypeFormatterCollection();

        //    //remove all json/xml formatters then add our custom one
        //    var toRemove = controllerContext.Configuration.Formatters.Where(t => (t is JsonMediaTypeFormatter) || (t is XmlMediaTypeFormatter)).ToList();
        //    foreach (var r in toRemove)
        //    {
        //        controllerContext.Configuration.Formatters.Remove(r);
        //    }
        //    controllerContext.Configuration.Formatters.Add(new AngularJsonMediaTypeFormatter());
        //}
    }
}