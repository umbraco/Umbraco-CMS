using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Api.Common.Routing;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;

namespace Umbraco.Cms.Api.Common.Configuration;

public class ConfigureMvcOptions : IConfigureOptions<MvcOptions>
{
    private readonly IOptions<GlobalSettings> _globalSettings;

    public ConfigureMvcOptions(IOptions<GlobalSettings> globalSettings) => _globalSettings = globalSettings;

    public void Configure(MvcOptions options)
    {
        // these MVC options may be applied more than once; let's make sure we only execute once.
        if (options.Conventions.Any(convention => convention is UmbracoBackofficeToken))
        {
            return;
        }

        // Replace the BackOfficeToken in routes.

        var backofficePath = _globalSettings.Value.UmbracoPath.TrimStart(Constants.CharArrays.TildeForwardSlash);
        options.Conventions.Add(new UmbracoBackofficeToken(Constants.Web.AttributeRouting.BackOfficeToken, backofficePath));
    }
}
