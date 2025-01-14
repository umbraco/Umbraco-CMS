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
using Umbraco.Extensions;

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

        // starting in v14 the passed in value is always a guid, we store it as a document Udi string. Else it's an invalid value
        public override object? FromEditor(ContentPropertyData editorValue, object? currentValue) =>
            editorValue.Value is not null
            && Guid.TryParse(editorValue.Value.ToString(), out Guid guidValue)
                ? GuidUdi.Create(Constants.UdiEntityType.Document, guidValue).ToString()
                : null;

        public override object? ToEditor(IProperty property, string? culture = null, string? segment = null)
        {
            // since our storage type is a string, we can expect the base to return a string
            var stringValue = base.ToEditor(property, culture, segment) as string;

            if (stringValue.IsNullOrWhiteSpace())
            {
                return null;
            }

            // this string can actually be an Int value from old versions => convert to it's guid counterpart
            if (int.TryParse(stringValue, out var oldInt))
            {
                // todo: This is a temporary code path that should be removed ASAP
                Attempt<Guid> conversionAttempt = StaticServiceProvider.Instance.GetRequiredService<IIdKeyMap>()
                    .GetKeyForId(oldInt, UmbracoObjectTypes.Document);
                return conversionAttempt.Success ? conversionAttempt.Result : null;
            }

            // if its not an old value, it should be a udi
            if (UdiParser.TryParse(stringValue, out GuidUdi? guidUdi) is false)
            {
                return null;
            }

            return guidUdi.Guid;
        }
    }
}
