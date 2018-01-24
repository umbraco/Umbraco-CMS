using LightInject;
using Umbraco.Core.Composing;

namespace Umbraco.Core.PropertyEditors
{
    internal class ManifestValidatorCollectionBuilder : LazyCollectionBuilderBase<ManifestValidatorCollectionBuilder, ManifestValidatorCollection, ManifestValidator>
    {
        public ManifestValidatorCollectionBuilder(IServiceContainer container)
            : base(container)
        { }

        protected override ManifestValidatorCollectionBuilder This => this;
    }
}
