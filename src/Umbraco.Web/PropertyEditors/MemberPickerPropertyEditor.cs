using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [PropertyEditor(Constants.PropertyEditors.MemberPickerAlias, "Member Picker", PropertyEditorValueTypes.Integer, "memberpicker", Group = "People", Icon = "icon-user")]
    public class MemberPickerPropertyEditor : PropertyEditor
    {
        /// <summary>
        /// The constructor will setup the property editor based on the attribute if one is found
        /// </summary>
        public MemberPickerPropertyEditor(ILogger logger) : base(logger)
        {
        }
    }
}
