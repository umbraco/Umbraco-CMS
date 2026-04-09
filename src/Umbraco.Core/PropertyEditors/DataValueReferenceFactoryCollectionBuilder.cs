using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Represents a builder for the <see cref="DataValueReferenceFactoryCollection"/>.
/// </summary>
public class DataValueReferenceFactoryCollectionBuilder : OrderedCollectionBuilderBase<DataValueReferenceFactoryCollectionBuilder, DataValueReferenceFactoryCollection, IDataValueReferenceFactory>
{
    /// <inheritdoc />
    protected override DataValueReferenceFactoryCollectionBuilder This => this;
}
