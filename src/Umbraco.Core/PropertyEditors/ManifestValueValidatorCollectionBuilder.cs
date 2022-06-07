using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.PropertyEditors;

public class ManifestValueValidatorCollectionBuilder : SetCollectionBuilderBase<ManifestValueValidatorCollectionBuilder, ManifestValueValidatorCollection, IManifestValueValidator>
{
    protected override ManifestValueValidatorCollectionBuilder This => this;
}
