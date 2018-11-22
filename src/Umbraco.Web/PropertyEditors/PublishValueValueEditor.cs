using System.Collections.Generic;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// A custom value editor for any property editor that stores a pre-value int id so that we can ensure that the 'value' not the ID get's put into cache
    /// </summary>
    /// <remarks>
    /// This is required for legacy/backwards compatibility, otherwise we'd just store the string version and cache the string version without
    /// needing additional lookups.
    /// </remarks>
    internal class PublishValueValueEditor : PropertyValueEditorWrapper
    {
        private readonly IDataTypeService _dataTypeService;

        internal PublishValueValueEditor(IDataTypeService dataTypeService, PropertyValueEditor wrapped)
            : base(wrapped)
        {
            _dataTypeService = dataTypeService;
        }

        public PublishValueValueEditor(PropertyValueEditor wrapped)
            : this(ApplicationContext.Current.Services.DataTypeService, wrapped)
        {
        }

        /// <summary>
        /// Need to lookup the pre-values and put the string version in cache, not the ID (which is what is stored in the db)
        /// </summary>
        /// <param name="property"></param>
        /// <param name="propertyType"></param>
        /// <param name="dataTypeService"></param>
        /// <returns></returns>
        public override string ConvertDbToString(Property property, PropertyType propertyType, IDataTypeService dataTypeService)
        {
            if (property.Value == null)
                return null;

            var idAttempt = property.Value.TryConvertTo<int>();
            if (idAttempt.Success)
            {
                var preValId = idAttempt.Result;
                var preVals = GetPreValues(property);
                if (preVals != null)
                {
                    if (preVals.Any(x => x.Value.Id == preValId))
                    {
                        return preVals.Single(x => x.Value.Id == preValId).Value.Value;
                    }

                    LogHelper.Warn<PublishValueValueEditor>("Could not find a pre value with ID " + preValId + " for property alias " + property.Alias);
                }
            }
            
            return base.ConvertDbToString(property, propertyType, dataTypeService);
        }

        protected IDictionary<string, PreValue> GetPreValues(Property property)
        {
            var preVals = _dataTypeService.GetPreValuesCollectionByDataTypeId(property.PropertyType.DataTypeDefinitionId);
            if (preVals != null)
            {
                var dictionary = preVals.FormatAsDictionary();
                return dictionary;
            }
            return null;
        }
    }
}