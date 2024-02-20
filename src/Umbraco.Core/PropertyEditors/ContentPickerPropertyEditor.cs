// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Content property editor that stores UDI
/// </summary>
[DataEditor(
    Constants.PropertyEditors.Aliases.ContentPicker,
    ValueType = ValueTypes.String,
    ValueEditorIsReusable = true)]
public class ContentPickerPropertyEditor : DataEditor
{
    private readonly IIOHelper _ioHelper;

    public ContentPickerPropertyEditor(IDataValueEditorFactory dataValueEditorFactory, IIOHelper ioHelper)
        : base(dataValueEditorFactory)
    {
        _ioHelper = ioHelper;
        SupportsReadOnly = true;
    }

    protected override IConfigurationEditor CreateConfigurationEditor() =>
        new ContentPickerConfigurationEditor(_ioHelper);

    protected override IDataValueEditor CreateValueEditor() =>
        DataValueEditorFactory.Create<ContentPickerPropertyValueEditor>(Attribute!);

    internal class ContentPickerPropertyValueEditor : DataValueEditor, IDataValueReference
    {
        public ContentPickerPropertyValueEditor(
            IShortStringHelper shortStringHelper,
            IJsonSerializer jsonSerializer,
            IIOHelper ioHelper,
            DataEditorAttribute attribute)
            : base(shortStringHelper, jsonSerializer, ioHelper, attribute)
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
    }
}
