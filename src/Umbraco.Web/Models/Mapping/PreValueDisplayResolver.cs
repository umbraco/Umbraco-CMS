using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Models.Mapping
{
    internal class PreValueDisplayResolver : ValueResolver<IDataTypeDefinition, IEnumerable<PreValueFieldDisplay>>
    {
        private readonly IDataTypeService _dataTypeService;

        public PreValueDisplayResolver(IDataTypeService dataTypeService)
        {
            _dataTypeService = dataTypeService;
        }

        /// <summary>
        /// Maps pre-values in the dictionary to the values for the fields.
        /// </summary>
        /// <param name="fields">The fields.</param>
        /// <param name="preValues">The pre-values.</param>
        /// <param name="editorAlias">The editor alias.</param>
        internal static void MapPreValueValuesToPreValueFields(IEnumerable<PreValueFieldDisplay> fields, IDictionary<string, object> preValues, string editorAlias)
        {
            if (fields == null) throw new ArgumentNullException(nameof(fields));
            if (preValues == null) throw new ArgumentNullException(nameof(preValues));

            // Now we need to wire up the pre-values values with the actual fields defined
            foreach (var field in fields)
            {
                // If the dictionary would be constructed with StringComparer.InvariantCultureIgnoreCase, we could just use TryGetValue
                var preValue = preValues.SingleOrDefault(x => x.Key.InvariantEquals(field.Key));
                if (preValue.Key == null)
                {
                    LogHelper.Warn<PreValueDisplayResolver>("Could not find persisted pre-value for field {0} on property editor {1}", () => field.Key, () => editorAlias);
                    continue;
                }

                field.Value = preValue.Value;
            }
        }

        internal IEnumerable<PreValueFieldDisplay> Convert(IDataTypeDefinition source)
        {
            PropertyEditor propEd = null;
            if (source.PropertyEditorAlias.IsNullOrWhiteSpace() == false)
            {
                propEd = PropertyEditorResolver.Current.GetByAlias(source.PropertyEditorAlias);
                if (propEd == null)
                {
                    throw new InvalidOperationException("Could not find property editor with alias " + source.PropertyEditorAlias);
                }
            }

            // Set up the defaults
            var dataTypeService = _dataTypeService;
            var preVals = dataTypeService.GetPreValuesCollectionByDataTypeId(source.Id);
            IDictionary<string, object> dictionaryVals = preVals.FormatAsDictionary().ToDictionary(x => x.Key, x => (object)x.Value);
            var result = Enumerable.Empty<PreValueFieldDisplay>();

            // If we have a prop editor, then format the pre-values based on it and create it's fields
            if (propEd != null)
            {
                result = propEd.PreValueEditor.Fields.Select(Mapper.Map<PreValueFieldDisplay>).AsEnumerable();
                if (source.IsBuildInDataType)
                {
                    result = RemovePreValuesNotSupportedOnBuildInTypes(result);
                }
                dictionaryVals = propEd.PreValueEditor.ConvertDbToEditor(propEd.DefaultPreValues, preVals);
            }

            result = result.ToArray();
            MapPreValueValuesToPreValueFields(result, dictionaryVals, source.PropertyEditorAlias);

            return result;
        }

        private IEnumerable<PreValueFieldDisplay> RemovePreValuesNotSupportedOnBuildInTypes(IEnumerable<PreValueFieldDisplay> preValues)
        {
            return preValues.Where(preValue => string.Equals(preValue.Key, Constants.DataTypes.ReservedPreValueKeys.IgnoreUserStartNodes) == false);
        }

        protected override IEnumerable<PreValueFieldDisplay> ResolveCore(IDataTypeDefinition source)
        {
            return Convert(source);
        }
    }
}
