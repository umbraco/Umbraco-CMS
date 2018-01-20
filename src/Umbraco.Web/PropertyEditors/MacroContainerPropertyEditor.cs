using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.Aliases.MacroContainer, "(Obsolete) Macro Picker", "macrocontainer", ValueType = PropertyEditorValueTypes.Text, Group="rich content", Icon="icon-settings-alt", IsDeprecated = true)]
    public class MacroContainerPropertyEditor : PropertyEditor
    {
        public MacroContainerPropertyEditor(ILogger logger)
            : base(logger)
        {
        }

        protected override PreValueEditor CreateConfigurationEditor()
        {
            return new MacroContainerPreValueEditor();
        }

        internal class MacroContainerPreValueEditor : PreValueEditor
        {
            [DataTypeConfigurationField("max", "Max items", "number", Description = "The maximum number of macros that are allowed in the container")]
            public int MaxItems { get; set; }

            [DataTypeConfigurationField("allowed", "Allowed items", "views/propertyeditors/macrocontainer/macrolist.prevalues.html", Description = "The macro types allowed, if none are selected all macros will be allowed")]
            public object AllowedItems { get; set; }
        }
    }
}
