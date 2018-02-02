using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    /// <summary>
    /// Represents the configuration for the date value editor.
    /// </summary>
    public class DateConfiguration
    {
        [ConfigurationField("format", "Date format", "textstring", Description = "If left empty then the format is YYYY-MM-DD. (see momentjs.com for supported formats)")]
        public string DefaultValue { get; set; } // fixme datetime?
    }
}