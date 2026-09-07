using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Search.Core.Models.Indexing;

namespace Umbraco.Cms.Search.Core.PropertyValueHandlers;

/// <summary>
/// Fallback handler for property editors that are not (yet) indexed. Always yields no index fields.
/// </summary>
internal sealed class NoopPropertyValueHandler : IPropertyValueHandler, ICorePropertyValueHandler
{
    /// <inheritdoc />
    public bool CanHandle(IPropertyType propertyType)
        => propertyType.PropertyEditorAlias is Cms.Core.Constants.PropertyEditors.Aliases.EmailAddress
            or Cms.Core.Constants.PropertyEditors.Aliases.ColorPicker
            or Cms.Core.Constants.PropertyEditors.Aliases.ColorPickerEyeDropper
            or Cms.Core.Constants.PropertyEditors.Aliases.MediaPicker3
            or Cms.Core.Constants.PropertyEditors.Aliases.ImageCropper
            or Cms.Core.Constants.PropertyEditors.Aliases.UploadField;

    /// <inheritdoc />
    public IEnumerable<IndexField> GetIndexFields(IProperty property, string? culture, string? segment, bool published, IContentBase contentContext)
        => [];
}
