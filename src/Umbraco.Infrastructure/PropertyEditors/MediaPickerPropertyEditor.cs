// Copyright (c) Umbraco.
// See LICENSE for more details.

using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Web.Common.DependencyInjection;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents a media picker property editor.
/// </summary>
/// <remarks>
///     Named "(legacy)" as it's best to use the NEW Media Picker aka MediaPicker3
/// </remarks>
[DataEditor(
    Constants.PropertyEditors.Aliases.MediaPicker,
    EditorType.PropertyValue | EditorType.MacroParameter,
    "Media Picker (legacy)",
    "mediapicker",
    ValueType = ValueTypes.Text,
    Group = Constants.PropertyEditors.Groups.Media,
    Icon = Constants.Icons.MediaImage,
    IsDeprecated = false,
    ValueEditorIsReusable = true)]
public class MediaPickerPropertyEditor : DataEditor
{
    private readonly IEditorConfigurationParser _editorConfigurationParser;
    private readonly IIOHelper _ioHelper;

    // Scheduled for removal in v12
    [Obsolete("Please use constructor that takes an IEditorConfigurationParser instead")]
    public MediaPickerPropertyEditor(
        IDataValueEditorFactory dataValueEditorFactory,
        IIOHelper ioHelper)
        : this(dataValueEditorFactory, ioHelper, StaticServiceProvider.Instance.GetRequiredService<IEditorConfigurationParser>())
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="MediaPickerPropertyEditor" /> class.
    /// </summary>
    public MediaPickerPropertyEditor(
        IDataValueEditorFactory dataValueEditorFactory,
        IIOHelper ioHelper,
        IEditorConfigurationParser editorConfigurationParser)
        : base(dataValueEditorFactory)
    {
        _ioHelper = ioHelper;
        _editorConfigurationParser = editorConfigurationParser;
        SupportsReadOnly = true;
    }

    /// <inheritdoc />
    protected override IConfigurationEditor CreateConfigurationEditor() =>
        new MediaPickerConfigurationEditor(_ioHelper, _editorConfigurationParser);

    protected override IDataValueEditor CreateValueEditor() =>
        DataValueEditorFactory.Create<MediaPickerPropertyValueEditor>(Attribute!);

    public class MediaPickerPropertyValueEditor : DataValueEditor, IDataValueReference
    {
        public MediaPickerPropertyValueEditor(
            ILocalizedTextService localizedTextService,
            IShortStringHelper shortStringHelper,
            IJsonSerializer jsonSerializer,
            IIOHelper ioHelper,
            DataEditorAttribute attribute)
            : base(localizedTextService, shortStringHelper, jsonSerializer, ioHelper, attribute) =>
            SupportsReadOnly = true;

        public IEnumerable<UmbracoEntityReference> GetReferences(object? value)
        {
            var asString = value is string str ? str : value?.ToString();

            if (string.IsNullOrEmpty(asString))
            {
                yield break;
            }

            foreach (var udiStr in asString.Split(','))
            {
                if (UdiParser.TryParse(udiStr, out Udi? udi))
                {
                    yield return new UmbracoEntityReference(udi);
                }
            }
        }
    }
}
