using System.Collections.Generic;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents the configuration editor for the nested content value editor.
    /// </summary>
    public class NestedContentConfigurationEditor : ConfigurationEditor<NestedContentConfiguration>
    {
        // fixme
        public override IDictionary<string, object> DefaultConfiguration =>  new Dictionary<string, object>
        {
            {"contentTypes", ""},
            {"minItems", 0},
            {"maxItems", 0},
            {"confirmDeletes", "1"},
            {"showIcons", "1"}
        };
    }
}