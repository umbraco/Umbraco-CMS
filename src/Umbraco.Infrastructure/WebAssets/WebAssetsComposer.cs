using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

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
