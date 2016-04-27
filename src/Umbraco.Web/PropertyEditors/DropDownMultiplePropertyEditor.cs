using Newtonsoft.Json;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// A property editor to allow multiple selection of pre-defined items
    /// </summary>
    /// <remarks>
    /// Due to maintaining backwards compatibility this data type stores the value as a string which is a comma separated value of the 
    /// ids of the individual items so we have logic in here to deal with that.
    /// </remarks>
    [ParameterEditor("propertyTypePickerMultiple", "Name", "textbox")]
    [ParameterEditor("contentTypeMultiple", "Name", "textbox")]
    [ParameterEditor("tabPickerMultiple", "Name", "textbox")]
    [PropertyEditor(Constants.PropertyEditors.DropDownListMultipleAlias, "Dropdown list multiple", "dropdown", Group = "lists", Icon="icon-bulleted-list")]
    public class DropDownMultiplePropertyEditor : DropDownMultipleWithKeysPropertyEditor
    {
        /// <summary>
        /// The constructor will setup the property editor based on the attribute if one is found
        /// </summary>
        public DropDownMultiplePropertyEditor(ILogger logger, ILocalizedTextService textService) : base(logger, textService)
        {
        }

        protected override PropertyValueEditor CreateValueEditor()
        {
            return new PublishValuesMultipleValueEditor(false, base.CreateValueEditor());
        }

    }

    

    
}