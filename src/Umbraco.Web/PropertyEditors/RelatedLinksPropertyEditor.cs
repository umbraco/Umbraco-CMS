using System;
using System.Collections.Generic;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    [Obsolete("This editor is obsolete, use RelatedLinks2PropertyEditor instead which stores UDI")]
    [PropertyEditor(Constants.PropertyEditors.RelatedLinksAlias, "(Obsolete) Related links", "relatedlinks", ValueType = PropertyEditorValueTypes.Json, Icon="icon-thumbnail-list", Group="pickers", IsDeprecated = true)]
    public class RelatedLinksPropertyEditor : RelatedLinks2PropertyEditor
    {
        /// <summary>
        /// The constructor will setup the property editor based on the attribute if one is found
        /// </summary>
        public RelatedLinksPropertyEditor(ILogger logger) : base(logger)
        {
            InternalPreValues = new Dictionary<string, object>
            {
                {"idType", "int"}
            };
        }
    }
}
