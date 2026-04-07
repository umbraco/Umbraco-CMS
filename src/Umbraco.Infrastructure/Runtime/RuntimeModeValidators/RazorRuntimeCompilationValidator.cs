using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Exceptions;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Runtime.RuntimeModeValidators;

/// <summary>
/// Validates whether Razor runtime compilation is enabled in the current runtime mode.
/// Ensures that Razor runtime compilation is only used in appropriate environments, such as development,
/// and not in production for security and performance reasons.
/// </summary>
public class RazorRuntimeCompilationValidator : INotificationHandler<UmbracoApplicationStartedNotification>
{
    private readonly IOptionsMonitor<ModelsBuilderSettings> _modelsBuilderSettings;
    private readonly IPublishedModelFactory _publishedModelFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="RazorRuntimeCompilationValidator"/> class.
    /// </summary>
    /// <param name="modelsBuilderSettings">An <see cref="IOptionsMonitor{TOptions}"/> for <see cref="ModelsBuilderSettings"/> used to access the current ModelsBuilder configuration.</param>
    /// <param name="publishedModelFactory">An <see cref="IPublishedModelFactory"/> instance used to create published models.</param>
    public RazorRuntimeCompilationValidator(
        IOptionsMonitor<ModelsBuilderSettings> modelsBuilderSettings,
        IPublishedModelFactory publishedModelFactory)
    {
        _modelsBuilderSettings = modelsBuilderSettings;
        _publishedModelFactory = publishedModelFactory;
    }

    /// <summary>
    /// Handles the <see cref="UmbracoApplicationStartedNotification"/> by validating that the Razor runtime compilation settings
    /// are compatible with the current ModelsBuilder configuration. Throws a <see cref="BootFailedException"/> if the configuration is invalid.
    /// </summary>
    /// <param name="notification">The notification instance triggered when the Umbraco application has started.</param>
    public void Handle(UmbracoApplicationStartedNotification notification)
    {
        if (_modelsBuilderSettings.CurrentValue.ModelsMode == "InMemoryAuto" && _publishedModelFactory.IsLiveFactoryEnabled() is false)
        {
            throw new BootFailedException("InMemoryAuto requires the Umbraco.Cms.DevelopmentMode.Backoffice package to be installed. Install the package or change ModelsBuilder mode.");
        }
    }
}
