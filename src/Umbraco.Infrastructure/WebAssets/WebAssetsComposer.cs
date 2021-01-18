using Umbraco.Core.DependencyInjection;
using Umbraco.Core.Composing;

namespace Umbraco.Web.WebAssets
{
    public sealed class WebAssetsComposer : ComponentComposer<WebAssetsComponent>
    {
        public override void Compose(IUmbracoBuilder builder)
        {
            base.Compose(builder);
            builder.Services.AddUnique<BackOfficeWebAssets>();
        }
    }
}
