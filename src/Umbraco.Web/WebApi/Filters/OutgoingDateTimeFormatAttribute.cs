using System;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web.Http.Controllers;
using Newtonsoft.Json.Converters;

namespace Umbraco.Web.WebApi.Filters
{
    /// <summary>
    /// Sets the json outgoing/serialized datetime format
    /// </summary>
    internal sealed class JsonDateTimeFormatAttributeAttribute : Attribute, IControllerConfiguration
    {
        private readonly string _format = "yyyy-MM-dd HH:mm:ss";

        /// <summary>
        /// Specify a custom format
        /// </summary>
        /// <param name="format"></param>
        public JsonDateTimeFormatAttributeAttribute(string format)
        {
            if (format == null) throw new ArgumentNullException(nameof(format));
            if (string.IsNullOrEmpty(format)) throw new ArgumentException("Value can't be empty.", nameof(format));

            _format = format;
        }

        /// <summary>
        /// Will use the standard ISO format
        /// </summary>
        public JsonDateTimeFormatAttributeAttribute()
        {

        }

        public void Initialize(HttpControllerSettings controllerSettings, HttpControllerDescriptor controllerDescriptor)
        {
            var jsonFormatter = controllerSettings.Formatters.OfType<JsonMediaTypeFormatter>();
            foreach (var r in jsonFormatter)
            {
                r.SerializerSettings.Converters.Add(
                    new IsoDateTimeConverter
                    {
                        DateTimeFormat = _format
                    });
            }
        }

    }
}
