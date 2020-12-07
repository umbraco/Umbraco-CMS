using System;
using Umbraco.Core.DependencyInjection;
using Umbraco.Web.BackOffice.Security;

namespace Umbraco.Extensions
{
    public static class AuthenticationBuilderExtensions
    {
        public static IUmbracoBuilder AddBackOfficeExternalLogins(this IUmbracoBuilder umbracoBuilder, Action<BackOfficeExternalLoginsBuilder> builder)
        {
            builder(new BackOfficeExternalLoginsBuilder(umbracoBuilder.Services));
            return umbracoBuilder;
        }
    }

}
