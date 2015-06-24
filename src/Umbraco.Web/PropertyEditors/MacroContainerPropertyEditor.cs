using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;


namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.MacroContainerAlias, "Macro container", "macrocontainer", ValueType = "TEXT", Group="rich content", Icon="icon-settings-alt")]
    public class MacroContainerPropertyEditor : PropertyEditor
    {
        /// <summary>
        /// Creates a pre value editor instance
        /// </summary>
        /// <returns></returns>
        protected override PreValueEditor CreatePreValueEditor()
        {
            return new MacroContainerPreValueEditor();
        }

        protected override PropertyValueEditor CreateValueEditor()
        {
            //TODO: Need to add some validation to the ValueEditor to ensure that any media chosen actually exists!

            return base.CreateValueEditor();
        }

        internal class MacroContainerPreValueEditor : PreValueEditor
        {
            [PreValueField("max", "Max items", "number", Description = "The maximum number of macros that are allowed in the container")]
            public int MaxItems { get; set; }

            [PreValueField("allowed", "Allowed items", "views/propertyeditors/macrocontainer/macrolist.prevalues.html", Description = "The macro types allowed, if none are selected all macros will be allowed")]
            public object AllowedItems { get; set; }
        }

    }
}
