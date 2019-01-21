using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    // todo - MacroContainerPropertyEditor is deprecated, but what's the alternative?
    [DataEditor(Constants.PropertyEditors.Aliases.MacroContainer, "(Obsolete) Macro Picker", "macrocontainer", ValueType = ValueTypes.Text, Group="rich content", Icon="icon-settings-alt", IsDeprecated = true)]
    public class MacroContainerPropertyEditor : DataEditor
    {
        public MacroContainerPropertyEditor(ILogger logger)
            : base(logger)
        { }

        protected override IConfigurationEditor CreateConfigurationEditor() => new MacroContainerConfigurationEditor();
    }
}
