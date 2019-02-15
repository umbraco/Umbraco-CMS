using System;
using System.Collections.Generic;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents the configuration editor for the datetime value editor.
    /// </summary>
    public class DateTimeConfigurationEditor : ConfigurationEditor<DateTimeConfiguration>
    {
        private static readonly HashSet<string> TimeFormats = new HashSet<string>( new [] {"HH", "H", "mm", "m"});

        public override IDictionary<string, object> ToValueEditor(object configuration)
        {
            var d = base.ToValueEditor(configuration);

            var format = d["format"].ToString();

            d["pickTime"] = ContainsTimeElement(format);
            return d;
        }

        private bool ContainsTimeElement(string format)
        {
            if (string.IsNullOrWhiteSpace(format))
            {
                return false;
            }

            var elements = format.Split(new[] {' ', '-', '.', ':', '/', '\\'}, StringSplitOptions.RemoveEmptyEntries);

            foreach (var element in elements)
            {
                if (TimeFormats.Contains(element))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
