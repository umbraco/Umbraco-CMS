using System.Buffers;
using System.IO;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Newtonsoft.Json;

namespace Umbraco.Web.Common.Formatters
{
    /// <summary>
    /// This will format the JSON output for use with AngularJs's approach to JSON Vulnerability attacks
    /// </summary>
    /// <remarks>
    /// See: http://docs.angularjs.org/api/ng.$http (Security considerations)
    /// </remarks>
    public class AngularJsonMediaTypeFormatter : NewtonsoftJsonOutputFormatter
    {
        public const string XsrfPrefix = ")]}',\n";

        public AngularJsonMediaTypeFormatter(JsonSerializerSettings serializerSettings, ArrayPool<char> charPool, MvcOptions mvcOptions)
            : base(serializerSettings, charPool, mvcOptions)
        {
        }

        protected override JsonWriter CreateJsonWriter(TextWriter writer)
        {
            var jsonWriter = base.CreateJsonWriter(writer);

            jsonWriter.WriteRaw(XsrfPrefix);

            return jsonWriter;
        }
    }
}
