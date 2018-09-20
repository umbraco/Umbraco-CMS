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
    /// Due to remaining backwards compatible, this stores the id of the drop down item in the database which is why it is marked
    /// as INT and we have logic in here to ensure it is formatted correctly including ensuring that the INT ID value is published
    /// in cache and not the string value.
    /// </remarks>
    [DataEditor(Constants.PropertyEditors.Aliases.DropdownlistPublishKeys, "Dropdown list, publishing keys", "dropdown", ValueType = ValueTypes.Integer, Group = "lists", Icon = "icon-indent", IsDeprecated = true)]
    public class DropDownWithKeysPropertyEditor : DataEditor
    {
        private readonly ILocalizedTextService _textService;

        /// <summary>
        /// The constructor will setup the property editor based on the attribute if one is found
        /// </summary>
        public DropDownWithKeysPropertyEditor(ILogger logger, ILocalizedTextService textService)
            : base(logger)
        {
            _textService = textService;
        }

        /// <summary>
        /// Return a custom pre-value editor
        /// </summary>
        /// <returns></returns>
        protected override IConfigurationEditor CreateConfigurationEditor() => new ValueListConfigurationEditor(_textService);
    }
}
