using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    // fixme - if deprecated, what's the alternative?
    [ValueEditor(Constants.PropertyEditors.Aliases.MacroContainer, "(Obsolete) Macro Picker", "macrocontainer", ValueType = ValueTypes.Text, Group="rich content", Icon="icon-settings-alt", IsDeprecated = true)]
    public class MacroContainerPropertyEditor : PropertyEditor
    {
        public MacroContainerPropertyEditor(ILogger logger)
            : base(logger)
        { }

        protected override ConfigurationEditor CreateConfigurationEditor()
        {
            return new MacroContainerConfigurationEditor();
        }
    }
}
