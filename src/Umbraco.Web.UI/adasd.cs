using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Umbraco.Cms.Web.UI;

public static class ConfigureCustomMemberLoginPathExtensions
{
    public static IUmbracoBuilder SetCustomMemberLoginPath(this IUmbracoBuilder builder)
    {
        builder.Services.ConfigureOptions<ConfigureCustomMemberLoginPath>();
        return builder;
    }

    private class ConfigureCustomMemberLoginPath : IConfigureNamedOptions<CookieAuthenticationOptions>
    {
        public void Configure(string? name, CookieAuthenticationOptions options)
        {
            if (name != IdentityConstants.ApplicationScheme)
            {
                return;
            }

            Configure(options);
        }

        // replace options.LoginPath with the path you want to use for default member logins
        public void Configure(CookieAuthenticationOptions options)
            => options.LoginPath = "/login";
    }
}
