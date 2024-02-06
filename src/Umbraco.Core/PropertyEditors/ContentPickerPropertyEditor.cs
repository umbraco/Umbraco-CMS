// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Content property editor that stores UDI
/// </summary>
[DataEditor(
    Constants.PropertyEditors.Aliases.ContentPicker,
    EditorType.PropertyValue | EditorType.MacroParameter,
    "Content Picker",
    "contentpicker",
    ValueType = ValueTypes.String,
    Group = Constants.PropertyEditors.Groups.Pickers,
    ValueEditorIsReusable = true)]
public class ContentPickerPropertyEditor : DataEditor
{
    private readonly IEditorConfigurationParser _editorConfigurationParser;
    private readonly IIOHelper _ioHelper;

    // Scheduled for removal in v12
    [Obsolete("Please use constructor that takes an IEditorConfigurationParser instead")]
    public ContentPickerPropertyEditor(
        IDataValueEditorFactory dataValueEditorFactory,
        IIOHelper ioHelper)
        : this(dataValueEditorFactory, ioHelper, StaticServiceProvider.Instance.GetRequiredService<IEditorConfigurationParser>())
    {
    }

    public ContentPickerPropertyEditor(
        IDataValueEditorFactory dataValueEditorFactory,
        IIOHelper ioHelper,
        IEditorConfigurationParser editorConfigurationParser)
        : base(dataValueEditorFactory)
    {
        _ioHelper = ioHelper;
        _editorConfigurationParser = editorConfigurationParser;
        SupportsReadOnly = true;
    }

    protected override IConfigurationEditor CreateConfigurationEditor() =>
        new ContentPickerConfigurationEditor(_ioHelper, _editorConfigurationParser);

    protected override IDataValueEditor CreateValueEditor() =>
        DataValueEditorFactory.Create<ContentPickerPropertyValueEditor>(Attribute!);

    internal class ContentPickerPropertyValueEditor : DataValueEditor, IDataValueReference
    {
        public ContentPickerPropertyValueEditor(
            ILocalizedTextService localizedTextService,
            IShortStringHelper shortStringHelper,
            IJsonSerializer jsonSerializer,
            IIOHelper ioHelper,
            DataEditorAttribute attribute)
            : base(localizedTextService, shortStringHelper, jsonSerializer, ioHelper, attribute)
        {
        }

        public IEnumerable<UmbracoEntityReference> GetReferences(object? value)
        {
            var asString = value is string str ? str : value?.ToString();

            if (string.IsNullOrEmpty(asString))
            {
                yield break;
            }

            if (UdiParser.TryParse(asString, out Udi? udi))
            {
                yield return new UmbracoEntityReference(udi);
            }
        }

        public override object? FromEditor(ContentPropertyData editorValue, object? currentValue)
        {
            if (editorValue.Value is null)
            {
                return null;
            }

            // starting in v14 the passed in value is always a guid, we store it as a document Udi string. Else it's an invalid value
            return Guid.TryParse(editorValue.Value as string, out Guid guidValue)
                ? GuidUdi.Create(Constants.UdiEntityType.Document, guidValue).ToString()
                : null;
        }

        public override object? ToEditor(IProperty property, string? culture = null, string? segment = null)
        {
            // since our storage type is a string, we can expect the base to return a string
            var stringValue = base.ToEditor(property, culture, segment) as string;

            // this string can actually be an Int value from old versions => convert to it's guid counterpart
            if (int.TryParse(stringValue, out var oldInt))
            {
                // This is a temporary code path that should be removed ASAP
                Attempt<Guid> conversionAttempt = StaticServiceProvider.Instance.GetRequiredService<IIdKeyMap>()
                    .GetKeyForId(oldInt, UmbracoObjectTypes.Document);
                return conversionAttempt.Success ? conversionAttempt.Result : null;
            }

            return Guid.TryParse(stringValue, out Guid guid) ? guid : null;
        }
    }
}
