using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;
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

    }
}