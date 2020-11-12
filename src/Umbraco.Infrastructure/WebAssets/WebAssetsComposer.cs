using Umbraco.Core;
using Umbraco.Core.Composing;

namespace Umbraco.Web.WebAssets
{
    public sealed class WebAssetsComposer : ComponentComposer<WebAssetsComponent>
    {
        public override void Compose(Composition composition)
        {
            base.Compose(composition);
            composition.Services.AddUnique<BackOfficeWebAssets>();
        }
    }
}
