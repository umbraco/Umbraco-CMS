using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;

namespace Umbraco.Web.PropertyEditors
{
    [DataEditor(Constants.PropertyEditors.Aliases.DropDownListFlexible, "Dropdown", "dropdownFlexible", Group = "lists", Icon = "icon-indent")]
    public class DropDownFlexiblePropertyEditor : DataEditor
    {
        public DropDownFlexiblePropertyEditor(ILogger logger)
            : base(logger)
        { }

        protected override IDataValueEditor CreateValueEditor()
        {
            return new PublishValuesMultipleValueEditor(false, Attribute);
        }

        protected override IConfigurationEditor CreateConfigurationEditor() => new DropDownFlexibleConfigurationEditor();
    }
}
