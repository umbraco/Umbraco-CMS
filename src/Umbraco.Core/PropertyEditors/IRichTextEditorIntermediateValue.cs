using Umbraco.Cms.Core.Models.Blocks;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Models the intermediate value for Rich Text Editor property value converters.
/// </summary>
public interface IRichTextEditorIntermediateValue
{
    /// <summary>
    ///     Gets the HTML markup content of the rich text value.
    /// </summary>
    public string Markup { get; }

    /// <summary>
    ///     Gets the block model associated with the rich text value.
    /// </summary>
    /// <remarks>
    ///     Can be <c>null</c> if no blocks are embedded in the rich text content.
    /// </remarks>
    public RichTextBlockModel? RichTextBlockModel { get; }
}
