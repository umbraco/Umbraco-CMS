using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    internal class DatePreValueEditor : PreValueEditor
    {
        [PreValueField("format", "Date format", "textstring", Description = "If left empty then the format is YYYY-MM-DD. (see momentjs.com for supported formats)")]
        public string DefaultValue { get; set; }

        [PreValueField("pickDate", "Pick date", "boolean", Description = "Choose if editor should be able to pick date.")]
        public string PickDate { get; set; }

        [PreValueField("pickTime", "Pick time", "boolean", Description = "Choose if editor should be able to pick time.")]
        public string PickTime { get; set; }

        [PreValueField("useSeconds", "Use seconds", "boolean", Description = "Choose if seconds should be used.")]
        public string UseSeconds { get; set; }
    }
}