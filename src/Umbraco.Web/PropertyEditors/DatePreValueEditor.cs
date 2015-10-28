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

        [PreValueField("showToday", "Show today button", "boolean", Description = "Choose if today button should be shown.")]
        public string ShowTodayButton { get; set; }

        [PreValueField("calendarWeeks", "Show calendar weeks", "boolean", Description = "Choose if calendar weeks should be shown.")]
        public string CalendarWeeks { get; set; }

        [PreValueField("minuteStepping", "Minute stepping", "number", Description = "Choose the number of minutes the time should increase/decrease (default is 1 minute).")]
        public string MinuteStepping { get; set; }

    }
}