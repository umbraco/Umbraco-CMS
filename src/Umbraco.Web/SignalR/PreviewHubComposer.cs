using Microsoft.AspNet.SignalR;
using Umbraco.Core;
using Umbraco.Core.Components;

namespace Umbraco.Web.SignalR
{
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    public class PreviewHubComposer : ComponentComposer<PreviewHubComponent>, ICoreComposer
    {
        public override void Compose(Composition composition)
        {
            base.Compose(composition);

            composition.RegisterUnique(_ => GlobalHost.ConnectionManager.GetHubContext<PreviewHub, IPreviewHub>());
        }
    }
}
