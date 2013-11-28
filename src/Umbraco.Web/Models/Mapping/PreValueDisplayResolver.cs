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
        private readonly Lazy<IDataTypeService> _dataTypeService;

        public PreValueDisplayResolver(Lazy<IDataTypeService> dataTypeService)
        {
            _dataTypeService = dataTypeService;
        }

        /// <summary>
        /// Maps pre-values in the dictionary to the values for the fields
        /// </summary>
        /// <param name="fields"></param>
        /// <param name="preValues"></param>        
        internal static void MapPreValueValuesToPreValueFields(PreValueFieldDisplay[] fields, IDictionary<string, object> preValues)
        {
            if (fields == null) throw new ArgumentNullException("fields");
            if (preValues == null) throw new ArgumentNullException("preValues");
            //now we need to wire up the pre-values values with the actual fields defined            
            foreach (var field in fields)
            {
                var found = preValues.Any(x => x.Key.InvariantEquals(field.Key));
                if (found == false)
                {
                    LogHelper.Warn<PreValueDisplayResolver>("Could not find persisted pre-value for field " + field.Key);
                    continue;
                }
                field.Value = preValues.Single(x => x.Key.InvariantEquals(field.Key)).Value;
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

            //set up the defaults
            var dataTypeService = _dataTypeService.Value;
            var preVals = dataTypeService.GetPreValuesCollectionByDataTypeId(source.Id);
            IDictionary<string, object> dictionaryVals = preVals.FormatAsDictionary().ToDictionary(x => x.Key, x => (object)x.Value);
            var result = Enumerable.Empty<PreValueFieldDisplay>().ToArray();

            //if we have a prop editor, then format the pre-values based on it and create it's fields.
            if (propEd != null)
            {
                result = propEd.PreValueEditor.Fields.Select(Mapper.Map<PreValueFieldDisplay>).ToArray();
                dictionaryVals = propEd.PreValueEditor.ConvertDbToEditor(propEd.DefaultPreValues, preVals);
            }

            MapPreValueValuesToPreValueFields(result, dictionaryVals);

            return result;
        }

        protected override IEnumerable<PreValueFieldDisplay> ResolveCore(IDataTypeDefinition source)
        {
            return Convert(source);
        }
    }
}