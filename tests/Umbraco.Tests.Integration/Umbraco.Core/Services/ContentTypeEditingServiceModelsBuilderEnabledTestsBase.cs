using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Infrastructure.ModelsBuilder.Options;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.Services;

internal class ContentTypeEditingServiceModelsBuilderEnabledTestsBase : ContentTypeEditingServiceTestsBase
{
    protected override void CustomTestSetup(IUmbracoBuilder builder)
    {
        builder.Services.ConfigureOptions<ConfigurePropertySettingsOptions>();
    }
}
