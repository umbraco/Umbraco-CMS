using System.Linq;
using Microsoft.AspNetCore.Mvc.Filters;
using Umbraco.Core;

namespace Umbraco.Web.BackOffice.Filters
{
    // TODO: This should probably be deleted, anything requiring this should move to a different controller
    public class OverrideAuthorizationFilterProvider : IFilterProvider, IFilterMetadata
    {
        public void OnProvidersExecuted(FilterProviderContext context)
        {

        }

        public void OnProvidersExecuting(FilterProviderContext context)
        {
            if (context.ActionContext.ActionDescriptor.FilterDescriptors != null)
            {
                //Does the action have any UmbracoAuthorizeFilter?
                var overrideFilters = context.Results.Where(filterItem => filterItem.Filter is OverrideAuthorizationAttribute).ToArray();
                foreach (var overrideFilter in overrideFilters)
                {
                    context.Results.RemoveAll(filterItem =>
                        //Remove any filter for the type indicated in the UmbracoAuthorizeFilter attribute
                        filterItem.Descriptor.Filter.GetType() == ((OverrideAuthorizationAttribute)overrideFilter.Filter).FiltersToOverride &&
                        //Remove filters with lower scope (ie controller) than the override filter (ie action method)
                        filterItem.Descriptor.Scope < overrideFilter.Descriptor.Scope);
                }
            }
        }

        //all framework providers have negative orders, so ours will come later
        public int Order => 1;
    }
}
