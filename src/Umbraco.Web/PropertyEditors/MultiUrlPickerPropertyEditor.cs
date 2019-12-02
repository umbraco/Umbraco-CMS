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
    public class MultiUrlPickerPropertyEditor : DataEditor, IDataValueReference
    {
        private readonly IEntityService _entityService;
        private readonly IPublishedSnapshotAccessor _publishedSnapshotAccessor;

        public MultiUrlPickerPropertyEditor(ILogger logger, IEntityService entityService, IPublishedSnapshotAccessor publishedSnapshotAccessor) : base(logger, EditorType.PropertyValue)
        {
            _entityService = entityService ?? throw new ArgumentNullException(nameof(entityService));
            _publishedSnapshotAccessor = publishedSnapshotAccessor ?? throw new ArgumentNullException(nameof(publishedSnapshotAccessor));
        }
        
        public IEnumerable<UmbracoEntityReference> GetReferences(object value)
        {
            var asString = value == null ? string.Empty : value is string str ? str : value.ToString();

            if (string.IsNullOrEmpty(asString)) yield break;

            var links = JsonConvert.DeserializeObject<List<MultiUrlPickerValueEditor.LinkDto>>(asString);
            foreach (var link in links)
                if (link.Udi != null) // Links can be absolute links without a Udi
                    yield return new UmbracoEntityReference(link.Udi);
        }

        protected override IConfigurationEditor CreateConfigurationEditor() => new MultiUrlPickerConfigurationEditor();

        protected override IDataValueEditor CreateValueEditor() => new MultiUrlPickerValueEditor(_entityService, _publishedSnapshotAccessor, Logger, Attribute);
    }
}
