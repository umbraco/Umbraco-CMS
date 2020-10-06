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
        "Multi URL Picker",
        "multiurlpicker",
        ValueType = ValueTypes.Json,
        Group = Constants.PropertyEditors.Groups.Pickers,
        Icon = "icon-link")]
    public class MultiUrlPickerPropertyEditor : DataEditor
    {
        private readonly IEntityService _entityService;
        private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;

        public MultiUrlPickerPropertyEditor(ILogger logger, IEntityService entityService, IPublishedSnapshotAccessor publishedSnapshotAccessor) : base(logger, EditorType.PropertyValue)
        {
            _entityService = entityService ?? throw new ArgumentNullException(nameof(entityService));
            _publishedSnapshotAccessor = publishedSnapshotAccessor ?? throw new ArgumentNullException(nameof(publishedSnapshotAccessor));
        }
        
        protected override IConfigurationEditor CreateConfigurationEditor() => new MultiUrlPickerConfigurationEditor();

        protected override IDataValueEditor CreateValueEditor() => new MultiUrlPickerValueEditor(_entityService, _publishedSnapshotAccessor, Logger, Attribute);
    }
}
