using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Web.BackOffice.SignalR;

namespace Umbraco.Web.SignalR
{
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    public class PreviewHubComposer : ComponentComposer<PreviewHubComponent>, ICoreComposer
    {
        public override void Compose(Composition composition)
        {
            base.Compose(composition);
        }
    }
}
