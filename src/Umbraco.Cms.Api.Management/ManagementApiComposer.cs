using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Extensions;

namespace Umbraco.Cms.Api.Management;

public class ManagementApiComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder) =>
        builder.AddUmbracoManagementApi();
}

