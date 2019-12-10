using System;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;
using Umbraco.Web.PublishedCache;
using System.Collections.Generic;
using Umbraco.Core.Models.Editors;
using Newtonsoft.Json;

namespace Umbraco.Web.PropertyEditors
{
    [DataEditor(
        Constants.PropertyEditors.Aliases.MultiUrlPicker,
        EditorType.PropertyValue,
        "Multi Url Picker",
        "multiurlpicker",
        ValueType = ValueTypes.Json,
        Group = Constants.PropertyEditors.Groups.Pickers,
        Icon = "icon-link")]
    public class MultiUrlPickerPropertyEditor : DataEditor
    {
        private readonly IEntityService _entityService;
        private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;
        private readonly IDataTypeService _dataTypeService;
        private readonly ILocalizationService _localizationService;

        public MultiUrlPickerPropertyEditor(ILogger logger, IEntityService entityService, IPublishedSnapshotAccessor publishedSnapshotAccessor, IDataTypeService dataTypeService, ILocalizationService localizationService) : base(logger, EditorType.PropertyValue)
        {
            _entityService = entityService ?? throw new ArgumentNullException(nameof(entityService));
            _publishedSnapshotAccessor = publishedSnapshotAccessor ?? throw new ArgumentNullException(nameof(publishedSnapshotAccessor));
            _dataTypeService = dataTypeService;
            _localizationService = localizationService;
        }
        
        protected override IConfigurationEditor CreateConfigurationEditor() => new MultiUrlPickerConfigurationEditor();

        protected override IDataValueEditor CreateValueEditor() => new MultiUrlPickerValueEditor(_entityService, _publishedSnapshotAccessor, Logger, _dataTypeService, _localizationService, Attribute);
    }
}
