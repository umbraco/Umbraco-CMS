using System;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Umbraco.Core;

namespace Umbraco.Web.WebApi
{
    /// <summary>
    /// Used to convert the format of a DateTime object when serializing
    /// </summary>
    internal class CustomDateTimeConvertor : IsoDateTimeConverter
    {
        private readonly string _dateTimeFormat;
        
        public CustomDateTimeConvertor(string dateTimeFormat)
        {
            Mandate.ParameterNotNullOrEmpty(dateTimeFormat, "dateTimeFormat");
            _dateTimeFormat = dateTimeFormat;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            try
            {
                writer.WriteValue(((DateTime)value).ToString(_dateTimeFormat));
            }
            catch (ArgumentOutOfRangeException ex)
            {
                // in some calendars like Persian, dates are much smaller than Gregorian,
                // so DateTime.MinValue will cause ArgumentOutOfRangeException
                // here we make sure if ArgumentOutOfRangeException happends we ignore the culture
                writer.WriteValue(((DateTime)value).ToString(_dateTimeFormat, CultureInfo.InvariantCulture));
            }
        }
    }
}
