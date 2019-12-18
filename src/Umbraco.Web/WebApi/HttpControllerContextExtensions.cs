using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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
        /// Invokes the authorization filters for the controller action.
        /// </summary>
        /// <returns>The response of the first filter returning a result, if any, otherwise null (authorized).</returns>
        internal static async Task<HttpResponseMessage> InvokeAuthorizationFiltersForRequest(this HttpControllerContext controllerContext)
        {
            var controllerDescriptor = controllerContext.ControllerDescriptor;
            var controllerServices = controllerDescriptor.Configuration.Services;
            var actionDescriptor = controllerServices.GetActionSelector().SelectAction(controllerContext);

            var filters = actionDescriptor.GetFilterPipeline();
            var filterGrouping = new FilterGrouping(filters);

            // because the continuation gets built from the inside out we need to reverse the filter list
            // so that least specific filters (Global) get run first and the most specific filters (Action) get run last.
            var authorizationFilters = filterGrouping.AuthorizationFilters.Reverse().ToList();

            if (authorizationFilters.Count == 0)
                return null;

            // if the authorization filter returns a result, it means it failed to authorize
            var actionContext = new HttpActionContext(controllerContext, actionDescriptor);
            return await ExecuteAuthorizationFiltersAsync(actionContext, CancellationToken.None, authorizationFilters);
        }

        /// <summary>
        /// Executes a chain of filters.
        /// </summary>
        /// <remarks>
        /// Recursively calls in to itself as its continuation for the next filter in the chain.
        /// </remarks>
        private static async Task<HttpResponseMessage> ExecuteAuthorizationFiltersAsync(HttpActionContext actionContext, CancellationToken token, IList<IAuthorizationFilter> filters, int index = 0)
        {
            return await filters[index].ExecuteAuthorizationFilterAsync(actionContext, token,
                () => ++index == filters.Count
                    ? Task.FromResult<HttpResponseMessage>(null)
                    : ExecuteAuthorizationFiltersAsync(actionContext, token, filters, index));
        }
    }
}
