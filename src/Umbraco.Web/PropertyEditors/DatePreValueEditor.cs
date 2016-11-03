using Umbraco.Core.PropertyEditors;

namespace Umbraco.Web.PropertyEditors
{
    internal class DatePreValueEditor : PreValueEditor
    {
        [PreValueField("format", "Date format", "textstring", Description = "If left empty then the format is YYYY-MM-DD. (see momentjs.com for supported formats)")]
        public string DefaultValue { get; set; }

        [PreValueField("lowestYear", "Lowest allowable year", "number", Description = "The default is 1900 and the lowest is 1753 (SQLCE limitation)")]
   
        public int LowestYear { get; set; }
        
        [PreValueField("numberOfYearsIntoTheFuture", "No. Years into the future", "number", Description = "How many years into the future should be allowed, default 100")]
        public int YearsIntoTheFuture { get; set; }

    }
}