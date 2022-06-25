using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;
using Umbraco.Cms.Web.Common.DependencyInjection;

namespace Umbraco.Cms.Core.PropertyEditors
{
    /// <summary>
    /// Represents a OEmbed picker property editor.
    /// </summary>
    [DataEditor(
        Constants.PropertyEditors.Aliases.OEmbedPicker,
        EditorType.PropertyValue,
        "OEmbed Picker",
        "oembedpicker",
        ValueType = ValueTypes.Json,
        Group = Constants.PropertyEditors.Groups.Media,
        Icon = Constants.Icons.MediaVideo)]
    public class OEmbedPickerPropertyEditor : DataEditor
    {
        private readonly IIOHelper _ioHelper;
        private readonly IEditorConfigurationParser _editorConfigurationParser;

        // Scheduled for removal in v12
        [Obsolete("Please use constructor that takes an IEditorConfigurationParser instead")]
        public OEmbedPickerPropertyEditor(
            IDataValueEditorFactory dataValueEditorFactory,
            IIOHelper ioHelper,
            EditorType type = EditorType.PropertyValue)
            : this(dataValueEditorFactory, ioHelper, StaticServiceProvider.Instance.GetRequiredService<IEditorConfigurationParser>(), type)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OEmbedPickerPropertyEditor" /> class.
        /// </summary>
        public OEmbedPickerPropertyEditor(
            IDataValueEditorFactory dataValueEditorFactory,
            IIOHelper ioHelper,
            IEditorConfigurationParser editorConfigurationParser,
            EditorType type = EditorType.PropertyValue)
            : base(dataValueEditorFactory, type)
        {
            _ioHelper = ioHelper;
            _editorConfigurationParser = editorConfigurationParser;
        }

        /// <inheritdoc />
        protected override IConfigurationEditor CreateConfigurationEditor() =>
            new OEmbedPickerConfigurationEditor(_ioHelper, _editorConfigurationParser);

        /// <inheritdoc />
        protected override IDataValueEditor CreateValueEditor() =>
            DataValueEditorFactory.Create<OEmbedPickerPropertyValueEditor>(Attribute!);

        internal class OEmbedPickerPropertyValueEditor : DataValueEditor
        {
            private readonly IJsonSerializer _jsonSerializer;
            private readonly IDataTypeService _dataTypeService;

            public OEmbedPickerPropertyValueEditor(
                ILocalizedTextService localizedTextService,
                IShortStringHelper shortStringHelper,
                IJsonSerializer jsonSerializer,
                IIOHelper ioHelper,
                DataEditorAttribute attribute,
                IDataTypeService dataTypeService)
                : base(localizedTextService, shortStringHelper, jsonSerializer, ioHelper, attribute)
            {
                _jsonSerializer = jsonSerializer;
                _dataTypeService = dataTypeService;
            }
        }
    }
}
