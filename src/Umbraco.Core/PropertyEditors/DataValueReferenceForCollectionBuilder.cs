using Umbraco.Core.Composing;

namespace Umbraco.Core.PropertyEditors
{
    public class DataValueReferenceForCollectionBuilder : OrderedCollectionBuilderBase<DataValueReferenceForCollectionBuilder, DataValueReferenceForCollection, IDataValueReferenceFor>
    {
        protected override DataValueReferenceForCollectionBuilder This => this;         
    }
}
