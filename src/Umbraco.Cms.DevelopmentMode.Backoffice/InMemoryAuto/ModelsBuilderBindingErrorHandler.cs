using System.Reflection;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Notifications;

namespace Umbraco.Cms.DevelopmentMode.Backoffice.InMemoryAuto;

internal sealed class ModelsBuilderBindingErrorHandler : INotificationHandler<ModelBindingErrorNotification>
{
    /// <summary>
    ///     Handles when a model binding error occurs.
    /// </summary>
    public void Handle(ModelBindingErrorNotification notification)
    {
        ModelsBuilderAssemblyAttribute? sourceAttr =
            notification.SourceType.Assembly.GetCustomAttribute<ModelsBuilderAssemblyAttribute>();
        ModelsBuilderAssemblyAttribute? modelAttr =
            notification.ModelType.Assembly.GetCustomAttribute<ModelsBuilderAssemblyAttribute>();

        // if source or model is not a ModelsBuider type...
        if (sourceAttr == null || modelAttr == null)
        {
            // if neither are ModelsBuilder types, give up entirely
            if (sourceAttr == null && modelAttr == null)
            {
                return;
            }

            // else report, but better not restart (loops?)
            notification.Message.Append(" The ");
            notification.Message.Append(sourceAttr == null ? "view model" : "source");
            notification.Message.Append(" is a ModelsBuilder type, but the ");
            notification.Message.Append(sourceAttr != null ? "view model" : "source");
            notification.Message.Append(" is not. The application is in an unstable state and should be restarted.");
            return;
        }

        // both are ModelsBuilder types
        var pureSource = sourceAttr.IsInMemory;
        var pureModel = modelAttr.IsInMemory;

        if (sourceAttr.IsInMemory || modelAttr.IsInMemory)
        {
            if (pureSource == false || pureModel == false)
            {
                // only one is pure - report, but better not restart (loops?)
                notification.Message.Append(pureSource
                    ? " The content model is in memory generated, but the view model is not."
                    : " The view model is in memory generated, but the content model is not.");
                notification.Message.Append(" The application is in an unstable state and should be restarted.");
            }
            else
            {
                // both are pure - report, and if different versions, restart
                // if same version... makes no sense... and better not restart (loops?)
                Version? sourceVersion = notification.SourceType.Assembly.GetName().Version;
                Version? modelVersion = notification.ModelType.Assembly.GetName().Version;
                notification.Message.Append(" Both view and content models are in memory generated, with ");
                notification.Message.Append(sourceVersion == modelVersion
                    ? "same version. The application is in an unstable state and should be restarted."
                    : "different versions. The application is in an unstable state and should be restarted.");
            }
        }
    }
}
