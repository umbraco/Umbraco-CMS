using System;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;
using Umbraco.Web.PublishedCache;

namespace Umbraco.Web.PropertyEditors
{
    [DataEditor(Constants.PropertyEditors.Aliases.MultiUrlPicker, EditorType.PropertyValue|EditorType.MacroParameter, "Multi Url Picker", "multiurlpicker", ValueType = "JSON", Group = "pickers", Icon = "icon-link")]
    public class MultiUrlPickerPropertyEditor : DataEditor
    {
        private readonly IEntityService _entityService;
        private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;

        public MultiUrlPickerPropertyEditor(ILogger logger, IEntityService entityService, IPublishedSnapshotAccessor publishedSnapshotAccessor) : base(logger, EditorType.PropertyValue|EditorType.MacroParameter)
        {
            _entityService = entityService ?? throw new ArgumentNullException(nameof(entityService));
            _publishedSnapshotAccessor = publishedSnapshotAccessor ?? throw new ArgumentNullException(nameof(publishedSnapshotAccessor));
        }

        protected override IConfigurationEditor CreateConfigurationEditor() => new MultiUrlPickerConfigurationEditor();

        protected override IDataValueEditor CreateValueEditor() => new MultiUrlPickerValueEditor(_entityService, _publishedSnapshotAccessor, Logger, Attribute);
    }
}
