using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Web.Composing;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// A custom value editor for any property editor that stores a pre-value int id so that we can ensure that the 'value' not the ID get's put into cache
    /// </summary>
    /// <remarks>
    /// This is required for legacy/backwards compatibility, otherwise we'd just store the string version and cache the string version without
    /// needing additional lookups.
    /// </remarks>
    internal class PublishValueValueEditor : DataValueEditor
    {
        private readonly ILogger _logger;

        internal PublishValueValueEditor(DataEditorAttribute attribute, ILogger logger)
            : base(attribute)
        {
            _logger = logger;
        }

        /// <inheritdoc />
        public override string ConvertDbToString(PropertyType propertyType, object value, IDataTypeService dataTypeService)
        {
            if (value == null)
                return null;

            // get the configuration items
            // if none, fallback to base
            var configuration = dataTypeService.GetDataType(propertyType.DataTypeId).ConfigurationAs<ValueListConfiguration>();
            if (configuration == null)
                return base.ConvertDbToString(propertyType, value, dataTypeService);

            var items = configuration.Items;

            var idAttempt = value.TryConvertTo<int>();
            if (idAttempt.Success)
            {
                var itemId = idAttempt.Result;
                var item = items.FirstOrDefault(x => x.Id == itemId);
                if (item != null) return item.Value;

                _logger.Warn<PublishValueValueEditor>("Could not find a configuration item with ID " + itemId + " for property alias " + propertyType.Alias);
            }

            // fallback to default
            return base.ConvertDbToString(propertyType, value, dataTypeService);
        }
    }
}
