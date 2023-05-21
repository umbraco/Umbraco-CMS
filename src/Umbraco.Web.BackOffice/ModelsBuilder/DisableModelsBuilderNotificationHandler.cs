using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Features;
using Umbraco.Cms.Core.Notifications;

namespace Umbraco.Cms.Web.BackOffice.ModelsBuilder;

/// <summary>
/// Used in conjunction with <see cref="UmbracoBuilderExtensions.DisableModelsBuilderControllers"/>
/// </summary>
internal class DisableModelsBuilderNotificationHandler : INotificationHandler<UmbracoApplicationStartingNotification>
{
    private readonly UmbracoFeatures _features;

    public DisableModelsBuilderNotificationHandler(UmbracoFeatures features) => _features = features;

    /// <summary>
    ///     Handles the <see cref="UmbracoApplicationStartingNotification" /> notification to disable MB controller features
    /// </summary>
    public void Handle(UmbracoApplicationStartingNotification notification) =>
        // disable the embedded dashboard controller
        _features.Disabled.Controllers.Add<ModelsBuilderDashboardController>();
}
