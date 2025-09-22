using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Exceptions;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Runtime.RuntimeModeValidators;

public class RazorRuntimeCompilationValidator : INotificationHandler<UmbracoApplicationStartedNotification>
{
    private readonly IOptionsMonitor<ModelsBuilderSettings> _modelsBuilderSettings;
    private readonly IPublishedModelFactory _publishedModelFactory;

    public RazorRuntimeCompilationValidator(
        IOptionsMonitor<ModelsBuilderSettings> modelsBuilderSettings,
        IPublishedModelFactory publishedModelFactory)
    {
        _modelsBuilderSettings = modelsBuilderSettings;
        _publishedModelFactory = publishedModelFactory;
    }

    public void Handle(UmbracoApplicationStartedNotification notification)
    {
        if (_modelsBuilderSettings.CurrentValue.ModelsMode == "InMemoryAuto" && _publishedModelFactory.IsLiveFactoryEnabled() is false)
        {
            throw new BootFailedException("InMemoryAuto requires the Umbraco.Cms.DevelopmentMode.Backoffice package to be installed. Install the package or change ModelsBuilder mode.");
        }
    }
}
