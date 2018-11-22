using Umbraco.Core.Components;
using Umbraco.Core.Composing;

namespace Umbraco.Core.Logging
{
    public sealed class LoggingComponent : UmbracoComponentBase
    {
        public override void Compose(Composition composition)
        {
            var container = composition.Container;

            container.RegisterSingleton<IProfiler, LogProfiler>();
            container.RegisterSingleton<ProfilingLogger>();
        }
    }
}
