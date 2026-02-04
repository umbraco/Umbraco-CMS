using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management;

public class ManagementApiComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        // Only register Management API services if backoffice is enabled.
        if (builder.Services.Any(s => s.ServiceType == typeof(IBackOfficeEnabledMarker)) is false)
        {
            return;
        }

        builder.AddUmbracoManagementApi();
    }
}

