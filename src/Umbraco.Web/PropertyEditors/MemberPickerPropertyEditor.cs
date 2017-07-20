using System;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [Obsolete("This editor is obsolete, use MemberPickerPropertyEditor2 instead which stores UDI")]
    [PropertyEditor(Constants.PropertyEditors.MemberPickerAlias, "(Obsolete) Member Picker", PropertyEditorValueTypes.Integer, "memberpicker", Group = "People", Icon = "icon-user", IsDeprecated = true)]
    public class MemberPickerPropertyEditor : MemberPicker2PropertyEditor
    {
        /// <summary>
        /// The constructor will setup the property editor based on the attribute if one is found
        /// </summary>
        public MemberPickerPropertyEditor(ILogger logger)
            : base(logger)
        {
            InternalPreValues["idType"] = "int";
        }
    }
}
