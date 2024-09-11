using Umbraco.Cms.Core.Models.Blocks;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Models Intermediate Value for Rich Text Editors Property Value Converter.
/// </summary>
public interface IRichTextEditorIntermediateValue
{
    public string Markup { get; }

    public RichTextBlockModel? RichTextBlockModel { get; }
}
