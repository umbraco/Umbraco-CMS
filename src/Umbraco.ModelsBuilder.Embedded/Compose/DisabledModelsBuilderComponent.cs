using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.ModelsBuilder.Embedded.BackOffice;
using Umbraco.Web.Features;

namespace Umbraco.ModelsBuilder.Embedded.Compose
{
    /// <summary>
    /// Special component used for when MB is disabled with the legacy MB is detected
    /// </summary>
    public sealed class DisabledModelsBuilderComponent : IComponent
    {
        private readonly UmbracoFeatures _features;

        public DisabledModelsBuilderComponent(UmbracoFeatures features)
        {
            _features = features;
        }

        public void Initialize()
        {
            //disable the embedded dashboard controller
            _features.Disabled.Controllers.Add<ModelsBuilderDashboardController>();
        }

        public void Terminate()
        {
        }
    }
}
