using Umbraco.Core;
using Umbraco.Core.Composing;

namespace Umbraco.Web.Unversion
{
    public class UnversionComposer : ICoreComposer
    {
        public void Compose(Composition composition)
        {
            composition.Register<IUnversionConfig, UnversionConfig>();
            composition.Register<IUnversionService, UnversionService>();
            composition.Components().Append<UnversionComponent>();
        }
    }
}
