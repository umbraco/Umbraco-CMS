using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

public abstract class ValueTypePropertyValueConverterBase : PropertyValueConverterBase
{
    private readonly PropertyEditorCollection _propertyEditors;

    protected abstract string[] SupportedValueTypes { get; }

    protected ValueTypePropertyValueConverterBase(PropertyEditorCollection propertyEditors)
        => _propertyEditors = propertyEditors;

    public override bool IsConverter(IPublishedPropertyType propertyType)
        => _propertyEditors.TryGet(propertyType.EditorAlias, out IDataEditor? editor)
           && SupportedValueTypes.InvariantContains(editor.GetValueEditor().ValueType);
}
