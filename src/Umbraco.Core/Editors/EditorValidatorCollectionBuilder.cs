using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.Editors;

/// <summary>
/// Builds the <see cref="EditorValidatorCollection"/> using lazy initialization.
/// </summary>
/// <remarks>
/// Use this builder during composition to register <see cref="IEditorValidator"/> implementations.
/// </remarks>
public class EditorValidatorCollectionBuilder : LazyCollectionBuilderBase<EditorValidatorCollectionBuilder,
    EditorValidatorCollection, IEditorValidator>
{
    /// <inheritdoc />
    protected override EditorValidatorCollectionBuilder This => this;
}
