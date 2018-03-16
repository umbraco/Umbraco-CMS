using LightInject;
using Umbraco.Core.Composing;

namespace Umbraco.Core.PropertyEditors
{
    internal class ManifestValueValidatorCollectionBuilder : LazyCollectionBuilderBase<ManifestValueValidatorCollectionBuilder, ManifestValueValidatorCollection, IManifestValueValidator>
    {
        public ManifestValueValidatorCollectionBuilder(IServiceContainer container)
            : base(container)
        { }

        protected override ManifestValueValidatorCollectionBuilder This => this;
    }
}
