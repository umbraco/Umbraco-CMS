using System;
using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    // TODO: Remove in V8
    [Obsolete("This editor is obsolete, use MultiUrlPickerPropertyEditor instead")]
    [PropertyEditor(Constants.PropertyEditors.RelatedLinks2Alias, "Related links", "relatedlinks", ValueType = PropertyEditorValueTypes.Json, Icon = "icon-thumbnail-list", Group = "pickers", IsDeprecated = true)]
    public class RelatedLinks2PropertyEditor : PropertyEditor
    {
        public RelatedLinks2PropertyEditor()
        {
            InternalPreValues = new Dictionary<string, object>
            {
                {"bypassUserPermissions", "0"},
                {"idType", "udi"}
            };
        }

        internal IDictionary<string, object> InternalPreValues;
        public override IDictionary<string, object> DefaultPreValues
        {
            get { return InternalPreValues; }
            set { InternalPreValues = value; }
        }

        protected override PreValueEditor CreatePreValueEditor()
        {
            return new RelatedLinksPreValueEditor();
        }

        internal class RelatedLinksPreValueEditor : PreValueEditor
        {
            [PreValueField("bypassUserPermissions", "Bypass User Permissions", "boolean", Description = "If set, user and group start node permissions will be bypassed.")]
            public bool BypassUserPermissions { get; set; }

            [PreValueField("max", "Maximum number of links", "number", Description = "Enter the maximum amount of links to be added, enter 0 for unlimited")]
            public int Maximum { get; set; }            
        }
    }
}
