using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    internal class DateConfigurationEditor : DataTypeConfigurationEditor
    {
        [DataTypeConfigurationField("format", "Date format", "textstring", Description = "If left empty then the format is YYYY-MM-DD. (see momentjs.com for supported formats)")]
        public string DefaultValue { get; set; }
    }
}
