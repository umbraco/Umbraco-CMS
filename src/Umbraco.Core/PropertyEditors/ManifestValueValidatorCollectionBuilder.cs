using Umbraco.Core.CodeAnnotations;
using Umbraco.Core.Composing;

namespace Umbraco.Core.PropertyEditors
{
    [UmbracoVolatile]
    public class ManifestValueValidatorCollectionBuilder : LazyCollectionBuilderBase<ManifestValueValidatorCollectionBuilder, ManifestValueValidatorCollection, IManifestValueValidator>
    {
        protected override ManifestValueValidatorCollectionBuilder This => this;
    }
}
