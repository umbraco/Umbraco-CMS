using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    internal class DatePreValueEditor : PreValueEditor
    {
        [PreValueField("format", "Date format", "textstring", Description = "If left empty then the format is YYYY-MM-DD. (see momentjs.com for supported formats)")]
        public string DefaultValue { get; set; }

        [PreValueField("defaultEmpty", "Default empty", "boolean", Description = "When enabled the date picker will remain empty when opened. Otherwise it will default to \"today\".")]
        public string DefaultEmpty { get; set; }
    }
}
