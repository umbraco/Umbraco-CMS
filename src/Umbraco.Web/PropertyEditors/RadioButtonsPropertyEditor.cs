using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// A property editor to allow the individual selection of pre-defined items.
    /// </summary>
    /// <remarks>
    /// Due to remaining backwards compatible, this stores the id of the item in the database which is why it is marked
    /// as INT and we have logic in here to ensure it is formatted correctly including ensuring that the INT ID value is published
    /// in cache and not the string value.
    /// </remarks>
    [PropertyEditor(Constants.PropertyEditors.RadioButtonListAlias, "Radio button list", "radiobuttons", ValueType = PropertyEditorValueTypes.Integer, Group="lists", Icon="icon-target")]
    public class RadioButtonsPropertyEditor : DropDownWithKeysPropertyEditor
    {
        /// <summary>
        /// The constructor will setup the property editor based on the attribute if one is found
        /// </summary>
        public RadioButtonsPropertyEditor(ILogger logger, ILocalizedTextService textService) : base(logger, textService)
        {
        }
    }
}