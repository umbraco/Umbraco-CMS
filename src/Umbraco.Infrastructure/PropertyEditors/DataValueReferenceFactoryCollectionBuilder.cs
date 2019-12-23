using Umbraco.Core.Composing;

namespace Umbraco.Core.PropertyEditors
{
    public class DataValueReferenceFactoryCollectionBuilder : OrderedCollectionBuilderBase<DataValueReferenceFactoryCollectionBuilder, DataValueReferenceFactoryCollection, IDataValueReferenceFactory>
    {
        protected override DataValueReferenceFactoryCollectionBuilder This => this;
    }
}
