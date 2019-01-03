using Microsoft.AspNet.SignalR;
using Umbraco.Core;
using Umbraco.Core.Components;

namespace Umbraco.Web.SignalR
{
    [RuntimeLevel(MinLevel = RuntimeLevel.Run)]
    public class PreviewHubComposer : ICoreComposer
    {
        public void Compose(Composition composition)
        {
            composition.Components().Append<PreviewHubComponent>();
            composition.RegisterUnique(_ => GlobalHost.ConnectionManager.GetHubContext<PreviewHub, IPreviewHub>());
        }
    }
}
