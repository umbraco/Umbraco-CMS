using System;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Umbraco.Web.BackOffice.Filters
{
    public class OverrideAuthorizationAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// Ensures a special type of authorization filter is ignored. Defaults to <see cref="IAuthorizationFilter"/>.
        /// </summary>
        /// <param name="type">The type of authorization filter to override. if null then <see cref="IAuthorizationFilter"/> is used.</param>
        /// <remarks>
        /// https://stackoverflow.com/questions/33558095/overrideauthorizationattribute-in-asp-net-5
        /// </remarks>
        public OverrideAuthorizationAttribute(Type filtersToOverride = null)
        {
            FiltersToOverride = filtersToOverride ?? typeof(IAuthorizationFilter);
        }

        public Type FiltersToOverride { get;}


    }
}
