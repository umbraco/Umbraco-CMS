using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    internal class DatePreValueEditor : PreValueEditor
    {
        [PreValueField("format", "Date format", "textstring", Description = "If left empty then the format is YYYY-MM-DD. (see momentjs.com for supported formats)")]
        public string DefaultValue { get; set; }

        [PreValueField("showToday", "Show today", "boolean", Description = "Choose if today should be shown.")]
        public bool ShowToday { get; set; }

        [PreValueField("calendarWeeks", "Show calendar weeks", "boolean", Description = "Choose if calendar weeks should be shown.")]
        public string CalendarWeeks { get; set; }

        [PreValueField("minuteStepping", "Minute stepping", "number", Description = "Choose the number of minutes the time should increase/decrease (default is 1 minute).")]
        public string MinuteStepping { get; set; }

        [PreValueField("minDate", "Minimum date", "textstring", Description = "Choose the minimum date (optional - default: year 1900).")]
        public string MinDate { get; set; }

        [PreValueField("maxDate", "Maximum date", "textstring", Description = "Choose the maximum date (optional - default: now + 100 years).")]
        public string MaxDate { get; set; }

        [PreValueField("daysOfWeekDisabled", "Days of week disabled", "views/propertyeditors/datepicker/weekdays.prevalues.html", Description = "Select the days of week that should be disabled (default is none).")]
        public string[] DaysOfWeekDisabled { get; set; }
    }
}