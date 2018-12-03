using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;

namespace Umbraco.Web.PropertyEditors
{
    [DataEditor(Constants.PropertyEditors.Aliases.DropDownListFlexible, "Dropdown", "dropdownFlexible", Group = "lists", Icon = "icon-indent")]
    public class DropDownFlexiblePropertyEditor : DataEditor
    {
        private readonly ILocalizedTextService _textService;

        public DropDownFlexiblePropertyEditor(ILocalizedTextService textService, ILogger logger)
            : base(logger)
        {
            _textService = textService;
        }

        protected override IDataValueEditor CreateValueEditor()
        {
            return new PublishValuesMultipleValueEditor(Logger, Attribute);
        }

        protected override IConfigurationEditor CreateConfigurationEditor() => new DropDownFlexibleConfigurationEditor(_textService);
    }
}
