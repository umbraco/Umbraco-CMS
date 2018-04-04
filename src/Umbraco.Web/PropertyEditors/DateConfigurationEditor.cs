using System.Collections.Generic;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents the configuration editor for the date value editor.
    /// </summary>
    public class DateConfigurationEditor : ConfigurationEditor<DateConfiguration>
    {
        public override IDictionary<string, object> ToValueEditor(object configuration)
        {
            var d = base.ToValueEditor(configuration);
            d["pickTime"] = false;
            return d;
        }
    }
}
