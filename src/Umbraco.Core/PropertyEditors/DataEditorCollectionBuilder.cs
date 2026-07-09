using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Represents a builder for the <see cref="DataEditorCollection"/>.
/// </summary>
public class
    DataEditorCollectionBuilder : LazyCollectionBuilderBase<DataEditorCollectionBuilder, DataEditorCollection, IDataEditor>
{
    /// <inheritdoc />
    protected override DataEditorCollectionBuilder This => this;
}
