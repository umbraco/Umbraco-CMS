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
///     Content property editor that stores UDI.
/// </summary>
[DataEditor(
    Constants.PropertyEditors.Aliases.ContentPicker,
    ValueType = ValueTypes.String,
    ValueEditorIsReusable = true)]
public class ContentPickerPropertyEditor : DataEditor
{
    private readonly IIOHelper _ioHelper;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentPickerPropertyEditor" /> class.
    /// </summary>
    /// <param name="dataValueEditorFactory">The data value editor factory.</param>
    /// <param name="ioHelper">The IO helper.</param>
    public ContentPickerPropertyEditor(IDataValueEditorFactory dataValueEditorFactory, IIOHelper ioHelper)
        : base(dataValueEditorFactory)
    {
        _ioHelper = ioHelper;
        SupportsReadOnly = true;
    }

    /// <inheritdoc />
    protected override IConfigurationEditor CreateConfigurationEditor() =>
        new ContentPickerConfigurationEditor(_ioHelper);

    /// <inheritdoc />
    protected override IDataValueEditor CreateValueEditor() =>
        DataValueEditorFactory.Create<ContentPickerPropertyValueEditor>(Attribute!);

    /// <summary>
    ///     Provides the value editor for the content picker property editor.
    /// </summary>
    internal sealed class ContentPickerPropertyValueEditor : DataValueEditor, IDataValueReference
    {
        /// <summary>
        ///     Initializes a new instance of the <see cref="ContentPickerPropertyValueEditor" /> class.
        /// </summary>
        /// <param name="shortStringHelper">The short string helper.</param>
        /// <param name="jsonSerializer">The JSON serializer.</param>
        /// <param name="ioHelper">The IO helper.</param>
        /// <param name="attribute">The data editor attribute.</param>
        public ContentPickerPropertyValueEditor(
            IShortStringHelper shortStringHelper,
            IJsonSerializer jsonSerializer,
            IIOHelper ioHelper,
            DataEditorAttribute attribute)
            : base(shortStringHelper, jsonSerializer, ioHelper, attribute)
        {
        }

        /// <inheritdoc />
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

        /// <inheritdoc />
        /// <remarks>
        /// Starting in v14, the passed in value is always a GUID. The value is stored as a document UDI string.
        /// </remarks>
        public override object? FromEditor(ContentPropertyData editorValue, object? currentValue) =>
            editorValue.Value is not null
            && Guid.TryParse(editorValue.Value.ToString(), out Guid guidValue)
                ? GuidUdi.Create(Constants.UdiEntityType.Document, guidValue).ToString()
                : null;

        /// <inheritdoc />
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
