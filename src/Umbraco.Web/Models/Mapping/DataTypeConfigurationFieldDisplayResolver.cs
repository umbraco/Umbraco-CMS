using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Web.Composing;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    internal class DataTypeConfigurationFieldDisplayResolver
    {
        private readonly IDataTypeService _dataTypeService;

        public DataTypeConfigurationFieldDisplayResolver(IDataTypeService dataTypeService)
        {
            _dataTypeService = dataTypeService;
        }

        /// <summary>
        /// Maps pre-values in the dictionary to the values for the fields
        /// </summary>
        internal static void MapPreValueValuesToPreValueFields(DataTypeConfigurationFieldDisplay[] fields, IDataTypeConfiguration configuration)
        {
            if (fields == null) throw new ArgumentNullException(nameof(fields));
            if (configuration == null) throw new ArgumentNullException(nameof(configuration));

            var preValues = configuration.ToDictionary();

            //now we need to wire up the pre-values values with the actual fields defined
            foreach (var field in fields)
            {
                // fixme - how can we do this with configuration?
                // we need the configuration to be able to be returned as Dictionary<string, object> in order to be edited!

                var found = preValues.Any(x => x.Key.InvariantEquals(field.Key));
                if (found == false)
                {
                    Current.Logger.Warn<DataTypeConfigurationFieldDisplayResolver>("Could not find persisted pre-value for field " + field.Key);
                    continue;
                }
                field.Value = preValues.Single(x => x.Key.InvariantEquals(field.Key)).Value;
            }
        }

        /// <summary>
        /// Creates a set of configuration fields for a data type.
        /// </summary>
        public IEnumerable<DataTypeConfigurationFieldDisplay> Resolve(IDataType dataType)
        {
            if (string.IsNullOrWhiteSpace(dataType.EditorAlias) || !Current.PropertyEditors.TryGet(dataType.EditorAlias, out var editor))
                throw new InvalidOperationException($"Could not find a property editor with alias \"{dataType.EditorAlias}\".");

            var configuration = dataType.Configuration;
            var fields = editor.PreValueEditor.Fields.Select(Mapper.Map<DataTypeConfigurationFieldDisplay>).ToArray();
            var wtf = editor.PreValueEditor.ConvertDbToEditor(editor.DefaultPreValues, configuration); // but wtf?

            //set up the defaults
            var dataTypeService = _dataTypeService.Value;
            var preVals = dataTypeService.GetPreValuesCollectionByDataTypeId(dataType.Id);
            IDictionary<string, object> dictionaryVals = preVals.FormatAsDictionary().ToDictionary(x => x.Key, x => (object)x.Value);
            var result = Enumerable.Empty<DataTypeConfigurationFieldDisplay>().ToArray();

            //if we have a prop editor, then format the pre-values based on it and create it's fields.
            if (propEd != null)
            {
                result = propEd.PreValueEditor.Fields.Select(Mapper.Map<DataTypeConfigurationFieldDisplay>).ToArray();
                dictionaryVals = propEd.PreValueEditor.ConvertDbToEditor(propEd.DefaultPreValues, preVals);
            }

            MapPreValueValuesToPreValueFields(result, dictionaryVals);

            return result;
        }
    }
}
