using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using umbraco;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// A property editor to allow the individual selection of pre-defined items.
    /// </summary>
    /// <remarks>
    /// Due to remaining backwards compatible, this stores the id of the drop down item in the database which is why it is marked
    /// as INT and we have logic in here to ensure it is formatted correctly including ensuring that the string value is published
    /// in cache and not the int ID.
    /// </remarks>
    [PropertyEditor(Constants.PropertyEditors.DropDownList, "Dropdown list", "dropdown", ValueType = "INT")]
    public class DropDownPropertyEditor : DropDownWithKeysPropertyEditor
    {

        /// <summary>
        /// We need to override the value editor so that we can ensure the string value is published in cache and not the integer ID value.
        /// </summary>
        /// <returns></returns>
        protected override ValueEditor CreateValueEditor()
        {
            return new DropDownValueEditor(base.CreateValueEditor());
        }

        /// <summary>
        /// A custom value editor for the drop down so that we can ensure that the 'value' not the ID get's put into cache
        /// </summary>
        /// <remarks>
        /// This is required for legacy/backwards compatibility, otherwise we'd just store the string version and cache the string version without
        /// needing additional lookups.
        /// </remarks>
        internal class DropDownValueEditor : ValueEditorWrapper
        {
            private readonly DataTypeService _dataTypeService;

            internal DropDownValueEditor(IDataTypeService dataTypeService, ValueEditor wrapped)
                : base(wrapped)
            {
                _dataTypeService = (DataTypeService)dataTypeService;
            }

            public DropDownValueEditor(ValueEditor wrapped)
                : this(ApplicationContext.Current.Services.DataTypeService, wrapped)
            {
            }

            /// <summary>
            /// Need to lookup the pre-values and put the string version in cache, not the ID (which is what is stored in the db)
            /// </summary>
            /// <param name="property"></param>
            /// <returns></returns>
            public override object FormatValueForCache(Property property)
            {
                var preValId = property.Value.TryConvertTo<int>();
                if (preValId.Success)
                {
                    var preVals = _dataTypeService.GetPreValuesCollectionByDataTypeId(property.PropertyType.DataTypeDefinitionId);
                    if (preVals != null)
                    {
                        var dictionary = PreValueCollection.AsDictionary(preVals);
                        if (dictionary.Any(x => x.Value.Id == preValId.Result))
                        {
                            return dictionary.Single(x => x.Value.Id == preValId.Result).Value.Value;
                        }

                        LogHelper.Warn<DropDownValueEditor>("Could not find a pre value with ID " + preValId + " for property alias " + property.Alias);
                    }
                }

                return base.FormatValueForCache(property);
            }
        }
    }
}