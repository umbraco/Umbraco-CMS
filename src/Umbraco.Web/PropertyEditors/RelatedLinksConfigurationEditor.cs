using System.Collections.Generic;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents the configuration editor for the related links value editor.
    /// </summary>
    public class RelatedLinksConfigurationEditor : ConfigurationEditor
    {
        public override IDictionary<string, object> ToValueEditor(object configuration)
        {
            var d = base.ToValueEditor(configuration);
            d["idType"] = "udi";
            return d;
        }
    }
}