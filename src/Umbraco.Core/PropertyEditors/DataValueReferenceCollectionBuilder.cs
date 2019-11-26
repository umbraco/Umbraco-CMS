using Umbraco.Core.Composing;

namespace Umbraco.Core.PropertyEditors
{
    public class DataValueReferenceCollectionBuilder : LazyCollectionBuilderBase<DataValueReferenceCollectionBuilder, DataValueReferenceCollection, IDataValueReference>
    {
        protected override DataValueReferenceCollectionBuilder This => this;
    }
}
