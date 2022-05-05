using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace Umbraco.Cms.Web.BackOffice.Extensions
{
    internal static class ControllerContextExtensions
    {
        /// <summary>
        /// Invokes the authorization filters for the controller action.
        /// </summary>
        /// <returns>Whether the user is authenticated or not.</returns>
        internal static async Task<bool> InvokeAuthorizationFiltersForRequest(this ControllerContext controllerContext, ActionContext actionContext)
        {

            var actionDescriptor = controllerContext.ActionDescriptor;

            var metadataCollection = new EndpointMetadataCollection(actionDescriptor.EndpointMetadata.Union(new []{actionDescriptor}));

            var authorizeData = metadataCollection.GetOrderedMetadata<IAuthorizeData>();
            var policyProvider = controllerContext.HttpContext.RequestServices.GetRequiredService<IAuthorizationPolicyProvider>();
            var policy = await AuthorizationPolicy.CombineAsync(policyProvider, authorizeData);
            if (policy is not null)
            {
                var policyEvaluator = controllerContext.HttpContext.RequestServices.GetRequiredService<IPolicyEvaluator>();
                var authenticateResult = await policyEvaluator.AuthenticateAsync(policy, controllerContext.HttpContext);

                if (!authenticateResult.Succeeded)
                {
                    return false;
                }

                // TODO this is super hacky, but we rely on the FeatureAuthorizeHandler can still handle endpoints
                // (The way before .NET 5). The .NET 5 way would need to use han http context, for the "inner" request
                // with the nested controller
                var resource = new Endpoint(null,metadataCollection, null);
                var authorizeResult = await policyEvaluator.AuthorizeAsync(policy, authenticateResult, controllerContext.HttpContext, resource);
                if (!authorizeResult.Succeeded)
                {
                    return false;
                }
            }

            var filters = actionDescriptor.FilterDescriptors;
            var filterGrouping = new FilterGrouping(filters, controllerContext.HttpContext.RequestServices);

            // because the continuation gets built from the inside out we need to reverse the filter list
            // so that least specific filters (Global) get run first and the most specific filters (Action) get run last.
            var authorizationFilters = filterGrouping.AuthorizationFilters.Reverse().ToList();
            var asyncAuthorizationFilters = filterGrouping.AsyncAuthorizationFilters.Reverse().ToList();

            if (authorizationFilters.Count == 0 && asyncAuthorizationFilters.Count == 0)
                return true;

            // if the authorization filter returns a result, it means it failed to authorize
            var authorizationFilterContext = new AuthorizationFilterContext(actionContext, filters.Select(x=>x.Filter).ToArray());
            return await ExecuteAuthorizationFiltersAsync(authorizationFilterContext, authorizationFilters, asyncAuthorizationFilters);
        }

        /// <summary>
        /// Executes a chain of filters.
        /// </summary>
        /// <remarks>
        /// Recursively calls in to itself as its continuation for the next filter in the chain.
        /// </remarks>
        private static async Task<bool> ExecuteAuthorizationFiltersAsync(
            AuthorizationFilterContext authorizationFilterContext,
            IList<IAuthorizationFilter> authorizationFilters,
            IList<IAsyncAuthorizationFilter> asyncAuthorizationFilters)
        {

            foreach (var authorizationFilter in authorizationFilters)
            {
                authorizationFilter.OnAuthorization(authorizationFilterContext);
                if (!(authorizationFilterContext.Result is null))
                {
                    return false;
                }

            }

            foreach (var asyncAuthorizationFilter in asyncAuthorizationFilters)
            {
                await asyncAuthorizationFilter.OnAuthorizationAsync(authorizationFilterContext);
                if (!(authorizationFilterContext.Result is null))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Quickly split filters into different types
        /// </summary>
        private class FilterGrouping
        {
            private readonly List<IActionFilter> _actionFilters = new List<IActionFilter>();
            private readonly List<IAsyncActionFilter> _asyncActionFilters = new List<IAsyncActionFilter>();
            private readonly List<IAuthorizationFilter> _authorizationFilters = new List<IAuthorizationFilter>();
            private readonly List<IAsyncAuthorizationFilter> _asyncAuthorizationFilters = new List<IAsyncAuthorizationFilter>();
            private readonly List<IExceptionFilter> _exceptionFilters = new List<IExceptionFilter>();
            private readonly List<IAsyncExceptionFilter> _asyncExceptionFilters = new List<IAsyncExceptionFilter>();

            public FilterGrouping(IEnumerable<FilterDescriptor> filters, IServiceProvider serviceProvider)
            {
                if (filters == null) throw new ArgumentNullException("filters");

                foreach (FilterDescriptor f in filters)
                {
                    var filter = f.Filter;
                    Categorize(filter, _actionFilters, serviceProvider);
                    Categorize(filter, _authorizationFilters, serviceProvider);
                    Categorize(filter, _exceptionFilters, serviceProvider);
                    Categorize(filter, _asyncActionFilters, serviceProvider);
                    Categorize(filter, _asyncAuthorizationFilters, serviceProvider);
                }
            }

            public IEnumerable<IActionFilter> ActionFilters => _actionFilters;
            public IEnumerable<IAsyncActionFilter> AsyncActionFilters => _asyncActionFilters;
            public IEnumerable<IAuthorizationFilter> AuthorizationFilters => _authorizationFilters;

            public IEnumerable<IAsyncAuthorizationFilter> AsyncAuthorizationFilters => _asyncAuthorizationFilters;

            public IEnumerable<IExceptionFilter> ExceptionFilters => _exceptionFilters;

            public IEnumerable<IAsyncExceptionFilter> AsyncExceptionFilters => _asyncExceptionFilters;

            private static void Categorize<T>(IFilterMetadata filter, List<T> list, IServiceProvider serviceProvider) where T : class
            {
                if(filter is TypeFilterAttribute typeFilterAttribute)
                {
                    filter = typeFilterAttribute.CreateInstance(serviceProvider);
                }

                T? match = filter as T;
                if (match != null)
                {
                    list.Add(match);
                }
            }
        }
    }
}
