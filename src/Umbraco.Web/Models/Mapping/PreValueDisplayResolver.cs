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

        protected override IEnumerable<PreValueFieldDisplay> ResolveCore(IDataTypeDefinition source)
        {
            var propEd = PropertyEditorResolver.Current.GetById(source.ControlId);
            if (propEd == null)
            {
                throw new InvalidOperationException("Could not find property editor with id " + source.ControlId);
            }

            var dataTypeService = (DataTypeService) _dataTypeService.Value;
            var preVals = dataTypeService.GetPreValuesCollectionByDataTypeId(source.Id);
            var dictionaryVals = PreValueCollection.AsDictionary(preVals);

            var result = propEd.PreValueEditor.Fields.Select(Mapper.Map<PreValueFieldDisplay>).ToArray();
            var currentIndex = 0; //used if the collection is non-dictionary based.
            foreach (var field in result)
            {
                if (preVals.IsDictionaryBased == false)
                {
                    //we'll just need to wire up the values based on the order that the pre-values are stored
                    var found = dictionaryVals.Any(x => x.Key.InvariantEquals(currentIndex.ToInvariantString()));
                    if (found == false)
                    {
                        LogHelper.Warn<PreValueDisplayResolver>("Could not find persisted pre-value for index " + currentIndex);
                        continue;
                    }
                    field.Value = (string) dictionaryVals.Single(x => x.Key.InvariantEquals(currentIndex.ToInvariantString())).Value;
                    currentIndex++;
                }
                else
                {
                    var found = dictionaryVals.Any(x => x.Key.InvariantEquals(field.Key));
                    if (found == false)
                    {
                        LogHelper.Warn<PreValueDisplayResolver>("Could not find persisted pre-value for field " + field.Key);
                        continue;
                    }
                    field.Value = (string)dictionaryVals.Single(x => x.Key.InvariantEquals(field.Key)).Value;
                }

                
            }

            return result;
        }
    }
}