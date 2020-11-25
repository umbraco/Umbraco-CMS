using Umbraco.Core;
using Umbraco.Core.Composing;

namespace Umbraco.Web.BackOffice.SignalR
{
    public class PreviewHubComposer : ComponentComposer<PreviewHubComponent>, ICoreComposer
    {
        public override void Compose(Composition composition)
        {
            base.Compose(composition);
        }
    }
}
