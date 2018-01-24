using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents a property editor that allows multiple selection of pre-defined items.
    /// </summary>
    /// <remarks>
    /// Due to backwards compatibility, this editor stores the value as a CSV string listing
    /// the ids of individual items.
    /// </remarks>
    [PropertyEditor(Constants.PropertyEditors.Aliases.DropdownlistMultiplePublishKeys, "Dropdown list multiple, publish keys", "dropdown", Group = "lists", Icon = "icon-bulleted-list")]
    public class DropDownMultipleWithKeysPropertyEditor : DropDownPropertyEditor
    {
        private readonly ILocalizedTextService _textService;

        /// <summary>
        /// Initializes a new instance of the <see cref="DropDownMultiplePropertyEditor"/> class.
        /// </summary>
        public DropDownMultipleWithKeysPropertyEditor(ILogger logger, ILocalizedTextService textService)
            : base(logger, textService)
        {
            _textService = textService;
        }

        /// <inheritdoc />
        protected override ValueEditor CreateValueEditor()
        {
            return new PublishValuesMultipleValueEditor(true, base.CreateValueEditor());
        }

        /// <inheritdoc />
        protected override ConfigurationEditor CreateConfigurationEditor()
        {
            return new DropDownMultipleConfigurationEditor(_textService, Logger);
        }
    }
}
