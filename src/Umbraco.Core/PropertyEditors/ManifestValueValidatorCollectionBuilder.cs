using Umbraco.Core.Composing;

namespace Umbraco.Core.PropertyEditors
{
    internal class ManifestValueValidatorCollectionBuilder : LazyCollectionBuilderBase<ManifestValueValidatorCollectionBuilder, ManifestValueValidatorCollection, IManifestValueValidator>
    {
        protected override ManifestValueValidatorCollectionBuilder This => this;
    }
}
