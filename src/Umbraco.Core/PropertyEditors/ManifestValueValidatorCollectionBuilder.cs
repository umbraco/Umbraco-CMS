using Umbraco.Core.Composing;

namespace Umbraco.Core.PropertyEditors
{
    public class ManifestValueValidatorCollectionBuilder : LazyCollectionBuilderBase<ManifestValueValidatorCollectionBuilder, ManifestValueValidatorCollection, IManifestValueValidator>
    {
        protected override ManifestValueValidatorCollectionBuilder This => this;
    }
}
