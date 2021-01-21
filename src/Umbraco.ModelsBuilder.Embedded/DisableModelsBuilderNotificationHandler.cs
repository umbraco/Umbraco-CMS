using System.Threading;
using System.Threading.Tasks;
using Umbraco.Core.Events;
using Umbraco.ModelsBuilder.Embedded.BackOffice;
using Umbraco.ModelsBuilder.Embedded.DependencyInjection;
using Umbraco.Web.Features;

namespace Umbraco.ModelsBuilder.Embedded
{
    /// <summary>
    /// Used in conjunction with <see cref="UmbracoBuilderExtensions.DisableModelsBuilderControllers"/>
    /// </summary>
    internal class DisableModelsBuilderNotificationHandler : INotificationHandler<UmbracoApplicationStarting>
    {
        private readonly UmbracoFeatures _features;

        public DisableModelsBuilderNotificationHandler(UmbracoFeatures features) => _features = features;

        /// <summary>
        /// Handles the <see cref="UmbracoApplicationStarting"/> notification to disable MB controller features
        /// </summary>
        public Task HandleAsync(UmbracoApplicationStarting notification, CancellationToken cancellationToken)
        {
            // disable the embedded dashboard controller
            _features.Disabled.Controllers.Add<ModelsBuilderDashboardController>();
            return Task.CompletedTask;
        }
    }
}
