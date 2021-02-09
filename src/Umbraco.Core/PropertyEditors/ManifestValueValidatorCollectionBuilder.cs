using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.PropertyEditors
{
    public class ManifestValueValidatorCollectionBuilder : LazyCollectionBuilderBase<ManifestValueValidatorCollectionBuilder, ManifestValueValidatorCollection, IManifestValueValidator>
    {
        protected override ManifestValueValidatorCollectionBuilder This => this;
    }
}
