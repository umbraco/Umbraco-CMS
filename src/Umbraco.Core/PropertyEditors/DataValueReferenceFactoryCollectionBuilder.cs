using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.PropertyEditors;

public class DataValueReferenceFactoryCollectionBuilder : OrderedCollectionBuilderBase<DataValueReferenceFactoryCollectionBuilder, DataValueReferenceFactoryCollection, IDataValueReferenceFactory>
{
    protected override DataValueReferenceFactoryCollectionBuilder This => this;
}
