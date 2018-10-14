using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Web.Composing;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    internal class DataTypeConfigurationFieldDisplayResolver
    {
        private readonly ILogger _logger;

        public DataTypeConfigurationFieldDisplayResolver(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Maps pre-values in the dictionary to the values for the fields
        /// </summary>
        internal static void MapConfigurationFields(ILogger logger, DataTypeConfigurationFieldDisplay[] fields, IDictionary<string, object> configuration)
        {
            if (fields == null) throw new ArgumentNullException(nameof(fields));
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            // now we need to wire up the pre-values values with the actual fields defined
            foreach (var field in fields)
            {
                if (configuration.TryGetValue(field.Key, out var value))
                    field.Value = value;
                else
                {
                    // weird - just leave the field without a value - but warn
                    logger.Warn<DataTypeConfigurationFieldDisplayResolver>("Could not find a value for configuration field '{ConfigField}'", field.Key);
                }
                
            }
        }

        /// <summary>
        /// Creates a set of configuration fields for a data type.
        /// </summary>
        public IEnumerable<DataTypeConfigurationFieldDisplay> Resolve(IDataType dataType)
        {
            // in v7 it was apparently fine to have an empty .EditorAlias here, in which case we would map onto
            // an empty fields list, which made no sense since there would be nothing to map to - and besides,
            // a datatype without an editor alias is a serious issue - v8 wants an editor here

            if (string.IsNullOrWhiteSpace(dataType.EditorAlias) || !Current.PropertyEditors.TryGet(dataType.EditorAlias, out var editor))
                throw new InvalidOperationException($"Could not find a property editor with alias \"{dataType.EditorAlias}\".");
 
            var configurationEditor = editor.GetConfigurationEditor();
            var fields = configurationEditor.Fields.Select(Mapper.Map<DataTypeConfigurationFieldDisplay>).ToArray();
            var configurationDictionary = configurationEditor.ToConfigurationEditor(dataType.Configuration);

            MapConfigurationFields(_logger, fields, configurationDictionary);

            return fields;
        }
    }
}
