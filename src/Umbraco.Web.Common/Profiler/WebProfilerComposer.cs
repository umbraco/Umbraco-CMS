using Umbraco.Core;
using Umbraco.Core.Composing;

namespace Umbraco.Web.Common.Profiler
{
    internal class WebProfilerComposer : ComponentComposer<WebProfilerComponent>, ICoreComposer
    {
        public override void Compose(Composition composition)
        {
            base.Compose(composition);

            composition.Services.AddUnique<WebProfilerHtml>();
        }
    }
}
