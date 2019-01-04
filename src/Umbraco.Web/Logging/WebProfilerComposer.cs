using Umbraco.Core.Components;

namespace Umbraco.Web.Logging
{
    internal class WebProfilerComposer : ICoreComposer
    {
        public void Compose(Composition composition)
        {
            composition.Components().Append<WebProfilerComponent>();
        }
    }
}